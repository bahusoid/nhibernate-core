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
using NHibernate.Collection.Generic;
using NHibernate.Engine;
using NHibernate.Impl;
using NHibernate.Loader;
using NHibernate.Persister.Collection;
using NHibernate.Type;
using NHibernate.Util;

namespace NHibernate.Collection
{
	using System.Threading.Tasks;
	using System.Threading;
	public abstract partial class AbstractPersistentCollection : IPersistentCollection, ILazyInitializedCollection
	{

		/// <summary>
		/// Initialize the collection, if possible, wrapping any exceptions
		/// in a runtime exception
		/// </summary>
		/// <param name="writing">currently obsolete</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <exception cref="LazyInitializationException">if we cannot initialize</exception>
		protected virtual Task InitializeAsync(bool writing, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				if (!initialized)
				{
					if (initializing)
					{
						return Task.FromException<object>(new LazyInitializationException("illegal access to loading collection"));
					}
					ThrowLazyInitializationExceptionIfNotConnected();
					return session.InitializeCollectionAsync(this, writing, cancellationToken);
				}
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		/// <summary>
		/// To be called internally by the session, forcing
		/// immediate initialization.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <remarks>
		/// This method is similar to <see cref="InitializeAsync(bool,CancellationToken)" />, except that different exceptions are thrown.
		/// </remarks>
		public virtual Task ForceInitializationAsync(CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			if (!initialized)
			{
				if (initializing)
				{
					return Task.FromException<object>(new AssertionFailure("force initialize loading collection"));
				}
				if (session == null)
				{
					return Task.FromException<object>(new HibernateException("collection is not associated with any session"));
				}
				if (!session.IsConnected)
				{
					return Task.FromException<object>(new HibernateException("disconnected session"));
				}
				return session.InitializeCollectionAsync(this, false, cancellationToken);
			}
			return Task.CompletedTask;
		}

		public Task<ICollection> GetQueuedOrphansAsync(string entityName, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<ICollection>(cancellationToken);
			}
			try
			{
				if (HasQueuedOperations)
				{
					List<object> additions = new List<object>(operationQueue.Count);
					List<object> removals = new List<object>(operationQueue.Count);
					for (int i = 0; i < operationQueue.Count; i++)
					{
						IDelayedOperation op = operationQueue[i];
						if (op.AddedInstance != null)
						{
							additions.Add(op.AddedInstance);
						}
						if (op.Orphan != null)
						{
							removals.Add(op.Orphan);
						}
					}
					return GetOrphansAsync(removals, additions, entityName, session, cancellationToken);
				}

				return Task.FromResult<ICollection>(CollectionHelper.EmptyCollection);
			}
			catch (Exception ex)
			{
				return Task.FromException<ICollection>(ex);
			}
		}

		/// <summary>
		/// Called before inserting rows, to ensure that any surrogate keys are fully generated
		/// </summary>
		/// <param name="persister"></param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		public virtual Task PreInsertAsync(ICollectionPersister persister, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				PreInsert(persister);
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		/// <summary>
		/// Get all "orphaned" elements
		/// </summary>
		public abstract Task<ICollection> GetOrphansAsync(object snapshot, string entityName, CancellationToken cancellationToken);

		/// <summary> 
		/// Given a collection of entity instances that used to
		/// belong to the collection, and a collection of instances
		/// that currently belong, return a collection of orphans
		/// </summary>
		protected virtual async Task<ICollection> GetOrphansAsync(ICollection oldElements, ICollection currentElements, string entityName, ISessionImplementor session, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			// short-circuit(s)
			if (currentElements.Count == 0)
			{
				// no new elements, the old list contains only Orphans
				return oldElements;
			}
			if (oldElements.Count == 0)
			{
				// no old elements, so no Orphans neither
				return oldElements;
			}

			IType idType = session.Factory.GetEntityPersister(entityName).IdentifierType;

			// create the collection holding the orphans
			List<object> res = new List<object>();

			// collect EntityIdentifier(s) of the *current* elements - add them into a HashSet for fast access
			var currentIds = new HashSet<TypedValue>();
			foreach (object current in currentElements)
			{
				if (current != null && await (ForeignKeys.IsNotTransientSlowAsync(entityName, current, session, cancellationToken)).ConfigureAwait(false))
				{
					object currentId = ForeignKeys.GetEntityIdentifierIfNotUnsaved(entityName, current, session);
					currentIds.Add(new TypedValue(idType, currentId, false));
				}
			}

			// iterate over the *old* list
			foreach (object old in oldElements)
			{
				object oldId = ForeignKeys.GetEntityIdentifierIfNotUnsaved(entityName, old, session);
				if (!currentIds.Contains(new TypedValue(idType, oldId, false)))
				{
					res.Add(old);
				}
			}

			return res;
		}

		public async Task IdentityRemoveAsync(IList list, object obj, string entityName, ISessionImplementor session, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (obj != null && await (ForeignKeys.IsNotTransientSlowAsync(entityName, obj, session, cancellationToken)).ConfigureAwait(false))
			{
				IType idType = session.Factory.GetEntityPersister(entityName).IdentifierType;

				object idOfCurrent = ForeignKeys.GetEntityIdentifierIfNotUnsaved(entityName, obj, session);
				List<object> toRemove = new List<object>(list.Count);
				foreach (object current in list)
				{
					if (current == null)
					{
						continue;
					}
					object idOfOld = ForeignKeys.GetEntityIdentifierIfNotUnsaved(entityName, current, session);
					if (idType.IsEqual(idOfCurrent, idOfOld, session.Factory))
					{
						toRemove.Add(current);
					}
				}
				foreach (object ro in toRemove)
				{
					list.Remove(ro);
				}
			}
		}

		/// <summary>
		/// Disassemble the collection, ready for the cache
		/// </summary>
		/// <param name="persister"></param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <returns></returns>
		public abstract Task<object> DisassembleAsync(ICollectionPersister persister, CancellationToken cancellationToken);

		/// <summary>
		/// Read the state of the collection from a disassembled cached value.
		/// </summary>
		/// <param name="persister"></param>
		/// <param name="disassembled"></param>
		/// <param name="owner"></param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		public abstract Task InitializeFromCacheAsync(ICollectionPersister persister, object disassembled, object owner, CancellationToken cancellationToken);

		/// <summary>
		/// Reads the row from the <see cref="DbDataReader"/>.
		/// </summary>
		/// <param name="reader">The DbDataReader that contains the value of the Identifier</param>
		/// <param name="role">The persister for this Collection.</param>
		/// <param name="descriptor">The descriptor providing result set column names</param>
		/// <param name="owner">The owner of this Collection.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <returns>The object that was contained in the row.</returns>
		public abstract Task<object> ReadFromAsync(DbDataReader reader, ICollectionPersister role, ICollectionAliases descriptor,
										object owner, CancellationToken cancellationToken);
	}
}
