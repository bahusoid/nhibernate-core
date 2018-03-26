using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NHibernate.Util
{
	//TODO 6.0: Make internal
	public static class EnumerableExtensions
	{
		//Since v5.1
		[Obsolete("Please use Enumerable.Any<T>(IEnumerable<T>) instead.")]
		public static bool Any(this IEnumerable source)
		{
			return Enumerable.Any(source.Cast<object>());
		}

		//Since v5.1
		[Obsolete("Please use Enumerable.First<T>(IEnumerable<T>) instead.")]
		public static object First(this IEnumerable source)
		{
			return Enumerable.First(source.Cast<object>());
		}

		//Since v5.1
		[Obsolete("Please use Enumerable.FirstOrDefault<T>(IEnumerable<T>) instead.")]
		public static object FirstOrNull(this IEnumerable source)
		{
			return Enumerable.FirstOrDefault(source.Cast<object>());
		}

		//Since v5.1
		[Obsolete("Please use a loop instead.")]
		public static void ForEach<T>(this IEnumerable<T> query, Action<T> method)
		{
			foreach (var item in query)
			{
				method(item);
			}
		}

		internal static IList ToIList<T>(this IEnumerable<T> list)
		{
			return list as IList ?? list.ToList();
		}
	}
}
