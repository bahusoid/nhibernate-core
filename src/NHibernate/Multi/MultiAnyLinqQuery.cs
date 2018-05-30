using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Linq;
using NHibernate.Util;
using Remotion.Linq.Parsing.ExpressionVisitors;

namespace NHibernate
{
	public partial class MultiAnyLinqQuery<T> : MultiAnyQuery<T>
	{
		private static Delegate _postExecuteTransformer;
		private static NhLinqExpression _linqEx;

		public MultiAnyLinqQuery(IQueryable query):base(GetQuery(query))
		{
		}
		
		public MultiAnyLinqQuery(IQuery query):base(query)
		{
		}

		internal MultiAnyLinqQuery(IQuery query, NhLinqExpression linq) : base(query)
		{
			_linqEx = linq;
			_postExecuteTransformer = _linqEx.ExpressionToHqlTranslationResults.PostExecuteTransformer;
		}

		private MultiAnyLinqQuery(IQueryable query, Expression modifiedOriginalExpression):base(GetQuery(query, modifiedOriginalExpression))
		{
		}
		
		public static MultiAnyLinqQuery<TResult> GetForSelector<TResult>(IQueryable<T> query, Expression<Func<IQueryable<T>, TResult>> selector)
		{
			var expression = ReplacingExpressionVisitor
				.Replace(selector.Parameters.Single(), query.Expression, selector.Body);
			return new MultiAnyLinqQuery<TResult>(query, expression);

		}

		private static IQuery GetQuery(IQueryable query, Expression ex = null)
		{
			var prov = query.Provider as INhQueryProviderSupportMultiBatch;
			
			var q = prov.GetPreparedQuery(ex ?? query.Expression, out _linqEx);
			_postExecuteTransformer = _linqEx.ExpressionToHqlTranslationResults.PostExecuteTransformer;
			return q;
		}

		protected override IList<T> ExecuteQueryNow()
		{
			if (_postExecuteTransformer == null)
			{
				return base.ExecuteQueryNow();
			}

			return GetTransformedResults(Query.List());
		}

		protected override List<T> DoGetResults()
		{
			if (_postExecuteTransformer != null)
			{
				var elementType = GetResultTypeIfChanged();

				IList transformerList = elementType == null
					? base.DoGetResults()
					: GetTypedResults(elementType);

				return GetTransformedResults(transformerList);
			}

			return base.DoGetResults();
		}

		private static List<T> GetTransformedResults(IList transformerList)
		{
			var res = _postExecuteTransformer.DynamicInvoke(transformerList.AsQueryable());
			return new List<T>
			{
				(T) res
			};
		}

		public System.Type GetResultTypeIfChanged()
		{
			if (_postExecuteTransformer == null)
			{
				return null;
			}
			var elementType = _postExecuteTransformer.Method.GetParameters()[1].ParameterType.GetGenericArguments()[0];
			if (typeof(T).IsAssignableFrom(elementType))
			{
				return null;
			}

			return elementType;
		}

		private IList GetTypedResults(System.Type type)
		{
			var method = ReflectHelper.GetMethod(() => GetTypedResults<T>())
									.GetGenericMethodDefinition();
			var generic = method.MakeGenericMethod(type);
			return (IList) generic.Invoke(this, null);
		}
	}
}
