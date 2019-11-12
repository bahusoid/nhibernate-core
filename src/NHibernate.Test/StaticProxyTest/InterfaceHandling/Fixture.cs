﻿using System;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.StaticProxyTest.InterfaceHandling
{
	[TestFixture]
	public class Fixture : TestCaseMappingByCode
	{
		private readonly Guid _id = Guid.NewGuid();

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			#region Subclass hierarchy

			mapper.Class<EntityClassProxy>(
				rc =>
				{
					rc.Id(x => x.Id);
					rc.Property(x => x.Name);
				});

			mapper.UnionSubclass<SubEntityInterfaceProxy>(
				rc =>
				{
					rc.Proxy(typeof(ISubEntityProxy));

					rc.Property(x => x.AnotherName);
				});

			mapper.UnionSubclass<AnotherSubEntityInterfaceProxy>(
				rc =>
				{
					rc.Proxy(typeof(IAmbigiousSubEntityProxy));

					rc.Property(x => x.AnotherName);
				});

			mapper.Class<EntityWithSuperClassInterfaceLookup>(
				rc =>
				{
					rc.Id(x => x.Id);
					rc.Property(x => x.Name);
					rc.ManyToOne(x => x.EntityLookup, x => x.Class(typeof(EntityClassProxy)));
				});

			#endregion Subclass hierarchy

			mapper.Class<EntitySimple>(
				rc =>
				{
					rc.Id(x => x.Id);
					rc.Property(x => x.Name);
				});

			mapper.Class<EntityExplicitInterface>(
				rc =>
				{
					rc.Id(x => x.Id);
					rc.Property(x => x.Name);
				});

			mapper.Class<EntityMultiInterfaces>(
				rc =>
				{
					rc.Id(x => x.Id);
					rc.Property(x => x.Name);
				});

			mapper.Class<EntityMixExplicitImplicitInterface>(
				rc =>
				{
					rc.Id(x => x.Id);
					rc.Property(x => x.Name);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		[Test]
		public void ProxyForBaseSubclassCanBeCreated()
		{
			using (var session = OpenSession())
			{
				var entity = session.Load<EntityClassProxy>(_id);
			}
		}

		//Id access via implicit interface should not lead to proxy initialization
		[Test]
		public void ProxyClassIdAccessByImplicitInterface()
		{
			using (var session = OpenSession())
			{
				var entity = (IEntity) session.Load<EntitySimple>(_id);
				CanAccessIEntityId(entity);
				ThrowOnIEntityNameAccess(entity);
				Assert.That(entity.Id, Is.EqualTo(_id));

				var multiInterface = session.Load<EntityMultiInterfaces>(_id);
				CanAccessIEntityId(multiInterface);
				CanAccessIEntity2Id(multiInterface);
				Assert.That(multiInterface.Id, Is.EqualTo(_id));
			}
		}

		[Test]
		public void ProxyClassIdAccessExplicitInterface()
		{
			using (var session = OpenSession())
			{
				var entity = session.Load<EntityExplicitInterface>(_id);

				ThrowOnIEntityIdAccess(entity);
				Assert.That(entity.Id, Is.EqualTo(_id));
			}
		}

		[Test]
		public void ProxyClassIdAccessBothImplicitExplicitInterfaces()
		{
			using (var session = OpenSession())
			{
				var entity = session.Load<EntityMixExplicitImplicitInterface>(_id);

				//IEntity2 is implicit and should be accessible without proxy initialization
				CanAccessIEntity2Id(entity);
				ThrowOnIEntityIdAccess(entity);
			}
		}

		[Test]
		public void ProxyInterface()
		{
			using (var session = OpenSession())
			{
				var entity = session.Load<ISubEntityProxy>(_id);

				CanAccessIEntityId(entity);
			}
		}

		[KnownBug("GH-2271")]
		[Test]
		public void ProxyInterfaceCanAccessIdFromDifferentInterfaces()
		{
			using (var session = OpenSession())
			{
				var entity = session.Load<IAmbigiousSubEntityProxy>(_id);

				CanAccessIEntityId(entity);
				CanAccessIEntity2Id(entity);
			}
		}

		private void ThrowOnIEntityNameAccess(IEntity entity)
		{
			Assert.That(() => entity.Name, Throws.TypeOf<ObjectNotFoundException>(), "IEntity.Name access should lead to proxy initialization");
		}

		private void ThrowOnIEntityIdAccess(IEntity entity)
		{
			Assert.That(() => entity.Id, Throws.TypeOf<ObjectNotFoundException>(), "IEntity.Id access should lead to proxy initialization");
		}

		private void ThrowOnIEntity2IdAccess(IEntity2 entity)
		{
			Assert.That(() => entity.Id, Throws.TypeOf<ObjectNotFoundException>(), "IEntityId.Id access should lead to proxy initialization");
		}

		private void CanAccessIEntityId(IEntity entity)
		{
			Assert.That(() => entity.Id, Throws.Nothing, "Failed to access proxy IEntity.Id interface");
			Assert.That(entity.Id, Is.EqualTo(_id));
		}

		private void CanAccessIEntity2Id(IEntity2 entity)
		{
			Assert.That(() => entity.Id, Throws.Nothing, "Failed to access proxy IEntity2.Id interface");
			Assert.That(entity.Id, Is.EqualTo(_id));
		}
	}
}
