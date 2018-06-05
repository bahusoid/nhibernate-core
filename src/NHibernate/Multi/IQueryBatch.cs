namespace NHibernate
{
	/// <summary>
	/// Universal query batcher
	/// </summary>
	public partial interface IQueryBatch
	{
		/// <summary>
		/// Executes batch
		/// </summary>
		void Execute();
		
		/// <summary>
		/// Adds query to batch.
		/// </summary>
		/// <param name="query">Query</param>
		void Add(IQueryBatchItem query);

		/// <summary>
		/// The timeout in seconds for the underlying ADO.NET query.
		/// </summary>
		int? Timeout { get; set; }
	}
}
