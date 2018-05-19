using System;

namespace NHibernate
{
	/// <summary>
	/// Represents a fetching strategy.
	/// </summary>
	/// <remarks>
	/// <para>
	/// For Hql queries, use the <c>FETCH</c> keyword instead.
	/// In Criteria SelectMode enum is used
	/// </para>
	/// </remarks>
	[Serializable]
	public enum FetchMode
	{
		/// <summary>
		/// Default to the setting configured in the mapping file.
		/// </summary>
		Default = 0,
		/// <summary>
		/// Fetch eagerly, using a separate select. Equivalent to
		/// <c>fetch="select"</c> (and <c>outer-join="false"</c>)
		/// </summary>
		Select = 1,
		/// <summary>
		/// Fetch using an outer join.  Equivalent to
		/// <c>fetch="join"</c> (and <c>outer-join="true"</c>)
		/// </summary>
		Join = 2,

		Lazy = Select,
		Eager = Join
	}
}
