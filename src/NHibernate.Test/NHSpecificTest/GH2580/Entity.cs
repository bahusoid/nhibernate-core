using System.Collections.Generic;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace NHibernate.Test.NHSpecificTest.GH2580
{
	using Cascade = NHibernate.Mapping.ByCode.Cascade;

	public class FamilyEntity
	{
		public virtual long Id { get; set; }
		public virtual ICollection<PersonEntity> Persons { get; set; }
	}

	public class FamilyMap : ClassMapping<FamilyEntity>
	{
		public FamilyMap()
		{
			Table("Families");

			Id(
				p => p.Id,
				id =>
				{
					id.Column("Family_Id");
					id.Generator(Generators.Increment);
				});

			Bag(
				p => p.Persons,
				bag =>
				{
					bag.Inverse(true);
					bag.Cascade(Cascade.All.Include(Cascade.DeleteOrphans));
					bag.Lazy(CollectionLazy.NoLazy);
					bag.Key(k => k.Column("Family_Id"));
				},
				a => a.OneToMany());
		}
	}

	public class PersonEntity
	{
		public virtual long Id { get; set; }

		private FamilyEntity _family;

		public virtual FamilyEntity Family
		{
			get { return _family; }
			set
			{
				_family?.Persons.Remove(this);
				_family = value;
				_family?.Persons.Add(this);
			}
		}
	}

	public class PersonMap : ClassMapping<PersonEntity>
	{
		public PersonMap()
		{
			Table("Persons");

			Id(
				p => p.Id,
				id =>
				{
					id.Column("Person_Id");
					id.Generator(Generators.Increment);
				});

			ManyToOne(
				p => p.Family,
				r =>
				{
					r.Lazy(LazyRelation.NoLazy);
					r.Access(Accessor.Field);
					r.Column("Family_Id");
					r.NotNullable(true);
				});
		}
	}

	public class AdultEntity : PersonEntity
	{
	}

	public class AdultMap : JoinedSubclassMapping<AdultEntity>
	{
		public AdultMap()
		{
			Table("Adults");
			Key(prop => prop.Column("Person_Id"));
		}
	}

	public class ChildEntity : PersonEntity
	{
	}

	public class ChildMap : JoinedSubclassMapping<ChildEntity>
	{
		public ChildMap()
		{
			Table("Children");
			Key(prop => prop.Column("Person_Id"));
		}
	}
}
