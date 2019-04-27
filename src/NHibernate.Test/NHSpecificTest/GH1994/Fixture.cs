﻿using System.Linq;
using NHibernate.Linq;
using NHibernate.Transform;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1994
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var a = new Asset();
				a.Documents.Add(new Document { IsDeleted = true });
				a.Documents.Add(new Document { IsDeleted = false });

				session.Save(a);
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

				session.Delete("from System.Object");

				transaction.Commit();
			}
		}

		[Test]
		public void TestUnfilteredLinqQuery()
		{
			using (var s = OpenSession())
			{
				var query = s.Query<Asset>()
				             .FetchMany(x => x.Documents)
				             .ToList();
				
				Assert.That(query.Count, Is.EqualTo(1), "unfiltered assets");
				Assert.That(query[0].Documents.Count, Is.EqualTo(2), "unfiltered asset documents");
			}
		}

		[Test]
		public void TestFilteredByWhereCollectionLinqQuery()
		{
			using (var s = OpenSession())
			{
				var query = s.Query<Asset>()
				             .FetchMany(x => x.DocumentsFiltered)
				             .ToList();
				
				Assert.That(query.Count, Is.EqualTo(1), "unfiltered assets");
				Assert.That(query[0].DocumentsFiltered.Count, Is.EqualTo(1), "unfiltered asset documents");
			}
		}

		[Test]
		public void TestFilteredLinqQuery()
		{
			using (var s = OpenSession())
			{
				s.EnableFilter("deletedFilter").SetParameter("deletedParam", false);
				var query = s.Query<Asset>()
				             .FetchMany(x => x.Documents)
				             .ToList();

				Assert.That(query.Count, Is.EqualTo(1), "filtered assets");
				Assert.That(query[0].Documents.Count, Is.EqualTo(1), "filtered asset documents");
			}
		}

		[Test]
		public void TestFilteredQueryOver()
		{
			using (var s = OpenSession())
			{
				s.EnableFilter("deletedFilter").SetParameter("deletedParam", false);

				var query = s.QueryOver<Asset>()
				             .Fetch(SelectMode.Fetch, x => x.Documents)
				             .TransformUsing(Transformers.DistinctRootEntity)
				             .List<Asset>();

				Assert.That(query.Count, Is.EqualTo(1), "filtered assets");
				Assert.That(query[0].Documents.Count, Is.EqualTo(1), "filtered asset documents");
			}
		}
	}
}
