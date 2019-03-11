using System;
using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.NH3866
{
	public class EntityA
	{
		public virtual Guid Id { get; set; }
		public virtual IList<EntityB> EntityBs { get; set; }
		public virtual IList<EntityC> EntityCs { get; set; }
	}
}
