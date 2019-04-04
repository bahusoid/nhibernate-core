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
using System.Data.Common;
using System.Linq;
using System.Reflection;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using System.Collections.Generic;

namespace NHibernate.Type
{
	using System.Threading.Tasks;
	using System.Threading;
	public partial class CompositeCustomType : AbstractType, IAbstractComponentType
	{

		public override Task<object> AssembleAsync(object cached, ISessionImplementor session, object owner, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				return Task.FromResult<object>(Assemble(cached, session, owner));
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		public override Task<object> DisassembleAsync(object value, ISessionImplementor session, object owner, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				return Task.FromResult<object>(Disassemble(value, session, owner));
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		public override Task<object> NullSafeGetAsync(DbDataReader rs, string name, ISessionImplementor session, object owner, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				return Task.FromResult<object>(NullSafeGet(rs, name, session, owner));
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		public override Task<object> NullSafeGetAsync(DbDataReader rs, string[] names, ISessionImplementor session, object owner, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				return Task.FromResult<object>(NullSafeGet(rs, names, session, owner));
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		public override Task<object> ReplaceAsync(object original, object current, ISessionImplementor session, object owner, IDictionary copiedAlready, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				return Task.FromResult<object>(Replace(original, current, session, owner, copiedAlready));
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

	}
}
