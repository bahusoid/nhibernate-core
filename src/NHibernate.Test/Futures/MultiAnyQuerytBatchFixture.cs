﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NHibernate.Transform;
using NUnit.Framework;

namespace NHibernate.Test.Criteria
{
	[TestFixture]
	public class MultiAnyQuerytBatchFixture : TestCaseMappingByCode
	{
		private const string customEntityName = "CustomEntityName";
		private EntityWithCompositeId _entityWithCompositeId;
		private EntityWithNoAssociation _noAssociation;
		private EntityCustomEntityName _entityWithCustomEntityName;
		private Guid _parentId;
		private Guid _eagerId;

		[Test]
		public void CanCombineCriteriaAndHqlAsFuture()
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
		public void CanCombineCriteriaAndHqlAsSepearateBatch()
		{
			using (var session = OpenSession())
			{
				var batch = NewBatch(session);

				var futureBatch1 = 
					batch.AddAsList<int>(
					session.QueryOver<EntityComplex>()
							.Where(x => x.Version >= 0)
							.TransformUsing(new ListTransformerToInt()));

				var futureEntComplList = batch.AddAsList(session.QueryOver<EntityComplex>().Where(x => x.Version >= 1));

				var futureList3 = batch.AddAsList(session.Query<EntityComplex>().Where(ec => ec.Version > 2));


				using (var sqlLog = new SqlLogSpy())
				{
					IList<int> list1 = futureBatch1.Value;
					IList<EntityComplex> list2 = futureEntComplList.Value;
					IList<EntityComplex> list3 = futureList3.Value;
					if(SupportsMultipleQueries)
						Assert.That(sqlLog.Appender.GetEvents().Length, Is.EqualTo(1));
				}
			}
		}
		
		[Test]
		public void TestMultiWithCollectionInitialized()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				var batch = NewBatch(session);

				var q1 = session.QueryOver<EntityComplex>()
								.Where(x => x.Version >= 0);

				batch.Add(new MultiAnyCriteriaQuery<object>(q1.RootCriteria));

