using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NHibernate.SqlCommand;
using NUnit.Framework;

namespace NHibernate.Test.Criteria
{
	[TestFixture]
	public class TestQueryBatch : TestCaseMappingByCode
	{
		private const string customEntityName = "CustomEntityName";
		private EntityWithCompositeId _entityWithCompositeId;
		private EntityWithNoAssociation _noAssociation;
		private EntityCustomEntityName _entityWithCustomEntityName;


		[Test]
		public void TestFutureQueryOver1()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				var results = session.QueryOver<EntityComplex>()
									.TransformUsing(new MyCustomTransformer())
									.Cacheable()
									.List<int>();

				var results2 = session.QueryOver<EntityComplex>()
									.TransformUsing(new MyCustomTransformer())
									.Cacheable()
									.List<int>();

				var results3 = session.QueryOver<EntitySimpleChild>()
									.List<int>();
			}
		}

		[Test]
		public void TestFutureQueryOverFuture()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				var si = session.GetSessionImplementation();
				var batch = new MultiUniBatch(si);

				var q1 = session.QueryOver<EntityComplex>()
								.Where(x => x.Version >= 0)
								.TransformUsing(new MyCustomTransformer());

				batch.Add(new CriteriaMultiSource(si, q1.RootCriteria));

				var q2 = session.QueryOver<EntityComplex>().Where(x => x.Version >= 1);
				batch.Add(new CriteriaMultiSource(si, q2.RootCriteria));

				batch.Add(new LinqMultiSource<EntityComplex>(si, session.Query<EntityComplex>().Where(ec => ec.Version > 2)));
				batch.Execute();
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
					rc.Property(x => x.Name);
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
			}
		}

		#endregion Test Setup
	}
}
