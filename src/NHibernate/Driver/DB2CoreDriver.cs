using System;
using System.Data.Common;
using System.Reflection;
using NHibernate.SqlTypes;

namespace NHibernate.Driver
{
	/// <summary>
	/// A NHibernate Driver for using the IBM.Data.DB2.Core or NET5.IBM.Data.DB2 DataProvider.
	/// </summary>
	public class DB2CoreDriver : DB2DriverBase
	{
		private static readonly string _assemblyName = "IBM.Data.DB2.Core";

		static DB2CoreDriver()
		{
			try
			{
				var net5AssemblyName = "IBM.Data.Db2";
				Assembly.Load(net5AssemblyName);
				_assemblyName = net5AssemblyName;
			}
			catch
			{
			}
		}

		public DB2CoreDriver() : base(_assemblyName)
		{
		}

		public override bool UseNamedPrefixInSql => true;

		public override bool UseNamedPrefixInParameter => true;

		public override string NamedPrefix => "@";

		protected override void InitializeParameter(DbParameter dbParam, string name, SqlType sqlType)
		{
			dbParam.ParameterName = FormatNameForParameter(name);
			base.InitializeParameter(dbParam, name, sqlType);
		}
	}
}
