using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Engine;
using NHibernate.Persister.Collection;
using NHibernate.Util;

namespace NHibernate.Loader.Collection
{
// <seealso cref= org.hibernate.loader.BatchFetchStyle </seealso>


/// <summary>
	/// Contract for building <seealso cref="ICollectionInitializer"/> instances capable of performing batch-fetch loading.
	/// </summary>
	public abstract class BatchingCollectionInitializerBuilder
	{
		// public static BatchingCollectionInitializerBuilder GetBuilder(ISessionFactoryImplementor factory)
		// {
		// 	switch (factory.getSettings().getBatchFetchStyle())
		// 	{
		// 		case PADDED:
		// 		{
		// 			return PaddedBatchingCollectionInitializerBuilder.INSTANCE;
		// 		}
		// 		case DYNAMIC:
		// 		{
		// 			return DynamicBatchingCollectionInitializerBuilder.INSTANCE;
		// 		}
		// 		default:
		// 		{
		// 			return LegacyBatchingCollectionInitializerBuilder.INSTANCE;
		// 		}
		// 	}
		// }

		/// <summary>
		/// Builds a batch-fetch capable ICollectionInitializer for basic and many-to-many collections (collections with
		/// a dedicated collection table).
		/// </summary>
		/// <param name="persister"> THe collection persister </param>
		/// <param name="maxBatchSize"> The maximum number of keys to batch-fetch together </param>
		/// <param name="factory"> The SessionFactory </param>
		/// <param name="enabledFilters"></param>
		/// <returns> The batch-fetch capable collection initializer </returns>
		public virtual ICollectionInitializer CreateBatchingCollectionInitializer(IQueryableCollection persister, int maxBatchSize, ISessionFactoryImplementor factory, IDictionary<string, IFilter> enabledFilters)
		{
			if (maxBatchSize <= 1)
			{
				// no batching
				return new BasicCollectionLoader(persister, factory, enabledFilters);
			}

			return CreateRealBatchingCollectionInitializer(persister, maxBatchSize, factory, enabledFilters);
		}

		protected abstract ICollectionInitializer CreateRealBatchingCollectionInitializer(IQueryableCollection persister, int maxBatchSize, ISessionFactoryImplementor factory, IDictionary<string,IFilter> enabledFilters);


		/// <summary>
		/// Builds a batch-fetch capable ICollectionInitializer for one-to-many collections (collections without
		/// a dedicated collection table).
		/// </summary>
		/// <param name="persister"> THe collection persister </param>
		/// <param name="maxBatchSize"> The maximum number of keys to batch-fetch together </param>
		/// <param name="factory"> The SessionFactory </param>
		/// <param name="enabledFilters"></param>
		/// <returns> The batch-fetch capable collection initializer </returns>
		public virtual ICollectionInitializer CreateBatchingOneToManyInitializer(IQueryableCollection persister, int maxBatchSize, ISessionFactoryImplementor factory, IDictionary<string,IFilter> enabledFilters)
		{
			if (maxBatchSize <= 1)
			{
				// no batching
				return new OneToManyLoader(persister, factory, enabledFilters);
			}

			return CreateRealBatchingOneToManyInitializer(persister, maxBatchSize, factory, enabledFilters);
		}

		protected abstract ICollectionInitializer CreateRealBatchingOneToManyInitializer(IQueryableCollection persister, int maxBatchSize, ISessionFactoryImplementor factory, IDictionary<string, IFilter> enabledFilters);
	}

	public abstract partial class AbstractBatchingCollectionInitializer : ICollectionInitializer
	{
		protected IQueryableCollection CollectionPersister { get; }

		protected AbstractBatchingCollectionInitializer(IQueryableCollection collectionPersister)
		{
			CollectionPersister = collectionPersister;
		}

