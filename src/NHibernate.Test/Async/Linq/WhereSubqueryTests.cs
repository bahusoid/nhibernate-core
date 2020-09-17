﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.DomainModel.Northwind.Entities;
using NUnit.Framework;
using NHibernate.Linq;

namespace NHibernate.Test.Linq
{
	using System.Threading.Tasks;
	[TestFixture]
	public class WhereSubqueryTestsAsync : LinqTestCase
	{
		[Test]
		public async Task TimesheetsWithNoEntriesAsync()
		{
			var query = await ((from timesheet in db.Timesheets
						 where !timesheet.Entries.Any()
						 select timesheet).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(1));
		}

		[Test]
		public async Task TimeSheetsWithCountSubqueryAsync()
		{
			if (!Dialect.SupportsScalarSubSelects)
				Assert.Ignore(Dialect.GetType().Name + " does not support scalar sub-queries");

// ReSharper disable UseMethodAny.1
			var query = await ((from timesheet in db.Timesheets
						 where timesheet.Entries.Count() >= 1
						 select timesheet).ToListAsync());
// ReSharper restore UseMethodAny.1

			Assert.That(query.Count, Is.EqualTo(2));
		}

		[Test]
		public async Task TimeSheetsWithCountSubqueryReversedAsync()
		{
			if (!Dialect.SupportsScalarSubSelects)
				Assert.Ignore(Dialect.GetType().Name + " does not support scalar sub-queries");

// ReSharper disable UseMethodAny.1
			var query = await ((from timesheet in db.Timesheets
						 where 1 <= timesheet.Entries.Count()
						 select timesheet).ToListAsync());
// ReSharper restore UseMethodAny.1

			Assert.That(query.Count, Is.EqualTo(2));
		}

		[Test]
		public async Task TimeSheetsWithCountSubqueryComparedToPropertyAsync()
		{
			if (!Dialect.SupportsScalarSubSelects)
				Assert.Ignore(Dialect.GetType().Name + " does not support scalar sub-queries");

			var query = await ((from timesheet in db.Timesheets
						 where timesheet.Entries.Count() > timesheet.Id
						 select timesheet).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(1));
		}

		[Test]
		public async Task TimeSheetsWithCountSubqueryComparedToPropertyReversedAsync()
		{
			if (!Dialect.SupportsScalarSubSelects)
				Assert.Ignore(Dialect.GetType().Name + " does not support scalar sub-queries");

			var query = await ((from timesheet in db.Timesheets
						 where timesheet.Id < timesheet.Entries.Count()
						 select timesheet).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(1));
		}

		[Test]
		public async Task TimeSheetsWithAverageSubqueryAsync()
		{
			if (!Dialect.SupportsScalarSubSelects)
				Assert.Ignore(Dialect.GetType().Name + " does not support scalar sub-queries");

			var query = await ((from timesheet in db.Timesheets
						 where timesheet.Entries.Average(e => e.NumberOfHours) > 12
						 select timesheet).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(1));
		}

		[Test]
		public async Task TimeSheetsWithAverageSubqueryReversedAsync()
		{
			if (!Dialect.SupportsScalarSubSelects)
				Assert.Ignore(Dialect.GetType().Name + " does not support scalar sub-queries");

			var query = await ((from timesheet in db.Timesheets
						 where 12 < timesheet.Entries.Average(e => e.NumberOfHours)
						 select timesheet).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(1));
		}

		[Test]
		public async Task TimeSheetsWithMaxSubqueryAsync()
		{
			if (!Dialect.SupportsScalarSubSelects)
				Assert.Ignore(Dialect.GetType().Name + " does not support scalar sub-queries");

			var query = await ((from timesheet in db.Timesheets
						 where timesheet.Entries.Max(e => e.NumberOfHours) == 14
						 select timesheet).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(1));
		}

