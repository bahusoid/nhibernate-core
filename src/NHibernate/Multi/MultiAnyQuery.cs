using System.Collections.Generic;
using System.Linq;
using NHibernate.Engine;
using NHibernate.Impl;

namespace NHibernate
{
	public partial class MultiAnyQuery<TResult> : MultiAnyQueryBase<TResult>, IMultiAnyQuery<TResult>
	{
		protected readonly AbstractQueryImpl Query;

		public MultiAnyQuery(IQuery query)
		{
			Query = (AbstractQueryImpl) query;
		}

		protected override List<QueryLoadInfo> GetQueryLoadInfo()
		{
			Query.VerifyParameters();
			QueryParameters queryParameters = Query.GetQueryParameters();
			queryParameters.ValidateParameters();

			return Query.GetTranslators(Session, queryParameters).Select(
				t => new QueryLoadInfo()
				{
					Loader = t.Loader,
					Parameters = queryParameters,
					QuerySpaces = new HashSet<string>(t.QuerySpaces),
				}).ToList();
		}

		protected override IList<TResult> ExecuteQueryNow()
		{
			return Query.List<TResult>();
		}

		protected override List<TResult> DoGetResults()
		{
			return GetTypedResults<TResult>();
		}
	}
}
