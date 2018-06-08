using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NHibernate.Multi;
using NHibernate.Transform;
using NUnit.Framework;

namespace NHibernate.Test.Futures
{
	[TestFixture]
	public class QueryBatchFixture : TestCaseMappingByCode
	{
		private Guid _parentId;
		private Guid _eagerId;

		[Test]
		public void CanCombineCriteriaAndHqlInFuture()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				var future1 = session.QueryOver<EntityComplex>()
						.Where(x => x.Version >= 0)
						.TransformUsing(new ListTransformerToInt()).Future<int>();

				var future2 = session.Query<EntityComplex>().Where(ec => ec.Version > 2).ToFuture();
				var future3 = session.Query<EntitySimpleChild>().Select(sc => sc.Name).ToFuture();

				var future4 = session
						.Query<EntitySimpleChild>()
						.ToFutureValue(sc => sc.FirstOrDefault());

				Assert.That(future1.GetEnumerable().Count(), Is.GreaterThan(0), "Empty results are not expected");
				Assert.That(future2.GetEnumerable().Count(), Is.EqualTo(0), "This query should not return results");
				Assert.That(future3.GetEnumerable().Count(), Is.GreaterThan(1), "Empty results are not expected");
				Assert.That(future4.Value, Is.Not.Null, "Loaded entity should not be null");

