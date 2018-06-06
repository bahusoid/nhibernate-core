using System;
using System.Collections.Generic;
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.SqlCommand;

namespace NHibernate
{
	/// <summary>
	/// Interface for wrapping query to be batchable by <see cref="IQueryBatch"/>
	/// </summary>
	public interface IQueryBatchItem<TResult> : IQueryBatchItem
	{
		/// <summary>
		/// Returns loaded typed results by query.
		/// Must be called only after <see cref="IQueryBatch.Execute"/>.
		/// </summary>
		IList<TResult> GetResults();

		/// <summary>
		/// Event is executed after results are loaded by batch.
		/// Loaded results are provided in action parameter.
		/// </summary>
		event Action<IList<TResult>> OnAfterLoad;
	}

	/// <summary>
	/// Interface for wrapping query to be batchable by <see cref="IQueryBatch"/>
	/// </summary>
	public partial interface IQueryBatchItem
	{
		/// <summary>
		/// Method is called right before batch execution.
		/// Can be used for various delayed initialization logic.
		/// </summary>
		/// <param name="session"></param>
		void Init(ISessionImplementor session);
		
		/// <summary>
		/// Returns commands generated by query
		/// </summary>
		IEnumerable<ISqlCommand> GetCommands();

		/// <summary>
		/// Returns delegates for processing commands generated by <see cref="GetCommands"/>.
		/// Delegate should return number of rows loaded by command.
		/// </summary>
		/// <returns></returns>
		IEnumerable<Func<DbDataReader, int>> GetProcessResultSetActions();

		/// <summary>
		/// Executed after all commands are processed
		/// </summary>
		void PostProcess();

		/// <summary>
		/// Immediate query execution in case dialect is non batchable
		/// </summary>
		void ExecuteNonBatchable();

		/// <summary>
		/// Get cache query spaces
		/// </summary>
		IEnumerable<string> GetQuerySpaces();
	}
}
