namespace NHibernate.Test.NHSpecificTest.NH2851
{
	public class Practitioner
	{
		private int _id = 0;

		public virtual int Id
		{
			get { return _id; }
		}

		public virtual User User { get; set; }
	}
}
