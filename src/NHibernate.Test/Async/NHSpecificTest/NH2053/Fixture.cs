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
using System.Collections.Generic;
using System.Text;
using NHibernate.Dialect;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2053
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		protected override bool AppliesTo(NHibernate.Dialect.Dialect dialect)
		{
			return dialect is MsSql2005Dialect;
		}
		protected override void OnSetUp()
		{
			using (var session = this.OpenSession())
			{
				using (var tran = session.BeginTransaction())
				{
                    Dog snoopy = new Dog()
                    {
                        Name = "Snoopy",
                        Talkable = false
                    };
                    snoopy.Name = "Snoopy";
                    Dog Jake = new Dog()
                    {
                        Name = "Jake the dog",
                        Talkable = true
                    };
                    session.Save(snoopy);
                    session.Save(Jake);
                    Cat kitty = new Cat()
                    {
                        Name = "Kitty"
                    };
                    session.Save(kitty);
					tran.Commit();
				}
			}
		}
		protected override void OnTearDown()
		{
			using (var session = this.OpenSession())
			{
				using (var tran = session.BeginTransaction())
				{
					session.Delete("from Dog");
                    session.Delete("from Animal");
                    tran.Commit();
				}
			}
		}

		[Test]
		public async Task JoinedSubClass_FilterAsync()
		{
			using (var session = this.OpenSession())
			{
				using (var tran = session.BeginTransaction())
				{
                    session.EnableFilter("talkableFilter").SetParameter("talkable", true);
                    var snoopy = await (session.QueryOver<Dog>().Where(x => x.Name == "Snoopy").SingleOrDefaultAsync());
                    Assert.AreEqual(null, snoopy); // there are no talking dog named Snoopy.

                    var jake = await (session.QueryOver<Dog>().Where(x => x.Name == "Jake the dog").SingleOrDefaultAsync());
                    Assert.AreNotEqual(null, jake);
                    Assert.AreEqual("Jake the dog", jake.Name);

                    var kitty = await (session.QueryOver<Cat>().Where(x => x.Name == "Kitty").SingleOrDefaultAsync());
                    Assert.AreNotEqual(null, kitty);
                    Assert.AreEqual("Kitty", kitty.Name);
				}
			}
		}
	}
}