				if (SupportsMultipleQueries)
					Assert.That(sqlLog.Appender.GetEvents().Length, Is.EqualTo(1));
			}
		}

		[Test]
		public void CanCombineCriteriaAndHqlInBatch()
		{
			using (var session = OpenSession())
			{
				var batch = session
					.CreateQueryBatch()

					.Add<int>(
						session
							.QueryOver<EntityComplex>()
							.Where(x => x.Version >= 0)
							.TransformUsing(new ListTransformerToInt()))

					.Add("queryOver", session.QueryOver<EntityComplex>().Where(x => x.Version >= 1))

					.Add(session.Query<EntityComplex>().Where(ec => ec.Version > 2));

				using (var sqlLog = new SqlLogSpy())
				{
					batch.GetResult<int>(0);
					batch.GetResult<EntityComplex>("queryOver");
					batch.GetResult<EntityComplex>(2);
					if (SupportsMultipleQueries)
						Assert.That(sqlLog.Appender.GetEvents().Length, Is.EqualTo(1));
				}
			}
		}

		[Test]
		public void CanFetchCollectionInBatch()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				var batch = session.CreateQueryBatch();

				var q1 = session.QueryOver<EntityComplex>()
								.Where(x => x.Version >= 0);

				batch.Add(q1);
				batch.Add(session.Query<EntityComplex>().Fetch(c => c.ChildrenList));
				batch.Execute();

				var parent = session.Load<EntityComplex>(_parentId);
				Assert.That(NHibernateUtil.IsInitialized(parent), Is.True);
				Assert.That(NHibernateUtil.IsInitialized(parent.ChildrenList), Is.True);
				if (SupportsMultipleQueries)
					Assert.That(sqlLog.Appender.GetEvents().Length, Is.EqualTo(1));
			}
		}

		//NH-3350 (Duplicate records using Future())
		[Test]
		public void SameCollectionFetches()
		{
			using (var session = OpenSession())
			{
				var entiyComplex = session.QueryOver<EntityComplex>().Where(c => c.Id == _parentId).FutureValue();

				session.QueryOver<EntityComplex>()
						.Fetch(SelectMode.Fetch, ec => ec.ChildrenList)
						.Where(c => c.Id == _parentId).Future();

				session.QueryOver<EntityComplex>()
						.Fetch(SelectMode.Fetch, ec => ec.ChildrenList)
						.Where(c => c.Id == _parentId).Future();

				var parent = entiyComplex.Value;
				Assert.That(NHibernateUtil.IsInitialized(parent), Is.True);
				Assert.That(NHibernateUtil.IsInitialized(parent.ChildrenList), Is.True);
				Assert.That(parent.ChildrenList.Count, Is.EqualTo(2));
				
			}
		}

		//NH-3864 - Cacheable Multicriteria/Future'd query with aliased join throw exception 
		[Test]
		public void CacheableCriteriaWithAliasedJoinFuture()
		{
			using (var session = OpenSession())
			{
				EntitySimpleChild child1 = null;
				var ecFuture = session.QueryOver<EntityComplex>()
									.JoinAlias(c => c.Child1, () => child1)
									.Where(c => c.Id == _parentId)
									.Cacheable()
									.FutureValue();
				EntityComplex value = null;
				Assert.DoesNotThrow(() => value = ecFuture.Value);
				Assert.That(value, Is.Not.Null);
			}

			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				EntitySimpleChild child1 = null;
				var ecFuture = session.QueryOver<EntityComplex>()
									.JoinAlias(c => c.Child1, () => child1)
									.Where(c => c.Id == _parentId)
									.Cacheable()
									.FutureValue();
				EntityComplex value = null;
				Assert.DoesNotThrow(() => value = ecFuture.Value);
				Assert.That(value, Is.Not.Null);

				Assert.That(sqlLog.Appender.GetEvents().Length, Is.EqualTo(0), "Query is expected to be retrieved from cache");
			}
		}

		//NH-3334 - 'collection is not associated with any session' upon refreshing objects from QueryOver<>().Future<>()
		[KnownBug("NH-3334")]
		[Test]
		public void RefreshFutureWithEagerCollections()
		{
			using (var session = OpenSession())
			{
				var ecFutureList = session.QueryOver<EntityEager>().Future();

				foreach(var ec in ecFutureList.GetEnumerable())
				{
					//trouble causes ec.ChildrenListEager with eager select mapping
					Assert.DoesNotThrow(() => session.Refresh(ec), "session.Refresh should not throw exception");
				}
			}
		}

		//Related to NH-3334. Eager mappings are not fetched by Future
		[KnownBug("NH-3334")]
		[Test]
		public void FutureForEagerMappedCollection()
		{
			//Note: This behavior might be considered as feature but it's not documented.
			using (var session = OpenSession())
			{
				var futureValue = session.QueryOver<EntityEager>().Where(e => e.Id == _eagerId).FutureValue();

				Assert.That(futureValue.Value, Is.Not.Null);
				Assert.That(NHibernateUtil.IsInitialized(futureValue.Value), Is.True);
				Assert.That(NHibernateUtil.IsInitialized(futureValue.Value.ChildrenListEager), Is.True);
				Assert.That(NHibernateUtil.IsInitialized(futureValue.Value.ChildrenListSubselect), Is.True);
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

					rc.ManyToOne(ep => ep.Child1, m => m.Column("Child1Id"));
					rc.ManyToOne(ep => ep.Child2, m => m.Column("Child2Id"));
					rc.ManyToOne(ep => ep.SameTypeChild, m => m.Column("SameTypeChildId"));

					rc.Bag(
						ep => ep.ChildrenList,
						m =>
						{
							m.Cascade(Mapping.ByCode.Cascade.All);
							m.Inverse(true);
						},
						a => a.OneToMany());
				});

			mapper.Class<EntitySimpleChild>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
					rc.ManyToOne(x => x.Parent);
					rc.Property(x => x.Name);
				});
			mapper.Class<EntityEager>(
				rc =>
				{
					rc.Lazy(false);

					rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
					rc.Property(x => x.Name);

					rc.Bag(ep => ep.ChildrenListSubselect,
							m =>
							{
								m.Cascade(Mapping.ByCode.Cascade.All);
								m.Inverse(true);
								m.Fetch(CollectionFetchMode.Subselect);
								m.Lazy(CollectionLazy.NoLazy);
							},
							a => a.OneToMany());

					rc.Bag(ep => ep.ChildrenListEager,
							m =>
							{
								m.Lazy(CollectionLazy.NoLazy);
							},
							a => a.OneToMany());
				});
			mapper.Class<EntitySubselectChild>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
					rc.Property(x => x.Name);
					rc.ManyToOne(c => c.Parent);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
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

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var child1 = new EntitySimpleChild
				{
					Name = "Child1",
				};
				var child2 = new EntitySimpleChild
				{
					Name = "Child2"
				};
				var complex = new EntityComplex
				{
					Name = "ComplexEnityParent",
					Child1 = child1,
					Child2 = child2,
					LazyProp = "SomeBigValue",
					SameTypeChild = new EntityComplex()
					{
						Name = "ComplexEntityChild"
					},
				};
				child1.Parent = child2.Parent = complex;

				var eager = new EntityEager()
				{
					Name = "eager1",
				};

				var eager2 = new EntityEager()
				{
					Name = "eager2",
				};
				eager.ChildrenListSubselect = new List<EntitySubselectChild>()
					{
						new EntitySubselectChild()
						{
							Name = "subselect1",
							Parent = eager,
						},
						new EntitySubselectChild()
						{
							Name = "subselect2",
							Parent = eager,
						},
					};

				session.Save(child1);
				session.Save(child2);
				session.Save(complex.SameTypeChild);
				session.Save(complex);
				session.Save(eager);
				session.Save(eager2);

				session.Flush();
				transaction.Commit();

				_parentId = complex.Id;
				_eagerId = eager.Id;
			}
		}

		public class ListTransformerToInt : IResultTransformer
		{
			public object TransformTuple(object[] tuple, string[] aliases)
			{
				return tuple.Length == 1 ? tuple[0] : tuple;
			}

			public IList TransformList(IList collection)
			{
				return new List<int>()
				{
					1,
					2,
					3,
					4,
				};
			}
		}

		private bool SupportsMultipleQueries => Sfi.ConnectionProvider.Driver.SupportsMultipleQueries;

		#endregion Test Setup
	}
}
