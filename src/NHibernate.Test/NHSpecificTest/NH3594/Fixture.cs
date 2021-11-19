using NHibernate.Cfg;
using NHibernate.Dialect;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3594
{
	/// <summary>
	/// GH-1169
	/// </summary>
	[TestFixture]
	public class Fixture : BugTestCase
	{
		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return dialect is MsSql2005Dialect;
		}

		protected override void Configure(Configuration configuration)
		{
			configuration.SetProperty(Environment.Dialect, typeof(MsSql2012Dialect).FullName);
			base.Configure(configuration);
		}

		protected override void OnSetUp()
		{
			using (var s = OpenSession())
			using (var tx = s.BeginTransaction())
			{
				for (var i = 0; i < 5; i++)
				{
					s.Save(new Person { FirstName = "Name" + i });
				}

				tx.Commit();
			}
		}

		[Test]
		public void ShouldWorkUsingDistinctAndLimits1()
		{
			using (var s = OpenSession())
			{
				var q = s.CreateQuery("select distinct p from Person p").SetFirstResult(0).SetMaxResults(10);
				Assert.That(q.List(), Has.Count.EqualTo(5));
			}
		}

		[Test]
		public void ShouldWorkUsingDistinctAndLimits2()
		{
			using (var s = OpenSession())
			{
				var q = s.CreateQuery("select distinct p from Person p").SetFirstResult(1).SetMaxResults(10);
				Assert.That(q.List(), Has.Count.EqualTo(4));
			}
		}

		[Test]
		public void ShouldWorkUsingDistinctAndLimitsAndOrderBy1()
		{
			using (var s = OpenSession())
			{
				var q = s.CreateQuery("select distinct p from Person p order by p.id").SetFirstResult(0).SetMaxResults(10);
				Assert.That(q.List(), Has.Count.EqualTo(5));
			}
		}

		[Test]
		public void ShouldWorkUsingDistinctAndLimitsAndOrderBy2()
		{
			using (var s = OpenSession())
			{
				var q = s.CreateQuery("select distinct p from Person p order by p.id").SetFirstResult(1).SetMaxResults(10);
				Assert.That(q.List(), Has.Count.EqualTo(4));
			}
		}

		protected override void OnTearDown()
		{
			using (var s = OpenSession())
			using (var tx = s.BeginTransaction())
			{
				s.Delete("from Person");
				tx.Commit();
			}
		}
	}
}
