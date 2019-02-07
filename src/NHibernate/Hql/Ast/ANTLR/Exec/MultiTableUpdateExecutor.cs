using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using NHibernate.Engine;
using NHibernate.Exceptions;
using NHibernate.Hql.Ast.ANTLR.Tree;
using NHibernate.Param;
using NHibernate.Persister.Entity;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;
using NHibernate.Util;
using IQueryable = NHibernate.Persister.Entity.IQueryable;

namespace NHibernate.Hql.Ast.ANTLR.Exec
{
	[CLSCompliant(false)]
	public partial class MultiTableUpdateExecutor : AbstractStatementExecutor
	{
		private static readonly INHibernateLogger log = NHibernateLogger.For(typeof (MultiTableDeleteExecutor));
		private readonly IQueryable persister;
		private readonly SqlString update;
		private readonly SqlString fromStatement;
		private readonly IParameterSpecification[][] hqlParameters;
		private readonly IParameterSpecification[] singleHqlParameters;


		public MultiTableUpdateExecutor(IStatement statement) : base(statement, log)
		{
			if (!Factory.Dialect.SupportsTemporaryTables)
			{
				throw new HibernateException("cannot perform multi-table updates using dialect not supporting temp tables");
			}
			var updateStatement = (UpdateStatement) statement;

			FromElement fromElement = updateStatement.FromClause.GetFromElement();
			string bulkTargetAlias = fromElement.TableAlias;
			persister = fromElement.Queryable;

			var fromStatement  = GenerateFromForUpdateStatement(persister, bulkTargetAlias, updateStatement.WhereClause);

			string[] tableNames = persister.ConstraintOrderedTableNameClosure;
			string[][] columnNames = persister.ConstraintOrderedTableKeyColumnClosure;

			//string idSubselect = GenerateIdSubselect(persister);
			IList<AssignmentSpecification> assignmentSpecifications = Walker.AssignmentSpecifications;

			hqlParameters = new IParameterSpecification[tableNames.Length][];
			SqlUpdateBuilder update =
	new SqlUpdateBuilder(Factory.Dialect, Factory).SetTableName(bulkTargetAlias)
	.SetFromUpdate(fromStatement);

			if (Factory.Settings.IsCommentsEnabled)
			{
				update.SetComment("bulk update");
			}
			var parameterList = new List<IParameterSpecification>();
			{
				foreach (var specification in assignmentSpecifications)
				{
					update.AppendAssignmentFragment(specification.SqlAssignmentFragment);
					if (specification.Parameters != null)
					{
						for (int paramIndex = 0; paramIndex < specification.Parameters.Length; paramIndex++)
						{
							parameterList.Add(specification.Parameters[paramIndex]);
						}
					}
				}
				this.singleHqlParameters = parameterList.ToArray();
				
			}
			this.update = update.ToSqlString();
			this.fromStatement = fromStatement;

		}

		public override SqlString[] SqlStatements
		{
			get { return new[] { update }; }
		}

		public override int Execute(QueryParameters parameters, ISessionImplementor session)
		{
			CoordinateSharedCacheCleanup(session);
			{
				// First, save off the pertinent ids, as the return value
				DbCommand ps = null;
				int resultCount = 0;
				// Start performing the updates
				try
				{
					try
					{
						IList<IParameterSpecification> allParams = Walker.Parameters;
						var sqlString = FilterHelper.ExpandDynamicFilterParameters(update, allParams, session);
						var sqlQueryParametersList = update.GetParameters()
							//.Concat(fromStatement.GetParameters())
							.ToList();
						SqlType[] parameterTypes = allParams.GetQueryParameterTypes(sqlQueryParametersList, session.Factory);
						ps = session.Batcher.PrepareCommand(CommandType.Text, sqlString, parameterTypes);
						foreach (var parameterSpecification in allParams)
						{
							parameterSpecification.Bind(ps, sqlQueryParametersList, parameters, session);
						}
						
						resultCount = session.Batcher.ExecuteNonQuery(ps);
					}
					finally
					{
						if (ps != null)
						{
							session.Batcher.CloseCommand(ps, null);
						}
					}
				}
				catch (DbException e)
				{
					throw ADOExceptionHelper.Convert(Factory.SQLExceptionConverter, e, "error performing bulk update", update);
				}

				return resultCount;
			}

		}

		protected override IQueryable[] AffectedQueryables
		{
			get { return new[] {persister}; }
		}
	}
}
