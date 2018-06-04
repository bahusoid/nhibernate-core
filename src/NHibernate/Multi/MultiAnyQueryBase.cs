using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using NHibernate.Cache;
using NHibernate.Engine;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using NHibernate.Util;

namespace NHibernate
{
	/// <summary>
	/// Base class for both ICriteria and IQuery queries
	/// </summary>
	public abstract partial class MultiAnyQueryBase<TResult> : IMultiAnyQuery, IMultiAnyQuery<TResult>
	{
		protected ISessionImplementor Session;
		private List<object>[] _hydratedObjects;
		private List<EntityKey[]>[] _subselectResultKeys;
		private IList[] _loaderResults;

		private List<QueryLoadInfo> _queryInfos;
		private DbDataReader _reader;
		private IList<TResult> _finalResults;

		protected class QueryLoadInfo
		{
			public Loader.Loader Loader;
			public QueryParameters Parameters;
			
			//Cache related properties:
			public ISet<string> QuerySpaces;
			public Action<IList> PutInCacheAction;
			public CacheableResultTransformer CacheTransformer;
		}

		protected abstract List<QueryLoadInfo> GetQueryLoadInfo();

		public virtual void Init(ISessionImplementor session)
		{
			Session = session;

			_queryInfos = GetQueryLoadInfo();

			var count = _queryInfos.Count;
			NewArray(count, out _hydratedObjects);
			NewArray(count, out _subselectResultKeys);
			NewArray(count, out _loaderResults);
		}

		public IEnumerable<ISqlCommand> GetCommands()
		{
			for (var index = 0; index < _queryInfos.Count; index++)
			{
				var qi = _queryInfos[index];
				var resultsFromCache = qi.Loader.GetResultsIfCacheable(Session, qi.Parameters, out IQueryCache cache, out QueryKey key, qi.QuerySpaces, null);
				qi.CacheTransformer = key?.ResultTransformer;

				if (resultsFromCache != null)
				{
					_loaderResults[index] = resultsFromCache;
					continue;
				}

				if (cache != null)
				{
					qi.PutInCacheAction = (list) => qi.Loader.PutResultInQueryCache(Session, qi.Parameters, cache, key, list);
				}

				yield return qi.Loader.CreateSqlCommand(qi.Parameters, Session);
			}
		}

		public IEnumerable<Func<DbDataReader, int>> GetProcessResultSetActions()
		{
			var dialect = Session.Factory.Dialect;

			for (var i = 0; i < _queryInfos.Count; i++)
			{
				Loader.Loader loader = _queryInfos[i].Loader;
				var queryParameters = _queryInfos[i].Parameters;

				//Skip processing for items already loaded from cache
				if (_queryInfos[i].CacheTransformer != null && _loaderResults[i] != null)
				{
					loader.ProcessCachedResults(queryParameters, _queryInfos[i].CacheTransformer, ref _loaderResults[i]);
					continue;
				}

				int entitySpan = loader.EntityPersisters.Length;
				_hydratedObjects[i] = entitySpan == 0 ? null : new List<object>(entitySpan);
				EntityKey[] keys = new EntityKey[entitySpan];

				RowSelection selection = queryParameters.RowSelection;
				bool createSubselects = loader.IsSubselectLoadingEnabled;

				_subselectResultKeys[i] = createSubselects ? new List<EntityKey[]>() : null;
				int maxRows = Loader.Loader.HasMaxRows(selection) ? selection.MaxRows : int.MaxValue;
				bool advanceSelection = !dialect.SupportsLimitOffset || !loader.UseLimit(selection, dialect);

				var tmpResults = new List<object>();
				var index = i;
				yield return reader =>
				{
					_reader = reader;
					if (advanceSelection)
					{
						Loader.Loader.Advance(reader, selection);
					}
					if (queryParameters.HasAutoDiscoverScalarTypes)
					{
						loader.AutoDiscoverTypes(reader, queryParameters, null);
					}

					LockMode[] lockModeArray = loader.GetLockModes(queryParameters.LockModes);
					EntityKey optionalObjectKey = Loader.Loader.GetOptionalObjectKey(queryParameters, Session);
					int rowCount = 0;

					int count;
					for (count = 0; count < maxRows && reader.Read(); count++)
					{
						rowCount++;

						object o =
							loader.GetRowFromResultSet(
								reader,
								Session,
								queryParameters,
								lockModeArray,
								optionalObjectKey,
								_hydratedObjects[index],
								keys,
								true,
								_queryInfos[index].CacheTransformer
							);
						if (loader.IsSubselectLoadingEnabled)
						{
							_subselectResultKeys[index].Add(keys);
							keys = new EntityKey[entitySpan]; //can't reuse in this case
						}

						tmpResults.Add(o);
					}
					return rowCount;
				};

				_loaderResults[index] = tmpResults;
			}
		}

		public void PostProcess()
		{
			if (_reader == null)
				return;

			for (int i = 0; i < _queryInfos.Count; i++)
			{
				Loader.Loader loader = _queryInfos[i].Loader;
				loader.InitializeEntitiesAndCollections(
					_hydratedObjects[i], _reader, Session, Session.PersistenceContext.DefaultReadOnly);

				if (_subselectResultKeys[i] != null)
				{
					loader.CreateSubselects(_subselectResultKeys[i], _queryInfos[i].Parameters, Session);
				}

				//Maybe put in cache...
				_queryInfos[i].PutInCacheAction?.Invoke(_loaderResults[i]);
			}
			_reader = null;
		}

		public void ExecuteNonBatchable()
		{
			_finalResults = ExecuteQueryNow();
		}

		protected abstract IList<TResult> ExecuteQueryNow();

		protected List<T> GetTypedResults<T>()
		{
			if (_loaderResults == null)
			{
				throw new HibernateException("Batch wasn't executed. You must call IMultiAnyQueryBatch.Execute() before accessing results.");
			}
			List<T> results = new List<T>(_loaderResults.Sum(tr => tr.Count));
			for (int i = 0; i < _queryInfos.Count; i++)
			{
				var list = _queryInfos[i].Loader.GetResultList(
					_loaderResults[i],
					_queryInfos[i].Parameters.ResultTransformer);
				ArrayHelper.AddAll(results, list);
			}

			return results;
		}

		public IList<TResult> GetResults()
		{
			return _finalResults ?? (_finalResults = DoGetResults());
		}

		protected abstract List<TResult> DoGetResults();

		private static void NewArray<T>(int count, out T[] list)
		{
			list = new T[count];
		}
	}
}
