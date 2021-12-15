using System;
using NHibernate.Criterion;
using NHibernate.Engine;
using NHibernate.SqlCommand;

namespace NHibernate.Loader
{
	internal class DynamicBatchingHelper
	{
		private static string BatchIdPlaceholder = "$$BATCH_ID_PLACEHOLDER$$";

		public static SqlStringBuilder BuildBatchFetchRestrictionFragment()
		{
			return new SqlStringBuilder(1).Add(DynamicBatchingHelper.BatchIdPlaceholder);
		}

		public static void ExpandBatchIdPlaceholder(SqlString sqlString, QueryParameters queryParameters, string[] columns, Dialect.Dialect dialect, out Parameter[] parameters, out SqlString result)
		{
			var bogusParam = Parameter.Placeholder;
			var wherePart = InExpression.GetSqlString(columns, queryParameters.PositionalParameterValues.Length, dialect, bogusParam);
			var paramsCount = wherePart.GetParameterCount();
			parameters = new Parameter[paramsCount];
			for (var i = 0; i < parameters.Length; i++)
			{
				parameters[i] = Parameter.Placeholder;
			}

			wherePart.SubstituteBogusParameters(parameters, bogusParam);
			result = sqlString.ReplaceLast(DynamicBatchingHelper.BatchIdPlaceholder, wherePart);
		}

		public static int GetIdsToLoad(object[] batch, out object[] idsToLoad)
		{
			int numberOfIds = Array.IndexOf(batch, null); //TODO: IS IT SAFE?
			if (numberOfIds < 0)
			{
				idsToLoad = batch;
				return batch.Length;
			}

			//TODO: Remove
			if (Array.FindIndex(batch, numberOfIds, x => x != null) > 0)
			{
				throw new InvalidOperationException("broken batch" + string.Join(", ", batch));
			}

			if (numberOfIds == 1)
			{
				idsToLoad = null;
				return numberOfIds;
			}

			idsToLoad = new object[numberOfIds];
			Array.Copy(batch, 0, idsToLoad, 0, numberOfIds);
			return numberOfIds;
		}
	}
}
