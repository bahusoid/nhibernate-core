using System.Linq;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2116
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var parent1 = new ClientTradingLimit() {ClientID = 1};
				parent1.TradingLimitDetails.Add(new ClientTradingLimitDetail()
				{
					ClientTradingLimit = parent1
				});
				parent1.TradingLimitDetails.Add(new ClientTradingLimitDetail()
				{
					ClientTradingLimit = parent1
				});	
				var parent2 = new ClientTradingLimit() {ClientID = 2};

				session.Save(parent1);
				session.Save(parent2);

				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				// The HQL delete does all the job inside the database without loading the entities, but it does
				// not handle delete order for avoiding violating constraints if any. Use
				// session.Delete("from System.Object");
				// instead if in need of having NHibernate ordering the deletes, but this will cause
				// loading the entities in the session.
				session.CreateQuery("delete from System.Object").ExecuteUpdate();

				transaction.Commit();
			}
		}

		[Test]
		public void YourTestName()
		{
			NHibernate.Criterion.ICriterion[] clientIDCriterions =
			{
				NHibernate.Criterion.Expression.Or(
					NHibernate.Criterion.Expression.Eq("ClientID", 1),
					NHibernate.Criterion.Expression.Eq("ClientID", 2))
			};

			using(new SqlLogSpy())
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{


				ICriteria criteria = session.CreateCriteria(typeof(ClientTradingLimit));

				foreach (var criterion in clientIDCriterions)
				{
					criteria.Add(criterion);
				}

				var clientTradingLimits = criteria.List<ClientTradingLimit>();
				var l1 = clientTradingLimits.First(x => x.ClientID == 1);
				var l2 = clientTradingLimits.First(x => x.ClientID == 2);
				Assert.That(NHibernateUtil.IsInitialized(l1.TradingLimitDetails), Is.True);
				Assert.That(l1.TradingLimitDetails, Has.Count.EqualTo(1));
				Assert.That(NHibernateUtil.IsInitialized(l2.TradingLimitDetails), Is.True);
				Assert.That(l2.TradingLimitDetails, Has.Count.EqualTo(0));
			}
		}
	}
}
