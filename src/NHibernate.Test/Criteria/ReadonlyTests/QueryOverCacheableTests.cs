using System.Linq;
using NHibernate.Cfg;
using NHibernate.DomainModel.Northwind.Entities;
using NUnit.Framework;

namespace NHibernate.Test.Criteria.ReadonlyTests
{
	[TestFixture]
	public class QueryOverCacheableTests : CriteriaNorthwindReadonlyTestCase
	{
		//Just for discoverability
		private class CriteriaCacheableTest{}
		
		protected override void Configure(Configuration cfg)
		{
			cfg.SetProperty(Environment.UseQueryCache, "true");
			cfg.SetProperty(Environment.GenerateStatistics, "true");
			base.Configure(cfg);
		}

		[Test]
		public void QueryIsCacheable()
		{
			Sfi.Statistics.Clear();
			Sfi.EvictQueries();

			var x = db.Customers.Cacheable().List();
			var x2 = db.Customers.Cacheable().List();

			Assert.That(Sfi.Statistics.QueryExecutionCount, Is.EqualTo(1), "Unexpected execution count");
			Assert.That(Sfi.Statistics.QueryCachePutCount, Is.EqualTo(1), "Unexpected cache put count");
			Assert.That(Sfi.Statistics.QueryCacheHitCount, Is.EqualTo(1), "Unexpected cache hit count");
		}

		[Test]
		public void QueryIsCacheable2()
		{
			Sfi.Statistics.Clear();
			Sfi.EvictQueries();

			var x = db.Customers.Cacheable().List();
			var x2 = db.Customers.List();

			Assert.That(Sfi.Statistics.QueryExecutionCount, Is.EqualTo(2), "Unexpected execution count");
			Assert.That(Sfi.Statistics.QueryCachePutCount, Is.EqualTo(1), "Unexpected cache put count");
			Assert.That(Sfi.Statistics.QueryCacheHitCount, Is.EqualTo(0), "Unexpected cache hit count");
		}

		[Test]
		public void QueryIsCacheableWithRegion()
		{
			Sfi.Statistics.Clear();
			Sfi.EvictQueries();
			Sfi.EvictQueries("test");
			Sfi.EvictQueries("other");

			var x = db.Customers.Cacheable().CacheRegion("test").List();
			var x2 = db.Customers.Cacheable().CacheRegion("test").List();
			var x3 = db.Customers.Cacheable().CacheRegion("other").List();

			Assert.That(Sfi.Statistics.QueryExecutionCount, Is.EqualTo(2), "Unexpected execution count");
			Assert.That(Sfi.Statistics.QueryCachePutCount, Is.EqualTo(2), "Unexpected cache put count");
			Assert.That(Sfi.Statistics.QueryCacheHitCount, Is.EqualTo(1), "Unexpected cache hit count");
		}


		[Test]
		public void CanBeCombinedWithFetch()
		{

			Sfi.Statistics.Clear();
			Sfi.EvictQueries();

			db.Customers
			.Cacheable()
			.List();

			db.Orders
				.Cacheable()
				.List();

			db.Customers
				.Fetch(SelectMode.Fetch, x => x.Orders)
				.Cacheable()
				.List();

			db.Orders
				.Fetch(SelectMode.Fetch, x => x.OrderLines)
				.Cacheable()
				.List();

			var customer = db.Customers
				.Fetch(SelectMode.Fetch, x => x.Address)
				.Where(x => x.CustomerId == "VINET")
				.Cacheable()
				.SingleOrDefault();

			customer = db.Customers
				.Fetch(SelectMode.Fetch, x => x.Address)
				.Where(x => x.CustomerId == "VINET")
				.Cacheable()
				.SingleOrDefault();

			Assert.That(NHibernateUtil.IsInitialized(customer.Address), Is.True, "Expected the fetched Address to be initialized");
			Assert.That(Sfi.Statistics.QueryExecutionCount, Is.EqualTo(5), "Unexpected execution count");
			Assert.That(Sfi.Statistics.QueryCachePutCount, Is.EqualTo(5), "Unexpected cache put count");
			Assert.That(Sfi.Statistics.QueryCacheHitCount, Is.EqualTo(1), "Unexpected cache hit count");
		}

