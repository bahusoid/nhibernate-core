using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3472
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		protected override void OnSetUp()
		{
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var c = new Cat
				{
					Age = 6,
					Children = new HashSet<Cat>
					{
						new Cat
						{
							Age = 4,
							Children = new HashSet<Cat>
							{
								new Cat { Color = "Ginger", Age = 1 },
								new Cat { Color = "Black", Age = 3 }
							}
						}
					}
				};
				s.Save(c);
				t.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				s.Delete("from Cat");
				t.Commit();
			}
		}

		[Test]
		public void CriteriaQueryWithMultipleJoinsToSameAssociation()
		{
			using (var s = OpenSession())
			{
				var list =
					s
						.CreateCriteria<Cat>("cat")
						.CreateAlias(
							"cat.Children",
							"gingerCat",
							JoinType.LeftOuterJoin,
							Restrictions.Eq("Color", "Ginger"))
						.CreateAlias(
							"cat.Children",
							"blackCat",
							JoinType.LeftOuterJoin,
							Restrictions.Eq("Color", "Black"))
						.SetProjection(
							Projections.Alias(Projections.Property("gingerCat.Age"), "gingerCatAge"),
							Projections.Alias(Projections.Property("blackCat.Age"), "blackCatAge")
						).AddOrder(new Order(Projections.Property("Age"), true)).List<object[]>();
				Assert.That(list, Has.Count.EqualTo(4));
				Assert.That(list[0], Is.EqualTo(new object[] { null, null }));
				Assert.That(list[1], Is.EqualTo(new object[] { null, null }));
				Assert.That(list[2], Is.EqualTo(new object[] { 1, 3 }));
				Assert.That(list[3], Is.EqualTo(new object[] { null, null }));
			}
		}

		[Test]
		public void QueryWithFetchesAndAliasDoNotDuplicateJoin()
		{
			using (var s = OpenSession())
			{
				Cat parent = null;
				using (var spy = new SqlLogSpy())
				{
					var list =
						s
							.QueryOver<Cat>()
							.Fetch(SelectMode.Fetch, o => o.Parent)
							.Fetch(SelectMode.Fetch, o => o.Parent.Parent)
							.JoinAlias(o => o.Parent, () => parent)
							.Where(x => parent.Age == 4)
							.List();

					// Two joins to Cat are expected: one for the immediate parent, and a second for the grand-parent.
					// So checking if it does not contain three joins or more. (The regex uses "[\s\S]" instead of "."
					// because the SQL is formatted by default and contains "\n" which are not matched by ".".)
					Assert.That(spy.GetWholeLog(), Does.Not.Match(@"(?:\bjoin\s*Cat\b[\s\S]*){3,}").IgnoreCase);
					Assert.That(list, Has.Count.EqualTo(2));
					Assert.That(
						NHibernateUtil.IsInitialized(list[0].Parent),
						Is.True,
						"first cat parent initialization status");
					Assert.That(
						NHibernateUtil.IsInitialized(list[1].Parent),
						Is.True,
						"second cat parent initialization status");
					Assert.That(
						NHibernateUtil.IsInitialized(list[0].Parent.Parent),
						Is.True,
						"first cat parent parent initialization status");
					Assert.That(
						NHibernateUtil.IsInitialized(list[1].Parent.Parent),
						Is.True,
						"second cat parent parent initialization status");
				}
			}
		}

		[Test]
		public void QueryWithJoinQueryOverAndAliasDoNotDuplicateJoin()
		{
			using (var s = OpenSession())
			{
				Cat parent = null;
				using (var spy = new SqlLogSpy())
				{
					var list =
						s
							.QueryOver<Cat>()
							//.Fetch(SelectMode.Fetch, o => o.Parent)
							//.Fetch(SelectMode.Fetch, o => o.Parent.Parent)
							.JoinAlias(o => o.Parent, () => parent)
							.JoinQueryOver(o => o.Parent.Parent)
							.Where(x => parent.Age == 4)
							.List();

					// Two joins to Cat are expected: one for the immediate parent, and a second for the grand-parent.
					// So checking if it does not contain three joins or more. (The regex uses "[\s\S]" instead of "."
					// because the SQL is formatted by default and contains "\n" which are not matched by ".".)
					Assert.That(spy.GetWholeLog(), Does.Not.Match(@"(?:\bjoin\s*Cat\b[\s\S]*){3,}").IgnoreCase);
					Assert.That(list, Has.Count.EqualTo(2));
					Assert.That(
						NHibernateUtil.IsInitialized(list[0].Parent),
						Is.True,
						"first cat parent initialization status");
					Assert.That(
						NHibernateUtil.IsInitialized(list[1].Parent),
						Is.True,
						"second cat parent initialization status");
					Assert.That(
						NHibernateUtil.IsInitialized(list[0].Parent.Parent),
						Is.True,
						"first cat parent parent initialization status");
					Assert.That(
						NHibernateUtil.IsInitialized(list[1].Parent.Parent),
						Is.True,
						"second cat parent parent initialization status");
				}
			}
		}

		[Test, Explicit("Debatable use case")]
		public void QueryWithFetchesAndMultipleJoinsToSameAssociation()
		{
			using (var s = OpenSession())
			{
				Cat ginger = null;
				Cat black = null;
				var list =
					s
						.QueryOver<Cat>()
						.Fetch(SelectMode.Fetch, o => o.Children)
						.JoinAlias(o => o.Children, () => ginger)
						.Where(x => ginger.Color == "Ginger")
						.JoinAlias(o => o.Children, () => black)
						.Where(x => black.Color == "Black")
						.List();

				Assert.That(list, Has.Count.EqualTo(1));
				Assert.That(list[0].Children, Has.Count.EqualTo(2));
			}
		}
	}
}