		public abstract void Initialize(object id, ISessionImplementor session);
	}
	/// <summary>
	/// "Batch" loads collections, using multiple foreign key values in the SQL Where clause
	/// </summary>
	/// <seealso cref="BasicCollectionLoader"/>
	/// <seealso cref="OneToManyLoader"/>
	public partial class BatchingCollectionInitializer : AbstractBatchingCollectionInitializer
	{
		private readonly Loader[] loaders;
		private readonly int[] batchSizes;
		private readonly ICollectionPersister collectionPersister;

		public BatchingCollectionInitializer(ICollectionPersister collectionPersister, int[] batchSizes, Loader[] loaders) 
			: this((IQueryableCollection) collectionPersister, batchSizes, loaders)
		{
		}

		public BatchingCollectionInitializer(IQueryableCollection collectionPersister, int[] batchSizes, Loader[] loaders): base(collectionPersister)
		
		{
			this.loaders = loaders;
			this.batchSizes = batchSizes;
			this.collectionPersister = collectionPersister;
		}

		public override void Initialize(object id, ISessionImplementor session)
		{
			object[] batch =
				session.PersistenceContext.BatchFetchQueue.GetCollectionBatch(collectionPersister, id, batchSizes[0]);

			for (int i = 0; i < batchSizes.Length; i++)
			{
				int smallBatchSize = batchSizes[i];
				if (batch[smallBatchSize - 1] != null)
				{
					object[] smallBatch = new object[smallBatchSize];
					Array.Copy(batch, 0, smallBatch, 0, smallBatchSize);
					loaders[i].LoadCollectionBatch(session, smallBatch, collectionPersister.KeyType);
					return; //EARLY EXIT!
				}
			}

			loaders[batchSizes.Length - 1].LoadCollection(session, id, collectionPersister.KeyType);
		}

		[Obsolete]
		public static ICollectionInitializer CreateBatchingOneToManyInitializer(
			OneToManyPersister persister,
			int maxBatchSize,
			ISessionFactoryImplementor factory,
			IDictionary<string, IFilter> enabledFilters)
		{
			return CreateBatchingOneToManyInitializer((IQueryableCollection) persister, maxBatchSize, factory, enabledFilters);
		}

		public static ICollectionInitializer CreateBatchingOneToManyInitializer(IQueryableCollection persister, int maxBatchSize,
																				ISessionFactoryImplementor factory,
																				IDictionary<string, IFilter> enabledFilters)
		{
			if (maxBatchSize > 1)
			{
				int[] batchSizesToCreate = ArrayHelper.GetBatchSizes(maxBatchSize);
				Loader[] loadersToCreate = new Loader[batchSizesToCreate.Length];
				for (int i = 0; i < batchSizesToCreate.Length; i++)
				{
					loadersToCreate[i] = new OneToManyLoader(persister, batchSizesToCreate[i], factory, enabledFilters);
				}

				return new BatchingCollectionInitializer(persister, batchSizesToCreate, loadersToCreate);
			}
			else
			{
				return new OneToManyLoader(persister, factory, enabledFilters);
			}
		}

		public static ICollectionInitializer CreateBatchingCollectionInitializer(IQueryableCollection persister,
																				 int maxBatchSize,
																				 ISessionFactoryImplementor factory,
																				 IDictionary<string, IFilter> enabledFilters)
		{
			if (maxBatchSize > 1)
			{
				int[] batchSizesToCreate = ArrayHelper.GetBatchSizes(maxBatchSize);
				Loader[] loadersToCreate = new Loader[batchSizesToCreate.Length];
				for (int i = 0; i < batchSizesToCreate.Length; i++)
				{
					loadersToCreate[i] = new BasicCollectionLoader(persister, batchSizesToCreate[i], factory, enabledFilters);
				}
				return new BatchingCollectionInitializer(persister, batchSizesToCreate, loadersToCreate);
			}
			else
			{
				return new BasicCollectionLoader(persister, factory, enabledFilters);
			}
		}
	}
}
