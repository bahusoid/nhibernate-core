using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Dialect;
using NHibernate.Linq;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3242
{
	[TestFixture]
	public class SampleTest : BugTestCase
	{
		protected override void OnSetUp()
		{
			base.OnSetUp();
			using (ISession session = this.OpenSession())
			{
				var user = new User() {FullName = "Bob Smith"};
				session.Save(user);

				var booking = new BookingUser() {Type = BookingTypes.User, DateBooked = DateTime.UtcNow.AddDays(-1), User = user};
				session.Save(booking);

				session.Flush();
			}
		}

		protected override void OnTearDown()
		{
			base.OnTearDown();
			using (ISession session = this.OpenSession())
			{
				string hql = "from System.Object";
				session.Delete(hql);
				session.Flush();
			}
		}

		protected override bool AppliesTo(NHibernate.Dialect.Dialect dialect)
		{
			return dialect as MsSql2005Dialect != null;
		}

		[Test]
		public void JoinOnBasePropertyShouldWork()
		{
			using (new SqlLogSpy())
			using (ISession session = this.OpenSession())
			{
				var users = session.Query<User>().Where(u => u.Bookings.Count(b => b.DateBooked <= DateTime.UtcNow) == 1).ToList();

				Assert.AreEqual(1, users.Count);
			}
		}
	}
}
