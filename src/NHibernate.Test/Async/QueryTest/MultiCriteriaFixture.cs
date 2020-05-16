﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections;
using System.Collections.Generic;
using NHibernate.Cfg;
using NHibernate.Criterion;
using NHibernate.Test.SecondLevelCacheTests;
using NUnit.Framework;
using Environment = NHibernate.Cfg.Environment;

namespace NHibernate.Test.QueryTest
{
	using System.Threading.Tasks;
	using System.Threading;
	[TestFixture, Obsolete]
	public class MultiCriteriaFixtureAsync : TestCase
	{
		protected override string MappingsAssembly
		{
			get { return "NHibernate.Test"; }
		}

		protected override string[] Mappings
		{
			get { return new[] { "SecondLevelCacheTest.Item.hbm.xml" }; }
		}

		protected override bool AppliesTo(Engine.ISessionFactoryImplementor factory)
		{
			return factory.ConnectionProvider.Driver.SupportsMultipleQueries;
		}

		protected override void Configure(Configuration configuration)
		{
			base.Configure(configuration);

			configuration.SetProperty(Environment.GenerateStatistics, "true");
		}

		protected override void OnSetUp()
		{
			base.OnSetUp();

			this.Sfi.Statistics.Clear();
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Delete("from System.Object");
				session.Flush();
				transaction.Commit();
			}
		}

		[Test]
		public async Task CanExecuteMultiplyQueriesInSingleRoundTrip_InTransactionAsync()
		{
			using (var s = OpenSession())
			{
				var item = new Item();
				item.Id = 1;
				item.Name = "foo";
				await (s.SaveAsync(item));
				await (s.FlushAsync());
			}

			using (var s = OpenSession())
			{
				var transaction = s.BeginTransaction();
				var getItems = s.CreateCriteria(typeof(Item));
				var countItems = s.CreateCriteria(typeof(Item))
					.SetProjection(Projections.RowCount());

				var multiCriteria = s.CreateMultiCriteria()
					.Add(getItems)
					.Add(countItems);
				var results = await (multiCriteria.ListAsync());
				var items = (IList)results[0];
				var fromDb = (Item)items[0];
				Assert.AreEqual(1, fromDb.Id);
				Assert.AreEqual("foo", fromDb.Name);

				var counts = (IList)results[1];
				var count = (int)counts[0];
				Assert.AreEqual(1, count);

				await (transaction.CommitAsync());
			}
		}

		[Test]
		public async Task CanExecuteMultiplyQueriesInSingleRoundTripAsync()
		{
			using (var s = OpenSession())
			{
				var item = new Item();
				item.Id = 1;
				await (s.SaveAsync(item));
				await (s.FlushAsync());
			}

			using (var s = OpenSession())
			{
				var getItems = s.CreateCriteria(typeof(Item));
				var countItems = s.CreateCriteria(typeof(Item))
					.SetProjection(Projections.RowCount());

				var multiCriteria = s.CreateMultiCriteria()
					.Add(getItems)
					.Add(countItems);
				var results = await (multiCriteria.ListAsync());
				var items = (IList)results[0];
				var fromDb = (Item)items[0];
				Assert.AreEqual(1, fromDb.Id);

				var counts = (IList)results[1];
				var count = (int)counts[0];
				Assert.AreEqual(1, count);
			}
		}

		[Test]
		public async Task CanUseSecondLevelCacheWithPositionalParametersAsync()
		{
			var cacheHashtable = MultipleQueriesFixtureAsync.GetHashTableUsedAsQueryCache(Sfi);
			cacheHashtable.Clear();

			await (CreateItemsAsync());

			await (DoMutiQueryAndAssertAsync());

			Assert.AreEqual(1, cacheHashtable.Count);
		}

