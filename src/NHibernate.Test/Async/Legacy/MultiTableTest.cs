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
using NHibernate.Dialect;
using NHibernate.DomainModel;
using NUnit.Framework;

using MultiEntity = NHibernate.DomainModel.Multi;

namespace NHibernate.Test.Legacy
{
	using System.Threading.Tasks;
	/// <summary>
	/// Summary description for MultiTableTest.
	/// </summary>
	[TestFixture]
	public class MultiTableTestAsync : TestCase
	{
		protected override string[] Mappings
		{
			get { return new string[] {"Multi.hbm.xml", "MultiExtends.hbm.xml"}; }
		}

		[Test]
		public async Task FetchManyToOneAsync()
		{
			ISession s = OpenSession();
			await (s.CreateCriteria(typeof(Po)).Fetch("Set").ListAsync());
			await (s.CreateCriteria(typeof(Po)).Fetch("List").ListAsync());
			s.Close();
		}


		[Test]
		public async Task JoinsAsync()
		{
			ISession s = OpenSession();
			await (s.CreateQuery("from Lower l join l.YetAnother l2 where lower(l2.Name) > 'a'").ListAsync());
			await (s.CreateQuery("from SubMulti sm join sm.Children smc where smc.Name > 'a'").ListAsync());
			await (s.CreateQuery("select s, ya from Lower s join s.YetAnother ya").ListAsync());
			await (s.CreateQuery("from Lower s1 join s1.Bag s2").ListAsync());
			await (s.CreateQuery("from Lower s1 left join s1.Bag s2").ListAsync());
			await (s.CreateQuery("select s, a from Lower s join s.Another a").ListAsync());
			await (s.CreateQuery("select s, a from Lower s left join s.Another a").ListAsync());
			await (s.CreateQuery("from Top s, Lower ls").ListAsync());
			await (s.CreateQuery("from Lower ls join ls.Set s where s.Name > 'a'").ListAsync());
			await (s.CreateQuery("from Po po join po.List sm where sm.Name > 'a'").ListAsync());
			await (s.CreateQuery("from Lower ls inner join ls.Another s where s.Name is not null").ListAsync());
			await (s.CreateQuery("from Lower ls where ls.Other.Another.Name is not null").ListAsync());
			await (s.CreateQuery("from Multi m where m.Derived like 'F%'").ListAsync());
			await (s.CreateQuery("from SubMulti m where m.Derived like 'F%'").ListAsync());
			s.Close();
		}

