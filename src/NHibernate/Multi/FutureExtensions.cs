using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace NHibernate
{
	public static partial class FutureExtensions
	{
		public static IFutureEnumerable<TResult> AddAsEnumerable<TResult>(this IQueryBatch batch, IQueryOver queryOver)
		{
			return new FutureEnumerable<TResult>(AddAsList<TResult>(batch, queryOver));
		}

		public static IFutureEnumerable<TResult> AddAsEnumerable<TResult>(this IQueryBatch batch, ICriteria criteria)
		{
			return new FutureEnumerable<TResult>(AddAsList<TResult>(batch, criteria));
		}

		public static IFutureEnumerable<TResult> AddAsEnumerable<TResult>(this IQueryBatch batch, IQuery query)
		{
			return new FutureEnumerable<TResult>(AddAsList<TResult>(batch, query));
		}

		public static IFutureEnumerable<TResult> AddAsEnumerable<TResult>(this IQueryBatch batch, IQueryable<TResult> query)
		{
			return new FutureEnumerable<TResult>(AddAsList(batch, query));
		}

		public static IFutureList<TResult> AddAsList<TResult>(this IQueryBatch batch, IQuery query)
		{
			return AddAsList(batch, For<TResult>(query));
		}

		public static IFutureList<TResult> AddAsList<TResult>(this IQueryBatch batch, ICriteria query)
		{
			return AddAsList(batch, For<TResult>(query));
		}

		public static IFutureList<TResult> AddAsList<TResult>(this IQueryBatch batch, IQueryOver queryOver)
		{
			return AddAsList(batch, For<TResult>(queryOver));
		}

		public static IFutureList<TResult> AddAsList<TResult>(this IQueryBatch batch, IQueryOver<TResult> queryOver)
		{
			return AddAsList(batch, new CriteriaBatchItem<TResult>(queryOver.RootCriteria));
		}

		public static IFutureList<TResult> AddAsList<TResult>(this IQueryBatch batch, IQueryable<TResult> query)
		{
			return AddAsList(batch, For(query));
		}

		public static IFutureList<TResult> AddAsList<TSource, TResult>(this IQueryBatch batch, IQueryable<TSource> query, Expression<Func<IQueryable<TSource>, TResult>> selector)
		{
			return AddAsList(batch, For(query, selector));
		}

		public static IFutureList<TResult> AddAsList<TResult>(this IQueryBatch batch, IQueryBatchItem<TResult> query)
		{
			batch.Add((IQueryBatchItem) query);
			return new FutureList<TResult>(batch, query);
		}

		public static IFutureValue<TResult> AddAsValue<TSource, TResult>(this IQueryBatch batch, IQueryable<TSource> source, Expression<Func<IQueryable<TSource>, TResult>> selector)
		{
			return AddAsValue(batch, For(source, selector));
		}

		public static IFutureValue<TSource> AddAsValue<TSource>(this IQueryBatch batch, IQueryable<TSource> source)
		{
			return AddAsValue(batch, For(source));
		}

		public static IFutureValue<TResult> AddAsValue<TResult>(this IQueryBatch batch, ICriteria query)
		{
			return AddAsValue(batch, For<TResult>(query));
		}

		public static IFutureValue<TResult> AddAsValue<TResult>(this IQueryBatch batch, IQuery query)
		{
			return AddAsValue(batch, For<TResult>(query));
		}

		public static IFutureValue<TResult> AddAsValue<TResult>(this IQueryBatch batch, IQueryBatchItem<TResult> query)
		{
			batch.Add((IQueryBatchItem) query);
			return new FutureValue<TResult>(batch, query);
		}

		public static void AddOnAfterLoad<TResult>(this IQueryBatch batch, IQueryable<TResult> query, Action<IList<TResult>> processResults)
		{
			AddOnAfterLoad(batch, For<TResult>(query), processResults);
		}

		public static void AddOnAfterLoad<TResult>(this IQueryBatch batch, ICriteria query, Action<IList<TResult>> processResults)
		{
			AddOnAfterLoad(batch, For<TResult>(query), processResults);
		}

		public static void AddOnAfterLoad<TResult>(this IQueryBatch batch, IQueryOver<TResult> query, Action<IList<TResult>> processResults)
		{
			AddOnAfterLoad(batch, For<TResult>(query), processResults);
		}

		public static void AddOnAfterLoad<TResult>(this IQueryBatch batch, IQueryOver query, Action<IList<TResult>> processResults)
		{
			AddOnAfterLoad(batch, For<TResult>(query), processResults);
		}

		public static void AddOnAfterLoad<TResult>(this IQueryBatch batch, IQuery query, Action<IList<TResult>> processResults)
		{
			AddOnAfterLoad(batch, For<TResult>(query), processResults);
		}

		public static void AddOnAfterLoad<TResult>(this IQueryBatch batch, IQueryBatchItem<TResult> query, Action<IList<TResult>> processResults)
		{
			query.OnAfterLoad += processResults;
			batch.Add(query);
		}

		private static LinqBatchItem<TResult> For<TResult>(IQueryable<TResult> source)
		{
			return new LinqBatchItem<TResult>(source);
		}

		private static LinqBatchItem<TResult> For<TSource, TResult>(IQueryable<TSource> source, Expression<Func<IQueryable<TSource>, TResult>> selector)
		{
			return LinqBatchItem<TSource>.GetForSelector(source, selector);
		}

		private static QueryBatchItem<TResult> For<TResult>(IQuery query)
		{
			return new QueryBatchItem<TResult>(query);
		}

		private static CriteriaBatchItem<TResult> For<TResult>(ICriteria query)
		{
			return new CriteriaBatchItem<TResult>(query);
		}

		private static CriteriaBatchItem<TResult> For<TResult>(IQueryOver query)
		{
			return For<TResult>(query.RootCriteria);
		}

		#region Helper classes

		partial class FutureValue<TResult> : IFutureValue<TResult>
		{
			private IQueryBatch _batch;
			private IQueryBatchItem<TResult> _query;

			private TResult _result;

			public FutureValue(IQueryBatch batch, IQueryBatchItem<TResult> query)
			{
				_batch = batch;
				_query = query;
			}

			public TResult Value
			{
				get
				{
					if (_batch == null)
						return _result;

					_batch.Execute();
					_result = _query.GetResults().FirstOrDefault();

					_batch = null;
					_query = null;

					return _result;
				}
			}
		}

		partial class FutureList<TResult> : IFutureList<TResult>
		{
			private IQueryBatch _batch;
			private IQueryBatchItem<TResult> _query;

			private IList<TResult> _list;

			public FutureList(IQueryBatch batch, IQueryBatchItem<TResult> query)
			{
				_batch = batch;
				_query = query;
			}

			public IList<TResult> Value
			{
				get
				{
					if (_batch == null)
						return _list;

					_batch.Execute();
					_list = _query.GetResults();

					_batch = null;
					_query = null;

					return _list;
				}
			}
		}

		class FutureEnumerable<TResult> : IFutureEnumerable<TResult>
		{
			private readonly IFutureList<TResult> _result;

			public FutureEnumerable(IFutureList<TResult> result)
			{
				_result = result;
			}

			public async Task<IEnumerable<TResult>> GetEnumerableAsync(CancellationToken cancellationToken = default(CancellationToken))
			{
				return await _result.GetValueAsync(cancellationToken);
			}

			public IEnumerable<TResult> GetEnumerable()
			{
				return _result.Value;
			}

			IEnumerator<TResult> IFutureEnumerable<TResult>.GetEnumerator()
			{
				return GetEnumerable().GetEnumerator();
			}

			IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator()
			{
				return GetEnumerable().GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerable().GetEnumerator();
			}
		}

		#endregion Helper classes
	}
}
