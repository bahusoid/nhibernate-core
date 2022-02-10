using System;
using FluentNHibernate.Mapping;

namespace NHibernate.Test.NHSpecificTest.GH3003
{
	public class FooRef1
	{
		public virtual int Id { get; protected set; }
		public virtual string Description { get; protected set; }
	}

	public class FooRef2
	{
		public virtual int Id { get; protected set; }
		public virtual string Description { get; protected set; }
	}

	public class Foo
	{
		public virtual FooRef1 Ref1 { get;  set; }
		public virtual FooRef2 Ref2 { get; set; }

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 13;
				hash = (hash * 7) + (Ref1.Id.GetHashCode());
				hash = (hash * 7) + (Ref2.Id.GetHashCode());
				return hash;
			}
		}

		public override bool Equals(object obj)
		{
			return obj is Foo foo &&
					Ref1.Id == foo.Ref1.Id &&
					Ref2.Id == foo.Ref2.Id;
		}
	}

	public class Bar
	{
		public virtual int Id { get; set; }
		public virtual Foo Foo { get; set; }
	}

	public class FooRef1Map : ClassMap<FooRef1>
	{
		public FooRef1Map()
		{
			Table("FOO_REF_1");
			Id(x => x.Id).Column("REF_1_ID");
			Map(x => x.Description).Column("REF_1_DESC");
			ReadOnly();
		}
	}

	public class FooRef2Map : ClassMap<FooRef2>
	{
		public FooRef2Map()
		{
			Table("FOO_REF_2");
			Id(x => x.Id).Column("REF_2_ID");
			Map(x => x.Description).Column("REF_2_DESC");
			ReadOnly();
		}
	}

	public class FooMap : ClassMap<Foo>
	{
		public FooMap()
		{
			Table("FOO");
			CompositeId()
				.KeyReference(x => x.Ref1, "REF_1_ID")
				.KeyReference(x => x.Ref2, "REF_2_ID");
			ReadOnly();
		}
	}

	public class BarMap : ClassMap<Bar>
	{
		public BarMap()
		{
			Table("BAR");
			Id(x => x.Id).Column("BAR_ID");
			References(x => x.Foo).Columns("REF_1_ID", "REF_2_ID");
			ReadOnly();
		}
	}
}