		[Test]
		public async Task TimeSheetsWithMaxSubqueryReversedAsync()
		{
			if (!Dialect.SupportsScalarSubSelects)
				Assert.Ignore(Dialect.GetType().Name + " does not support scalar sub-queries");

			var query = await ((from timesheet in db.Timesheets
						 where 14 == timesheet.Entries.Max(e => e.NumberOfHours)
						 select timesheet).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(1));
		}

		[Test]
		public async Task TimeSheetsWithMaxSubqueryComparedToPropertyAsync()
		{
			if (!Dialect.SupportsScalarSubSelects)
				Assert.Ignore(Dialect.GetType().Name + " does not support scalar sub-queries");

			var query = await ((from timesheet in db.Timesheets
						 where timesheet.Entries.Max(e => e.NumberOfHours) > timesheet.Id
						 select timesheet).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(2));
		}

		[Test]
		public async Task TimeSheetsWithMaxSubqueryComparedToPropertyReversedAsync()
		{
			if (!Dialect.SupportsScalarSubSelects)
				Assert.Ignore(Dialect.GetType().Name + " does not support scalar sub-queries");

			var query = await ((from timesheet in db.Timesheets
						 where timesheet.Id < timesheet.Entries.Max(e => e.NumberOfHours)
						 select timesheet).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(2));
		}

		[Test]
		public async Task TimeSheetsWithMinSubqueryAsync()
		{
			if (!Dialect.SupportsScalarSubSelects)
				Assert.Ignore(Dialect.GetType().Name + " does not support scalar sub-queries");

			var query = await ((from timesheet in db.Timesheets
						 where timesheet.Entries.Min(e => e.NumberOfHours) < 7
						 select timesheet).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(2));
		}

		[Test]
		public async Task TimeSheetsWithMinSubqueryReversedAsync()
		{
			if (!Dialect.SupportsScalarSubSelects)
				Assert.Ignore(Dialect.GetType().Name + " does not support scalar sub-queries");

			var query = await ((from timesheet in db.Timesheets
						 where 7 > timesheet.Entries.Min(e => e.NumberOfHours)
						 select timesheet).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(2));
		}

		[Test]
		public async Task TimeSheetsWithMinSubqueryComparedToPropertyAsync()
		{
			if (!Dialect.SupportsScalarSubSelects)
				Assert.Ignore(Dialect.GetType().Name + " does not support scalar sub-queries");

			var query = await ((from timesheet in db.Timesheets
						 where timesheet.Entries.Min(e => e.NumberOfHours) > timesheet.Id
						 select timesheet).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(2));
		}

		[Test]
		public async Task TimeSheetsWithMinSubqueryComparedToPropertyReversedAsync()
		{
			if (!Dialect.SupportsScalarSubSelects)
				Assert.Ignore(Dialect.GetType().Name + " does not support scalar sub-queries");

			var query = await ((from timesheet in db.Timesheets
						 where timesheet.Id < timesheet.Entries.Min(e => e.NumberOfHours)
						 select timesheet).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(2));
		}

		[Test]
		public async Task TimeSheetsWithSumSubqueryAsync()
		{
			if (!Dialect.SupportsScalarSubSelects)
				Assert.Ignore(Dialect.GetType().Name + " does not support scalar sub-queries");

			var query = await ((from timesheet in db.Timesheets
						 where timesheet.Entries.Sum(e => e.NumberOfHours) <= 20
						 select timesheet).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(1));
		}

		[Test]
		public async Task TimeSheetsWithSumSubqueryReversedAsync()
		{
			if (!Dialect.SupportsScalarSubSelects)
				Assert.Ignore(Dialect.GetType().Name + " does not support scalar sub-queries");

			var query = await ((from timesheet in db.Timesheets
						 where 20 >= timesheet.Entries.Sum(e => e.NumberOfHours)
						 select timesheet).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(1));
		}

		[Test]
		public async Task TimeSheetsWithStringContainsSubQueryAsync()
		{
			var query = await ((from timesheet in db.Timesheets
						 where timesheet.Entries.Any(e => e.Comments.Contains("testing"))
						 select timesheet).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(2));
		}

