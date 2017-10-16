﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections;
using System.Data;
using NHibernate.Cfg;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH1677
{
	using System.Threading.Tasks;
	[TestFixture]
	public class EntityModeMapCriteriaAsync : BugTestCase
	{
		private const int NumberOfRecordPerEntity = 10;
		private const string Entity1Name = "Entity1";
		private const string Entity1Property = "Entity1Property";
		private const string Entity2Name = "Entity2";
		private const string Entity2Property = "Entity2Property";
		private const string EntityPropertyPrefix = "Record";

		protected override void OnSetUp()
		{
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction(IsolationLevel.ReadCommitted))
				{
					for (int i = 0; i < NumberOfRecordPerEntity; i++)
					{
						var entity1 = new Hashtable();
						entity1[Entity1Property] = EntityPropertyPrefix + i;
						s.SaveOrUpdate(Entity1Name, entity1);
					}

					for (int i = 0; i < NumberOfRecordPerEntity; i++)
					{
						var entity2 = new Hashtable();
						entity2[Entity2Property] = EntityPropertyPrefix + i;
						s.SaveOrUpdate(Entity2Name, entity2);
					}
					tx.Commit();
				}
			}
		}

		protected override void OnTearDown()
		{
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					s.Delete(string.Format("from {0}", Entity1Name));
					s.Delete(string.Format("from {0}", Entity2Name));
					tx.Commit();
				}
			}
		}

		[Test]
		public async Task EntityModeMapFailsWithCriteriaAsync()
		{
			using (ISessionFactory sf = cfg.BuildSessionFactory())
			{
				using (ISession s = sf.OpenSession())
				{
					IQuery query = s.CreateQuery(string.Format("from {0}", Entity1Name));
					IList entity1List = await (query.ListAsync());
					Assert.AreEqual(NumberOfRecordPerEntity, entity1List.Count); // OK, Count == 10
				}

				using (ISession s = sf.OpenSession())
				{
					ICriteria entity1Criteria = s.CreateCriteria(Entity1Name);
					IList entity1List = await (entity1Criteria.ListAsync());
					Assert.AreEqual(NumberOfRecordPerEntity, entity1List.Count); // KO !!! Count == 20 !!!
				}
			}
		}
	}
}