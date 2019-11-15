﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections;
using System.Linq;
using System.Threading;
using NHibernate.Dialect;
using NHibernate.DomainModel;
using NHibernate.Hql.Ast.ANTLR;
using NHibernate.Linq;
using NHibernate.Test.LinqBulkManipulation.Domain;
using NUnit.Framework;

namespace NHibernate.Test.LinqBulkManipulation
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : TestCase
	{
		protected override string[] Mappings => Array.Empty<string>();

		protected override void Configure(Cfg.Configuration configuration)
		{
			var type = typeof(FixtureAsync);
			var assembly = type.Assembly;
			var mappingNamespace = type.Namespace;
			foreach (var resource in assembly.GetManifestResourceNames())
			{
				if (resource.StartsWith(mappingNamespace) && resource.EndsWith(".hbm.xml"))
				{
					configuration.AddResource(resource, assembly);
				}
			}
		}

		private Animal _polliwog;
		private Animal _catepillar;
		private Animal _frog;
		private Animal _butterfly;
		private Zoo _zoo;
		private Zoo _pettingZoo;
		private Human _joe;
		private Human _doll;
		private Human _stevee;
		private IntegerVersioned _intVersioned;
		private TimestampVersioned _timeVersioned;

		protected override void OnSetUp()
		{
			using (var s = OpenSession())
			using (var txn = s.BeginTransaction())
			{
				_polliwog = new Animal { BodyWeight = 12, Description = "Polliwog" };
				_catepillar = new Animal { BodyWeight = 10, Description = "Catepillar" };
				_frog = new Animal { BodyWeight = 34, Description = "Frog" };
				_butterfly = new Animal { BodyWeight = 9, Description = "Butterfly" };

				_polliwog.Father = _frog;
				_frog.AddOffspring(_polliwog);
				_catepillar.Mother = _butterfly;
				_butterfly.AddOffspring(_catepillar);

				s.Save(_frog);
				s.Save(_polliwog);
				s.Save(_butterfly);
				s.Save(_catepillar);

				var dog = new Dog { BodyWeight = 200, Description = "dog" };
				s.Save(dog);
				var cat = new Cat { BodyWeight = 100, Description = "cat" };
				s.Save(cat);

				var dragon = new Dragon();
				dragon.SetFireTemperature(200);
				s.Save(dragon);

				_zoo = new Zoo { Name = "Zoo" };
				var add = new Address { City = "MEL", Country = "AU", Street = "Main st", PostalCode = "3000" };
				_zoo.Address = add;

				_pettingZoo = new PettingZoo { Name = "Petting Zoo" };
				var addr = new Address { City = "Sydney", Country = "AU", Street = "High st", PostalCode = "2000" };
				_pettingZoo.Address = addr;

				s.Save(_zoo);
				s.Save(_pettingZoo);

				var joiner = new Joiner { JoinedName = "joined-name", Name = "name" };
				s.Save(joiner);

				var car = new Car { Vin = "123c", Owner = "Kirsten" };
				s.Save(car);
				var truck = new Truck { Vin = "123t", Owner = "Steve" };
				s.Save(truck);
				var suv = new SUV { Vin = "123s", Owner = "Joe" };
				s.Save(suv);
				var pickup = new Pickup { Vin = "123p", Owner = "Cecelia" };
				s.Save(pickup);

				var entCompKey = new EntityWithCrazyCompositeKey { Id = new CrazyCompositeKey { Id = 1, OtherId = 1 }, Name = "Parent" };
				s.Save(entCompKey);

				_joe = new Human { Name = new Name { First = "Joe", Initial = 'Q', Last = "Public" } };
				_doll = new Human { Name = new Name { First = "Kyu", Initial = 'P', Last = "Doll" }, Friends = new[] { _joe } };
				_stevee = new Human { Name = new Name { First = "Stevee", Initial = 'X', Last = "Ebersole" } };
				s.Save(_joe);
				s.Save(_doll);
				s.Save(_stevee);

				_intVersioned = new IntegerVersioned { Name = "int-vers", Data = "foo" };
				s.Save(_intVersioned);

				_timeVersioned = new TimestampVersioned { Name = "ts-vers", Data = "foo" };
				s.Save(_timeVersioned);

				var scwc = new SimpleClassWithComponent { Name = new Name { First = "Stevee", Initial = 'X', Last = "Ebersole" } };
				s.Save(scwc);

				var mainEntWithAssoc = new SimpleEntityWithAssociation() { Name = "main" };
				var otherEntWithAssoc = new SimpleEntityWithAssociation() { Name = "many-to-many-association" };
				mainEntWithAssoc.ManyToManyAssociatedEntities.Add(otherEntWithAssoc);
				mainEntWithAssoc.AddAssociation("one-to-many-association");
				s.Save(mainEntWithAssoc);

				var owner = new SimpleEntityWithAssociation { Name = "myEntity-1" };
				owner.AddAssociation("assoc-1");
				owner.AddAssociation("assoc-2");
				owner.AddAssociation("assoc-3");
				s.Save(owner);
				var owner2 = new SimpleEntityWithAssociation { Name = "myEntity-2" };
				owner2.AddAssociation("assoc-1");
				owner2.AddAssociation("assoc-2");
				owner2.AddAssociation("assoc-3");
				owner2.AddAssociation("assoc-4");
				s.Save(owner2);
				var owner3 = new SimpleEntityWithAssociation { Name = "myEntity-3" };
				s.Save(owner3);

				txn.Commit();
			}
		}

		protected override void OnTearDown()
		{
			if (!Dialect.SupportsTemporaryTables)
			{
				// Give-up usual cleanup due to TPC: cannot perform multi-table deletes using dialect not supporting temp tables
				DropSchema();
				CreateSchema();
				return;
			}

			using (var s = OpenSession())
			using (var txn = s.BeginTransaction())
			{
				// workaround FK
				var doll = s.Query<Human>().SingleOrDefault(h => h.Id == _doll.Id);
				if (doll != null)
					s.Delete(doll);
				var entity = s.Query<SimpleEntityWithAssociation>().SingleOrDefault(e => e.ManyToManyAssociatedEntities.Any());
				if (entity != null)
				{
					s.Delete(entity.ManyToManyAssociatedEntities.First());
					s.Delete(entity);
				}
				s.Flush();
				s.CreateQuery("delete from Animal where Mother is not null or Father is not null").ExecuteUpdate();

				s.CreateQuery("delete from Animal").ExecuteUpdate();
				s.CreateQuery("delete from Zoo").ExecuteUpdate();
				s.CreateQuery("delete from Joiner").ExecuteUpdate();
				s.CreateQuery("delete from Vehicle").ExecuteUpdate();
				s.CreateQuery("delete EntityReferencingEntityWithCrazyCompositeKey").ExecuteUpdate();
				s.CreateQuery("delete EntityWithCrazyCompositeKey").ExecuteUpdate();
				s.CreateQuery("delete IntegerVersioned").ExecuteUpdate();
				s.CreateQuery("delete TimestampVersioned").ExecuteUpdate();
				s.CreateQuery("delete SimpleClassWithComponent").ExecuteUpdate();
				s.CreateQuery("delete SimpleAssociatedEntity").ExecuteUpdate();
				s.CreateQuery("delete SimpleEntityWithAssociation").ExecuteUpdate();

				txn.Commit();
			}
		}

		#region INSERTS

		[Test]
		public async Task SimpleInsertAsync()
		{
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count = await (s.Query<Car>().InsertIntoAsync(x => new Pickup { Id = -x.Id, Vin = x.Vin, Owner = x.Owner }));
				Assert.AreEqual(1, count);

				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task SimpleAnonymousInsertAsync()
		{
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count = await (s.Query<Car>().InsertIntoAsync<Car, Pickup>(x => new { Id = -x.Id, x.Vin, x.Owner }));
				Assert.AreEqual(1, count);

				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task SimpleInsertFromAggregateAsync()
		{
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count = await (s
					.Query<Car>()
					.GroupBy(x => x.Id)
					.Select(x => new { Id = x.Key, Vin = x.Max(y => y.Vin), Owner = x.Max(y => y.Owner) })
					.InsertIntoAsync(x => new Pickup { Id = -x.Id, Vin = x.Vin, Owner = x.Owner }));
				Assert.AreEqual(1, count);

				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task SimpleInsertFromLimitedAsync()
		{
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count = await (s
					.Query<Vehicle>()
					.Skip(1)
					.Take(1)
					.InsertIntoAsync(x => new Pickup { Id = -x.Id, Vin = x.Vin, Owner = x.Owner }));
				Assert.AreEqual(1, count);

				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task SimpleInsertWithConstantsAsync()
		{
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count = await (s
					.Query<Car>()
					.InsertBuilder().Into<Pickup>().Value(y => y.Id, y => -y.Id).Value(y => y.Vin, y => y.Vin).Value(y => y.Owner, "The owner")
					.InsertAsync());
				Assert.AreEqual(1, count);

				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task SimpleInsertFromProjectionAsync()
		{
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count = await (s
					.Query<Car>()
					.Select(x => new { x.Id, x.Owner, UpperOwner = x.Owner.ToUpper() })
					.InsertBuilder().Into<Pickup>().Value(y => y.Id, y => -y.Id).Value(y => y.Vin, y => y.UpperOwner)
					.InsertAsync());
				Assert.AreEqual(1, count);

				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task InsertWithClientSideRequirementsThrowsExceptionAsync()
		{
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				Assert.ThrowsAsync<NotSupportedException>(
					() => s
						.Query<Car>()
						.InsertIntoAsync(x => new Pickup { Id = -x.Id, Vin = x.Vin, Owner = x.Owner.PadRight(200) }));

				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task InsertWithManyToOneAsync()
		{
			CheckSupportOfBulkInsertionWithGeneratedId<Animal>();

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count = await (s
					.Query<Human>()
					.InsertIntoAsync(x => new Animal { Description = x.Description, BodyWeight = x.BodyWeight, Mother = x.Mother }));
				Assert.AreEqual(3, count);

				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task InsertWithManyToOneAsParameterAsync()
		{
			CheckSupportOfBulkInsertionWithGeneratedId<Animal>();

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count = await (s
					.Query<Human>()
					.InsertIntoAsync(x => new Animal { Description = x.Description, BodyWeight = x.BodyWeight, Mother = _butterfly }));
				Assert.AreEqual(3, count);

				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task InsertWithManyToOneWithCompositeKeyAsync()
		{
			CheckSupportOfBulkInsertionWithGeneratedId<EntityWithCrazyCompositeKey>();

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count = await (s
					.Query<EntityWithCrazyCompositeKey>()
					.InsertIntoAsync(x => new EntityReferencingEntityWithCrazyCompositeKey { Name = "Child", Parent = x }));
				Assert.AreEqual(1, count);

				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task InsertIntoSuperclassPropertiesFailsAsync()
		{
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				Assert.ThrowsAsync<QueryException>(
					() => s.Query<Lizard>().InsertIntoAsync(x => new Human { Id = -x.Id, BodyWeight = x.BodyWeight }),
					"superclass prop insertion did not error");

				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task InsertAcrossMappedJoinFailsAsync()
		{
			CheckSupportOfBulkInsertionWithGeneratedId<Joiner>();

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				Assert.ThrowsAsync<QueryException>(
					() => s.Query<Car>().InsertIntoAsync(x => new Joiner { Name = x.Vin, JoinedName = x.Owner }),
					"mapped-join insertion did not error");

				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task InsertWithGeneratedIdAsync()
		{
			CheckSupportOfBulkInsertionWithGeneratedId<PettingZoo>();

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count = await (s.Query<Zoo>().Where(z => z.Id == _zoo.Id).InsertIntoAsync(x => new PettingZoo { Name = x.Name }));
				Assert.That(count, Is.EqualTo(1), "unexpected insertion count");
				await (t.CommitAsync());
			}

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var pz = await (s.Query<PettingZoo>().SingleAsync(z => z.Name == _zoo.Name));
				await (t.CommitAsync());

				Assert.That(_zoo.Id != pz.Id);
			}
		}

		[Test]
		public async Task InsertWithGeneratedVersionAndIdAsync()
		{
			CheckSupportOfBulkInsertionWithGeneratedId<IntegerVersioned>();

			var initialId = _intVersioned.Id;
			var initialVersion = _intVersioned.Version;

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count = await (s
					.Query<IntegerVersioned>()
					.Where(x => x.Id == initialId)
					.InsertIntoAsync(x => new IntegerVersioned { Name = x.Name, Data = x.Data }));
				Assert.That(count, Is.EqualTo(1), "unexpected insertion count");
				await (t.CommitAsync());
			}

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var created = await (s.Query<IntegerVersioned>().SingleAsync(iv => iv.Id != initialId));
				Assert.That(created.Version, Is.EqualTo(initialVersion), "version was not seeded");
				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task InsertWithGeneratedTimestampVersionAsync()
		{
			CheckSupportOfBulkInsertionWithGeneratedId<TimestampVersioned>();

			var initialId = _timeVersioned.Id;

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count = await (s
					.Query<TimestampVersioned>()
					.Where(x => x.Id == initialId)
					.InsertIntoAsync(x => new TimestampVersioned { Name = x.Name, Data = x.Data }));
				Assert.That(count, Is.EqualTo(1), "unexpected insertion count");

				await (t.CommitAsync());
			}

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var created = await (s.Query<TimestampVersioned>().SingleAsync(tv => tv.Id != initialId));
				Assert.That(created.Version, Is.GreaterThan(DateTime.Today));
				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task InsertWithSelectListUsingJoinsAsync()
		{
			CheckSupportOfBulkInsertionWithGeneratedId<Animal>();

			// this is just checking parsing and syntax...
			using (var s = OpenSession())
			{
				s.BeginTransaction();

				Assert.DoesNotThrowAsync(() =>
				{
					return s
						.Query<Human>().Where(x => x.Mother.Mother != null)
						.InsertIntoAsync(x => new Animal { Description = x.Description, BodyWeight = x.BodyWeight });
				});

				await (s.Transaction.CommitAsync());
			}
		}

		[Test]
		public async Task InsertToComponentAsync()
		{
			CheckSupportOfBulkInsertionWithGeneratedId<SimpleClassWithComponent>();

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				const string correctName = "Steve";

				var count = await (s
					.Query<SimpleClassWithComponent>()
					// Avoid Firebird unstable cursor bug by filtering.
					// https://firebirdsql.org/file/documentation/reference_manuals/fblangref25-en/html/fblangref25-dml-insert.html#fblangref25-dml-insert-select-unstable
					.Where(sc => sc.Name.First != correctName)
					.InsertBuilder().Into<SimpleClassWithComponent>().Value(y => y.Name.First, y => correctName)
					.InsertAsync());
				Assert.That(count, Is.EqualTo(1), "incorrect insert count from individual setters");

				count = await (s
					.Query<SimpleClassWithComponent>()
					.Where(x => x.Name.First == correctName && x.Name.Initial != 'Z')
					.InsertIntoAsync(x => new SimpleClassWithComponent { Name = new Name { First = x.Name.First, Last = x.Name.Last, Initial = 'Z' } }));
				Assert.That(count, Is.EqualTo(1), "incorrect insert from non anonymous selector");

				count = await (s
					.Query<SimpleClassWithComponent>()
					.Where(x => x.Name.First == correctName && x.Name.Initial == 'Z')
					.InsertIntoAsync<SimpleClassWithComponent, SimpleClassWithComponent>(x => new { Name = new { x.Name.First, x.Name.Last, Initial = 'W' } }));
				Assert.That(count, Is.EqualTo(1), "incorrect insert from anonymous selector");

				count = await (s
					.Query<SimpleClassWithComponent>()
					.Where(x => x.Name.First == correctName && x.Name.Initial == 'Z')
					.InsertIntoAsync<SimpleClassWithComponent, SimpleClassWithComponent>(x => new { Name = new Name { First = x.Name.First, Last = x.Name.Last, Initial = 'V' } }));
				Assert.That(count, Is.EqualTo(1), "incorrect insert from hybrid selector");
				await (t.CommitAsync());
			}
		}

		private void CheckSupportOfBulkInsertionWithGeneratedId<T>()
		{
			// Make sure the env supports bulk inserts with generated ids...
			var persister = Sfi.GetEntityPersister(typeof(T).FullName);
			var generator = persister.IdentifierGenerator;
			if (!HqlSqlWalker.SupportsIdGenWithBulkInsertion(generator))
			{
				Assert.Ignore($"Identifier generator {generator.GetType().Name} for entity {typeof(T).FullName} does not support bulk insertions.");
			}
		}

		#endregion

		#region UPDATES

		[Test]
		public async Task SimpleUpdateAsync()
		{
			using (var s = OpenSession())
			using (s.BeginTransaction())
			{
				var count = await (s
					.Query<Car>()
					.UpdateAsync(a => new Car { Owner = a.Owner + " a" }));
				Assert.AreEqual(1, count);
			}
		}

		[Test]
		public async Task SimpleAnonymousUpdateAsync()
		{
			using (var s = OpenSession())
			using (s.BeginTransaction())
			{
				var count = await (s
					.Query<Car>()
					.UpdateAsync(a => new { Owner = a.Owner + " a" }));
				Assert.AreEqual(1, count);
			}
		}

		[Test]
		public async Task UpdateWithWhereExistsSubqueryAsync()
		{
			if (!Dialect.SupportsTemporaryTables)
			{
				Assert.Ignore("Cannot perform multi-table updates using dialect not supporting temp tables.");
			}

			// multi-table ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count = await (s
					.Query<Human>()
					.Where(x => x.Friends.OfType<Human>().Any(f => f.Name.Last == "Public"))
					.UpdateBuilder().Set(y => y.Description, "updated")
					.UpdateAsync());
				Assert.That(count, Is.EqualTo(1));
				await (t.CommitAsync());
			}

			// single-table (one-to-many & many-to-many) ~~~~~~~~~~~~~~~~~~~~~~~~~~
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				// one-to-many test
				var count = await (s
					.Query<SimpleEntityWithAssociation>()
					.Where(x => x.AssociatedEntities.Any(a => a.Name == "one-to-many-association"))
					.UpdateBuilder().Set(y => y.Name, "updated")
					.UpdateAsync());
				Assert.That(count, Is.EqualTo(1));
				// many-to-many test
				if (Dialect.SupportsSubqueryOnMutatingTable)
				{
					count = await (s
						.Query<SimpleEntityWithAssociation>()
						.Where(x => x.ManyToManyAssociatedEntities.Any(a => a.Name == "many-to-many-association"))
						.UpdateBuilder().Set(y => y.Name, "updated")
						.UpdateAsync());

					Assert.That(count, Is.EqualTo(1));
				}
				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task IncrementCounterVersionAsync()
		{
			var initialVersion = _intVersioned.Version;

			using (var s = OpenSession())
			{
				using (var t = s.BeginTransaction())
				{
					// Note: Update more than one column to showcase NH-3624, which involved losing some columns. /2014-07-26
					var count = await (s
						.Query<IntegerVersioned>()
						.UpdateBuilder().Set(y => y.Name, y => y.Name + "upd").Set(y => y.Data, y => y.Data + "upd")
						.UpdateVersionedAsync());
					Assert.That(count, Is.EqualTo(1), "incorrect exec count");
					await (t.CommitAsync());
				}

				using (var t = s.BeginTransaction())
				{
					var entity = await (s.GetAsync<IntegerVersioned>(_intVersioned.Id));
					Assert.That(entity.Version, Is.EqualTo(initialVersion + 1), "version not incremented");
					Assert.That(entity.Name, Is.EqualTo("int-versupd"));
					Assert.That(entity.Data, Is.EqualTo("fooupd"));
					await (t.CommitAsync());
				}
			}
		}

		[Test]
		public async Task IncrementTimestampVersionAsync()
		{
			var initialVersion = _timeVersioned.Version;

			await (Task.Delay(1300));

			using (var s = OpenSession())
			{
				using (var t = s.BeginTransaction())
				{
					// Note: Update more than one column to showcase NH-3624, which involved losing some columns. /2014-07-26
					var count = await (s
						.Query<TimestampVersioned>()
						.UpdateBuilder().Set(y => y.Name, y => y.Name + "upd").Set(y => y.Data, y => y.Data + "upd")
						.UpdateVersionedAsync());
					Assert.That(count, Is.EqualTo(1), "incorrect exec count");
					await (t.CommitAsync());
				}

				using (var t = s.BeginTransaction())
				{
					var entity = await (s.LoadAsync<TimestampVersioned>(_timeVersioned.Id));
					Assert.That(entity.Version, Is.GreaterThan(initialVersion), "version not incremented");
					Assert.That(entity.Name, Is.EqualTo("ts-versupd"));
					Assert.That(entity.Data, Is.EqualTo("fooupd"));
					await (t.CommitAsync());
				}
			}
		}

		[Test]
		public async Task UpdateOnComponentAsync()
		{
			if (!Dialect.SupportsTemporaryTables)
			{
				Assert.Ignore("Cannot perform multi-table updates using dialect not supporting temp tables.");
			}

			using (var s = OpenSession())
			{
				const string correctName = "Steve";

				using (var t = s.BeginTransaction())
				{
					var count =
						await (s.Query<Human>().Where(x => x.Id == _stevee.Id).UpdateAsync(x => new Human { Name = { First = correctName } }));

					Assert.That(count, Is.EqualTo(1), "incorrect update count");
					await (t.CommitAsync());
				}

				using (var t = s.BeginTransaction())
				{
					await (s.RefreshAsync(_stevee));

					Assert.That(_stevee.Name.First, Is.EqualTo(correctName), "Update did not execute properly");

					await (t.CommitAsync());
				}
			}
		}

		[Test]
		public async Task UpdateWithClientSideRequirementsThrowsExceptionAsync()
		{
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				Assert.ThrowsAsync<NotSupportedException>(
					() => s.Query<Human>().Where(x => x.Id == _stevee.Id).UpdateAsync(x => new Human { Name = { First = x.Name.First.PadLeft(200) } })
				);

				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task UpdateOnManyToOneAsync()
		{
			if (!Dialect.SupportsTemporaryTables)
			{
				Assert.Ignore("Cannot perform multi-table updates using dialect not supporting temp tables.");
			}

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				Assert.DoesNotThrowAsync(() => { return s.Query<Animal>().Where(x => x.Id == 2).UpdateBuilder().Set(y => y.Mother, y => null).UpdateAsync(); });

				if (Dialect.SupportsSubqueryOnMutatingTable)
				{
					Assert.DoesNotThrowAsync(
						() => { return s.Query<Animal>().Where(x => x.Id == 2).UpdateBuilder().Set(y => y.Mother, y => s.Query<Animal>().First(z => z.Id == 1)).UpdateAsync(); });
				}

				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task UpdateOnDiscriminatorSubclassAsync()
		{
			using (var s = OpenSession())
			{
				using (var t = s.BeginTransaction())
				{
					var count = await (s.Query<PettingZoo>().UpdateBuilder().Set(y => y.Name, y => y.Name).UpdateAsync());
					Assert.That(count, Is.EqualTo(1), "Incorrect discrim subclass update count");

					await (t.RollbackAsync());
				}
				using (var t = s.BeginTransaction())
				{
					var count = await (s.Query<PettingZoo>().Where(x => x.Id == _pettingZoo.Id).UpdateBuilder().Set(y => y.Name, y => y.Name).UpdateAsync());
					Assert.That(count, Is.EqualTo(1), "Incorrect discrim subclass update count");

					await (t.RollbackAsync());
				}
				using (var t = s.BeginTransaction())
				{
					var count = await (s.Query<Zoo>().UpdateBuilder().Set(y => y.Name, y => y.Name).UpdateAsync());
					Assert.That(count, Is.EqualTo(2), "Incorrect discrim subclass update count");

					await (t.RollbackAsync());
				}
				using (var t = s.BeginTransaction())
				{
					// TODO : not so sure this should be allowed.  Seems to me that if they specify an alias,
					// property-refs should be required to be qualified.
					var count = await (s.Query<Zoo>().Where(x => x.Id == _zoo.Id).UpdateBuilder().Set(y => y.Name, y => y.Name).UpdateAsync());
					Assert.That(count, Is.EqualTo(1), "Incorrect discrim subclass update count");

					await (t.CommitAsync());
				}
			}
		}

		[Test]
		public async Task UpdateOnAnimalAsync()
		{
			if (!Dialect.SupportsTemporaryTables)
			{
				Assert.Ignore("Cannot perform multi-table updates using dialect not supporting temp tables.");
			}

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				//var count = s.Query<Animal>().Where(x => x.Description == data.Frog.Description).Update().Set(y => y.Description, y => y.Description));
				//Assert.That(count, Is.EqualTo(1), "Incorrect entity-updated count");

				var count =
					await (s.Query<Animal>().Where(x => x.Description == _polliwog.Description).UpdateBuilder().Set(y => y.Description, y => "Tadpole").UpdateAsync());
				Assert.That(count, Is.EqualTo(1), "Incorrect entity-updated count");

				var tadpole = await (s.LoadAsync<Animal>(_polliwog.Id));

				Assert.That(tadpole.Description, Is.EqualTo("Tadpole"), "Update did not take effect");

				count =
					await (s.Query<Dragon>().UpdateBuilder().Set(y => y.FireTemperature, 300).UpdateAsync());
				Assert.That(count, Is.EqualTo(1), "Incorrect entity-updated count");

				count =
					await (s.Query<Animal>().UpdateBuilder().Set(y => y.BodyWeight, y => y.BodyWeight + 1 + 1).UpdateAsync());
				Assert.That(count, Is.EqualTo(10), "incorrect count on 'complex' update assignment");

				if (Dialect.SupportsSubqueryOnMutatingTable)
				{
					Assert.DoesNotThrowAsync(() => { return s.Query<Animal>().UpdateBuilder().Set(y => y.BodyWeight, y => s.Query<Animal>().Max(z => z.BodyWeight)).UpdateAsync(); });
				}

				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task UpdateOnDragonWithProtectedPropertyAsync()
		{
			if (!Dialect.SupportsTemporaryTables)
			{
				Assert.Ignore("Cannot perform multi-table updates using dialect not supporting temp tables.");
			}

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count =
					await (s.Query<Dragon>().UpdateBuilder().Set(y => y.FireTemperature, 300).UpdateAsync());
				Assert.That(count, Is.EqualTo(1), "Incorrect entity-updated count");

				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task UpdateMultiplePropertyOnAnimalAsync()
		{
			if (!Dialect.SupportsTemporaryTables)
			{
				Assert.Ignore("Cannot perform multi-table updates using dialect not supporting temp tables.");
			}

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count =
					await (s.Query<Animal>()
					 .Where(x => x.Description == _polliwog.Description)
					 .UpdateBuilder().Set(y => y.Description, y => "Tadpole").Set(y => y.BodyWeight, 3).UpdateAsync());

				Assert.That(count, Is.EqualTo(1));
				await (t.CommitAsync());
			}

			using (var s = OpenSession())
			using (s.BeginTransaction())
			{
				var tadpole = await (s.GetAsync<Animal>(_polliwog.Id));
				Assert.That(tadpole.Description, Is.EqualTo("Tadpole"));
				Assert.That(tadpole.BodyWeight, Is.EqualTo(3));
			}
		}

		[Test]
		public async Task UpdateOnMammalAsync()
		{
			if (!Dialect.SupportsTemporaryTables)
			{
				Assert.Ignore("Cannot perform multi-table updates using dialect not supporting temp tables.");
			}

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count = await (s.Query<Mammal>().UpdateBuilder().Set(y => y.Description, y => y.Description).UpdateAsync());

				Assert.That(count, Is.EqualTo(5), "incorrect update count against 'middle' of joined-subclass hierarchy");

				count = await (s.Query<Mammal>().UpdateBuilder().Set(y => y.BodyWeight, 25).UpdateAsync());
				Assert.That(count, Is.EqualTo(5), "incorrect update count against 'middle' of joined-subclass hierarchy");

				if (Dialect.SupportsSubqueryOnMutatingTable)
				{
					count = await (s.Query<Mammal>().UpdateBuilder().Set(y => y.BodyWeight, y => s.Query<Animal>().Max(z => z.BodyWeight)).UpdateAsync());
					Assert.That(count, Is.EqualTo(5), "incorrect update count against 'middle' of joined-subclass hierarchy");
				}

				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task UpdateSetNullUnionSubclassAsync()
		{
			if (!Dialect.SupportsTemporaryTables)
			{
				Assert.Ignore("Cannot perform multi-table updates using dialect not supporting temp tables.");
			}

			// These should reach out into *all* subclass tables...
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count = await (s.Query<Vehicle>().UpdateBuilder().Set(y => y.Owner, "Steve").UpdateAsync());
				Assert.That(count, Is.EqualTo(4), "incorrect restricted update count");
				count = await (s.Query<Vehicle>().Where(x => x.Owner == "Steve").UpdateBuilder().Set(y => y.Owner, default(string)).UpdateAsync());
				Assert.That(count, Is.EqualTo(4), "incorrect restricted update count");

				count = await (s.CreateQuery("delete Vehicle where Owner is null").ExecuteUpdateAsync());
				Assert.That(count, Is.EqualTo(4), "incorrect restricted update count");

				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task UpdateSetNullOnDiscriminatorSubclassAsync()
		{
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count = await (s.Query<PettingZoo>().UpdateBuilder().Set(y => y.Address.City, default(string)).UpdateAsync());

				Assert.That(count, Is.EqualTo(1), "Incorrect discrim subclass delete count");
				count = await (s.CreateQuery("delete Zoo where Address.City is null").ExecuteUpdateAsync());
				Assert.That(count, Is.EqualTo(1), "Incorrect discrim subclass delete count");

				count = await (s.Query<Zoo>().UpdateBuilder().Set(y => y.Address.City, default(string)).UpdateAsync());
				Assert.That(count, Is.EqualTo(1), "Incorrect discrim subclass delete count");
				count = await (s.CreateQuery("delete Zoo where Address.City is null").ExecuteUpdateAsync());
				Assert.That(count, Is.EqualTo(1), "Incorrect discrim subclass delete count");

				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task UpdateSetNullOnJoinedSubclassAsync()
		{
			if (!Dialect.SupportsTemporaryTables)
			{
				Assert.Ignore("Cannot perform multi-table updates using dialect not supporting temp tables.");
			}

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count = await (s.Query<Mammal>().UpdateBuilder().Set(y => y.BodyWeight, -1).UpdateAsync());
				Assert.That(count, Is.EqualTo(5), "Incorrect update count on joined subclass");

				count = await (s.Query<Mammal>().CountAsync(m => m.BodyWeight > -1.0001 && m.BodyWeight < -0.9999));
				Assert.That(count, Is.EqualTo(5), "Incorrect body weight count");

				await (t.CommitAsync());
			}
		}

		[Test]
		public void UpdateOnOtherClassThrowsAsync()
		{
			using (var s = OpenSession())
			using (s.BeginTransaction())
			{
				var query = s
					.Query<Animal>().Where(x => x.Mother == _butterfly);
				Assert.That(() => query.UpdateAsync(a => new Human { Description = a.Description + " humanized" }), Throws.TypeOf<TypeMismatchException>());
			}
		}

		#endregion

		#region DELETES

		[Test]
		public async Task DeleteWithSubqueryAsync()
		{
			if (Dialect is MsSqlCeDialect)
			{
				Assert.Ignore("Test failing on Ms SQL CE.");
			}

			using (var s = OpenSession())
			{
				s.BeginTransaction();
				var count = await (s.Query<SimpleEntityWithAssociation>().Where(x => x.AssociatedEntities.Count == 0 && x.Name.Contains("myEntity")).DeleteAsync());
				Assert.That(count, Is.EqualTo(1), "Incorrect delete count");
				await (s.Transaction.CommitAsync());
			}
		}

		[Test]
		public async Task SimpleDeleteOnAnimalAsync()
		{
			if (Dialect.HasSelfReferentialForeignKeyBug)
			{
				Assert.Ignore("Self referential FK bug");
			}
			if (!Dialect.SupportsTemporaryTables)
			{
				Assert.Ignore("Cannot perform multi-table deletes using dialect not supporting temp tables.");
			}

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				// Get rid of FK which may fail the test
				_doll.Friends = Array.Empty<Human>();
				await (s.UpdateAsync(_doll));
				await (t.CommitAsync());
			}

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count = await (s.Query<Animal>().Where(x => x.Id == _polliwog.Id).DeleteAsync());
				Assert.That(count, Is.EqualTo(1), "Incorrect delete count");

				count = await (s.Query<Animal>().Where(x => x.Id == _catepillar.Id).DeleteAsync());
				Assert.That(count, Is.EqualTo(1), "Incorrect delete count");

				if (Dialect.SupportsSubqueryOnMutatingTable)
				{
					count = await (s.Query<User>().Where(x => s.Query<User>().Contains(x)).DeleteAsync());
					Assert.That(count, Is.EqualTo(0));
				}

				count = await (s.Query<Animal>().DeleteAsync());
				Assert.That(count, Is.EqualTo(8), "Incorrect delete count");

				IList list = await (s.Query<Animal>().ToListAsync());
				Assert.That(list, Is.Empty, "table not empty");

				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task DeleteOnDiscriminatorSubclassAsync()
		{
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count = await (s.Query<PettingZoo>().DeleteAsync());
				Assert.That(count, Is.EqualTo(1), "Incorrect discrim subclass delete count");

				count = await (s.Query<Zoo>().DeleteAsync());
				Assert.That(count, Is.EqualTo(1), "Incorrect discrim subclass delete count");

				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task DeleteOnJoinedSubclassAsync()
		{
			if (!Dialect.SupportsTemporaryTables)
			{
				Assert.Ignore("Cannot perform multi-table deletes using dialect not supporting temp tables.");
			}

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				// Get rid of FK which may fail the test
				_doll.Friends = Array.Empty<Human>();
				await (s.UpdateAsync(_doll));
				await (t.CommitAsync());
			}

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count = await (s.Query<Mammal>().Where(x => x.BodyWeight > 150).DeleteAsync());
				Assert.That(count, Is.EqualTo(1), "Incorrect deletion count on joined subclass");

				count = await (s.Query<Mammal>().DeleteAsync());
				Assert.That(count, Is.EqualTo(4), "Incorrect deletion count on joined subclass");

				count = await (s.Query<SubMulti>().DeleteAsync());
				Assert.That(count, Is.EqualTo(0), "Incorrect deletion count on joined subclass");

				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task DeleteOnMappedJoinAsync()
		{
			if (!Dialect.SupportsTemporaryTables)
			{
				Assert.Ignore("Cannot perform multi-table deletes using dialect not supporting temp tables.");
			}

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count = await (s.Query<Joiner>().Where(x => x.JoinedName == "joined-name").DeleteAsync());
				Assert.That(count, Is.EqualTo(1), "Incorrect deletion count on joined class");

				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task DeleteUnionSubclassAbstractRootAsync()
		{
			if (!Dialect.SupportsTemporaryTables)
			{
				Assert.Ignore("Cannot perform multi-table deletes using dialect not supporting temp tables.");
			}

			// These should reach out into *all* subclass tables...
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count = await (s.Query<Vehicle>().Where(x => x.Owner == "Steve").DeleteAsync());
				Assert.That(count, Is.EqualTo(1), "incorrect restricted update count");

				count = await (s.Query<Vehicle>().DeleteAsync());
				Assert.That(count, Is.EqualTo(3), "incorrect update count");

				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task DeleteUnionSubclassConcreteSubclassAsync()
		{
			if (!Dialect.SupportsTemporaryTables)
			{
				Assert.Ignore("Cannot perform multi-table deletes using dialect not supporting temp tables.");
			}

			// These should only affect the given table
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count = await (s.Query<Truck>().Where(x => x.Owner == "Steve").DeleteAsync());
				Assert.That(count, Is.EqualTo(1), "incorrect restricted update count");

				count = await (s.Query<Truck>().DeleteAsync());
				Assert.That(count, Is.EqualTo(2), "incorrect update count");
				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task DeleteUnionSubclassLeafSubclassAsync()
		{
			// These should only affect the given table
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count = await (s.Query<Car>().Where(x => x.Owner == "Kirsten").DeleteAsync());
				Assert.That(count, Is.EqualTo(1), "incorrect restricted update count");

				count = await (s.Query<Car>().DeleteAsync());
				Assert.That(count, Is.EqualTo(0), "incorrect update count");

				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task DeleteRestrictedOnManyToOneAsync()
		{
			if (!Dialect.SupportsTemporaryTables)
			{
				Assert.Ignore("Cannot perform multi-table deletes using dialect not supporting temp tables.");
			}

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var count = await (s.Query<Animal>().Where(x => x.Mother == _butterfly).DeleteAsync());
				Assert.That(count, Is.EqualTo(1));

				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task DeleteSyntaxWithCompositeIdAsync()
		{
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				await (s.Query<EntityWithCrazyCompositeKey>().Where(x => x.Id.Id == 1 && x.Id.OtherId == 2).DeleteAsync());

				await (t.CommitAsync());
			}
		}

		[Test]
		public void DeleteOnProjectionThrowsAsync()
		{
			using (var s = OpenSession())
			using (s.BeginTransaction())
			{
				var query = s
					.Query<Animal>().Where(x => x.Mother == _butterfly)
					.Select(x => new Car { Id = x.Id });
				Assert.That(() => query.DeleteAsync(), Throws.InvalidOperationException);
			}
		}

		[Test]
		public async Task DeleteOnFilterThrowsAsync()
		{
			using (var s = OpenSession())
			using (s.BeginTransaction())
			{
				var a = await (s.Query<SimpleEntityWithAssociation>().Take(1).SingleOrDefaultAsync());
				var query = a.AssociatedEntities.AsQueryable();
				Assert.That(() => query.DeleteAsync(), Throws.InstanceOf<NotSupportedException>());
			}
		}

		#endregion
	}
}
