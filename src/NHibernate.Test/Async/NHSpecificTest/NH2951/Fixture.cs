﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Linq;
using NHibernate.Linq;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2951
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return dialect.SupportsScalarSubSelects;
		}

		protected override void OnTearDown()
		{
			using (ISession session = OpenSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				session.Delete("from System.Object");

				session.Flush();
				transaction.Commit();
			}
		}

		[Test]
		public async Task UpdateWithSubqueryToJoinedSubclassAsync()
		{
            using (ISession session = OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                var c = new Customer { Name = "Bob" };
                await (session.SaveAsync(c));

                var i = new Invoice { Amount = 10 };
                await (session.SaveAsync(i));

                await (session.FlushAsync());
                await (transaction.CommitAsync());
            }

            using (ISession session = OpenSession())
			using (session.BeginTransaction())
			{
                // Using (select c.Id ...) works.
                string hql = "update Invoice i set i.Customer = (select c from Customer c where c.Name = 'Bob')";

			    int result = await (session.CreateQuery(hql).ExecuteUpdateAsync());

                Assert.AreEqual(1, result);
			}
		}
	}
}
