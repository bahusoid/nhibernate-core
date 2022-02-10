using System;
using System.Linq;
using FluentNHibernate.Cfg;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH3003
{
	[TestFixture]
	public class ByCodeFixture : TestCaseMappingByCode
	{
		private int _fooRef11Id;
		private int _fooRef22Id;

		protected override HbmMapping GetMappings()
		{
			throw new ApplicationException();
		}

		protected override void AddMappings(Configuration configuration)
		{
			//configuration.SetProperty(NHibernate.Cfg.Environment.UseProxyValidator, "false");
			Fluently.Configure(configuration).Mappings(
						m =>
							m.FluentMappings.Add<FooRef1Map>().Add<FooRef2Map>().Add<FooMap>().Add<BarMap>())

					.BuildConfiguration();
		}

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var fooRef11 = new FooRef1();
				var fooRef12 = new FooRef1();
				session.Save(fooRef11);
				session.Save(fooRef12);
				var fooRef21 = new FooRef2();
				var fooRef22 = new FooRef2();
				session.Save(fooRef21);
				session.Save(fooRef22);

				var foo = new Foo() { Ref1 = fooRef11, Ref2 = fooRef22 };

				session.Save(foo);
				var bar = new Bar() { Foo = foo };
				session.Save(bar);
				_fooRef11Id = fooRef11.Id;
				_fooRef22Id = fooRef22.Id;
				transaction.Commit();

			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Delete("from Bar");
				session.Delete("from Foo");
				session.Delete("from System.Object");

				transaction.Commit();
			}
		}

		[Test]
		public void AccessComposedIdMultipleTimes()
		{
			using (var session = OpenSession())
			{
				var result = session.Query<Bar>().FirstOrDefault();

				Assert.That(result.Foo.Ref1.Id, Is.EqualTo(_fooRef11Id));
				Assert.That(result.Foo.Ref1.Id, Is.EqualTo(_fooRef11Id));
				Assert.That(result.Foo.Ref2.Id, Is.EqualTo(_fooRef22Id));
				Assert.That(result.Foo.Ref2.Id, Is.EqualTo(_fooRef22Id));
			}
		}
	}
}
