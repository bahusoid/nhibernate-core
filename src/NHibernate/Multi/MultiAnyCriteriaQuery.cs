using System.Collections.Generic;
using NHibernate.Impl;
using NHibernate.Loader.Criteria;
using NHibernate.Persister.Entity;

namespace NHibernate
{
	public partial class MultiAnyCriteriaQuery<T> : MultiAnyQueryBase<T>, IMultiAnyQuery<T>
	{
		private readonly CriteriaImpl _criteria;

		public MultiAnyCriteriaQuery(ICriteria criteria)
		{
			_criteria = (CriteriaImpl) criteria;
		}

		protected override List<QueryLoadInfo> GetQueryLoadInfo()
		{
			var factory = Session.Factory;
			string[] implementors = factory.GetImplementors(_criteria.EntityOrClassName);
			int size = implementors.Length;
			var list = new List<QueryLoadInfo>(size);
			for (int i = 0; i < size; i++)
			{
				CriteriaLoader loader = new CriteriaLoader(
					factory.GetEntityPersister(implementors[i]) as IOuterJoinLoadable,
					factory,
					_criteria,
					implementors[i],
					Session.EnabledFilters
				);

				list.Add(
					new QueryLoadInfo()
					{
						Loader = loader,
						Parameters = loader.Translator.GetQueryParameters(),
						QuerySpaces = loader.QuerySpaces,
					});
			}

			return list;
		}

		protected override IList<T> ExecuteQueryNow()
		{
			return _criteria.List<T>();
		}

		protected override List<T> DoGetResults()
		{
			return GetTypedResults<T>();
		}
	}
}
