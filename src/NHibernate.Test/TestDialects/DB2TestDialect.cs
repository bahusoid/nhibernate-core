namespace NHibernate.Test.TestDialects
{
	public class DB2TestDialect: TestDialect
	{
		public DB2TestDialect(Dialect.Dialect dialect) : base(dialect)
		{
		}

		public override bool SupportsComplexExpressionInGroupBy => false;
		public override bool SupportsNonDataBoundCondition => false;
		public override bool SupportsSubSelectsInOrderBy => false;
	}
}
