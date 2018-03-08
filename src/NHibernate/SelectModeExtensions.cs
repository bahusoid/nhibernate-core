using System;
using System.Linq.Expressions;
using NHibernate.Criterion;
using NHibernate.Impl;
using NHibernate.Loader;
using NHibernate.Type;
using NHibernate.Util;

namespace NHibernate
{
	public static class SelectModeExtensions
	{
		public static IQueryOver<TRoot, TSubType> With<TRoot, TSubType>(this IQueryOver<TRoot,TSubType> queryOver, SelectMode mode, params Expression<Func<TSubType, object>>[] associationPaths)
		{
			var q = CastOrThrow<ISupportSelectModeQueryOver<TRoot, TSubType>>(queryOver);
			foreach (var associationPath in associationPaths)
			{
				q.SetSelectMode(mode, associationPath);
			}

			return queryOver;
		}
		
		public static TThis With<TThis>(this TThis queryOver, SelectMode mode, params Expression<Func<object>>[] aliasedAssociationPaths) where TThis: IQueryOver
		{
			var criteria = queryOver.UnderlyingCriteria;
			foreach (var aliasedPath in aliasedAssociationPaths)
			{
				var expressionPath = ExpressionProcessor.FindMemberExpression(aliasedPath.Body);

				StringHelper.IsNotRoot(expressionPath, out var alias, out var path);

				criteria.With(mode, path, alias);
			}

			return queryOver;
		}
		
		public static ICriteria With(this ICriteria criteria, SelectMode mode, string associationPath, string alias)
		{
			var q = CastOrThrow<ISupportSelectModeCriteria>(criteria);
			q.SetSelectMode(mode, associationPath, alias);

			return criteria;
		}

		private static T CastOrThrow<T>(object obj) where T : class
		{
			return TypeHelper.CastOrThrow<T>(obj, "SelectMode");
		}
	}
}