		[Test(Description = "NG-2998")]
		public async Task TimeSheetsWithStringContainsSubQueryWithAsQueryableAsync()
		{
			var query = await ((from timesheet in db.Timesheets
						 where timesheet.Entries.AsQueryable().Any(e => e.Comments.Contains("testing"))
						 select timesheet).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(2));
		}

		[Test(Description = "NH-2998")]
		public async Task TimeSheetsWithStringContainsSubQueryWithAsQueryableAndExternalPredicateAsync()
		{
			Expression<Func<TimesheetEntry, bool>> predicate = e => e.Comments.Contains("testing");

			var query = await ((from timesheet in db.Timesheets
						 where timesheet.Entries.AsQueryable().Any(predicate)
						 select timesheet).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(2));
		}

		[Test(Description = "NH-2998")]
		public async Task CategoriesSubQueryWithAsQueryableAndExternalPredicateWithClosureAsync()
		{
			var ids = new[] { 1 };
			var quantities = new[] { 100 };

			Expression<Func<OrderLine, bool>> predicate2 = e => quantities.Contains(e.Quantity);
			Expression<Func<Product, bool>> predicate1 = e => !ids.Contains(e.ProductId)
															  && e.OrderLines.AsQueryable().Any(predicate2);

			var query = await ((from category in db.Categories
						 where category.Products.AsQueryable().Any(predicate1)
						 select category).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(6));
		}

		[Test(Description = "NH-2998")]
		public async Task TimeSheetsSubQueryWithAsQueryableAndExternalPredicateWithSecondLevelClosureAsync()
		{
			var ids = new[] { 1 };

			Expression<Func<TimesheetEntry, bool>> predicate = e => !ids.Contains(e.Id);

			var query = await ((from timesheet in db.Timesheets
						 where timesheet.Entries.AsQueryable().Any(predicate)
						 select timesheet).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(2));
		}

		[Test(Description = "NH-2998")]
		public async Task TimeSheetsSubQueryWithAsQueryableAndExternalPredicateWithArrayAsync()
		{
			Expression<Func<TimesheetEntry, bool>> predicate = e => !new[] { 1 }.Contains(e.Id);

			var query = await ((from timesheet in db.Timesheets
						 where timesheet.Entries.AsQueryable().Any(predicate)
						 select timesheet).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(2));
		}

		[Test(Description = "NH-2998")]
		public async Task TimeSheetsSubQueryWithAsQueryableWithArrayAsync()
		{
			var query = await ((from timesheet in db.Timesheets
						 where timesheet.Entries.AsQueryable().Any(e => !new[] { 1 }.Contains(e.Id))
						 select timesheet).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(2));
		}

		[Test(Description = "GH-2471")]
		public async Task TimeSheetsWithStringContainsSubQueryWithAsQueryableAfterWhereAsync()
		{
			var query = await ((
				from timesheet in db.Timesheets
				where timesheet.Entries.Where(e => e.Comments != null).AsQueryable().Any(e => e.Comments.Contains("testing"))
				select timesheet).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(2));
		}

