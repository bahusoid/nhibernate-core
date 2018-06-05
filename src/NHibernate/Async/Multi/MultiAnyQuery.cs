﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections.Generic;
using System.Linq;
using NHibernate.Engine;
using NHibernate.Impl;

namespace NHibernate
{
	using System.Threading.Tasks;
	using System.Threading;
	public partial class MultiAnyQuery<TResult> : MultiAnyQueryBase<TResult>
	{

		protected override Task<IList<TResult>> ExecuteQueryNowAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<IList<TResult>>(cancellationToken);
			}
			return Query.ListAsync<TResult>(cancellationToken);
		}
	}
}