				batch.Add(new MultiAnyLinqQuery<EntityComplex>(session.Query<EntityComplex>().Fetch(c => c.ChildrenList)));
				batch.Execute();
				var parent = session.Load<EntityComplex>(_parentId);
				Assert.That(NHibernateUtil.IsInitialized(parent), Is.True);
				Assert.That(NHibernateUtil.IsInitialized(parent.ChildrenList), Is.True);
			}
		}
		
		[Test]
		public void TestMultiWithCollectionInitialized_AsFutureList()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				var si = session.GetSessionImplementation();
				var batch = new MultiAnyQueryBatch(si);

				var q1 = session.QueryOver<EntityComplex>()
								.Where(x => x.Version >= 0);

				batch.Add(new MultiAnyCriteriaQuery<object>(q1.RootCriteria));

				batch.Add(new MultiAnyLinqQuery<EntityComplex>(session.Query<EntityComplex>().Fetch(c => c.ChildrenList)));
				batch.Execute();
				var parent = session.Load<EntityComplex>(_parentId);
				//Assert.That(parent, Is.Not.Null);
				Assert.That(NHibernateUtil.IsInitialized(parent), Is.True);
				Assert.That(NHibernateUtil.IsInitialized(parent.ChildrenList), Is.True);
				
			}
		}

		#region Temp tests for debugging

		[Test, Explicit]
		public void DebugInternals()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				var results = session.QueryOver<EntityComplex>()
									.TransformUsing(new ListTransformerToInt())
									.Cacheable()
									.List<int>();

				var results2 = session.QueryOver<EntityComplex>()
									.TransformUsing(new ListTransformerToInt())
									.Cacheable()
									.List<int>();

				var results3 = session.QueryOver<EntitySimpleChild>()
									.List<int>();
			}
		}

		[Test, Explicit]
		public void TestQueryWithSubselect()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				var ec = session.Get<EntityEager>(_eagerId);
			}
		}

		[Test, Explicit]
		public void TestQueryWithSubselectList()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				var ec = session.QueryOver<EntityEager>().List();
				var eager = session.Load<EntityEager>(_eagerId);
				Assert.That(NHibernateUtil.IsInitialized(eager), Is.True);
				Assert.That(NHibernateUtil.IsInitialized(eager.ChildrenListSubselect), Is.True);
			}
		}

		[Test, Explicit]
		public void TestQueryWithSubselectListFuture()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				var ec = session.QueryOver<EntityEager>().Future();
				var eager = ec.GetEnumerable().First();
				eager = session.Load<EntityEager>(_eagerId);
				Assert.That(NHibernateUtil.IsInitialized(eager), Is.True);
				NHibernateUtil.Initialize(eager.ChildrenListSubselect);
				Assert.That(NHibernateUtil.IsInitialized(eager.ChildrenListSubselect), Is.True);
			}
		}

		[Test, Explicit]
		public void FutureValueWithLinqSelector()
		{
			using (var session = OpenSession())
			{
				var b = NewBatch(session);
				var pq = session
									.Query<EntitySimpleChild>()
									//.Where(x => x.Id == _parentId)
									;
				//var r = pq.SingleOrDefault();
				var futureValue = b.AddAsValue(pq, c => c.SingleOrDefault());
				var value = futureValue.Value;

			}
		}

		[Test, Explicit]
		public void TestMultiWithSubselect()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				var batch = NewBatch(session);
				var eagerEntity = batch.AddAsList(session.QueryOver<EntityEager>());

				var list = eagerEntity.Value;
				var eager = session.Load<EntityEager>(_eagerId);
				Assert.That(NHibernateUtil.IsInitialized(eager), Is.True);
				Assert.That(NHibernateUtil.IsInitialized(eager.ChildrenListSubselect), Is.True);
			}
		}

		#endregion Temp tests for debugging


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
					rc.Property(x => x.Name);
				});			
			mapper.Class<EntityEager>(
				rc =>
				{
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
				});			
			mapper.Class<EntitySubselectChild>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
					rc.Property(x => x.Name);
					rc.ManyToOne(c => c.Parent);
				});

			mapper.Class<EntityWithCompositeId>(
				rc =>
				{
					rc.ComponentAsId(
						e => e.Key,
						ekm =>
						{
							ekm.Property(ek => ek.Id1);
							ekm.Property(ek => ek.Id2);
						});

					rc.Property(e => e.Name);
				});

			mapper.Class<EntityWithCompositeId>(
				rc =>
				{
					rc.ComponentAsId(
						e => e.Key,
						ekm =>
						{
							ekm.Property(ek => ek.Id1);
							ekm.Property(ek => ek.Id2);
						});

					rc.Property(e => e.Name);
				});

			mapper.Class<EntityWithNoAssociation>(
				rc =>
				{
					rc.Id(e => e.Id, m => m.Generator(Generators.GuidComb));

					rc.Property(e => e.Complex1Id);
					rc.Property(e => e.Complex2Id);
					rc.Property(e => e.Simple1Id);
					rc.Property(e => e.Simple2Id);
					rc.Property(e => e.Composite1Key1);
					rc.Property(e => e.Composite1Key2);
					rc.Property(e => e.CustomEntityNameId);

				});

			mapper.Class<EntityCustomEntityName>(
				rc =>
				{
					rc.EntityName(customEntityName);

					rc.Id(e => e.Id, m => m.Generator(Generators.GuidComb));
					rc.Property(e => e.Name);
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
					Name = "Child1"
				};
				var child2 = new EntitySimpleChild
				{
					Name = "Child1"
				};

				var parent = new EntityComplex
				{
					Name = "ComplexEnityParent",
					Child1 = child1,
					Child2 = child2,
					LazyProp = "SomeBigValue",
					SameTypeChild = new EntityComplex()
					{
						Name = "ComplexEntityChild"
					}
				};
				
				var eager = new EntityEager()
				{
					Name = "eager",
					ChildrenListSubselect = new List<EntitySubselectChild>()
					{
						new EntitySubselectChild()
						{
							Name = "subselect1",
							Parent = parent,
						},
						new EntitySubselectChild()
						{
							Name = "subselect2",
							Parent = parent,
						},
					}
				};

				_entityWithCompositeId = new EntityWithCompositeId
				{
					Key = new CompositeKey
					{
						Id1 = 1,
						Id2 = 2
					},
					Name = "Composite"
				};

				session.Save(child1);
				session.Save(child2);
				session.Save(parent.SameTypeChild);
				session.Save(parent);
				session.Save(_entityWithCompositeId);
				session.Save(eager);

				_entityWithCustomEntityName = new EntityCustomEntityName()
				{
					Name = "EntityCustomEntityName"
				};

				session.Save(customEntityName, _entityWithCustomEntityName);

				_noAssociation = new EntityWithNoAssociation()
				{
					Complex1Id = parent.Id,
					Complex2Id = parent.SameTypeChild.Id,
					Composite1Key1 = _entityWithCompositeId.Key.Id1,
					Composite1Key2 = _entityWithCompositeId.Key.Id2,
					Simple1Id = child1.Id,
					Simple2Id = child2.Id,
					CustomEntityNameId = _entityWithCustomEntityName.Id
				};
				

				session.Save(_noAssociation);

				session.Flush();
				transaction.Commit();

				_parentId = parent.Id;
				_eagerId = eager.Id;
			}
		}

		private static MultiAnyQueryBatch NewBatch(ISession session)
		{
			var si = session.GetSessionImplementation();
			var batch = new MultiAnyQueryBatch(si);
			return batch;
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
