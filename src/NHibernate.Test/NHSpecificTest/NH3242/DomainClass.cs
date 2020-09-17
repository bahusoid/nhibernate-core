using System;
using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.NH3242
{
	public class Booking
	{
		public virtual int BookingID { get; set; }
		public virtual BookingTypes Type { get; set; }
		public virtual DateTime DateBooked { get; set; }
	}

	public enum BookingTypes
	{
		User = 1,
		Guest = 2
	}

	public class BookingGuest : Booking
	{
		public virtual string FullName { get; set; }
	}

	public class BookingUser : Booking
	{
		public virtual User User { get; set; }
	}

	public class User
	{
		public virtual int UserID { get; set; }
		public virtual string FullName { get; set; }
		public virtual IList<BookingUser> Bookings { get; set; } = new List<BookingUser>();
	}
}
