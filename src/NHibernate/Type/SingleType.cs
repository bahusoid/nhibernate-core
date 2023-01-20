using System;
using System.Data;
using System.Data.Common;
using System.Numerics;
using NHibernate.Engine;
using NHibernate.SqlTypes;

namespace NHibernate.Type
{
	/// <summary>
	/// Maps a <see cref="System.Single" /> Property to an 
	/// <see cref="DbType.Single" /> column.
	/// </summary>
	/// <remarks>
	/// Verify through your database's documentation if there is a column type that
	/// matches up with the capabilities of <see cref="System.Single" />  
	/// </remarks>
	[Serializable]
	public class SingleType : PrimitiveType
	{
		/// <summary></summary>
		public SingleType() : base(SqlTypeFactory.Single)
		{
		}

		public SingleType(SqlType sqlType) : base(sqlType) {}

		/// <summary></summary>
		public override string Name
		{
			get { return "Single"; }
		}

		private static readonly Single ZERO = 0;
		public override object Get(DbDataReader rs, int index, ISessionImplementor session)
		{
			return GetNumeric(rs, index, o => Convert.ToSingle(o), i => (float) i);
		}

		public override object Get(DbDataReader rs, string name, ISessionImplementor session)
		{
			return Get(rs, rs.GetOrdinal(name), session);
		}

		public override System.Type ReturnedClass
		{
			get { return typeof(Single); }
		}

		public override void Set(DbCommand rs, object value, int index, ISessionImplementor session)
		{
			rs.Parameters[index].Value = Convert.ToSingle(value);
		}

		// Since 5.2
		[Obsolete("This method has no more usages and will be removed in a future version.")]
		public object StringToObject(string xml)
		{
			return FromStringValue(xml);
		}

		// Since 5.2
		[Obsolete("This method has no more usages and will be removed in a future version.")]
		public override object FromStringValue(string xml)
		{
			return Single.Parse(xml);
		}

		public override System.Type PrimitiveClass
		{
			get { return typeof(Single); }
		}

		public override object DefaultValue
		{
			get { return ZERO; }
		}

		public override string ObjectToSQLString(object value, Dialect.Dialect dialect)
		{
			return value.ToString();
		}
	}
}
