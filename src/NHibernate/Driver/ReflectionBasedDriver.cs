using System;
using System.Data.Common;
using System.Reflection;
using NHibernate.Util;

namespace NHibernate.Driver
{
	public abstract class ReflectionBasedDriver : DriverBase
	{
		protected const string ReflectionTypedProviderExceptionMessageTemplate = "The DbCommand and DbConnection implementation in the assembly {0} could not be found. "
		                                                                       + "Ensure that the assembly {0} is located in the application directory or in the Global "
		                                                                       + "Assembly Cache. If the assembly is in the GAC, use <qualifyAssembly/> element in the "
		                                                                       + "application configuration file to specify the full name of the assembly.";

		private readonly IDriveConnectionCommandProvider connectionCommandProvider;

		/// <summary>
		/// If the driver use a third party driver (not a .Net Framework DbProvider), its assembly version.
		/// </summary>
		protected Version DriverVersion { get; } 

		/// <summary>
		/// Initializes a new instance of <see cref="ReflectionBasedDriver" /> with
		/// type names that are loaded from the specified assembly.
		/// </summary>
		/// <param name="driverAssemblyName">Assembly to load the types from.</param>
		/// <param name="connectionTypeName">Connection type name.</param>
		/// <param name="commandTypeName">Command type name.</param>
		protected ReflectionBasedDriver(string driverAssemblyName, string connectionTypeName, string commandTypeName)
			: this(null, driverAssemblyName, connectionTypeName, commandTypeName)
		{
		}

		/// <summary>
		/// Initializes a new instance of <see cref="ReflectionBasedDriver" /> with
		/// type names that are loaded from the specified assembly.
		/// </summary>
		/// <param name="providerInvariantName">The Invariant name of a provider.</param>
		/// <param name="driverAssemblyName">Assembly to load the types from.</param>
		/// <param name="connectionTypeName">Connection type name.</param>
		/// <param name="commandTypeName">Command type name.</param>
		/// <seealso cref="DbProviderFactories.GetFactory(string)"/>
		protected ReflectionBasedDriver(string providerInvariantName, string driverAssemblyName, string connectionTypeName, string commandTypeName)
		{
			// Try to get the types from an already loaded assembly
			var connectionType = ReflectHelper.TypeFromAssembly(connectionTypeName, driverAssemblyName, false);
			var commandType = ReflectHelper.TypeFromAssembly(commandTypeName, driverAssemblyName, false);

			if (connectionType == null || commandType == null)
			{
				if (string.IsNullOrEmpty(providerInvariantName))
				{
					throw new HibernateException(string.Format(ReflectionTypedProviderExceptionMessageTemplate, driverAssemblyName));
				}
				var factory = DbProviderFactories.GetFactory(providerInvariantName);
				connectionCommandProvider = new DbProviderFactoryDriveConnectionCommandProvider(factory);
			}
			else
			{
				connectionCommandProvider = new ReflectionDriveConnectionCommandProvider(connectionType, commandType);
				DriverVersion = connectionType.Assembly.GetName().Version;
			}
		}

		public override DbConnection CreateConnection()
		{
			return connectionCommandProvider.CreateConnection();
		}

		public override DbCommand CreateCommand()
		{
			return connectionCommandProvider.CreateCommand();
		}

		/// <summary>
		/// Helper for clearing connection pools used by a reflection driver. Assumes the connection has a parameter-less
		/// <c>ClearAllPools</c> method and a <c>ClearPool</c> method taking as argument a connection.
		/// </summary>
		/// <typeparam name="T">The driver type for which the pool has to be cleared. This driver type must
		/// always use the same connection type.</typeparam>
		/// <remarks>Having <c>ClearAllPools</c> and <c>ClearPool</c> is a common pattern. <c>SqlConnection</c>,
		/// <c>OracleConnection</c> (managed, un-managed and from <c>System.Data</c>), <c>FirebirdConnection</c>
		/// <c>NpgsqlConnection</c>, <c>MySqlConnection</c> and <c>SQLiteConnection</c> have them.
		/// (<c>SqlCeConnection</c>, <c>OdbcConnection</c> and <c>OleDbConnection</c> lack them.)</remarks>
		protected static class PoolHelper<T> where T : IDriver
		{
			// Static field in generic class => one field per concrete type used. This is exactly what
			// we need here, do not move the generic argument to the method. Otherwise it will cache the
			// method info of the first driver type used, and reuse it for other driver types, which
			// would fail.
			private static volatile MethodInfo _clearPool;
			private static volatile MethodInfo _clearAllPools;

			/// <summary>
			/// Clears the connection pool.
			/// </summary>
			/// <param name="driver">The driver for which the connection pool has to be cleared.</param>
			/// <param name="connectionString">The connection string of connections for which to clear the pool.
			/// <c>null</c> for clearing them all.</param>
			internal static void ClearPool(T driver, string connectionString)
			{
				// In case of concurrent threads, may initialize many times. We do not care.
				// Members are volatile for avoiding they get used while their constructor is not yet ended.
				if (_clearPool == null || _clearAllPools == null)
				{
					using (var clearConnection = driver.CreateConnection())
					{
						var connectionType = clearConnection.GetType();
						_clearPool = connectionType.GetMethod("ClearPool") ?? throw new InvalidOperationException("Unable to resolve ClearPool method.");
						_clearAllPools = connectionType.GetMethod("ClearAllPools") ?? throw new InvalidOperationException("Unable to resolve ClearAllPools method.");
					}
				}

				if (connectionString != null)
				{
					using (var clearConnection = driver.CreateConnection())
					{
						clearConnection.ConnectionString = connectionString;
						_clearPool.Invoke(null, new object[] {clearConnection});
					}
					return;
				}

				_clearAllPools.Invoke(null, Array.Empty<object>());
			}
		}
	}
}
