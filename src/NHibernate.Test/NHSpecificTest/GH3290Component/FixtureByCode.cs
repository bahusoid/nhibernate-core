using System;
using System.Collections.Generic;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH3290Component
{
public class RootEntity
{
	public virtual Guid Id { get; set; }
	public virtual IList<ComponentListEntry> Entries { get; set; } = new List<ComponentListEntry>();
}
public class ComponentListEntry
{
	public virtual string DummyString { get; set; }
	public virtual EntityWithParent ComponentReference { get; set; }
}

public class EntityWithParent
{
	public virtual Guid Id { get; set; }
	public virtual ParentEntity Parent { get; set; }
}
public class ParentEntity
{
	public virtual Guid Id { get; set; }
}

public class Test3290 : TestCaseMappingByCode
{
	[Test]
	public void ThisFails()
	{
		RootEntity root;
		using (var session = Sfi.OpenSession())
		{
			root = new RootEntity();
			root.Entries.Add(new ComponentListEntry
			{
				// loading (and also subsequent updates) fail when this is null
				ComponentReference = null,
				DummyString = "one",
			});

			session.Save(root);
			session.Flush();
		}

		using var newSession = Sfi.OpenSession();
		var reloadedRoot = newSession.Get<RootEntity>(root.Id);
		Assert.AreEqual(1, reloadedRoot.Entries.Count);
	}

	[Test]
	public void ThisWorks()
	{
		RootEntity root;
		using (var session = Sfi.OpenSession())
		{
			var parent = new ParentEntity();
			session.Save(parent);
			var entityWithParent = new EntityWithParent { Parent = parent };
			session.Save(entityWithParent);

			root = new RootEntity();
			root.Entries.Add(new ComponentListEntry
			{
				// loading (and also subsequent updates) work when this is set
				ComponentReference = entityWithParent,
				DummyString = "one",
			});

			session.Save(root);
			session.Flush();
		}

		using var newSession = Sfi.OpenSession();
		var reloadedRoot = newSession.Get<RootEntity>(root.Id);
		Assert.AreEqual(1, reloadedRoot.Entries.Count);
	}

	protected override void OnTearDown()
	{
		using var session = OpenSession();
		using var transaction = session.BeginTransaction();
		session.Delete("from System.Object");
		transaction.Commit();
	}

	protected override HbmMapping GetMappings()
	{
		var mapper = new ModelMapper();

		mapper.Class<RootEntity>(rc =>
		{
			rc.Id(x => x.Id, map => map.Generator(Generators.GuidComb));

			rc.Bag(
				x => x.Entries,
				v =>
				{
					v.Access(Accessor.Property);
					v.Lazy(CollectionLazy.NoLazy);
					v.Fetch(CollectionFetchMode.Select);
				},
				h => h.Component(cmp =>
				{
					// real world code has other members here, both nullable and not-nullable
					cmp.ManyToOne(x => x.ComponentReference);
					cmp.Property(x => x.DummyString);
				}));
		});
		mapper.Class<EntityWithParent>(rc =>
		{
			rc.Id(x => x.Id, map => map.Generator(Generators.GuidComb));
			rc.ManyToOne(x => x.Parent, m => m.NotNullable(true));
		});
		mapper.Class<ParentEntity>(rc =>
		{
			rc.Id(x => x.Id, map => map.Generator(Generators.GuidComb));
		});

		return mapper.CompileMappingForAllExplicitlyAddedEntities();
	}
}}
