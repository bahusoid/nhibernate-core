using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using NHibernate.Driver;
using NHibernate.Engine;
using NHibernate.Exceptions;
using NHibernate.Impl;
using NHibernate.Linq;
using NHibernate.Loader.Criteria;
using NHibernate.Persister.Entity;
using NHibernate.SqlCommand;

namespace NHibernate
{
	public interface IMultiUniBatch
	{

		void Execute();
		void Add(IMultiUniQuery commandSource);
	}

	public interface IResult<TResult>
	{
		TResult GetResult();
	}

	public interface IMultiUniQuery
	{
		void Init();
		IEnumerable<ISqlCommand> GetCommands();
		IEnumerable<System.Action<DbDataReader>> GetResultSetDelegates();
		void PostProcess(DbDataReader reader);
		IEnumerable<IList> GetResults();
	}

	/// <summary>
	/// Base class for both ICriteria and IQuery queries
	/// </summary>
	public abstract class MultiNhibQueryBase : IMultiUniQuery
	{
		protected readonly ISessionImplementor _session;
		protected List<object>[] hydratedObjects;
		protected List<EntityKey[]>[] subselectResultKeys;
		private List<IList> results = new List<IList>();
		protected List<QueryLoadInfo> _queryLoaders;

		public class QueryLoadInfo
		{
			public Loader.Loader Loader;
			public QueryParameters Parameters;
		}

		protected abstract List<QueryLoadInfo> GetQueryLoadInfo();

		public MultiNhibQueryBase(ISessionImplementor session)
		{
			_session = session;
		}

		public virtual void Init()
		{
			_queryLoaders = GetQueryLoadInfo();

			var count = _queryLoaders.Count;
			hydratedObjects = new List<object>[count];
			subselectResultKeys = new List<EntityKey[]>[count];
		}

		public IEnumerable<ISqlCommand> GetCommands()
		{
			foreach (var qi in _queryLoaders)
			{
				yield return qi.Loader.CreateSqlCommand(qi.Parameters, _session);
			}
		}

		public IEnumerable<System.Action<DbDataReader>> GetResultSetDelegates()
		{
			var dialect = _session.Factory.Dialect;

			for (var i = 0; i < _queryLoaders.Count; i++)
			{
				Loader.Loader loader = _queryLoaders[i].Loader;
				int entitySpan = loader.EntityPersisters.Length;
				hydratedObjects[i] = entitySpan == 0 ? null : new List<object>(entitySpan);
				EntityKey[] keys = new EntityKey[entitySpan];

				var queryParameters = _queryLoaders[i].Parameters;
				RowSelection selection = queryParameters.RowSelection;
				bool createSubselects = loader.IsSubselectLoadingEnabled;

				subselectResultKeys[i] = createSubselects ? new List<EntityKey[]>() : null;
				int maxRows = Loader.Loader.HasMaxRows(selection) ? selection.MaxRows : int.MaxValue;
				bool advanceSelection = !dialect.SupportsLimitOffset || !loader.UseLimit(selection, dialect);
				IList tmpResults = new List<object>();

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
					EntityKey optionalObjectKey = Loader.Loader.GetOptionalObjectKey(queryParameters, _session);

					int count;
					for (count = 0; count < maxRows && reader.Read(); count++)
					{
						//rowCount++;

						object o =
							loader.GetRowFromResultSet(
								reader,
								_session,
								queryParameters,
								lockModeArray,
								optionalObjectKey,
								hydratedObjects[index],
								keys,
								true);
						if (loader.IsSubselectLoadingEnabled)
						{
							subselectResultKeys[index].Add(keys);
							keys = new EntityKey[entitySpan]; //can't reuse in this case
						}

						tmpResults.Add(o);
					}
				};

				results.Add(tmpResults);
			}
		}

		public void PostProcess(DbDataReader reader)
		{
			for (int i = 0; i < _queryLoaders.Count; i++)
			{
				Loader.Loader loader = _queryLoaders[i].Loader;
				loader.InitializeEntitiesAndCollections(hydratedObjects[i], reader, _session, _session.PersistenceContext.DefaultReadOnly);

				if (subselectResultKeys[i] != null)
				{
					loader.CreateSubselects(subselectResultKeys[i], _queryLoaders[i].Parameters, _session);
				}
			}
		}