		[Test]
		public void FetchIsCacheable()
		{
			Sfi.Statistics.Clear();
			Sfi.EvictQueries();

			Order order;
			order = db.Orders
					.Fetch(
						SelectMode.Fetch,
						x => x.Customer,
						x => x.OrderLines,
						x => x.OrderLines.First().Product,
						x => x.OrderLines.First().Product.OrderLines)
					.Where(x => x.OrderId == 10248)
					.Cacheable()
					.List()
					.First();

			AssertFetchedOrder(order);

			Assert.That(Sfi.Statistics.QueryExecutionCount, Is.EqualTo(1), "Unexpected execution count");
			Assert.That(Sfi.Statistics.QueryCachePutCount, Is.EqualTo(1), "Unexpected cache put count");
			Assert.That(Sfi.Statistics.QueryCacheMissCount, Is.EqualTo(1), "Unexpected cache miss count");

			Sfi.Statistics.Clear();
			Session.Clear();

			order = db.Orders
					.Fetch(
						SelectMode.Fetch,
						x => x.Customer,
						x => x.OrderLines,
						x => x.OrderLines.First().Product,
						x => x.OrderLines.First().Product.OrderLines)
					.Where(x => x.OrderId == 10248)
					.Cacheable()
					.List()
					.First();

			AssertFetchedOrder(order);

			Assert.That(Sfi.Statistics.QueryExecutionCount, Is.EqualTo(0), "Unexpected execution count");
			Assert.That(Sfi.Statistics.QueryCachePutCount, Is.EqualTo(0), "Unexpected cache put count");
			Assert.That(Sfi.Statistics.QueryCacheMissCount, Is.EqualTo(0), "Unexpected cache miss count");
			Assert.That(Sfi.Statistics.QueryCacheHitCount, Is.EqualTo(1), "Unexpected cache hit count");
		}
		
		[Test]
		public void FutureFetchIsCacheable()
		{
			Sfi.Statistics.Clear();
			Sfi.EvictQueries();
			var multiQueries = Sfi.ConnectionProvider.Driver.SupportsMultipleQueries;

			Order order;

			db.Orders
			.Fetch(SelectMode.Fetch, x => x.Customer)
			.Where(x => x.OrderId == 10248)
			.Cacheable()
			.Future();

			order = db.Orders
					.Fetch(
						SelectMode.Fetch,
						x => x.OrderLines,
						x => x.OrderLines.First().Product,
						x => x.OrderLines.First().Product.OrderLines)
					.Where(x => x.OrderId == 10248)
					.Cacheable()
					.Future()
					.ToList()
					.First();

			AssertFetchedOrder(order);

			Assert.That(Sfi.Statistics.QueryExecutionCount, Is.EqualTo(multiQueries ? 1 : 2), "Unexpected execution count");
			Assert.That(Sfi.Statistics.QueryCachePutCount, Is.EqualTo(2), "Unexpected cache put count");
			Assert.That(Sfi.Statistics.QueryCacheMissCount, Is.EqualTo(2), "Unexpected cache miss count");

			Sfi.Statistics.Clear();
			Session.Clear();

			db.Orders
			.Fetch(SelectMode.Fetch, x => x.Customer)
			.Where(x => x.OrderId == 10248)
			.Cacheable()
			.Future();

			order = db.Orders
					.Fetch(
						SelectMode.Fetch,
						x => x.OrderLines,
						x => x.OrderLines.First().Product,
						x => x.OrderLines.First().Product.OrderLines)
					.Where(x => x.OrderId == 10248)
					.Cacheable()
					.Future()
					.ToList()
					.First();
			
			AssertFetchedOrder(order);

			Assert.That(Sfi.Statistics.QueryExecutionCount, Is.EqualTo(0), "Unexpected execution count");
			Assert.That(Sfi.Statistics.QueryCachePutCount, Is.EqualTo(0), "Unexpected cache put count");
			Assert.That(Sfi.Statistics.QueryCacheMissCount, Is.EqualTo(0), "Unexpected cache miss count");
			Assert.That(Sfi.Statistics.QueryCacheHitCount, Is.EqualTo(2), "Unexpected cache hit count");
		}

		private static void AssertFetchedOrder(Order order)
		{
			Assert.That(NHibernateUtil.IsInitialized(order.Customer), Is.True, "Expected the fetched Customer to be initialized");
			Assert.That(NHibernateUtil.IsInitialized(order.OrderLines), Is.True, "Expected the fetched  OrderLines to be initialized");
			Assert.That(order.OrderLines, Has.Count.EqualTo(3), "Expected the fetched OrderLines to have 3 items");
			var orderLine = order.OrderLines.First();
			Assert.That(NHibernateUtil.IsInitialized(orderLine.Product), Is.True, "Expected the fetched Product to be initialized");
			Assert.That(NHibernateUtil.IsInitialized(orderLine.Product.OrderLines), Is.True, "Expected the fetched OrderLines to be initialized");
		}
	}
}
