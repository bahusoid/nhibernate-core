using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NHibernate.Driver;
using NHibernate.Engine;
using NHibernate.Exceptions;
using NHibernate.Impl;

namespace NHibernate
{
	/// <summary>
	/// Universal query batcher
	/// </summary>
	public partial class MultiAnyQueryBatch : IMultiAnyQueryBatch
	{
		private static readonly INHibernateLogger Log = NHibernateLogger.For(typeof(MultiAnyQueryBatch));

		private readonly ISessionImplementor _session;
		List<IMultiAnyQuery> _queries = new List<IMultiAnyQuery>();

		public MultiAnyQueryBatch(ISessionImplementor session)
		{
			_session = session;
		}

		protected ISessionImplementor Session
		{
			get { return _session; }
		}

		/// <inheritdoc />
		public int? Timeout { get; set; }

		/// <inheritdoc />
		public void Execute()
		{
			if (_queries.Count == 0)
				return;
			try
			{
				Init();

				if (!Session.Factory.ConnectionProvider.Driver.SupportsMultipleQueries)
				{
					foreach (var query in _queries)
					{
						query.ExecuteNonBatchable();
					}
					return;
				}

				using (Session.BeginProcess())
				{
					DoExecute();
				}
			}
			finally
			{
				_queries.Clear();
			}
		}

		/// <inheritdoc />
		public void Add(IMultiAnyQuery query)
		{
			_queries.Add(query);
		}

		private void Init()
		{
			foreach (var query in _queries)
			{
				query.Init(Session);
			}
		}

		private void CombineQueries(IResultSetsCommand resultSetsCommand)
		{
			foreach (var multiSource in _queries)
			foreach (var cmd in multiSource.GetCommands())
			{
				resultSetsCommand.Append(cmd);
			}
		}

		protected void DoExecute()
		{
			var resultSetsCommand = Session.Factory.ConnectionProvider.Driver.GetResultSetsCommand(Session);
			CombineQueries(resultSetsCommand);

			bool statsEnabled = Session.Factory.Statistics.IsStatisticsEnabled;
			Stopwatch stopWatch = null;
			if (statsEnabled)
			{
				stopWatch = new Stopwatch();
				stopWatch.Start();
			}
			if (Log.IsDebugEnabled())
			{
				Log.Debug("Multi query with {0} queries: {1}", _queries.Count, resultSetsCommand.Sql);
			}

			int rowCount = 0;
			try
			{
				if (resultSetsCommand.HasQueries)
				{
					using (var reader = resultSetsCommand.GetReader(Timeout))
					{
						foreach (var multiSource in _queries)
						{
							foreach (var processResultSetAction in multiSource.GetProcessResultSetActions())
							{
								rowCount += processResultSetAction(reader);
								reader.NextResult();
							}
						}
					}
				}

				foreach (var multiSource in _queries)
				{
					multiSource.PostProcess();
				}
			}
			catch (Exception sqle)
			{
				Log.Error(sqle, "Failed to execute multi query: [{0}]", resultSetsCommand.Sql);
				throw ADOExceptionHelper.Convert(Session.Factory.SQLExceptionConverter, sqle, "Failed to execute multi query", resultSetsCommand.Sql);
			}

			if (statsEnabled)
			{
				stopWatch.Stop();
				Session.Factory.StatisticsImplementor.QueryExecuted($"{_queries.Count} queries", rowCount, stopWatch.Elapsed);
			}
		}
	}
}
