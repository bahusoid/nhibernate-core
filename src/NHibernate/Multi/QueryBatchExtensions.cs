using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Engine;

namespace NHibernate.Multi
{
	public static partial class QueryBatchExtensions
	{
		/// <summary>
		/// Adds a query to the batch.
		/// </summary>
		/// <param name="batch">The batch.</param>
		/// <param name="query">The query.</param>
		/// <exception cref="InvalidOperationException">Thrown if the batch has already been executed.</exception>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is <see langword="null"/>.</exception>
		/// <returns>The batch instance for method chain.</returns>
		public static IQueryBatch Add<TResult>(this IQueryBatch batch, IQueryOver query)
		{
			batch.Add(For<TResult>(query));
			return batch;
		}

		/// <summary>
		/// Adds a query to the batch.
		/// </summary>
		/// <param name="batch">The batch.</param>
		/// <param name="key">A key for retrieval of the query result.</param>
		/// <param name="query">The query.</param>
		/// <exception cref="InvalidOperationException">Thrown if the batch has already been executed.</exception>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is <see langword="null"/>.</exception>
		/// <returns>The batch instance for method chain.</returns>
		public static IQueryBatch Add<TResult>(this IQueryBatch batch, string key, IQueryOver query)
		{
			if (batch == null)
				throw new ArgumentNullException(nameof(batch));
			batch.Add(key, For<TResult>(query));
			return batch;
		}

		/// <summary>
		/// Adds a query to the batch.
		/// </summary>
		/// <param name="batch">The batch.</param>
		/// <param name="query">The query.</param>
		/// <exception cref="InvalidOperationException">Thrown if the batch has already been executed.</exception>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is <see langword="null"/>.</exception>
		/// <returns>The batch instance for method chain.</returns>
		public static IQueryBatch Add<TResult>(this IQueryBatch batch, IQueryOver<TResult> query)
		{
			batch.Add(For<TResult>(query));
			return batch;
		}

		/// <summary>
		/// Adds a query to the batch.
		/// </summary>
		/// <param name="batch">The batch.</param>
		/// <param name="key">A key for retrieval of the query result.</param>
		/// <param name="query">The query.</param>
		/// <exception cref="InvalidOperationException">Thrown if the batch has already been executed.</exception>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is <see langword="null"/>.</exception>
		/// <returns>The batch instance for method chain.</returns>
		public static IQueryBatch Add<TResult>(this IQueryBatch batch, string key, IQueryOver<TResult> query)
		{
			batch.Add(key, For<TResult>(query));
			return batch;
		}

		/// <summary>
		/// Adds a query to the batch.
		/// </summary>
		/// <param name="batch">The batch.</param>
		/// <param name="query">The query.</param>
		/// <exception cref="InvalidOperationException">Thrown if the batch has already been executed.</exception>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is <see langword="null"/>.</exception>
		/// <returns>The batch instance for method chain.</returns>
		public static IQueryBatch Add<TResult>(this IQueryBatch batch, ICriteria query)
		{
			batch.Add(For<TResult>(query));
			return batch;
		}

		/// <summary>
		/// Adds a query to the batch.
		/// </summary>
		/// <param name="batch">The batch.</param>
		/// <param name="key">A key for retrieval of the query result.</param>
		/// <param name="query">The query.</param>
		/// <exception cref="InvalidOperationException">Thrown if the batch has already been executed.</exception>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is <see langword="null"/>.</exception>
		/// <returns>The batch instance for method chain.</returns>
		public static IQueryBatch Add<TResult>(this IQueryBatch batch, string key, ICriteria query)
		{
			batch.Add(key, For<TResult>(query));
			return batch;
		}

		/// <summary>
		/// Adds a query to the batch.
		/// </summary>
		/// <param name="batch">The batch.</param>
		/// <param name="query">The query.</param>
		/// <exception cref="InvalidOperationException">Thrown if the batch has already been executed.</exception>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is <see langword="null"/>.</exception>
		/// <returns>The batch instance for method chain.</returns>
		public static IQueryBatch Add<TResult>(this IQueryBatch batch, IQuery query)
		{
			batch.Add(For<TResult>(query));
			return batch;
		}

		/// <summary>
		/// Adds a query to the batch.
		/// </summary>
		/// <param name="batch">The batch.</param>
		/// <param name="key">A key for retrieval of the query result.</param>
		/// <param name="query">The query.</param>
		/// <exception cref="InvalidOperationException">Thrown if the batch has already been executed.</exception>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is <see langword="null"/>.</exception>
		/// <returns>The batch instance for method chain.</returns>
		public static IQueryBatch Add<TResult>(this IQueryBatch batch, string key, IQuery query)
		{
			batch.Add(key, For<TResult>(query));
			return batch;
		}

