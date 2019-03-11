using System.Collections.Generic;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.SqlCommand;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3866
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		private static readonly EntityB bAlias = null;
		private static readonly EntityC cAlias = null;

		protected override void Configure(Configuration configuration)
		{
			if (!(Dialect is MsSql2000Dialect))
				Assert.Ignore("Test is for MS SQL Server dialect only");

			cfg.SetProperty(Environment.Dialect, typeof(MsSql2005Dialect).AssemblyQualifiedName);
		}

		[Test]
		public void QueryOverWithTwoInnerJoinsAndSkipDoesNotThrowException()
		{
			using (var session = OpenSession())
			{
				var result = session.QueryOver<EntityA>()
									.JoinAlias(e => e.EntityBs, () => bAlias, JoinType.InnerJoin)
									.JoinAlias(e => e.EntityCs, () => cAlias, JoinType.InnerJoin)
									.Skip(1)
									.List();

				Assert.That(result.Count, Is.EqualTo(1));
			}
		}

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var entity1 = CreateEntity();
				var entity2 = CreateEntity();
				session.Save(entity1);
				session.Save(entity2);

				transaction.Commit();
			}
		}

		private static EntityA CreateEntity()
		{
			return new EntityA
			{
				EntityBs = new List<EntityB>
				{
					new EntityB()
				},
				EntityCs = new List<EntityC>
				{
					new EntityC()
				}
			};
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Delete("from EntityA");
				transaction.Commit();
			}
		}
	}
}
