using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.NH3079
{
	public class PersonCpId
	{
		public int IdA { get; set; }

		public int IdB { get; set; }

		public override bool Equals(object obj)
		{
			if (!(obj is PersonCpId objCpId))
				return false;

			return IdA == objCpId.IdA && IdB == objCpId.IdB;
		}

		public override int GetHashCode()
		{
			return IdA.GetHashCode() ^ IdB.GetHashCode();
		}
	}

	public class Person
	{
		public virtual PersonCpId CpId { get; set; }

		public virtual string Name { get; set; }

		public virtual ICollection<Employment> EmploymentList { get; set; }
	}

	public class EmployerCpId
	{
		public virtual int IdA { get; set; }

		public virtual int IdB { get; set; }

		public override bool Equals(object obj)
		{
			if (!(obj is EmployerCpId objCpId))
				return false;

			return IdA == objCpId.IdA && IdB == objCpId.IdB;
		}

		public override int GetHashCode()
		{
			return IdA.GetHashCode() ^ IdB.GetHashCode();
		}
	}

	public class Employer
	{
		public virtual EmployerCpId CpId { get; set; }

		public virtual string Name { get; set; }

		public virtual ICollection<Employment> EmploymentList { get; set; }
	}

	public class EmploymentCpId
	{
		public virtual int Id { get; set; }

		public virtual Person PersonObj { get; set; }

		public virtual Employer EmployerObj { get; set; }

		public override bool Equals(object obj)
		{
			if (!(obj is EmploymentCpId objCpId))
				return false;

			return Id == objCpId.Id && PersonObj.CpId == objCpId.PersonObj.CpId &&
				EmployerObj.CpId == objCpId.EmployerObj.CpId;
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode() ^ PersonObj.CpId.GetHashCode() ^ EmployerObj.CpId.GetHashCode();
		}
	}

	public class Employment
	{
		public virtual EmploymentCpId CpId { get; set; }

		public virtual string Name { get; set; }
	}

	public class PersonNoComponent
	{
		public virtual int IdA { get; set; }

		public virtual int IdB { get; set; }

		public virtual string Name { get; set; }

		public override bool Equals(object obj)
		{
			if (!(obj is PersonNoComponent objNoComponent))
				return false;

			return IdA == objNoComponent.IdA && IdB == objNoComponent.IdB;
		}

		public override int GetHashCode()
		{
			return IdA.GetHashCode() ^ IdB.GetHashCode();
		}
	}
}