		/// <summary>
		/// Adds a query to the batch.
		/// </summary>
		/// <param name="batch">The batch.</param>
		/// <param name="query">The query.</param>
		/// <exception cref="InvalidOperationException">Thrown if the batch has already been executed.</exception>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is <see langword="null"/>.</exception>
		/// <returns>The batch instance for method chain.</returns>
		public static IQueryBatch Add<TResult>(this IQueryBatch batch, IQueryable<TResult> query)
		{
			batch.Add(For(query));
			return batch;
		}

		/// <summary>
		/// Adds a query to the batch.
		/// </summary>
		/// <param name="batch">The batch.</param>
		/// <param name="key">A key for retrieval of the query result.</param>
		/// <param name="query">The query.</param>
		/// <exception cref="InvalidOperationException">Thrown if the batch has already been executed.</exception>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is <see langword="null"/>.</exception>
		/// <returns>The batch instance for method chain.</returns>
		public static IQueryBatch Add<TResult>(this IQueryBatch batch, string key, IQueryable<TResult> query)
		{
			batch.Add(key, For(query));
			return batch;
		}

		/// <summary>
		/// Sets the timeout in seconds for the underlying ADO.NET query.
		/// </summary>
		/// <param name="batch">The batch.</param>
		/// <param name="timeout">The timeout for the batch.</param>
		/// <returns>The batch instance for method chain.</returns>
		public static IQueryBatch SetTimeout(this IQueryBatch batch, int? timeout)
		{
			batch.Timeout = timeout == RowSelection.NoValue ? null : timeout;
			return batch;
		}

		/// <summary>
		/// Overrides the current session flush mode, just for this query batch.
		/// </summary>
		/// <param name="batch">The batch.</param>
		/// <param name="mode">The flush mode for the batch.</param>
		/// <returns>The batch instance for method chain.</returns>
		public static IQueryBatch SetFlushMode(this IQueryBatch batch, FlushMode mode)
		{
			batch.FlushMode = mode;
			return batch;
		}

		internal static IFutureEnumerable<TResult> AddAsEnumerable<TResult>(this IQueryBatch batch, IQueryOver query)
		{
			return AddAsEnumerable(batch, For<TResult>(query));
		}

		internal static IFutureEnumerable<TResult> AddAsEnumerable<TResult>(this IQueryBatch batch, IQueryOver<TResult> query)
		{
			return AddAsEnumerable(batch, For<TResult>(query));
		}

		internal static IFutureEnumerable<TResult> AddAsEnumerable<TResult>(this IQueryBatch batch, ICriteria query)
		{
			return AddAsEnumerable(batch, For<TResult>(query));
		}

		internal static IFutureEnumerable<TResult> AddAsEnumerable<TResult>(this IQueryBatch batch, IQuery query)
		{
			return AddAsEnumerable(batch, For<TResult>(query));
		}

		internal static IFutureEnumerable<TResult> AddAsEnumerable<TResult>(this IQueryBatch batch, IQueryable<TResult> query)
		{
			return AddAsEnumerable(batch, For(query));
		}

		internal static IFutureEnumerable<TResult> AddAsEnumerable<TResult>(this IQueryBatch batch, IQueryBatchItem<TResult> query)
		{
			return new FutureEnumerable<TResult>(AddAsList(batch, query));
		}

		internal static IFutureList<TResult> AddAsList<TResult>(this IQueryBatch batch, IQueryBatchItem<TResult> query)
		{
			batch.Add(query);
			return new FutureList<TResult>(batch, query);
		}

		internal static IFutureValue<TResult> AddAsValue<TSource, TResult>(this IQueryBatch batch, IQueryable<TSource> source, Expression<Func<IQueryable<TSource>, TResult>> selector)
		{
			return AddAsValue(batch, For(source, selector));
		}

		internal static IFutureValue<TSource> AddAsValue<TSource>(this IQueryBatch batch, IQueryable<TSource> source)
		{
			return AddAsValue(batch, For(source));
		}

		internal static IFutureValue<TResult> AddAsValue<TResult>(this IQueryBatch batch, ICriteria query)
		{
			return AddAsValue(batch, For<TResult>(query));
		}

		internal static IFutureValue<TResult> AddAsValue<TResult>(this IQueryBatch batch, IQuery query)
		{
			return AddAsValue(batch, For<TResult>(query));
		}

		internal static IFutureValue<TResult> AddAsValue<TResult>(this IQueryBatch batch, IQueryBatchItem<TResult> query)
		{
			batch.Add(query);
			return new FutureValue<TResult>(batch, query);
		}

		private static LinqBatchItem<TResult> For<TResult>(IQueryable<TResult> query)
		{
			return LinqBatchItem<TResult>.GetForQuery<TResult>(query);
		}

		private static LinqBatchItem<TResult> For<TSource, TResult>(IQueryable<TSource> query, Expression<Func<IQueryable<TSource>, TResult>> selector)
		{
			return LinqBatchItem<TSource>.GetForSelector(query, selector);
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
			if (query == null)
				throw new ArgumentNullException(nameof(query));
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