		[Test]
		public async Task SubclassCollectionAsync()
		{
			ISession s = OpenSession();
			SubMulti sm = new SubMulti();
			SubMulti sm1 = new SubMulti();
			SubMulti sm2 = new SubMulti();
			sm.Children = new List<SubMulti> {sm1, sm2};
			sm.MoreChildren = new List<SubMulti> {sm1, sm2};
			sm.ExtraProp = "foo";
			sm1.Parent = sm;
			sm2.Parent = sm;
			object id = await (s.SaveAsync(sm));
			await (s.SaveAsync(sm1));
			await (s.SaveAsync(sm2));
			await (s.FlushAsync());
			s.Close();

			await (Sfi.EvictAsync(typeof(SubMulti)));

			s = OpenSession();
			// TODO: I don't understand why h2.0.3/h2.1 issues a select statement here

			Assert.AreEqual(2,
							(await (s.CreateQuery(
								"select s from SubMulti as sm join sm.Children as s where s.Amount>-1 and s.Name is null").ListAsync())).
								Count);
			Assert.AreEqual(2, (await (s.CreateQuery("select elements(sm.Children) from SubMulti as sm").ListAsync())).Count);
			Assert.AreEqual(1,
							(await (s.CreateQuery(
								"select distinct sm from SubMulti as sm join sm.Children as s where s.Amount>-1 and s.Name is null")
								.ListAsync())).Count);
			sm = (SubMulti) await (s.LoadAsync(typeof(SubMulti), id));
			Assert.AreEqual(2, sm.Children.Count);

			ICollection filterColl =
				await (s.CreateFilter(sm.MoreChildren, "select count(*) where this.Amount>-1 and this.Name is null").ListAsync());
			foreach (object obj in filterColl)
			{
				Assert.AreEqual(2, obj);
				// only want the first one
				break;
			}
			Assert.AreEqual("FOO", sm.Derived, "should have uppercased the column in a formula");

			IEnumerator enumer =
				(await (s.CreateQuery("select distinct s from s in class SubMulti where s.MoreChildren[1].Amount < 1.0").EnumerableAsync())).
					GetEnumerator();
			Assert.IsTrue(enumer.MoveNext());
			Assert.AreSame(sm, enumer.Current);
			Assert.AreEqual(2, sm.MoreChildren.Count);
			await (s.DeleteAsync(sm));

			foreach (object obj in sm.Children)
			{
				await (s.DeleteAsync(obj));
			}
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task CollectionOnlyAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			NotMono m = new NotMono();
			long id = (long) await (s.SaveAsync(m));
			await (t.CommitAsync());
			s.Close();

			s = OpenSession();
			t = s.BeginTransaction();
			await (s.UpdateAsync(m, id));
			await (s.FlushAsync());
			m.Address = "foo bar";
			await (s.FlushAsync());
			await (s.DeleteAsync(m));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task QueriesAsync()
		{
			ISession s = OpenSession();
			long id = 1L;

			if (TestDialect.HasIdentityNativeGenerator)
			{
				id = (long) await (s.SaveAsync(new TrivialClass()));
			}
			else
			{
				await (s.SaveAsync(new TrivialClass(), id));
			}

			await (s.FlushAsync());
			s.Close();

			s = OpenSession();
			TrivialClass tc = (TrivialClass) await (s.LoadAsync(typeof(TrivialClass), id));
			await (s.CreateQuery("from s in class TrivialClass where s.id = 2").ListAsync());
			await (s.CreateQuery("select s.Count from s in class Top").ListAsync());
			await (s.CreateQuery("from s in class Lower where s.Another.Name='name'").ListAsync());
			await (s.CreateQuery("from s in class Lower where s.YetAnother.Name='name'").ListAsync());
			await (s.CreateQuery("from s in class Lower where s.YetAnother.Name='name' and s.YetAnother.Foo is null").ListAsync());
			await (s.CreateQuery("from s in class Top where s.Count=1").ListAsync());
			await (s.CreateQuery("select s.Count from s in class Top, ls in class Lower where ls.Another=s").ListAsync());
			await (s.CreateQuery("select elements(ls.Bag), elements(ls.Set) from ls in class Lower").ListAsync());
			await (s.CreateQuery("from s in class Lower").EnumerableAsync());
			await (s.CreateQuery("from s in class Top").EnumerableAsync());
			await (s.DeleteAsync(tc));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task ConstraintsAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			SubMulti sm = new SubMulti();
			sm.Amount = 66.5f;
			if (TestDialect.HasIdentityNativeGenerator)
			{
				await (s.SaveAsync(sm));
			}
			else
			{
				await (s.SaveAsync(sm, (long) 2));
			}
			await (t.CommitAsync());
			s.Close();

			s = OpenSession();
			await (s.DeleteAsync("from sm in class SubMulti"));
			t = s.BeginTransaction();
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task MultiTableAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			MultiEntity multi = new MultiEntity();
			multi.ExtraProp = "extra";
			multi.Name = "name";
			Top simp = new Top();
			simp.Date = DateTime.Now;
			simp.Name = "simp";
			object mid;
			object sid;
			if (TestDialect.HasIdentityNativeGenerator)
			{
				mid = await (s.SaveAsync(multi));
				sid = await (s.SaveAsync(simp));
			}
			else
			{
				mid = 123L;
				sid = 1234L;
				await (s.SaveAsync(multi, mid));
				await (s.SaveAsync(simp, sid));
			}
			SubMulti sm = new SubMulti();
			sm.Amount = 66.5f;
			object smid;
			if (TestDialect.HasIdentityNativeGenerator)
			{
				smid = await (s.SaveAsync(sm));
			}
			else
			{
				smid = 2L;
				await (s.SaveAsync(sm, smid));
			}
			await (t.CommitAsync());
			s.Close();

			s = OpenSession();
			t = s.BeginTransaction();
			multi.ExtraProp = multi.ExtraProp + "2";
			multi.Name = "new name";
			await (s.UpdateAsync(multi, mid));
			simp.Name = "new name";
			await (s.UpdateAsync(simp, sid));
			sm.Amount = 456.7f;
			await (s.UpdateAsync(sm, smid));
			await (t.CommitAsync());
			s.Close();

			s = OpenSession();
			t = s.BeginTransaction();
			multi = (MultiEntity) await (s.LoadAsync(typeof(MultiEntity), mid));
			Assert.AreEqual("extra2", multi.ExtraProp);
			multi.ExtraProp = multi.ExtraProp + "3";
			Assert.AreEqual("new name", multi.Name);
			multi.Name = "newer name";
			sm = (SubMulti) await (s.LoadAsync(typeof(SubMulti), smid));
			Assert.AreEqual(456.7f, sm.Amount);
			sm.Amount = 23423f;
			await (t.CommitAsync());
			s.Close();

			s = OpenSession();
			t = s.BeginTransaction();
			multi = (MultiEntity) await (s.LoadAsync(typeof(Top), mid));
			simp = (Top) await (s.LoadAsync(typeof(Top), sid));
			Assert.IsFalse(simp is MultiEntity);
			Assert.AreEqual("extra23", multi.ExtraProp);
			Assert.AreEqual("newer name", multi.Name);
			await (t.CommitAsync());
			s.Close();

			s = OpenSession();
			t = s.BeginTransaction();
			IEnumerator enumer = (await (s.CreateQuery("select\n\ns from s in class Top where s.Count>0").EnumerableAsync())).GetEnumerator();
			bool foundSimp = false;
			bool foundMulti = false;
			bool foundSubMulti = false;
			while (enumer.MoveNext())
			{
				object o = enumer.Current;
				if ((o is Top) && !(o is MultiEntity)) foundSimp = true;
				if ((o is MultiEntity) && !(o is SubMulti)) foundMulti = true;
				if (o is SubMulti) foundSubMulti = true;
			}
			Assert.IsTrue(foundSimp);
			Assert.IsTrue(foundMulti);
			Assert.IsTrue(foundSubMulti);

			await (s.CreateQuery("from m in class Multi where m.Count>0 and m.ExtraProp is not null").ListAsync());
			await (s.CreateQuery("from m in class Top where m.Count>0 and m.Name is not null").ListAsync());
			await (s.CreateQuery("from m in class Lower where m.Other is not null").ListAsync());
			await (s.CreateQuery("from m in class Multi where m.Other.id = 1").ListAsync());
			await (s.CreateQuery("from m in class SubMulti where m.Amount > 0.0").ListAsync());

			Assert.AreEqual(2, (await (s.CreateQuery("from m in class Multi").ListAsync())).Count);

			//if( !(dialect is Dialect.HSQLDialect) ) 
			//{
			Assert.AreEqual(1, (await (s.CreateQuery("from m in class Multi where m.class = SubMulti").ListAsync())).Count);
			Assert.AreEqual(1, (await (s.CreateQuery("from m in class Top where m.class = Multi").ListAsync())).Count);
			//}

			Assert.AreEqual(3, (await (s.CreateQuery("from s in class Top").ListAsync())).Count);
			Assert.AreEqual(0, (await (s.CreateQuery("from ls in class Lower").ListAsync())).Count);
			Assert.AreEqual(1, (await (s.CreateQuery("from sm in class SubMulti").ListAsync())).Count);

			await (s.CreateQuery("from ls in class Lower, s in elements(ls.Bag) where s.id is not null").ListAsync());
			await (s.CreateQuery("from ls in class Lower, s in elements(ls.Set) where s.id is not null").ListAsync());
			await (s.CreateQuery("from sm in class SubMulti where exists elements(sm.Children)").ListAsync());

			await (t.CommitAsync());
			s.Close();

			s = OpenSession();
			t = s.BeginTransaction();
			if (TestDialect.SupportsSelectForUpdateOnOuterJoin)
				multi = (MultiEntity)await (s.LoadAsync(typeof(Top), mid, LockMode.Upgrade));
			simp = (Top) await (s.LoadAsync(typeof(Top), sid));
			await (s.LockAsync(simp, LockMode.UpgradeNoWait));
			await (t.CommitAsync());
			s.Close();

			s = OpenSession();
			t = s.BeginTransaction();
			await (s.UpdateAsync(multi, mid));
			await (s.DeleteAsync(multi));
			Assert.AreEqual(2, await (s.DeleteAsync("from s in class Top")));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task MultiTableGeneratedIdAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			MultiEntity multi = new MultiEntity();
			multi.ExtraProp = "extra";
			multi.Name = "name";
			Top simp = new Top();
			simp.Date = DateTime.Now;
			simp.Name = "simp";
			object multiId = await (s.SaveAsync(multi));
			object simpId = await (s.SaveAsync(simp));
			SubMulti sm = new SubMulti();
			sm.Amount = 66.5f;
			object smId = await (s.SaveAsync(sm));
			await (t.CommitAsync());
			s.Close();

			s = OpenSession();
			t = s.BeginTransaction();
			multi.ExtraProp += "2";
			multi.Name = "new name";
			await (s.UpdateAsync(multi, multiId));
			simp.Name = "new name";
			await (s.UpdateAsync(simp, simpId));
			sm.Amount = 456.7f;
			await (s.UpdateAsync(sm, smId));
			await (t.CommitAsync());
			s.Close();

			s = OpenSession();
			t = s.BeginTransaction();
			multi = (MultiEntity) await (s.LoadAsync(typeof(MultiEntity), multiId));
			Assert.AreEqual("extra2", multi.ExtraProp);
			multi.ExtraProp += "3";
			Assert.AreEqual("new name", multi.Name);
			multi.Name = "newer name";
			sm = (SubMulti) await (s.LoadAsync(typeof(SubMulti), smId));
			Assert.AreEqual(456.7f, sm.Amount);
			sm.Amount = 23423f;
			await (t.CommitAsync());
			s.Close();

			s = OpenSession();
			t = s.BeginTransaction();
			multi = (MultiEntity) await (s.LoadAsync(typeof(Top), multiId));
			simp = (Top) await (s.LoadAsync(typeof(Top), simpId));
			Assert.IsFalse(simp is MultiEntity);
			Assert.AreEqual("extra23", multi.ExtraProp);
			Assert.AreEqual("newer name", multi.Name);
			await (t.CommitAsync());
			s.Close();

			s = OpenSession();
			t = s.BeginTransaction();
			IEnumerable enumer = await (s.CreateQuery("select\n\ns from s in class Top where s.Count>0").EnumerableAsync());
			bool foundSimp = false;
			bool foundMulti = false;
			bool foundSubMulti = false;

			foreach (object obj in enumer)
			{
				if ((obj is Top) && !(obj is MultiEntity)) foundSimp = true;
				if ((obj is MultiEntity) && !(obj is SubMulti)) foundMulti = true;
				if (obj is SubMulti) foundSubMulti = true;
			}
			Assert.IsTrue(foundSimp);
			Assert.IsTrue(foundMulti);
			Assert.IsTrue(foundSubMulti);

			await (s.CreateQuery("from m in class Multi where m.Count>0 and m.ExtraProp is not null").ListAsync());
			await (s.CreateQuery("from m in class Top where m.Count>0 and m.Name is not null").ListAsync());
			await (s.CreateQuery("from m in class Lower where m.Other is not null").ListAsync());
			await (s.CreateQuery("from m in class Multi where m.Other.id = 1").ListAsync());
			await (s.CreateQuery("from m in class SubMulti where m.Amount > 0.0").ListAsync());

			Assert.AreEqual(2, (await (s.CreateQuery("from m in class Multi").ListAsync())).Count);
			Assert.AreEqual(3, (await (s.CreateQuery("from s in class Top").ListAsync())).Count);
			Assert.AreEqual(0, (await (s.CreateQuery("from s in class Lower").ListAsync())).Count);
			Assert.AreEqual(1, (await (s.CreateQuery("from sm in class SubMulti").ListAsync())).Count);

			await (s.CreateQuery("from ls in class Lower, s in elements(ls.Bag) where s.id is not null").ListAsync());
			await (s.CreateQuery("from sm in class SubMulti where exists elements(sm.Children)").ListAsync());
			
			await (t.CommitAsync());
			s.Close();

			s = OpenSession();
			t = s.BeginTransaction();
			if (TestDialect.SupportsSelectForUpdateOnOuterJoin)
				multi = (MultiEntity) await (s.LoadAsync(typeof(Top), multiId, LockMode.Upgrade));
			simp = (Top) await (s.LoadAsync(typeof(Top), simpId));
			await (s.LockAsync(simp, LockMode.UpgradeNoWait));
			await (t.CommitAsync());
			s.Close();

			s = OpenSession();
			t = s.BeginTransaction();
			await (s.UpdateAsync(multi, multiId));
			await (s.DeleteAsync(multi));
			Assert.AreEqual(2, await (s.DeleteAsync("from s in class Top")));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task MultiTableCollectionsAsync()
		{
			if (Dialect is MySQLDialect)
			{
				return;
			}

			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Assert.AreEqual(0, (await (s.CreateQuery("from s in class Top").ListAsync())).Count);
			MultiEntity multi = new MultiEntity();
			multi.ExtraProp = "extra";
			multi.Name = "name";
			Top simp = new Top();
			simp.Date = DateTime.Now;
			simp.Name = "simp";
			object mid;
			object sid;
			if (TestDialect.HasIdentityNativeGenerator)
			{
				mid = await (s.SaveAsync(multi));
				sid = await (s.SaveAsync(simp));
			}
			else
			{
				mid = 123L;
				sid = 1234L;
				await (s.SaveAsync(multi, mid));
				await (s.SaveAsync(simp, sid));
			}
			Lower ls = new Lower();
			ls.Other = ls;
			ls.Another = ls;
			ls.YetAnother = ls;
			ls.Name = "Less Simple";
			ls.Set = new HashSet<Top> { multi, simp };

			object id;
			if (TestDialect.HasIdentityNativeGenerator)
			{
				id = await (s.SaveAsync(ls));
			}
			else
			{
				id = 2L;
				await (s.SaveAsync(ls, id));
			}
			await (t.CommitAsync());
			s.Close();
			Assert.AreSame(ls, ls.Other);
			Assert.AreSame(ls, ls.Another);
			Assert.AreSame(ls, ls.YetAnother);

			s = OpenSession();
			t = s.BeginTransaction();
			ls = (Lower) await (s.LoadAsync(typeof(Lower), id));
			Assert.AreSame(ls, ls.Other);
			Assert.AreSame(ls, ls.Another);
			Assert.AreSame(ls, ls.YetAnother);
			Assert.AreEqual(2, ls.Set.Count);

			int foundMulti = 0;
			int foundSimple = 0;

			foreach (object obj in ls.Set)
			{
				if (obj is Top) foundSimple++;
				if (obj is MultiEntity) foundMulti++;
			}
			Assert.AreEqual(2, foundSimple);
			Assert.AreEqual(1, foundMulti);
			Assert.AreEqual(3, await (s.DeleteAsync("from s in class Top")));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task MultiTableManyToOneAsync()
		{
			if (Dialect is MySQLDialect)
			{
				return;
			}

			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Assert.AreEqual(0, (await (s.CreateQuery("from s in class Top").ListAsync())).Count);
			MultiEntity multi = new MultiEntity();
			multi.ExtraProp = "extra";
			multi.Name = "name";
			Top simp = new Top();
			simp.Date = DateTime.Now;
			simp.Name = "simp";
			object mid;

			if (TestDialect.HasIdentityNativeGenerator)
			{
				mid = await (s.SaveAsync(multi));
			}
			else
			{
				mid = 123L;
				await (s.SaveAsync(multi, mid));
			}
			Lower ls = new Lower();
			ls.Other = ls;
			ls.Another = multi;
			ls.YetAnother = ls;
			ls.Name = "Less Simple";
			object id;
			if (TestDialect.HasIdentityNativeGenerator)
			{
				id = await (s.SaveAsync(ls));
			}
			else
			{
				id = 2L;
				await (s.SaveAsync(ls, id));
			}
			await (t.CommitAsync());
			s.Close();

			Assert.AreSame(ls, ls.Other);
			Assert.AreSame(multi, ls.Another);
			Assert.AreSame(ls, ls.YetAnother);

			s = OpenSession();
			t = s.BeginTransaction();
			ls = (Lower) await (s.LoadAsync(typeof(Lower), id));
			Assert.AreSame(ls, ls.Other);
			Assert.AreSame(ls, ls.YetAnother);
			Assert.AreEqual("name", ls.Another.Name);
			Assert.IsTrue(ls.Another is MultiEntity);
			await (s.DeleteAsync(ls));
			await (s.DeleteAsync(ls.Another));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task MultiTableNativeIdAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			MultiEntity multi = new MultiEntity();
			multi.ExtraProp = "extra";
			object id = await (s.SaveAsync(multi));
			Assert.IsNotNull(id);
			await (s.DeleteAsync(multi));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task CollectionAsync()
		{
			if (!TestDialect.SupportsEmptyInsertsOrHasNonIdentityNativeGenerator)
				Assert.Ignore("Support of empty inserts is required");

			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			MultiEntity multi1 = new MultiEntity();
			multi1.ExtraProp = "extra1";
			MultiEntity multi2 = new MultiEntity();
			multi2.ExtraProp = "extra2";
			Po po = new Po();
			multi1.Po = po;
			multi2.Po = po;
			po.Set = new HashSet<MultiEntity> {multi1, multi2};
			po.List = new List<SubMulti> {new SubMulti()};
			object id = await (s.SaveAsync(po));
			Assert.IsNotNull(id);
			await (t.CommitAsync());
			s.Close();

			s = OpenSession();
			t = s.BeginTransaction();
			po = (Po) await (s.LoadAsync(typeof(Po), id));
			Assert.AreEqual(2, po.Set.Count);
			Assert.AreEqual(1, po.List.Count);
			await (s.DeleteAsync(po));
			Assert.AreEqual(0, (await (s.CreateQuery("from s in class Top").ListAsync())).Count);
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task OneToOneAsync()
		{
			ISession s = OpenSession();
			Lower ls = new Lower();
			object id = await (s.SaveAsync(ls));
			await (s.FlushAsync());
			s.Close();

			s = OpenSession();
			await (s.LoadAsync(typeof(Lower), id));
			s.Close();

			s = OpenSession();
			await (s.DeleteAsync(await (s.LoadAsync(typeof(Lower), id))));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task CollectionPointerAsync()
		{
			ISession s = OpenSession();
			Lower ls = new Lower();
			IList<Top> list = new List<Top>();
			ls.Bag = list;
			Top simple = new Top();
			object id = await (s.SaveAsync(ls));
			await (s.SaveAsync(simple));
			await (s.FlushAsync());
			list.Add(simple);
			await (s.FlushAsync());
			s.Close();

			s = OpenSession();
			ls = (Lower) await (s.LoadAsync(typeof(Lower), id));
			Assert.AreEqual(1, ls.Bag.Count);
			await (s.DeleteAsync("from o in class System.Object"));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task DynamicUpdateAsync()
		{
			object id;
			Top simple = new Top();

			simple.Name = "saved";

			using (ISession s = OpenSession())
			{
				id = await (s.SaveAsync(simple));
				await (s.FlushAsync());

				simple.Name = "updated";
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				simple = (Top) await (s.LoadAsync(typeof(Top), id));
				Assert.AreEqual("updated", simple.Name, "name should have been updated");
			}

			using (ISession s = OpenSession())
			{
				await (s.DeleteAsync("from Top"));
				await (s.FlushAsync());
			}
		}
	}
}
