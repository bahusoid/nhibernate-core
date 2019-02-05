namespace NHibernate.Cfg.MappingSchema
{
	public interface IEntityMappingByCode
	{
		System.Type MappedType { get; }
	}

	internal static class IEntityMappingExtension
	{
		public static System.Type GetMappedType(this IEntityMapping mapping)
		{
			return (mapping as IEntityMappingByCode)?.MappedType;
		}
	}

	public interface IEntityMapping : IDecoratable, IEntitySqlsMapping, IPropertiesContainerMapping
	{
		string EntityName { get; }
		string Name { get; }
		string Node { get; }
		string Proxy { get; }
		bool? UseLazy { get; }
		HbmTuplizer[] Tuplizers { get; }
		bool DynamicUpdate { get; }
		bool DynamicInsert { get; }
		int? BatchSize { get; }
		bool SelectBeforeUpdate { get; }
		string Persister { get; }
		bool? IsAbstract { get; }
		HbmSynchronize[] Synchronize { get; }
	}
}
