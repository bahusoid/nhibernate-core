﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using NUnit.Framework;
using NHibernate.Cfg;

namespace NHibernate.Test.NHSpecificTest.NH2112
{
    using System.Threading.Tasks;
    [TestFixture]
    public class FixtureAsync : BugTestCase
    {
        protected override void Configure(Configuration configuration)
        {
            configuration.SetProperty(Environment.GenerateStatistics, "true");
            configuration.SetProperty(Environment.BatchSize, "0");
        }

        protected override void OnTearDown()
        {
            using (ISession s = OpenSession())
            using (ITransaction tx = s.BeginTransaction())
            {
                s.CreateSQLQuery("DELETE FROM AMapB").ExecuteUpdate();
                s.CreateSQLQuery("DELETE FROM TableA").ExecuteUpdate();
                s.CreateSQLQuery("DELETE FROM TableB").ExecuteUpdate();
                tx.Commit();
            }
        }

        [Test]
        public async Task TestAsync()
        {
            A a;
            using (ISession s = OpenSession())
            using (ITransaction tx = s.BeginTransaction())
            {
                a = new A();
                a.Name = "A";
                B b1 = new B{ Name = "B1"};
                await (s.SaveAsync(b1));
                B b2 = new B{ Name = "B2"};
                await (s.SaveAsync(b2));
                a.Map.Add(b1 , "B1Text");
                a.Map.Add(b2, "B2Text");
                await (s.SaveAsync(a));
                await (s.FlushAsync());
                await (tx.CommitAsync());
            }
            ClearCounts();
            using (ISession s = OpenSession())
            using (ITransaction tx = s.BeginTransaction())
            {
                A aCopy = (A)await (s.MergeAsync(a));
                await (s.FlushAsync());
                await (tx.CommitAsync());
            }
            AssertUpdateCount(0);
            AssertInsertCount(0);
        }
        protected void ClearCounts()
        {
            Sfi.Statistics.Clear();
        }

        protected void AssertInsertCount(long expected)
        {
            Assert.That(Sfi.Statistics.EntityInsertCount, Is.EqualTo(expected), "unexpected insert count");
        }

        protected void AssertUpdateCount(int expected)
        {
            Assert.That(Sfi.Statistics.EntityUpdateCount, Is.EqualTo(expected), "unexpected update count");
        }

        protected void AssertDeleteCount(int expected)
        {
            Assert.That(Sfi.Statistics.EntityDeleteCount, Is.EqualTo(expected), "unexpected delete count");
        }
	}
}
