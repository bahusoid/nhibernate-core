namespace NHibernate.Test.NHSpecificTest.NH2851
{
	public class User
	{
		private int _userID = 0;

		public virtual int UserID
		{
			get { return _userID; }
		}

		public virtual string UserName { get; set; }

		public virtual Practitioner Practitioner { get; protected set; }
	}
}
