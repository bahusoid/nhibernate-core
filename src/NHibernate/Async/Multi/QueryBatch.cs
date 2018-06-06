﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


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
	using System.Threading.Tasks;
	using System.Threading;
	public partial class QueryBatch : IQueryBatch
	{

		/// <inheritdoc />
		public async Task ExecuteAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (_queries.Count == 0)
				return;
			try
			{
				Init();

				if (!Session.Factory.ConnectionProvider.Driver.SupportsMultipleQueries)
				{
					foreach (var query in _queries)
					{
						await (query.ExecuteNonBatchableAsync(cancellationToken)).ConfigureAwait(false);
					}
					return;
				}

				using (Session.BeginProcess())
				{
					await (DoExecuteAsync(cancellationToken)).ConfigureAwait(false);
				}
			}
			finally
			{
				_queries.Clear();
			}
		}

		private async Task CombineQueriesAsync(IResultSetsCommand resultSetsCommand, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			foreach (var multiSource in _queries)
			foreach (var cmd in await (multiSource.GetCommandsAsync(cancellationToken)).ConfigureAwait(false))
			{
				resultSetsCommand.Append(cmd);
			}
		}

		protected async Task DoExecuteAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			var resultSetsCommand = Session.Factory.ConnectionProvider.Driver.GetResultSetsCommand(Session);
			await (CombineQueriesAsync(resultSetsCommand, cancellationToken)).ConfigureAwait(false);

			var querySpaces = new HashSet<string>(_queries.SelectMany(t => t.GetQuerySpaces()));
			if (resultSetsCommand.HasQueries)
			{
				await (Session.AutoFlushIfRequiredAsync(querySpaces, cancellationToken)).ConfigureAwait(false);
			}

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
					using (var reader = await (resultSetsCommand.GetReaderAsync(Timeout, cancellationToken)).ConfigureAwait(false))
					{
						foreach (var multiSource in _queries)
						{
							foreach (var processResultSetAction in multiSource.GetProcessResultSetActions())
							{
								rowCount += processResultSetAction(reader);
								await (reader.NextResultAsync(cancellationToken)).ConfigureAwait(false);
							}
						}
					}
				}

				foreach (var multiSource in _queries)
				{
					await (multiSource.PostProcessAsync(cancellationToken)).ConfigureAwait(false);
				}
			}
			catch (OperationCanceledException) { throw; }
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
