using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.Loader;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.Criteria.SelectModeTest
{
	/// <summary>
	/// Tests for explicit entity joins (not associated entities)
	/// </summary>
	[TestFixture]
	public class SelectModeTest : TestCaseMappingByCode
	{
		private Guid _parentId;


		[Test]
		public void SelectModeSkipTest()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				EntityComplex entityComplex = null;
				EntitySimpleChild root = null;
				root = session.QueryOver(() => root)
							.JoinEntityQueryOver(() => entityComplex, Restrictions.Where(() => root.ParentId == entityComplex.Id))
							.With(SelectMode.Skip, a => a)
							.Take(1)
							.SingleOrDefault();

				Assert.That(root, Is.Not.Null);
				Assert.That(NHibernateUtil.IsInitialized(root), Is.True);
				Assert.That(sqlLog.Appender.GetEvents().Length, Is.EqualTo(1), "Only one SQL select is expected");
			}
		}

		[Test]
		public void TestFetch()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				var list = session.QueryOver<EntityComplex>()
						.Fetch(ec => ec.SameTypeChild).Eager
						.Fetch(ec => ec.SameTypeChild.ChildrenList).Eager
						.List();
			}
		}
		
		[Test]
		public void SelectModeSkipTest_Aliased()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				EntityComplex entityComplex = null;
				EntitySimpleChild root = null;
				root = session.QueryOver(() => root)
							.JoinEntityQueryOver(() => entityComplex, Restrictions.Where(() => root.ParentId == entityComplex.Id))
							.With(SelectMode.Skip, () => entityComplex)
							.Take(1)
							.SingleOrDefault();

				Assert.That(root, Is.Not.Null);
				Assert.That(NHibernateUtil.IsInitialized(root), Is.True);
				Assert.That(sqlLog.Appender.GetEvents().Length, Is.EqualTo(1), "Only one SQL select is expected");
			}
		}
		
		[Test]
		public void SelectModeFetchLazyPropertiesForEntityAlias()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				EntityComplex entityComplex = null;
				EntitySimpleChild root = null;
				root = session.QueryOver(() => root)
							.JoinEntityQueryOver(() => entityComplex, Restrictions.Where(() => root.ParentId == entityComplex.Id))
							.With(SelectMode.FetchLazyProperties, ec => ec)
							.Take(1)
							.SingleOrDefault();
				entityComplex = session.Load<EntityComplex>(root.ParentId);

				Assert.That(root, Is.Not.Null);
				
				Assert.That(NHibernateUtil.IsInitialized(root), Is.True);
				Assert.That(NHibernateUtil.IsInitialized(entityComplex), Is.True);
				Assert.That(NHibernateUtil.IsPropertyInitialized(entityComplex, nameof(entityComplex.LazyProp)), Is.Not.Null.Or.Empty);
				Assert.That(entityComplex.LazyProp, Is.Not.Null.Or.Empty);
				
				Assert.That(sqlLog.Appender.GetEvents().Length, Is.EqualTo(1), "Only one SQL select is expected");
			}
		}	
		
		[Test]
		public void SelectModeFetchLazyPropertiesForCollection()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				EntityComplex root = null;
				root = session.QueryOver(() => root)
							.JoinQueryOver(ec => ec.ChildrenList)
							.With(SelectMode.FetchLazyProperties, simpleChild => simpleChild)
							.Take(1)
							.SingleOrDefault();


				Assert.That(root, Is.Not.Null);
				
				Assert.That(NHibernateUtil.IsInitialized(root), Is.True);
				//Assert.That(NHibernateUtil.IsInitialized(entityComplex), Is.True);
				//Assert.That(NHibernateUtil.IsPropertyInitialized(entityComplex, nameof(entityComplex.LazyProp)), Is.Not.Null.Or.Empty);
				//Assert.That(entityComplex.LazyProp, Is.Not.Null.Or.Empty);
				
				Assert.That(sqlLog.Appender.GetEvents().Length, Is.EqualTo(1), "Only one SQL select is expected");
			}
		}
		
		[Test]
		public void SelectModeReturnsNullForNotLoadedObject()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				EntityComplex root = null;
				root = session.QueryOver(() => root)
							.With(SelectMode.Id, r => r)
							.JoinQueryOver(ec => ec.ChildrenList)
							.With(SelectMode.Fetch, simpleChild => simpleChild)
							.Take(1)
							.SingleOrDefault();

				Assert.That(root, Is.Null);
				Assert.That(sqlLog.Appender.GetEvents().Length, Is.EqualTo(1), "Only one SQL select is expected");
			}
		}

		[Test]
		public void SelectModeChildFetch()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				EntityComplex root = null;
				root = session.QueryOver(() => root)
							.List()
							.First(r => r.Id == _parentId);

				session
					.QueryOver(() => root)
					.With(SelectMode.Id, ec => ec)
					.With(SelectMode.Fetch, ec => ec.ChildrenList)
					.List();

				Assert.That(root?.ChildrenList, Is.Not.Null);
				Assert.That(NHibernateUtil.IsInitialized(root?.ChildrenList));
			}
		}

		[Test]
		public void SelectModeChildFetchDeep()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				EntityComplex root = null;
				root = session.QueryOver(() => root)
							.List()
							.First(r => r.Id == _parentId);

				session
					.QueryOver(() => root)
					.With(SelectMode.Id, ec => ec)
					.With(SelectMode.Fetch, ec => ec.ChildrenList)
					.List();

				session
					.QueryOver(() => root)
					.With(SelectMode.Id, ec => ec)
					.With(SelectMode.Id, ec => ec.ChildrenList)
					.With(SelectMode.Fetch, ec => ec.ChildrenList[0].Children)
					.List();

				session
					.QueryOver(() => root)
					.With(SelectMode.Id, ec => ec)
					.With(SelectMode.Skip, ec => ec.ChildrenList)
					.With(SelectMode.Id, ec => ec.ChildrenList[0].Children)
					.With(SelectMode.Fetch, ec => ec.ChildrenList[0].Children[0].Children)
					.List();


				Assert.That(root?.ChildrenList, Is.Not.Null);
				Assert.That(NHibernateUtil.IsInitialized(root.ChildrenList));
				Assert.That(NHibernateUtil.IsInitialized(root.ChildrenList[0].Children));
				Assert.That(NHibernateUtil.IsInitialized(root.ChildrenList[0].Children[0].Children));
			}
		}			
		
		[Test]
		public void SelectModeChildFetchDeep_SkipRoot()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				EntityComplex root = null;
				root = session.QueryOver(() => root)
							.List()
							.First(r => r.Id == _parentId);

				var o1 = session
					.QueryOver(() => root)
					.With(SelectMode.Id, ec => ec)
					.With(SelectMode.Fetch, ec => ec.ChildrenList)
					.List();

				var o2 = session
					.QueryOver(() => root)
					.With(SelectMode.Skip, ec => ec)
					.With(SelectMode.Id, ec => ec.ChildrenList)
					.With(SelectMode.Fetch, ec => ec.ChildrenList[0].Children)
					.List();

				var o3 = session
					.QueryOver(() => root)
					.With(SelectMode.Skip, ec => ec, ec => ec.ChildrenList)
					.With(SelectMode.Id, ec => ec.ChildrenList[0].Children)
					.With(SelectMode.Fetch, ec => ec.ChildrenList[0].Children[0].Children)
					.List();


				Assert.That(root?.ChildrenList, Is.Not.Null);
				Assert.That(NHibernateUtil.IsInitialized(root.ChildrenList));
				Assert.That(NHibernateUtil.IsInitialized(root.ChildrenList[0].Children));
				Assert.That(NHibernateUtil.IsInitialized(root.ChildrenList[0].Children[0].Children));
			}
		}		
		
		[Test]
		public void SelectModeChildFetchDeep_CheckCaseWhenObjectIsMissedInSessionItDoesntThrow()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				EntityComplex root = null;
				root = session.QueryOver(() => root)
							.List()
							.First(r => r.Id == _parentId);

				session.Clear();
				session
					.QueryOver(() => root)
					.With(SelectMode.Id, ec => ec)
					.With(SelectMode.Fetch, ec => ec.ChildrenList)
					.List();

				session.Clear();
				session
					.QueryOver(() => root)
					.With(SelectMode.Id, ec => ec)
					.With(SelectMode.Id, ec => ec.ChildrenList)
					.With(SelectMode.Fetch, ec => ec.ChildrenList[0].Children)
					.List();
				session.Clear();

				session
					.QueryOver(() => root)
					.With(SelectMode.Id, ec => ec)
					.With(SelectMode.Skip, ec => ec.ChildrenList)
					.With(SelectMode.Id, ec => ec.ChildrenList[0].Children)
					.With(SelectMode.Fetch, ec => ec.ChildrenList[0].Children[0].Children)
					.List();

				
			}
		}

		[Test]
		public void SelectModeChildFetchDeep_SingleDbRoundtrip_Aliased()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				EntityComplex root = null;
				var rootFuture = session.QueryOver(() => root)
									.Future();

				session
					.QueryOver(() => root)
					.With(SelectMode.Id, () => root)
					.With(SelectMode.Fetch, () => root.ChildrenList)
					.Future();

				session
					.QueryOver(() => root)
					.With(SelectMode.Id, () => root, () => root.ChildrenList)
					.With(SelectMode.Fetch, () => root.ChildrenList[0].Children)
					.Future();

				session
					.QueryOver(() => root)
					.With(SelectMode.Id, () => root)
					.With(SelectMode.Skip, () => root.ChildrenList)
					.With(SelectMode.Id, () => root.ChildrenList[0].Children)
					.With(SelectMode.Fetch, () => root.ChildrenList[0].Children[0].Children)
					.Future();

				root = rootFuture.ToList().First(r => r.Id == _parentId);


				Assert.That(root?.ChildrenList, Is.Not.Null);
				Assert.That(NHibernateUtil.IsInitialized(root?.ChildrenList));
				Assert.That(NHibernateUtil.IsInitialized(root?.ChildrenList[0].Children));
				Assert.That(NHibernateUtil.IsInitialized(root?.ChildrenList[0].Children[0].Children));
				Assert.That(sqlLog.Appender.GetEvents().Length, Is.EqualTo(1), "Only one SQL select is expected");
			}
		}
		
		[Test]
		public void SelectModeChildFetchDeep_SingleDbRoundtrip_Aliased_SkipRoot()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				EntityComplex root = null;
				var rootFuture = session.QueryOver(() => root)
									.Future();

				session
					.QueryOver(() => root)
					.With(SelectMode.Id,    () => root)
					.With(SelectMode.Fetch, () => root.ChildrenList)
					.Future();

				session
					.QueryOver(() => root)
					.With(SelectMode.Skip, () => root)
					.With(SelectMode.Id, () => root.ChildrenList)
					.With(SelectMode.Fetch, () => root.ChildrenList[0].Children)
					.Future();

				session
					.QueryOver(() => root)
					.With(SelectMode.Skip, () => root, () => root.ChildrenList)
					.With(SelectMode.Id,    () => root.ChildrenList[0].Children)
					.With(SelectMode.Fetch, () => root.ChildrenList[0].Children[0].Children)
					.Future();

				root = rootFuture.ToList().First(r => r.Id == _parentId);

				Assert.That(root?.ChildrenList, Is.Not.Null);
				Assert.That(NHibernateUtil.IsInitialized(root?.ChildrenList));
				Assert.That(NHibernateUtil.IsInitialized(root?.ChildrenList[0].Children));
				Assert.That(NHibernateUtil.IsInitialized(root?.ChildrenList[0].Children[0].Children));
				Assert.That(sqlLog.Appender.GetEvents().Length, Is.EqualTo(1), "Only one SQL select is expected");
			}
		}
		
		[Test]
		public void SelectModeCollection()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				EntityComplex root = null;
			
				root = session.QueryOver(() => root)
							.With(SelectMode.FetchLazyProperties, ec => ec.ChildrenList)
							.Take(1)
							.SingleOrDefault();

				//Assert.That(NHibernateUtil.IsInitialized(entityComplex), Is.True);
				Assert.That(sqlLog.Appender.GetEvents().Length, Is.EqualTo(1), "Only one SQL select is expected");
			}
		}
		
		[Test]
		public void SelectModeCollectionSkip()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				EntityComplex root = null;
			
				root = session.QueryOver(() => root)
							.With(SelectMode.Skip, ec => ec.ChildrenList)
							.Take(1)
							.SingleOrDefault();


				//Assert.That(NHibernateUtil.IsInitialized(entityComplex), Is.True);
				Assert.That(sqlLog.Appender.GetEvents().Length, Is.EqualTo(1), "Only one SQL select is expected");
			}
		}
		
		[Test]
		public void SelectModeDeepFetch()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				EntityComplex root = null;

				var futureRoot = session.QueryOver(() => root)
										.With(SelectMode.Fetch, ec => ec.Child1)
										.Future();

				session.QueryOver(() => root)
						.With(SelectMode.Id, ec => ec)
						.With(SelectMode.Fetch, ec => ec.ChildrenList);


				root = futureRoot.FirstOrDefault();

				Assert.That(root, Is.Not.Null);
				Assert.That(NHibernateUtil.IsInitialized(root), Is.True);
				Assert.That(NHibernateUtil.IsInitialized(root.ChildrenList), Is.True);
				Assert.That(root.ChildrenList, Is.Not.Empty);
				Assert.That(sqlLog.Appender.GetEvents().Length, Is.EqualTo(1), "Only one SQL select is expected");
			}
		}
		
		[Test]
		public void SelectModeCollectionId()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				EntityComplex root = null;
			
				root = session.QueryOver(() => root)
							.With(SelectMode.Id, ec => ec.ChildrenList)
							.Take(1)
							.SingleOrDefault();

				//Assert.That(NHibernateUtil.IsInitialized(entityComplex), Is.True);
				Assert.That(sqlLog.Appender.GetEvents().Length, Is.EqualTo(1), "Only one SQL select is expected");
			}
		}

		[Test]
		public void SelectModeIdTest()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				EntityComplex entityComplex = null;
				EntityWithNoAssociation root = null;
				root = session.QueryOver(() => root)
							.JoinEntityQueryOver(() => entityComplex, Restrictions.Where(() => root.Complex1Id == entityComplex.Id))
							.With(SelectMode.Id, a => a)
							.Take(1)
							.SingleOrDefault();

				//Assert.That(NHibernateUtil.IsInitialized(entityComplex), Is.True);
				Assert.That(sqlLog.Appender.GetEvents().Length, Is.EqualTo(1), "Only one SQL select is expected");
			}
		}
	#region Test Setup

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<EntityComplex>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));

					rc.Version(ep => ep.Version, vm => { });

					rc.Property(x => x.Name);

					rc.Property(ep => ep.LazyProp, m => m.Lazy(true));

					rc.ManyToOne(ep => ep.Child1, m =>
					{
						m.Column("Child1Id");
						m.ForeignKey("none");
					});
					rc.ManyToOne(ep => ep.Child2, m =>
					{
						m.Column("Child2Id");
						m.ForeignKey("none");
					});
					rc.ManyToOne(ep => ep.SameTypeChild, m => m.Column("SameTypeChildId"));
					MapList(rc, ep => ep.ChildrenList);
				});

			MapSimpleChild(
				mapper,
				default(EntitySimpleChild),
				c => c.Children,
				rc => { rc.Property(sc => sc.LazyProp, mp => mp.Lazy(true)); });
			MapSimpleChild(mapper, default(Level2Child), c => c.Children);
			MapSimpleChild<Level3Child>(mapper);

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		private static void MapSimpleChild<TChild>(ModelMapper mapper) where TChild:BaseChild
		{
			MapSimpleChild<TChild, object>(mapper, default(TChild), null);
		}

		private static void MapSimpleChild<TChild, TSubChild>(ModelMapper mapper, TChild obj, Expression<Func<TChild, IEnumerable<TSubChild>>> expression, Action<IClassMapper<TChild>> action = null) where TChild : BaseChild
		{
			mapper.Class<TChild>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
					rc.Property(x => x.Name);
					rc.Property(c => c.ParentId);
					if (expression != null)
					{
						MapList(rc, expression);
					}
				});
		}

		private static void MapList<TParent,TElement>(IClassMapper<TParent> rc, Expression<Func<TParent, IEnumerable<TElement>>> expression) where TParent : class
		{
			rc.Bag(expression,
				m =>
				{
					m.Key(km =>
					{
						km.Column(ckm =>
						{
							ckm.Name("ParentId");
							//ckm.Check("none");

						});
						km.ForeignKey("none");

					});
					m.Cascade(Mapping.ByCode.Cascade.All);
				},
				a => a.OneToMany());
		}

		protected override void OnTearDown()
		{
			using (ISession session = OpenSession())
			using (ITransaction transaction = session.BeginTransaction(IsolationLevel.Serializable))
			{
				session.Query<Level3Child>().Delete();
				session.Query<Level2Child>().Delete();
				session.Query<EntityComplex>().Delete();
				session.Query<EntitySimpleChild>().Delete();


				session.Flush();
				transaction.Commit();
			}
		}

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var child1 = new EntitySimpleChild
				{
					Name = "Child1",
					LazyProp = "LazyFromSimpleChild1",
					Children = new List<Level2Child>
					{
						new Level2Child()
						{
							Name = "Level2.1",
							Children = new List<Level3Child>
							{
								new Level3Child
								{
									Name = "Level3.1.1",
								},
								new Level3Child
								{
									Name = "Level3.1.2"
								},
							}
						},
						new Level2Child
						{
							Name = "Level2.2",
							Children = new List<Level3Child>
							{
								new Level3Child
								{
									Name = "Level3.2.1"
								},
								new Level3Child
								{
									Name = "Level3.2.2"
								},
							}
						}
					}
					
				};
				var child2 = new EntitySimpleChild
				{
					Name = "Child2",
					LazyProp = "LazyFromSimpleChild2",
				};

				var parent = new EntityComplex
				{
					Name = "ComplexEntityParent",
					Child1 = child1,
					Child2 = child2,
					LazyProp = "SomeBigValue",
					SameTypeChild = new EntityComplex()
					{
						Name = "ComplexEntityChild"
					},
					ChildrenList = new List<EntitySimpleChild>() {child1}
				};

				session.Save(child1);
				session.Save(child2);
				session.Save(parent.SameTypeChild);
				session.Save(parent);
				

				session.Flush();
				transaction.Commit();
				_parentId = parent.Id;
			}
		}

		#endregion Test Setup
	}

	public abstract class BaseChild
	{
		public virtual Guid Id { get; set; }
		public virtual string Name { get; set; }
		public virtual Guid? ParentId { get; set; }
	}
	public class EntitySimpleChild : BaseChild
	{
		public virtual IList<Level2Child> Children { get; set; } = new List<Level2Child>();
		public virtual string LazyProp { get;set; }
	}

	public class Level2Child : BaseChild
	{
		public virtual IList<Level3Child> Children { get; set; } = new List<Level3Child>();
	}

	public class Level3Child : BaseChild
	{ }

	public class EntityComplex
	{
		public virtual Guid Id { get; set; }

		public virtual int Version { get; set; }

		public virtual string Name { get; set; }

		public virtual string LazyProp { get; set; }

		public virtual EntitySimpleChild Child1 { get; set; }
		public virtual EntitySimpleChild Child2 { get; set; }
		public virtual EntityComplex SameTypeChild { get; set; }

		public virtual IList<EntitySimpleChild> ChildrenList { get; set; }
	}

}
