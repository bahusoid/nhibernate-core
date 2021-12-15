using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1920
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		private List<Guid> _ids = new List<Guid>();
		protected override void OnSetUp()
		{
			_ids.Clear();
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				for (int i = 0; i < 5; i++)
				{
					_ids.Add((Guid) session.Save(new EntityWithBatchSize { Name = "some name" + 1 }));
				}

				transaction.Commit();
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public void CanLoadEntity(bool loadProxyOfOtherEntity)
		{
			using(new SqlLogSpy())
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				if (loadProxyOfOtherEntity)
					session.Load<EntityWithBatchSize>(_ids[0]);

				var result = session.Get<EntityWithBatchSize>(_ids[1]);

				Assert.That(result.Name, Is.Not.Null);
			}
		}

		[TestCase(1)]
		[TestCase(2)]
		[TestCase(3)]
		[TestCase(4)]
		[TestCase(5)]
		public void CanLoadBatch(int loadCount)
		{
			using(new SqlLogSpy())
			using (var session = OpenSession())
			{
				foreach (var id in _ids.Take(loadCount))
				{
					session.Load<EntityWithBatchSize>(id);
				}

				var result = session.Get<EntityWithBatchSize>(_ids[0]);

				var result2 = session.Get<EntityWithBatchSize>(_ids.Last());

				Assert.That(result?.Name, Is.Not.Null);
				Assert.That(result2?.Name, Is.Not.Null);
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.CreateQuery("delete from EntityWithBatchSize").ExecuteUpdate();
				transaction.Commit();
			}
		}
	}
}
