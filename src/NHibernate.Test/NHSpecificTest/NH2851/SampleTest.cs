using NHibernate.Dialect;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2851
{
	[TestFixture]
	public class SampleTest : BugTestCase
	{
		protected override void OnSetUp()
		{
			base.OnSetUp();
			using (ISession session = OpenSession())
			{
				for (int x = 0; x < 10; x++)
				{
					var user = new User
					{
						UserName = "TestUser" + x,
					};
					session.Save(user);

					var practitioner = new Practitioner
					{
						User = user
					};
					session.Save(practitioner);
				}

				session.Flush();
			}
		}

		protected override void OnTearDown()
		{
			base.OnTearDown();
			using (ISession session = OpenSession())
			{
				const string hql = "from System.Object";
				session.Delete(hql);
				session.Flush();
			}
		}

		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return dialect as MsSql2005Dialect != null;
		}

		[Test]
		public void QueryShouldNotResultInNPlusOne()
		{
			using(var log = new SqlLogSpy())
			using (var session = OpenSession())
			{
				const string hql = "from Practitioner p inner join fetch p.User";
				var query = session.CreateQuery(hql);
				var practitioners = query.List<Practitioner>();
				var practitioner = practitioners[0];
				var id = practitioner.User.UserID;
				var name = practitioner.User.UserName;
				foreach(var p in practitioners)
				{
					Assert.That(p, Is.Not.Null);
					Assert.That(NHibernateUtil.IsInitialized(p), Is.True);
					Assert.That(p.User, Is.Not.Null);
					Assert.That(NHibernateUtil.IsInitialized(p.User), Is.True);
				}
				Assert.That(log.Appender.GetEvents().Length, Is.EqualTo(1));
			}
		}
	}
}