		[Test]
		public async Task CanGetMultiQueryFromSecondLevelCacheAsync()
		{
			await (CreateItemsAsync());
			//set the query in the cache
			await (DoMutiQueryAndAssertAsync());

			var cacheHashtable = MultipleQueriesFixtureAsync.GetHashTableUsedAsQueryCache(Sfi);
			var cachedListEntry = (IList)new ArrayList(cacheHashtable.Values)[0];
			var cachedQuery = (IList)cachedListEntry[1];

			var firstQueryResults = (IList)cachedQuery[0];
			firstQueryResults.Clear();
			firstQueryResults.Add(3);
			firstQueryResults.Add(4);

			var secondQueryResults = (IList)cachedQuery[1];
			secondQueryResults[0] = 2;

			using (var s = Sfi.OpenSession())
			{
				var criteria = s.CreateCriteria(typeof(Item))
					.Add(Restrictions.Gt("id", 50));
				var multiCriteria = s.CreateMultiCriteria()
					.Add(CriteriaTransformer.Clone(criteria).SetFirstResult(10))
					.Add(CriteriaTransformer.Clone(criteria).SetProjection(Projections.RowCount()));
				multiCriteria.SetCacheable(true);
				var results = await (multiCriteria.ListAsync());
				var items = (IList)results[0];
				Assert.AreEqual(2, items.Count);
				var count = (int)((IList)results[1])[0];
				Assert.AreEqual(2L, count);
			}
		}

		[Test]
		public async Task CanUpdateStatisticsWhenGetMultiQueryFromSecondLevelCacheAsync()
		{
			await (CreateItemsAsync());

			await (DoMutiQueryAndAssertAsync());
			Assert.AreEqual(0, Sfi.Statistics.QueryCacheHitCount);
			Assert.AreEqual(1, Sfi.Statistics.QueryCacheMissCount);
			Assert.AreEqual(1, Sfi.Statistics.QueryCachePutCount);

			await (DoMutiQueryAndAssertAsync());
			Assert.AreEqual(1, Sfi.Statistics.QueryCacheHitCount);
			Assert.AreEqual(1, Sfi.Statistics.QueryCacheMissCount);
			Assert.AreEqual(1, Sfi.Statistics.QueryCachePutCount);
		}

		[Test]
		public async Task TwoMultiQueriesWithDifferentPagingGetDifferentResultsWhenUsingCachedQueriesAsync()
		{
			await (CreateItemsAsync());
			using (var s = OpenSession())
			{
				var criteria = s.CreateCriteria(typeof(Item))
					.Add(Restrictions.Gt("id", 50));
				var multiCriteria = s.CreateMultiCriteria()
					.Add(CriteriaTransformer.Clone(criteria).SetFirstResult(10))
					.Add(CriteriaTransformer.Clone(criteria).SetProjection(Projections.RowCount()));
				multiCriteria.SetCacheable(true);
				var results = await (multiCriteria.ListAsync());
				var items = (IList)results[0];
				Assert.AreEqual(89, items.Count);
				var count = (int)((IList)results[1])[0];
				Assert.AreEqual(99L, count);
			}

			using (var s = OpenSession())
			{
				var criteria = s.CreateCriteria(typeof(Item))
					.Add(Restrictions.Gt("id", 50));
				var multiCriteria = s.CreateMultiCriteria()
					.Add(CriteriaTransformer.Clone(criteria).SetFirstResult(20))
					.Add(CriteriaTransformer.Clone(criteria).SetProjection(Projections.RowCount()));
				multiCriteria.SetCacheable(true);
				var results = await (multiCriteria.ListAsync());
				var items = (IList)results[0];
				Assert.AreEqual(79, items.Count, "Should have gotten different result here, because the paging is different");
				var count = (int)((IList)results[1])[0];
				Assert.AreEqual(99L, count);
			}
		}

		[Test]
		public async Task CanUseWithParameterizedQueriesAndLimitAsync()
		{
			await (CreateItemsAsync());

			using (var s = OpenSession())
			{
				var criteria = s.CreateCriteria(typeof(Item))
					.Add(Restrictions.Gt("id", 50));

				var results = await (s.CreateMultiCriteria()
					.Add(CriteriaTransformer.Clone(criteria)
						.SetFirstResult(10))
					.Add(CriteriaTransformer.Clone(criteria)
						.SetProjection(Projections.RowCount()))
					.ListAsync());
				var items = (IList)results[0];
				Assert.AreEqual(89, items.Count);
				var count = (int)((IList)results[1])[0];
				Assert.AreEqual(99L, count);
			}
		}

