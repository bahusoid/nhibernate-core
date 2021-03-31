﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;
using NHibernate.Linq;

namespace NHibernate.Test.NHSpecificTest.GH2707
{
	using System.Threading.Tasks;
	[TestFixture]
	public class ConditionalFixtureAsync : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.AddMapping<Entity1Map>();
			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var e1 = new Entity1() {Id = "id1", IsChiusa = true};
				e1.CustomType = new MyType() {ToPersist = 1};
				session.Save(e1);
				var e2 = new Entity1() {Id = "id2", IsChiusa = false};
				session.Save(e2);
				e1.Parent = e1;
				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.CreateQuery("delete from System.Object").ExecuteUpdate();
				transaction.Commit();
			}
		}

		[Test]
		public async Task EntityAndCustomTypeInConditionalResultAsync()
		{
			using (var s = OpenSession())
				await ((from x in s.Query<Entity1>()
				let parent = x.Parent
				//NH-3005 - Contditional on custom type
				where (parent.IsChiusa ? x.CustomType : parent.CustomType) == x.CustomType
				select new
				{
					ParentIsChiusa = (((x == null) ? null : x.Parent) == null)
						? (bool?) null
						: x.Parent.IsChiusa,
				}).ToListAsync());
		}
	}
}
