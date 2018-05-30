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
using NHibernate.SqlCommand;

namespace NHibernate
{
	using System.Threading.Tasks;
	using System.Threading;

	public partial interface IMultiAnyQuery
	{

		/// <summary>
		/// Executed after all commands are processed
		/// </summary>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		Task PostProcessAsync(CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Immediate query execution in case dialect is non batchable
		/// </summary>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		Task ExecuteNonBatchableAsync(CancellationToken cancellationToken = default(CancellationToken));
	}
}
