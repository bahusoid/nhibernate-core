namespace NHibernate.Driver
{
	/// <summary>
	/// A NHibernate Driver for using the Oracle.ManagedDataAccess DataProvider
	/// </summary>
	public class OracleManagedDataClientDriver : OracleDataClientDriverBase
	{
		/// <summary>
		/// Initializes a new instance of <see cref="OracleManagedDataClientDriver"/>.
		/// </summary>
		/// <exception cref="HibernateException">
		/// Thrown when the <c>Oracle.ManagedDataAccess</c> assembly can not be loaded.
		/// </exception>
		public OracleManagedDataClientDriver()
			: base("Oracle.ManagedDataAccess")
		{
		}

		public override bool HasDelayedDistributedTransactionCompletion => true;

		/// <summary>
		/// Clears the connection pool.
		/// </summary>
		/// <param name="connectionString">The connection string of connections for which to clear the pool.
		/// <c>null</c> for clearing them all.</param>
		public void ClearPool(string connectionString)
		{
			// Do not move in a base class common to different connection types, or it may not clear
			// expected pool.
			PoolHelper<OracleManagedDataClientDriver>.ClearPool(this, connectionString);
		}
	}
}
