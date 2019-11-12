using System;

namespace NHibernate.Test.StaticProxyTest.InterfaceHandling
{
	public interface ISubEntityProxy : IEntity
	{
		string AnotherName { get; set; }
	}

	public class EntityClassProxy : IEntity
	{
		public virtual Guid Id { get; set; }

		public virtual string Name { get; set; }
	}

	class SubEntityInterfaceProxy : EntityClassProxy, ISubEntityProxy
	{
		public virtual string AnotherName { get; set; }
	}

	public interface IAmbigiousSubEntityProxy : IEntity, IEntity2
	{
		string AnotherName { get; set; }
	}

	class AnotherSubEntityInterfaceProxy : EntityClassProxy, IAmbigiousSubEntityProxy
	{
		public virtual string AnotherName { get; set; }
	}
}
