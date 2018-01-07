namespace NHibernate.Driver
{
	/// <summary>
	/// A NHibernate Driver for using the Oracle.DataAccess (unmanaged) DataProvider
	/// </summary>
	public class OracleDataClientDriver : OracleDataClientDriverBase
	{
		/// <summary>
		/// Initializes a new instance of <see cref="OracleDataClientDriver"/>.
		/// </summary>
		/// <exception cref="HibernateException">
		/// Thrown when the <c>Oracle.DataAccess</c> assembly can not be loaded.
		/// </exception>
		public OracleDataClientDriver()
			: base("Oracle.DataAccess")
		{
		}

		/// <summary>
		/// Clears the connection pool.
		/// </summary>
		/// <param name="connectionString">The connection string of connections for which to clear the pool.
		/// <c>null</c> for clearing them all.</param>
		public void ClearPool(string connectionString)
		{
			// Do not move in a base class common to different connection types, or it may not clear
			// expected pool.
			PoolHelper<OracleDataClientDriver>.ClearPool(this, connectionString);
		}
	}
}
