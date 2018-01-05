#define UPSTREAM
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NHibernate.Linq;
using NUnit.Framework;


namespace NHibernate.Test
{
	[TestFixture]
	public class RawBenchSessionManagerPerfomance
	{
#if UPSTREAM
		[Test]
		public void InternLevel_Upstream()
		{
			//var n = CfgXmlHelper.SessionFactoryCollectionsCacheExpression;
			//Just to force initialize static ctor and do not calculated memory conumed by Envirnoment
			//Cfg.Environment.VerifyProperties(CollectionHelper.EmptyDictionary<string, string>());

			RunTest();
		}

#else
		[Test]
		public void InternLevel_EntityNameAndReferencedEntityName()
		{
			Cfg.Environment.InternLevel = InternLevel.EntityName | InternLevel.ReferencedEntityName;
			RunTest();
		}

	[Test]
		public void InternLevel_Default()
		{
			Cfg.Environment.InternLevel = InternLevel.Default;
			RunTest();
		}

		[Test]
		public void InternLevel_SessionFactories()
		{
			Cfg.Environment.InternLevel = InternLevel.SessionFactories;
			RunTest();
		}

		[Test]
		public void InternLevel_AppDomains()
		{
			Cfg.Environment.InternLevel = InternLevel.AppDomains;
			RunTest();
		}


#endif
		


		
		int countIterations = 1000000;

		[Test]
		public void SuffixGeneration_ToString()
		{
			List<object> list = new List<object>(countIterations);
			List<int> generatedIndexes = new List<int>(countIterations);
			for (int i = 0; i < countIterations; i++)
			{
				generatedIndexes.Add(i%100);
			}

			using (Timer.Start)
			{
				foreach (var i in generatedIndexes)
				{
					list.Add(i.ToString() + "_");
				}
			}
			Console.WriteLine("Time taken: " + Timer.ElapsedMilliseconds);
		}


		//[Test]
		//public void SuffixGeneration_Pregenerated()
		//{

		//	List<object> list = new List<object>(countIterations);
		//	List<int> generatedIndexes = new List<int>(countIterations);
		//	for (int i = 0; i < countIterations; i++)
		//	{
		//		generatedIndexes.Add(i%100);
		//	}

		//	var temp = BasicLoader.GenerateSuffix(0);

		//	using (Timer.Start)
		//	{
		//		foreach (var i in generatedIndexes)
		//		{
		//			list.Add(BasicLoader.GenerateSuffix(i));
		//		}
		//	}
		//	Console.WriteLine("Time taken: " + Timer.ElapsedMilliseconds);
		//}

		public IEnumerable<NH.Bencher.EntityClasses.SalesOrderHeader> FetchGraph()
		{
			using (var session = NH.Bencher.SessionManager.OpenSession())
			{
				return session.Query<NH.Bencher.EntityClasses.SalesOrderHeader>()
					.Where(soh => soh.SalesOrderId > 50000 && soh.SalesOrderId <= 51000)
					.Fetch(x => x.Customer)
					.Fetch(x => x.SalesOrderDetails)
					.ToList();
			}
		}

		[Test]
		public void TestFetch()
		{
			var factory = NH.Bencher.SessionManager.SessionFactory;


			using (Timer.Start)
			{
				FetchGraph();
			}
			Console.WriteLine(Timer.ElapsedMilliseconds);

			using (Timer.Start)
			{
				FetchGraph();
			}
			Console.WriteLine(Timer.ElapsedMilliseconds);

			using (Timer.Start)
			{
				FetchGraph();
			}
			Console.WriteLine(Timer.ElapsedMilliseconds);
		}

		private static void RunTest()
		{
			var factory = NH.Bencher.SessionManager.SessionFactory;

			var setup = new AppDomainSetup();
			var si = AppDomain.CurrentDomain.SetupInformation;
			setup.ApplicationBase = si.ApplicationBase;
			setup.ConfigurationFile = si.ConfigurationFile;

			AppDomain newDomain = AppDomain.CreateDomain("New Domain", null, si);

#if !UPSTREAM
			newDomain.SetData("internLevel", Cfg.Environment.InternLevel);
#endif
			try
			{
				newDomain.DoCallBack(
					() =>
					{
						StringWriter s = new StringWriter();
						Console.SetOut(s);
						Console.WriteLine();
						Console.WriteLine();
						Console.WriteLine("From new App Domain...");
#if !UPSTREAM
						Cfg.Environment.InternLevel = (InternLevel) AppDomain.CurrentDomain.GetData("internLevel");
#endif
						try
						{
							
							var factory2 = NH.Bencher.SessionManager.SessionFactory;
						}
						finally
						{
							AppDomain.CurrentDomain.SetData("log", s.ToString());
						}
					});
			}
			finally
			{
				Console.WriteLine(newDomain.GetData("log"));
				AppDomain.Unload(newDomain);
			}
		}

		/// <summary>
		/// Stopwatch wrapper
		/// </summary>
		public class Timer : IDisposable
		{
			static Stopwatch stop = new Stopwatch();

			public Timer()
			{
				stop.Reset();
				stop.Start();
			}

			public static Timer Start { get { return new Timer(); } }

			public void Dispose()
			{
				stop.Stop();
			}

			static public long ElapsedMilliseconds { get { return stop.ElapsedMilliseconds; } }
		}
	}

}
	

