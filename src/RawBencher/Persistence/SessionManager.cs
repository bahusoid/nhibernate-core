﻿//------------------------------------------------------------------------------
// <auto-generated>This code was generated by LLBLGen Pro v4.2.</auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using NHibernate;
using Configuration = NHibernate.Cfg.Configuration;

namespace NH.Bencher
{

	/// <summary>
	/// Stopwatch wrapper
	/// </summary>
	class Timer : IDisposable
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


	/// <summary>Small, simple session manager class which initializes NHibernate's session factory and loads the configuration.</summary>
	public static partial class SessionManager
	{
		#region Class Member Declarations
		private static readonly ISessionFactory _sessionFactory;
		#endregion

		private static long _memoryInitial;
		private static long _memoryPrevios;
		private static int _index;

		public static List<ISessionFactory> Factories = new List<ISessionFactory>();

		/// <summary>Initializes the <see cref="SessionManager"/> class.</summary>
		static SessionManager()
		{
			const int numberOfSessionFactories = 3;
			//var conf = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			//Console.WriteLine("App.config path: " + conf.FilePath);

			//int.TryParse(System.Configuration.ConfigurationManager.AppSettings["SessionFactories"], out count);
			Console.WriteLine("Number of active session factories: " + numberOfSessionFactories);

			ForceCollect();
			_memoryInitial = GC.GetTotalMemory(true);

			Console.WriteLine();
			Console.WriteLine();

			for (_index = 0; _index < numberOfSessionFactories; ++_index)
			{
				{
					var configuration = new Configuration();
					configuration.Configure();
					configuration.AddAssembly(typeof(SessionManager).Assembly);
					using (Timer.Start)
					{
						Factories.Add(configuration.BuildSessionFactory());
					}
				}
				Console.WriteLine($"Time taken by session factory #{_index} creation: " + TimeSpan.FromMilliseconds(Timer.ElapsedMilliseconds));

				ForceCollect();
				ShowMemory();
			}

			Console.WriteLine();
			Console.WriteLine();
			_sessionFactory = Factories[0];

		}

		private static void ShowMemory()
		{
			Console.WriteLine();

			var totalMemory = GC.GetTotalMemory(true);
			var taken = totalMemory - _memoryInitial;
			_memoryPrevios = taken;
			Console.WriteLine($"Total allocated memory by all session factories: {ToKbSize(taken)}.");

			Console.WriteLine();
		}

		private static string ToKbSize(long bytes)
		{
			return (bytes / 1024.0).ToString("0,0.00") + " Kb";
		}

		private static void ForceCollect()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		/// <summary>Opens a new session on the existing session factory</summary>
		/// <returns>ready to use ISession instance</returns>
		/// <remarks>Dispose this instance after you're done with the instance, so after lazy loading has occured. The returned
		/// ISession instance is <b>not</b> thread safe.</remarks>
		public static ISession OpenSession()
		{
			return _sessionFactory.OpenSession();
		}

		#region Class Property Declarations
		/// <summary>Gets the session factory created from the initialized configuration. The returned factory is thread safe.</summary>
		public static ISessionFactory SessionFactory
		{
			get { return _sessionFactory; }
		}
		#endregion
	}
}
