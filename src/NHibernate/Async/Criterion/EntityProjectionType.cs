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
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.Type;

namespace NHibernate.Criterion
{
	using System.Threading.Tasks;
	using System.Threading;
	internal partial class EntityProjectionType : ManyToOneType, IType
	{

		Task<object> IType.NullSafeGetAsync(DbDataReader rs, string[] names, ISessionImplementor session, object owner, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			//names parameter is ignored (taken from projection)
			return NullSafeGetAsync(rs, string.Empty, session, owner, cancellationToken);
		}

		public override async Task<object> NullSafeGetAsync(DbDataReader rs, string name, ISessionImplementor session, object owner, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var identifier = await (_projection.Persister.IdentifierType.NullSafeGetAsync(rs, _projection.IdentifierColumnAliases, session, null, cancellationToken)).ConfigureAwait(false);

			if (identifier == null)
			{
				return null;
			}

			return _projection.Lazy
				? await (ResolveIdentifierAsync(identifier, session, cancellationToken)).ConfigureAwait(false)
				: await (GetInitializedEntityFromProjectionAsync(rs, session, identifier, cancellationToken)).ConfigureAwait(false);
		}

		private async Task<object> GetInitializedEntityFromProjectionAsync(DbDataReader rs, ISessionImplementor session, object identifier, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var hydratedObjects = new List<object>();

			var entity = await (Loader.Loader.GetOrCreateEntityFromDataReaderAsync(
				rs,
				null,
				null,
				hydratedObjects,
				session,
				session.GenerateEntityKey(identifier, _projection.Persister),
				_projection.Persister,
				LockMode.None,
				_projection.EntityAliases,
				false,
				_projection.FetchLazyProperties,
				null, cancellationToken)).ConfigureAwait(false);

			await (Loader.Loader.InitializeEntitiesAndCollectionsAsync(hydratedObjects, rs, session, _projection.IsReadOnly, null, cancellationToken)).ConfigureAwait(false);

			return entity;
		}
	}
}
