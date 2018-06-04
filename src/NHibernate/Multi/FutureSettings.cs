namespace NHibernate
{
	//TODO: Temp switch between old and new future batcher
	internal static class FutureSettings
	{
		//True - new batcher; False - old batcher
		public static bool IsUnifiedFuture { get; set; } = true;
	}
}
