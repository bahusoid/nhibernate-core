﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using NHibernate.Cfg;
using NHibernate.Util;

namespace NHibernate.Cache
{
	using System.Threading.Tasks;
	using System.Threading;
	public partial class UpdateTimestampsCache
	{

		public virtual Task ClearAsync(CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			return _updateTimestamps.ClearAsync(cancellationToken);
		}

		//Since v5.1
		[Obsolete("Please use PreInvalidate(IReadOnlyCollection<string>) instead.")]
		public Task PreInvalidateAsync(object[] spaces, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				//Only for backwards compatibility.
				return PreInvalidateAsync(spaces.OfType<string>().ToList(), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		public virtual async Task PreInvalidateAsync(IReadOnlyCollection<string> spaces, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (spaces.Count == 0)
				return;
			cancellationToken.ThrowIfCancellationRequested();

			using (await (_asyncReaderWriterLock.WriteLockAsync()).ConfigureAwait(false))
			{
				//TODO: to handle concurrent writes correctly, this should return a Lock to the client
				var ts = _updateTimestamps.NextTimestamp() + _updateTimestamps.Timeout;
				await (SetSpacesTimestampAsync(spaces, ts, cancellationToken)).ConfigureAwait(false);
				//TODO: return new Lock(ts);
			}
		}

		//Since v5.1
		[Obsolete("Please use Invalidate(IReadOnlyCollection<string>) instead.")]
		public Task InvalidateAsync(object[] spaces, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				//Only for backwards compatibility.
				return InvalidateAsync(spaces.OfType<string>().ToList(), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		public virtual async Task InvalidateAsync(IReadOnlyCollection<string> spaces, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (spaces.Count == 0)
				return;
			cancellationToken.ThrowIfCancellationRequested();

			using (await (_asyncReaderWriterLock.WriteLockAsync()).ConfigureAwait(false))
			{
				//TODO: to handle concurrent writes correctly, the client should pass in a Lock
				long ts = _updateTimestamps.NextTimestamp();
				//TODO: if lock.getTimestamp().equals(ts)
				if (log.IsDebugEnabled())
					log.Debug("Invalidating spaces [{0}]", StringHelper.CollectionToString(spaces));
				await (SetSpacesTimestampAsync(spaces, ts, cancellationToken)).ConfigureAwait(false);
			}
		}

		private Task SetSpacesTimestampAsync(IReadOnlyCollection<string> spaces, long ts, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				return _updateTimestamps.PutManyAsync(
					spaces.ToArray<object>(),
					ArrayHelper.Fill<object>(ts, spaces.Count), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		public virtual async Task<bool> IsUpToDateAsync(ISet<string> spaces, long timestamp /* H2.1 has Long here */, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (spaces.Count == 0)
				return true;
			cancellationToken.ThrowIfCancellationRequested();

			using (await (_asyncReaderWriterLock.ReadLockAsync()).ConfigureAwait(false))
			{
				var lastUpdates = await (_updateTimestamps.GetManyAsync(spaces.ToArray<object>(), cancellationToken)).ConfigureAwait(false);
				return lastUpdates.All(lastUpdate => !IsOutdated(lastUpdate as long?, timestamp));
			}
		}

		public virtual async Task<bool[]> AreUpToDateAsync(ISet<string>[] spaces, long[] timestamps, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (spaces.Length == 0)
				return Array.Empty<bool>();

			var allSpaces = new HashSet<string>();
			foreach (var sp in spaces)
			{
				allSpaces.UnionWith(sp);
			}

			if (allSpaces.Count == 0)
				return ArrayHelper.Fill(true, spaces.Length);

			var keys = allSpaces.ToArray<object>();
			cancellationToken.ThrowIfCancellationRequested();

			using (await (_asyncReaderWriterLock.ReadLockAsync()).ConfigureAwait(false))
			{
				var index = 0;
				var lastUpdatesBySpace =
					(await (_updateTimestamps
						.GetManyAsync(keys, cancellationToken)).ConfigureAwait(false))
						.ToDictionary(u => keys[index++], u => u as long?);

				var results = new bool[spaces.Length];
				for (var i = 0; i < spaces.Length; i++)
				{
					var timestamp = timestamps[i];
					results[i] = spaces[i].All(space => !IsOutdated(lastUpdatesBySpace[space], timestamp));
				}

				return results;
			}
		}
	}
}
