using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH2580
{
	[TestFixture]
	public class JoinOnSubclassIssue : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.AddMapping<FamilyMap>();
			mapper.AddMapping<PersonMap>();
			mapper.AddMapping<AdultMap>();
			mapper.AddMapping<ChildMap>();

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
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
		public void JoinOnSubclassWithBaseTableReferenceInOnClause()
		{
			using(new SqlLogSpy())
			using (var session = OpenSession())
			{
				var ids = (from adult in session.Query<AdultEntity>()
							join child in session.Query<ChildEntity>()
								on adult.Family.Id equals child.Family.Id
							where adult.Id == 1
							select child.Id)
						.Distinct()
						.ToList();
			}
		}
	}
}
