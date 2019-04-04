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
using System.Data.Common;
using System.Diagnostics;

using NHibernate.DebugHelpers;
using NHibernate.Engine;
using NHibernate.Loader;
using NHibernate.Persister.Collection;
using NHibernate.Type;

namespace NHibernate.Collection
{
	using System.Threading.Tasks;
	using System.Threading;
	public partial class PersistentArrayHolder : AbstractPersistentCollection, ICollection
	{

		public override async Task<ICollection> GetOrphansAsync(object snapshot, string entityName, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			object[] sn = (object[]) snapshot;
			object[] arr = (object[]) array;
			List<object> result = new List<object>(sn);
			for (int i = 0; i < sn.Length; i++)
			{
				await (IdentityRemoveAsync(result, arr[i], entityName, Session, cancellationToken)).ConfigureAwait(false);
			}
			return result;
		}

		public override async Task<object> ReadFromAsync(DbDataReader rs, ICollectionPersister role, ICollectionAliases descriptor, object owner, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			object element = await (role.ReadElementAsync(rs, owner, descriptor.SuffixedElementAliases, Session, cancellationToken)).ConfigureAwait(false);
			int index = (int) await (role.ReadIndexAsync(rs, descriptor.SuffixedIndexAliases, Session, cancellationToken)).ConfigureAwait(false);
			for (int i = tempList.Count; i <= index; i++)
			{
				tempList.Add(null);
			}
			tempList[index] = element;
			return element;
		}

		/// <summary>
		/// Initializes this array holder from the cached values.
		/// </summary>
		/// <param name="persister">The CollectionPersister to use to reassemble the Array.</param>
		/// <param name="disassembled">The disassembled Array.</param>
		/// <param name="owner">The owner object.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		public override async Task InitializeFromCacheAsync(ICollectionPersister persister, object disassembled, object owner, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			object[] cached = (object[]) disassembled;

			array = System.Array.CreateInstance(persister.ElementClass, cached.Length);

			for (int i = 0; i < cached.Length; i++)
			{
				array.SetValue(await (persister.ElementType.AssembleAsync(cached[i], Session, owner, cancellationToken)).ConfigureAwait(false), i);
			}
		}

		public override async Task<object> DisassembleAsync(ICollectionPersister persister, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			int length = array.Length;
			object[] result = new object[length];
			for (int i = 0; i < length; i++)
			{
				result[i] = await (persister.ElementType.DisassembleAsync(array.GetValue(i), Session, null, cancellationToken)).ConfigureAwait(false);
			}
			return result;
		}
	}
}