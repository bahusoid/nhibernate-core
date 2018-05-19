﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections;
using NHibernate.Cfg;
using NUnit.Framework;

namespace NHibernate.Test.Stateless
{
	using System.Threading.Tasks;
	using System.Threading;
	[TestFixture]
	public class StatelessSessionQueryFixtureAsync : TestCase
	{
		protected override string MappingsAssembly
		{
			get { return "NHibernate.Test"; }
		}

		protected override IList Mappings
		{
			get { return new[] {"Stateless.Contact.hbm.xml"}; }
		}

		protected override void Configure(Configuration configuration)
		{
			base.Configure(configuration);
			cfg.SetProperty(Environment.MaxFetchDepth, 1.ToString());
		}

		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return TestDialect.SupportsEmptyInsertsOrHasNonIdentityNativeGenerator;
		}

		private class TestData
		{
			internal readonly IList list = new ArrayList();

			private readonly ISessionFactory sessions;

			public TestData(ISessionFactory sessions)
			{
				this.sessions = sessions;
			}

			public virtual async Task createDataAsync(CancellationToken cancellationToken = default(CancellationToken))
			{
				using (ISession session = sessions.OpenSession())
				{
					using (ITransaction tx = session.BeginTransaction())
					{
						var usa = new Country();
						await (session.SaveAsync(usa, cancellationToken));
						list.Add(usa);
						var disney = new Org();
						disney.Country = usa;
						await (session.SaveAsync(disney, cancellationToken));
						list.Add(disney);
						var waltDisney = new Contact();
						waltDisney.Org = disney;
						await (session.SaveAsync(waltDisney, cancellationToken));
						list.Add(waltDisney);
						await (tx.CommitAsync(cancellationToken));
					}
				}
			}

			public virtual async Task cleanDataAsync(CancellationToken cancellationToken = default(CancellationToken))
			{
				using (ISession session = sessions.OpenSession())
				{
					using (ITransaction tx = session.BeginTransaction())
					{
						foreach (object obj in list)
						{
							await (session.DeleteAsync(obj, cancellationToken));
						}

						await (tx.CommitAsync(cancellationToken));
					}
				}
			}
		}

		[Test]
		public async Task CriteriaAsync()
		{
			var testData = new TestData(Sfi);
			await (testData.createDataAsync());

			using (IStatelessSession s = Sfi.OpenStatelessSession())
			{
				Assert.AreEqual(1, (await (s.CreateCriteria<Contact>().ListAsync())).Count);
			}

			await (testData.cleanDataAsync());
		}

		[Test]
		public async Task CriteriaWithSelectFetchModeAsync()
		{
			var testData = new TestData(Sfi);
			await (testData.createDataAsync());

			using (IStatelessSession s = Sfi.OpenStatelessSession())
			{
				Assert.AreEqual(1, (await (s.CreateCriteria<Contact>().Fetch(SelectMode.SkipJoin, "Org").ListAsync())).Count);
			}

			await (testData.cleanDataAsync());
		}

		[Test]
		public async Task HqlAsync()
		{
			var testData = new TestData(Sfi);
			await (testData.createDataAsync());

			using (IStatelessSession s = Sfi.OpenStatelessSession())
			{
				Assert.AreEqual(1, (await (s.CreateQuery("from Contact c join fetch c.Org join fetch c.Org.Country").ListAsync<Contact>())).Count);
			}

			await (testData.cleanDataAsync());
		}
	}
}
