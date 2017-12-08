using System;
using System.Collections.Generic;
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.Type;

namespace NHibernate.Criterion
{
	/// <summary>
	/// Specialized type for retrieving entity from Criteria/QueryOver API projection.
	/// Intended to be used only inside <see cref="EntityProjection"/>
	/// </summary>
	[Serializable]
	internal partial class EntityProjectionType : ManyToOneType, IType
	{
		private readonly EntityProjection _projection;

		public EntityProjectionType(EntityProjection projection) : base(projection.RootEntity.FullName, projection.Lazy)
		{
			_projection = projection;
		}

		public override int GetColumnSpan(IMapping mapping)
		{
			return _projection.ColumnAliases.Length;
		}

		object IType.NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
		{
			//names parameter is ignored (taken from projection)
			return NullSafeGet(rs, string.Empty, session, owner);
		}

		public override object NullSafeGet(DbDataReader rs, string name, ISessionImplementor session, object owner)
		{
			var identifier = _projection.Persister.IdentifierType.NullSafeGet(rs, _projection.IdentifierColumnAliases, session, null);

			if (identifier == null)
			{
				return null;
			}

			return _projection.Lazy
				? ResolveIdentifier(identifier, session)
				: GetInitializedEntityFromProjection(rs, session, identifier);
		}

		private object GetInitializedEntityFromProjection(DbDataReader rs, ISessionImplementor session, object identifier)
		{
			var hydratedObjects = new List<object>();

			var entity = Loader.Loader.GetOrCreateEntityFromDataReader(
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
				null);

			Loader.Loader.InitializeEntitiesAndCollections(hydratedObjects, rs, session, _projection.IsReadOnly, null);

			return entity;
		}
	}
}
