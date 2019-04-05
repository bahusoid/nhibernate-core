﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1738
{
	using System.Threading.Tasks;
	/// <summary>
	/// Fixture using 'by code' mappings
	/// </summary>
	/// <remarks>
	/// This fixture is identical to <see cref="Fixture" /> except the <see cref="Entity" /> mapping is performed 
	/// by code in the GetMappings method, and does not require the <c>Mappings.hbm.xml</c> file. Use this approach
	/// if you prefer.
	/// </remarks>
	[TestFixture]
	public class RefreshLocallyRemovedCollectionItemFixtureAsync : TestCaseMappingByCode
	{
		private Guid _id;

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<Entity>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
				rc.Property(x => x.Name);
				rc.Bag(
					x => x.Children,
					m =>
					{
						m.Cascade(Mapping.ByCode.Cascade.All | Mapping.ByCode.Cascade.DeleteOrphans);
						m.Inverse(true);
						m.Key(km => km.Column("Parent"));
					},
					relation => relation.OneToMany());
			});

			mapper.Class<Child>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
				rc.Property(x => x.Name);
				rc.ManyToOne(x => x.Parent);
			});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var e1 = new Entity { Name = "Bob"};
				e1.Children.Add(new Child() {Name = "Child", Parent = e1});
				session.Save(e1);
				transaction.Commit();
				_id = e1.Id;
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Delete("from System.Object");
				transaction.Commit();
			}
		}

		[Test]
		public async Task RefreshLocallyRemovedCollectionItemAsync()
		{
			Entity entity;
			using (var session = OpenSession())
			{
				entity = await (session.GetAsync<Entity>(_id));
				entity.Children.RemoveAt(0);
			}

			using (var session = OpenSession())
			{
				await (session.UpdateAsync(entity));
				await (session.RefreshAsync(entity));
				foreach (var child in entity.Children)
				{
					await (session.RefreshAsync(child));
				}

				Assert.That(session.GetSessionImplementation().PersistenceContext.IsReadOnly(entity), Is.False);
			}
		}
	}
}