		[Test]
		public async Task CanUseSetParameterListAsync()
		{
			using (var s = OpenSession())
			{
				var item = new Item();
				item.Id = 1;
				await (s.SaveAsync(item));
				await (s.FlushAsync());
			}

			using (var s = OpenSession())
			{
				var criteria = s.CreateCriteria(typeof(Item))
					.Add(Restrictions.In("id", new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }));
				var results = await (s.CreateMultiCriteria()
					.Add(CriteriaTransformer.Clone(criteria))
					.Add(CriteriaTransformer.Clone(criteria)
						.SetProjection(Projections.RowCount()))
					.ListAsync());

				var items = (IList)results[0];
				var fromDb = (Item)items[0];
				Assert.AreEqual(1, fromDb.Id);

				var counts = (IList)results[1];
				var count = (int)counts[0];
				Assert.AreEqual(1L, count);
			}
		}

		[Test]
		public async Task CanAddCriteriaWithKeyAndRetrieveResultsWithKeyAsync()
		{
			await (CreateItemsAsync());

			using (var session = OpenSession())
			{
				var multiCriteria = session.CreateMultiCriteria();

				var firstCriteria = session.CreateCriteria(typeof(Item))
					.Add(Restrictions.Lt("id", 50));

				var secondCriteria = session.CreateCriteria(typeof(Item));

				multiCriteria.Add("firstCriteria", firstCriteria);
				multiCriteria.Add("secondCriteria", secondCriteria);

				var secondResult = (IList)await (multiCriteria.GetResultAsync("secondCriteria"));
				var firstResult = (IList)await (multiCriteria.GetResultAsync("firstCriteria"));

				Assert.Greater(secondResult.Count, firstResult.Count);
			}
		}

		[Test]
		public async Task CanAddDetachedCriteriaWithKeyAndRetrieveResultsWithKeyAsync()
		{
			await (CreateItemsAsync());

			using (var session = OpenSession())
			{
				var multiCriteria = session.CreateMultiCriteria();

				var firstCriteria = DetachedCriteria.For(typeof(Item))
					.Add(Restrictions.Lt("id", 50));

				var secondCriteria = DetachedCriteria.For(typeof(Item));

				multiCriteria.Add("firstCriteria", firstCriteria);
				multiCriteria.Add("secondCriteria", secondCriteria);

				var secondResult = (IList)await (multiCriteria.GetResultAsync("secondCriteria"));
				var firstResult = (IList)await (multiCriteria.GetResultAsync("firstCriteria"));

				Assert.Greater(secondResult.Count, firstResult.Count);
			}
		}

		[Test]
		public async Task CanNotRetrieveCriteriaResultWithUnknownKeyAsync()
		{
			await (CreateItemsAsync());

			using (var session = OpenSession())
			{
				var multiCriteria = session.CreateMultiCriteria();

				var firstCriteria = session.CreateCriteria(typeof(Item))
					.Add(Restrictions.Lt("id", 50));

				multiCriteria.Add("firstCriteria", firstCriteria);

				try
				{
					await (multiCriteria.GetResultAsync("unknownKey"));
					Assert.Fail("This should've thrown an InvalidOperationException");
				}
				catch (InvalidOperationException)
				{
				}
				catch (Exception)
				{
					Assert.Fail("This should've thrown an InvalidOperationException");
				}
			}
		}

		[Test]
		public async Task CanNotRetrieveDetachedCriteriaResultWithUnknownKeyAsync()
		{
			await (CreateItemsAsync());

			using (var session = OpenSession())
			{
				var multiCriteria = session.CreateMultiCriteria();

				var firstCriteria = DetachedCriteria.For(typeof(Item))
					.Add(Restrictions.Lt("id", 50));

				multiCriteria.Add("firstCriteria", firstCriteria);

				try
				{
					await (multiCriteria.GetResultAsync("unknownKey"));
					Assert.Fail("This should've thrown an InvalidOperationException");
				}
				catch (InvalidOperationException)
				{
				}
				catch (Exception)
				{
					Assert.Fail("This should've thrown an InvalidOperationException");
				}
			}
		}

		private async Task DoMutiQueryAndAssertAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			using (var s = OpenSession())
			{
				var criteria = s.CreateCriteria(typeof(Item))
					.Add(Restrictions.Gt("id", 50));
				var multiCriteria = s.CreateMultiCriteria()
					.Add(CriteriaTransformer.Clone(criteria).SetFirstResult(10))
					.Add(CriteriaTransformer.Clone(criteria).SetProjection(Projections.RowCount()));
				multiCriteria.SetCacheable(true);
				var results = await (multiCriteria.ListAsync(cancellationToken));
				var items = (IList)results[0];
				Assert.AreEqual(89, items.Count);
				var count = (int)((IList)results[1])[0];
				Assert.AreEqual(99L, count);
			}
		}

		private async Task CreateItemsAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				for (var i = 0; i < 150; i++)
				{
					var item = new Item();
					item.Id = i;
					await (s.SaveAsync(item, cancellationToken));
				}
				await (t.CommitAsync(cancellationToken));
			}
		}

		[Test]
		public async Task CanGetResultInAGenericListAsync()
		{
			using (var s = OpenSession())
			{
				var getItems = s.CreateCriteria(typeof(Item));
				var countItems = s.CreateCriteria(typeof(Item))
					.SetProjection(Projections.RowCount());

				var multiCriteria = s.CreateMultiCriteria()
					.Add(getItems) // we expect a non-generic result from this (ArrayList)
					.Add<int>(countItems); // we expect a generic result from this (List<int>)
				var results = await (multiCriteria.ListAsync());

				Assert.That(results[0], Is.InstanceOf<List<object>>());
				Assert.That(results[1], Is.InstanceOf<List<int>>());
			}
		}

		[Test]
		public async Task UsingManyParametersAndQueries_DoesNotCauseParameterNameCollisionsAsync()
		{
			//GH-1357
			using (var s = OpenSession())
			{
				var item = new Item {Id = 15};
				await (s.SaveAsync(item));
				await (s.FlushAsync());
			}

			using (var s = OpenSession())
			{
				var multi = s.CreateMultiCriteria();

				for (var i = 0; i < 12; i++)
				{
					var criteria = s.CreateCriteria(typeof(Item));
					for (var j = 0; j < 12; j++)
					{
						criteria = criteria.Add(Restrictions.Gt("id", j));
					}
					multi.Add(criteria);
				}
				//Parameter combining is only used for cacheable queries
				multi.SetCacheable(true);
				foreach (IList result in await (multi.ListAsync()))
				{
					Assert.That(result.Count, Is.EqualTo(1));
				}
			}
		}

		//NH-2428 - Session.MultiCriteria and FlushMode.Auto inside transaction (GH865)
		[Test]
		public async Task MultiCriteriaAutoFlushAsync()
		{
			using (var s = OpenSession())
			using (var tx = s.BeginTransaction())
			{
				s.FlushMode = FlushMode.Auto;
				var p1 = new Item
				{
					Name = "Person name",
					Id = 15
				};
				await (s.SaveAsync(p1));
				await (s.FlushAsync());

				await (s.DeleteAsync(p1));
				var multi = s.CreateMultiCriteria();
				multi.Add<int>(s.QueryOver<Item>().ToRowCountQuery());
				var count = (int) ((IList) (await (multi.ListAsync()))[0])[0];
				await (tx.CommitAsync());

				Assert.That(count, Is.EqualTo(0), "Session wasn't auto flushed.");
			}
		}
	}
}