		[Test(Description = "NH-3002")]
		public async Task HqlOrderLinesWithInnerJoinAndSubQueryAsync()
		{
			var lines = await (session.CreateQuery(@"select c from OrderLine c
join c.Order o
where o.Customer.CustomerId = 'VINET'
	and not exists (from c.Order.Employee.Subordinates x where x.EmployeeId = 100)
").ListAsync<OrderLine>());

			Assert.That(lines.Count, Is.EqualTo(10));
		}

		[Test(Description = "NH-3002")]
		public async Task HqlOrderLinesWithImpliedJoinAndSubQueryAsync()
		{
			var lines = await (session.CreateQuery(@"from OrderLine c
where c.Order.Customer.CustomerId = 'VINET'
	and not exists (from c.Order.Employee.Subordinates x where x.EmployeeId = 100)
").ListAsync<OrderLine>());

			Assert.That(lines.Count, Is.EqualTo(10));
		}

		[Test(Description = "NH-2999 and NH-2988")]
		public async Task OrderLinesWithImpliedJoinAndSubQueryAsync()
		{
// ReSharper disable SimplifyLinqExpression
			var lines = await ((from l in db.OrderLines
						 where l.Order.Customer.CustomerId == "VINET"
						 where !l.Order.Employee.Subordinates.Any(x => x.EmployeeId == 100)
						 select l).ToListAsync());
// ReSharper restore SimplifyLinqExpression

			Assert.That(lines.Count, Is.EqualTo(10));
		}

		[Test(Description = "NH-2904")]
		public async Task OrdersWithSubquery1Async()
		{
			var query = await ((from order in db.Orders
						 where order.OrderLines.Any()
						 select order).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(830));
		}

		[Test(Description = "NH-2904")]
		public async Task OrdersWithSubquery2Async()
		{
			var subquery = from line in db.OrderLines
						   select line.Order;

			var query = await ((from order in db.Orders
						 where subquery.Contains(order)
						 select order).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(830));
		}

		[Test(Description = "NH-2904")]
		public async Task OrdersWithSubquery3Async()
		{
			var subquery = from line in db.OrderLines
						   select line.Order.OrderId;

			var query = await ((from order in db.Orders
						 where subquery.Contains(order.OrderId)
						 select order).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(830));
		}

		[Test(Description = "NH-2904")]
		public async Task OrdersWithSubquery4Async()
		{
			var subquery = from line in db.OrderLines
						   select line.Order;

			var query = await ((from order in db.Orders
						 where subquery.Any(x => x.OrderId == order.OrderId)
						 select order).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(830));
		}

		[Test(Description = "NH-2904")]
		public async Task OrdersWithSubquery5Async()
		{
			var query = await ((from order in db.Orders
						 where order.OrderLines.Any(x => x.Quantity == 5)
						 select order).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(61));
		}

		[Test(Description = "NH-2904")]
		public async Task OrdersWithSubquery6Async()
		{
			var subquery = from line in db.OrderLines
						   where line.Quantity == 5
						   select line.Order;

			var query = await ((from order in db.Orders
						 where subquery.Contains(order)
						 select order).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(61));
		}

		[Test(Description = "NH-2904")]
		public async Task OrdersWithSubquery7Async()
		{
			var subquery = from line in db.OrderLines
						   where line.Quantity == 5
						   select line.Order.OrderId;

			var query = await ((from order in db.Orders
						 where subquery.Contains(order.OrderId)
						 select order).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(61));
		}

		[Test(Description = "NH-2904")]
		public async Task OrdersWithSubquery8Async()
		{
			var subquery = from line in db.OrderLines
						   where line.Quantity == 5
						   select line.Order;

			var query = await ((from order in db.Orders
						 where subquery.Any(x => x.OrderId == order.OrderId)
						 select order).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(61));
		}

		[Test(Description = "NH-2654")]
		public async Task CategoriesWithDiscountedProductsAsync()
		{
			var query = await ((from c in db.Categories
						 where c.Products.Any(p => p.Discontinued)
						 select c).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(5));
		}

		[Test(Description = "NH-3147")]
		public async Task OrdersWithSubqueryWithJoinAsync()
		{
			var subquery = from line in db.OrderLines
						   join product in db.Products
							   on line.Product.ProductId equals product.ProductId
						   where line.Quantity == 5
						   select line.Order;

			var query = await ((from order in db.Orders
						 where subquery.Contains(order)
						 select order).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(61));
		}

		[Test(Description = "NH-2899")]
		public async Task ProductsWithSubqueryAsync()
		{
			var result = await ((from p in db.Products
						  where (from c in db.Categories
								 where c.Name == "Confections"
								 select c).Contains(p.Category)
						  select p)
				.ToListAsync());

			Assert.That(result.Count, Is.EqualTo(13));
		}

		[Test(Description = "NH-2762")]
		public async Task ProductsWithSubqueryAsIEnumerableAsync()
		{
// ReSharper disable RedundantEnumerableCastCall
			var categories = (await ((from c in db.Categories
							  where c.Name == "Confections"
							  select c).ToListAsync())).OfType<ProductCategory>();
// ReSharper restore RedundantEnumerableCastCall

			var result = await ((from p in db.Products
						  where categories.Contains(p.Category)
						  select p)
				.ToListAsync());

			Assert.That(result.Count, Is.EqualTo(13));
		}

		[Test(Description = "NH-2762")]
		public async Task ProductsWithSubqueryAsIGroupingAsync()
		{
			var categories = (from c in db.Categories
							  where c.Name == "Confections"
							  select c).ToLookup(c => c.Name).Single();

			var result = await ((from p in db.Products
						  where categories.Contains(p.Category)
						  select p)
				.ToListAsync());

			Assert.That(result.Count, Is.EqualTo(13));
		}

		[Test(Description = "NH-3111")]
		public async Task SubqueryWhereFailingTestAsync()
		{
			if (!Dialect.SupportsScalarSubSelects)
				Assert.Ignore(Dialect.GetType().Name + " does not support subquery in select clause");

			if (!TestDialect.SupportsOrderByAndLimitInSubQueries)
				Assert.Ignore("Dialect does not support sub-selects with order by or limit/top");

			var list = await ((db.OrderLines
				.Select(ol => new
				{
					ol.Discount,
					ShipperPhoneNumber = db.Shippers
						.Where(sh => sh.ShipperId == ol.Order.Shipper.ShipperId)
						.Select(sh => sh.PhoneNumber)
						.FirstOrDefault()
				})).ToListAsync());

			Assert.That(list.Count, Is.EqualTo(2155));
		}

		[Test(Description = "NH-3111")]
		public async Task SubqueryWhereFailingTest2Async()
		{
			if (!Dialect.SupportsScalarSubSelects)
				Assert.Ignore(Dialect.GetType().Name + " does not support subquery in select clause");

			if (!TestDialect.SupportsOrderByAndLimitInSubQueries)
				Assert.Ignore("Dialect does not support sub-selects with order by or limit/top");

			var list = await (db.OrderLines
				.Select(ol => new
				{
					ol.Discount,
					ShipperPhoneNumber = db.Shippers
						.Where(sh => sh == ol.Order.Shipper)
						.Select(sh => sh.PhoneNumber)
						.FirstOrDefault()
				}).ToListAsync());

			Assert.That(list.Count, Is.EqualTo(2155));
		}

		[Test(Description = "NH-3111")]
		public async Task SubqueryWhereFailingTest3Async()
		{
			if (!Dialect.SupportsScalarSubSelects)
				Assert.Ignore(Dialect.GetType().Name + " does not support subquery in select clause");

			if (!TestDialect.SupportsOrderByAndLimitInSubQueries)
				Assert.Ignore("Dialect does not support sub-selects with order by or limit/top");

			var list = await (db.OrderLines
				.Select(ol => new
				{
					ol.Discount,
					ShipperPhoneNumber = db.Orders
						.Where(sh => sh.Shipper.ShipperId == ol.Order.Shipper.ShipperId)
						.Select(sh => sh.Shipper.PhoneNumber)
						.FirstOrDefault()
				}).ToListAsync());

			Assert.That(list.Count, Is.EqualTo(2155));
		}

		[Test(Description = "NH-3190")]
		public async Task ProductsWithSubqueryReturningBoolFirstOrDefaultEqAsync()
		{
			if (!Dialect.SupportsScalarSubSelects)
				Assert.Ignore(Dialect.GetType().Name + " does not support scalar sub-queries");

			if (!TestDialect.SupportsOrderByAndLimitInSubQueries)
				Assert.Ignore("Dialect does not support sub-selects with order by or limit/top");

			var result = await ((from p in db.Products
						  where (from c in db.Categories
								 where c.Name == "Confections"
								 && c == p.Category
								 select p.Discontinued).FirstOrDefault() == false
						  select p)
				.ToListAsync());

			Assert.That(result.Count, Is.EqualTo(13));
		}

		[Test(Description = "NH-3190")]
		public async Task SubselectCanHaveBoolResultAsync()
		{
			if (!Dialect.SupportsScalarSubSelects)
				Assert.Ignore(Dialect.GetType().Name + " does not support scalar sub-queries");

			if (!TestDialect.SupportsOrderByAndLimitInSubQueries)
				Assert.Ignore("Dialect does not support sub-selects with order by or limit/top");

			var result = await ((from c in db.Categories
						  where c.Products.OrderBy(p => p.ProductId).Select(p => p.Discontinued).FirstOrDefault() == false
						  select c).ToListAsync());

			Assert.That(result.Count, Is.EqualTo(7));
		}

		[Test(Description = "NH-3190")]
		public async Task ProductsWithSubqueryReturningStringFirstOrDefaultEqAsync()
		{
			if (!Dialect.SupportsScalarSubSelects)
				Assert.Ignore(Dialect.GetType().Name + " does not support scalar sub-queries");

			if (!TestDialect.SupportsOrderByAndLimitInSubQueries)
				Assert.Ignore("Dialect does not support sub-selects with order by or limit/top");

			var result = await ((from p in db.Products
						  where (from c in db.Categories
								 where c.Name == "Confections"
								 && c == p.Category
								 select p.Name).FirstOrDefault() == p.Name
						  select p)
				.ToListAsync());

			Assert.That(result.Count, Is.EqualTo(13));
		}

		[Test(Description = "NH-3423")]
		public async Task NullComparedToNewExpressionInWhereClauseAsync()
		{
			// Construction will never be equal to null, so the ternary should be collapsed
			// to just the IfFalse expression. Without this collapsing, we cannot generate HQL.

			var result = await (db.Products
				.Select(p => new {Name = p.Name, Pr2 = new {ReorderLevel = p.ReorderLevel}})
				.Where(pr1 => (pr1.Pr2 == null ? (int?) null : pr1.Pr2.ReorderLevel) > 6)
				.ToListAsync());

			Assert.That(result.Count, Is.EqualTo(45));
		}

		private class Pr2
		{
			public int ReorderLevel { get; set; }
		}

		private class Pr1
		{
			public string Name { get; set; }
			public Pr2 Pr2 { get; set; }
		}

		[Test(Description = "NH-3423")]
		public async Task NullComparedToMemberInitExpressionInWhereClauseAsync()
		{
			// Construction will never be equal to null, so the ternary should be collapsed
			// to just the IfFalse expression. Without this collapsing, we cannot generate HQL.

			var result = await (db.Products
				.Select(p => new Pr1 { Name = p.Name, Pr2 = new Pr2 { ReorderLevel = p.ReorderLevel } })
				.Where(pr1 => (pr1.Pr2 == null ? (int?)null : pr1.Pr2.ReorderLevel) > 6)
				.ToListAsync());

			Assert.That(result.Count, Is.EqualTo(45));
		}

		public class Specification<T>
		{
			private Expression<Func<T, bool>> _expression;

			public Specification(Expression<Func<T, bool>> expression)
			{
				_expression = expression;
			}

			public static implicit operator Expression<Func<T, bool>>(Specification<T> specification)
			{
				return specification._expression;
			}
		}

		[Test]
		public async Task ImplicitConversionInsideWhereSubqueryExpressionAsync()
		{
			if (!Dialect.SupportsScalarSubSelects)
				Assert.Ignore(Dialect.GetType().Name + " does not support scalar sub-queries");

			var spec = new Specification<Order>(x => x.Freight > 1000);
			await (db.Orders.Where(o => db.Orders.Where(spec).Any(x => x.OrderId == o.OrderId)).ToListAsync());
		}
	}
}
