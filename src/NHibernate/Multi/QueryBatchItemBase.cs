using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using NHibernate.Cache;
using NHibernate.Engine;
using NHibernate.SqlCommand;
using NHibernate.Util;

namespace NHibernate.Multi
{
	/// <summary>
	/// Base class for both ICriteria and IQuery queries
	/// </summary>
	public abstract partial class QueryBatchItemBase<TResult> : IQueryBatchItem<TResult>
	{
		protected ISessionImplementor Session;
		private List<EntityKey[]>[] _subselectResultKeys;
		private IList[] _loaderResults;

		private List<QueryLoadInfo> _queryInfos;
		private IList<TResult> _finalResults;

		protected class QueryLoadInfo
		{
			public Loader.Loader Loader;
			public QueryParameters Parameters;
			
			//Cache related properties:
			public ISet<string> QuerySpaces;
			public IQueryCache Cache;
			public QueryKey CacheKey;
		}

		protected abstract List<QueryLoadInfo> GetQueryLoadInfo();

		public virtual void Init(ISessionImplementor session)
		{
			Session = session;

			_queryInfos = GetQueryLoadInfo();

			var count = _queryInfos.Count;
			_subselectResultKeys = new List<EntityKey[]>[count];
			_loaderResults = new IList[count];
		}

		public IEnumerable<ISqlCommand> GetCommands()
		{
			for (var index = 0; index < _queryInfos.Count; index++)
			{
				var qi = _queryInfos[index];

				IList resultsFromCache = null;
				qi.Loader.ProcessCachedResults(
					Session,
					qi.Parameters,
					qi.QuerySpaces,
					(cache, key, results) =>
					{
						qi.CacheKey = key;
						qi.Cache = cache;
						resultsFromCache = results;
					});

				if (resultsFromCache != null)
				{
					qi.Cache = null;
					_loaderResults[index] = resultsFromCache;
					continue;
				}

				yield return qi.Loader.CreateSqlCommand(qi.Parameters, Session);
			}
		}

		public IEnumerable<Func<DbDataReader, int>> GetResultSetHandler()
		{
			var dialect = Session.Factory.Dialect;
			List<object>[] hydratedObjects = new List<object>[_queryInfos.Count];

			for (var i = 0; i < _queryInfos.Count; i++)
			{
				Loader.Loader loader = _queryInfos[i].Loader;
				var queryParameters = _queryInfos[i].Parameters;

				//Skip processing for items already loaded from cache
				if (_queryInfos[i].CacheKey?.ResultTransformer != null && _loaderResults[i] != null)
				{
					loader.TransformCachedResults(queryParameters, _queryInfos[i].CacheKey.ResultTransformer, ref _loaderResults[i]);
					continue;
				}

				int entitySpan = loader.EntityPersisters.Length;
				hydratedObjects[i] = entitySpan == 0 ? null : new List<object>(entitySpan);
				EntityKey[] keys = new EntityKey[entitySpan];

				RowSelection selection = queryParameters.RowSelection;
				bool createSubselects = loader.IsSubselectLoadingEnabled;

				_subselectResultKeys[i] = createSubselects ? new List<EntityKey[]>() : null;
				int maxRows = Loader.Loader.HasMaxRows(selection) ? selection.MaxRows : int.MaxValue;
				bool advanceSelection = !dialect.SupportsLimitOffset || !loader.UseLimit(selection, dialect);

				var index = i;
				yield return reader =>
				{
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
					var tmpResults = new List<object>();

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
								hydratedObjects[index],
								keys,
								true,
								_queryInfos[index].CacheKey?.ResultTransformer
							);
						if (loader.IsSubselectLoadingEnabled)
						{
							_subselectResultKeys[index].Add(keys);
							keys = new EntityKey[entitySpan]; //can't reuse in this case
						}

						tmpResults.Add(o);
					}
					_loaderResults[index] = tmpResults;

					if (index == _queryInfos.Count - 1)
					{
						InitializeEntitiesAndCollections(reader, hydratedObjects);
					}
					return rowCount;
				};
			}
		}

		public void ProcessResults()
		{
			for (int i = 0; i < _queryInfos.Count; i++)
			{
				var queryInfo = _queryInfos[i];
				if (_subselectResultKeys[i] != null)
				{
					queryInfo.Loader.CreateSubselects(_subselectResultKeys[i], queryInfo.Parameters, Session);
				}

				// Handle cache if cacheable.
				if (queryInfo.Cache != null)
				{
					queryInfo.Loader.PutResultInQueryCache(Session, queryInfo.Parameters, queryInfo.Cache, queryInfo.CacheKey, _loaderResults[i]);
				}
			}
		}

		public void ExecuteNonBatched()
		{
			_finalResults = GetResultsNonBatched();
		}

		public IEnumerable<string> GetQuerySpaces()
		{
			return _queryInfos.SelectMany(q => q.QuerySpaces);
		}

		protected abstract IList<TResult> GetResultsNonBatched();

		protected List<T> GetTypedResults<T>()
		{
			if (_loaderResults == null)
			{
				throw new HibernateException("Batch wasn't executed. You must call IQueryBatch.Execute() before accessing results.");
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

		private void InitializeEntitiesAndCollections(DbDataReader reader, List<object>[] hydratedObjects)
		{
			for (int i = 0; i < _queryInfos.Count; i++)
			{
				_queryInfos[i].Loader.InitializeEntitiesAndCollections(
									hydratedObjects[i], reader, Session, Session.PersistenceContext.DefaultReadOnly);
			}
		}
	}
}
