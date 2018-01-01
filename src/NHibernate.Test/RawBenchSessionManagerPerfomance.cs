using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using NHibernate.Loader;
using NHibernate.Util;
using NUnit.Framework;

namespace NHibernate.Test
{
	[TestFixture]
	public class RawBenchSessionManagerPerfomance
	{
		[Test]
		public void InternLevel_Minimal()
		{
			Cfg.Environment.InternLevel = InternLevel.Minimal;
			RunTest();
		}

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

		private static void RunTest()
		{
			var factory = NH.Bencher.SessionManager.SessionFactory;

			var setup = new AppDomainSetup();
			var si = AppDomain.CurrentDomain.SetupInformation;
			setup.ApplicationBase = si.ApplicationBase;
			setup.ConfigurationFile = si.ConfigurationFile;

			AppDomain newDomain = AppDomain.CreateDomain("New Domain", null, si);
			newDomain.SetData("internLevel", Cfg.Environment.InternLevel);
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

						Cfg.Environment.InternLevel = (InternLevel) AppDomain.CurrentDomain.GetData("internLevel");
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
	