		public virtual IEnumerable<IList> GetResults()
		{
			for (int i = 0; i < _queryLoaders.Count; i++)
			{
				Loader.Loader loader = _queryLoaders[i].Loader;
				yield return (IList)loader.GetResultList((IList) results[i], _queryLoaders[i].Parameters.ResultTransformer);
			}
		}
	}

	public class QueryMultiSource : MultiNhibQueryBase
	{
		private readonly AbstractQueryImpl _query;

		public QueryMultiSource(ISessionImplementor session, IQuery query) : base(session)
		{
			_query = (AbstractQueryImpl)query;
		}

		protected override List<QueryLoadInfo> GetQueryLoadInfo()
		{
			_query.VerifyParameters();
			QueryParameters queryParameters = _query.GetQueryParameters();
			queryParameters.ValidateParameters();
			return _query.GetTranslators(_session, queryParameters).Select(t => new QueryLoadInfo()
			{
				Loader = t.Loader,
				Parameters = queryParameters
			}).ToList();
		}
	}

	public class LinqMultiSource<T> : QueryMultiSource
	{
		private static Delegate _postExecuteTransformer;
		private static NhLinqExpression _linqEx;

		public LinqMultiSource(ISessionImplementor session, IQueryable<T> query):base(session, GetQuery(session, query))
		{
		}

		private static IQuery GetQuery(ISessionImplementor session, IQueryable<T> query)
		{
			var prov = query.GetNhProvider() as INhQueryProviderSupportUniBatch;
			
			var q = prov.GetPreparedQuery(query.Expression, out _linqEx);
			_postExecuteTransformer = _linqEx.ExpressionToHqlTranslationResults.PostExecuteTransformer;
			return q;
		}

		public override IEnumerable<IList> GetResults()
		{
			var r = base.GetResults();
			_postExecuteTransformer?.DynamicInvoke(r.AsQueryable());
			//return results;
			//TODO:
			//if (_linqEx.ReturnType == NhLinqExpressionReturnType.Sequence)
			//{
			//	return r.AsQueryable();
			//}

			return r;
		}
	}


	public class CriteriaMultiSource : MultiNhibQueryBase
	{
		private readonly CriteriaImpl _criteria;

		public CriteriaMultiSource(ISessionImplementor session, ICriteria criteria) : base(session)
		{
			_criteria = (CriteriaImpl) criteria;
		}

		protected override List<QueryLoadInfo> GetQueryLoadInfo()
		{
			var factory = _session.Factory;
			string[] implementors = factory.GetImplementors(_criteria.EntityOrClassName);
			int size = implementors.Length;
			var list = new List<QueryLoadInfo>(size);
			for (int i = 0; i < size; i++)
			{
				CriteriaLoader loader = new CriteriaLoader(
					factory.GetEntityPersister(implementors[i]) as IOuterJoinLoadable,
					factory,
					_criteria,
					implementors[i],
					_session.EnabledFilters
				);
				list.Add(
					new QueryLoadInfo()
					{
						Loader = loader,
						Parameters = loader.Translator.GetQueryParameters()
					});
			}

			return list;
		}
	}

	public class MultiUniBatch : IMultiUniBatch
	{
		private static readonly INHibernateLogger Log = NHibernateLogger.For(typeof(MultiUniBatch));

		//private IResultTransformer _resultTransformer;
		private readonly ISessionImplementor _session;
		List<IMultiUniQuery> _queries = new List<IMultiUniQuery>();
		private readonly IResultSetsCommand _resultSetsCommand;
		//private readonly Dialect.Dialect _dialect;
		private int? _timeout;
		private int _commandsCount;

		public MultiUniBatch(ISessionImplementor session)
		{
			_session = session;
			//_dialect = session.Factory.Dialect;
			_resultSetsCommand = session.Factory.ConnectionProvider.Driver.GetResultSetsCommand(session);
		}

		public void Execute()
		{
			using (_session.BeginProcess())
			{
				//bool cacheable = session.Factory.Settings.IsQueryCacheEnabled && isCacheable;
				//combinedParameters = CreateCombinedQueryParameters();

				if (Log.IsDebugEnabled())
				{
					Log.Debug("Multi query with {0} queries.", _queries.Count);
					for (int i = 0; i < _queries.Count; i++)
					{
						Log.Debug("Query #{0}: {1}", i, _queries[i]);
					}
				}

				CombineQueries();

				DoExecute();
			}
		}

		private void CombineQueries()
		{
			foreach (var multiSource in _queries)
			foreach (var cmd in multiSource.GetCommands())
			{
				++_commandsCount;
				_resultSetsCommand.Append(cmd);
			}
		}

		protected void DoExecute()
		{
			bool statsEnabled = _session.Factory.Statistics.IsStatisticsEnabled;
			var stopWatch = new Stopwatch();
			if (statsEnabled)
			{
				stopWatch.Start();
			}
			//TODO:
			int rowCount = 0;

			//var results = new List<object>();
			try
			{
				using (var reader = _resultSetsCommand.GetReader(_timeout))
				{
					foreach (var multiSource in _queries)
					{
						foreach (var resultSetDelegate in multiSource.GetResultSetDelegates())
						{
							resultSetDelegate(reader);
							reader.NextResult();
						}
					}

					foreach (var multiSource in _queries)
					{
						multiSource.PostProcess(reader);
					}
				}
			}
			catch (Exception sqle)
			{
				Log.Error(sqle, "Failed to execute multi query: [{0}]", _resultSetsCommand.Sql);
				throw ADOExceptionHelper.Convert(_session.Factory.SQLExceptionConverter, sqle, "Failed to execute multi query", _resultSetsCommand.Sql);
			}

			if (statsEnabled)
			{
				stopWatch.Stop();
				_session.Factory.StatisticsImplementor.QueryExecuted(string.Format("{0} queries", _commandsCount), rowCount, stopWatch.Elapsed);
			}

			foreach (var q in _queries)
			{
				q.GetResults();
			}
		}

		public void Add(IMultiUniQuery commandSource)
		{
			commandSource.Init();
			_queries.Add(commandSource);
		}
	}
}

