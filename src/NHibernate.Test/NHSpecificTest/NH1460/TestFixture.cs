using System;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH1460
{
	[TestFixture]
	public class TestFixture : BugTestCase
	{
		protected override void OnSetUp()
		{
			base.OnSetUp();

			using (ISession s = this.OpenSession())
			{
				Item item = new Item() {Name = "SomeItem"};

				ItemDetail d1 = new ItemDetail() {Code = "A", Id = 1};

				ItemDetail d2 = new ItemDetail() {Code = "B", Id = 2};

				item.AddSubItem(d1);
				item.AddSubItem(d2);

				s.SaveOrUpdate(item);

				s.Flush();
			}

		}

		[Test]
		public void CompositeIdBagShouldNotThrowStackOverflowExceptionWithJoinFetch()
		{
			using (ISession s = this.OpenSession())
			{
				ICriteria c = s.CreateCriteria(typeof(Item));

				c.Fetch("ItemDetails");

				try
				{
					Item i = c.UniqueResult<Item>();

					Assert.AreEqual(2, i.ItemDetails.Count);
				}
				catch (StackOverflowException ex)
				{
					Console.WriteLine(ex.StackTrace);
					Assert.Fail("StackOverflowException thrown when retrieving bag with composite id " + ex.Message);
				}
			}
		}

		[Test]
		public void CompositeIdBagShouldNotThrowStackOverflowExceptionWithSelectFetch()
		{
			using (ISession s = this.OpenSession())
			{
				ICriteria c = s.CreateCriteria(typeof(Item));

				c.Fetch(SelectMode.Skip, "ItemDetails");

				try
				{
					Item i = c.UniqueResult<Item>();

					Assert.AreEqual(2, i.ItemDetails.Count);
				}
				catch (StackOverflowException ex)
				{
					Console.WriteLine(ex.StackTrace);
					Assert.Fail("StackOverflowException thrown when retrieving bag with CompositeId Using Select Fetch" + ex.Message);
				}

			}
		}

		protected override void OnTearDown()
		{
			// Make sure that we delete the stuff we've inserted.
			using (ISession s = this.OpenSession())
			{
				ICriteria c = s.CreateCriteria(typeof(Item));

				c.Fetch(SelectMode.Skip, "ItemDetails");

				Item i = c.UniqueResult<Item>();

				s.Delete(i);

				s.Flush();

			}
		}


	}
}
