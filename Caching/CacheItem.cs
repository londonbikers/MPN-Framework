using System;

namespace MPN.Framework.Caching
{
	/// <summary>
	/// Acts as a container-class for CacheManager items.
	/// </summary>
	public class CacheItem
	{
		#region accessors
	    /// <summary>
	    /// The identifier for the type of object this is, i.e. the Type name.
	    /// </summary>
	    public string TypeIdentifier { get; set; }

	    /// <summary>
	    /// The primary ID for the object being cached.
	    /// </summary>
	    public long PrimaryKey { get; set; }

	    /// <summary>
	    /// If appropriate, the secondary ID for the object being cached.
	    /// </summary>
	    public string SecondaryKey { get; set; }

	    /// <summary>
	    /// When the item was first cached, i.e. it's lifespan.
	    /// </summary>
	    public DateTime Created { get; set; }

	    /// <summary>
	    /// The number of times this object has been requested from the cache.
	    /// </summary>
	    public int RequestCount { get; set; }

	    /// <summary>
	    /// The actual object that is being cached.
	    /// </summary>
	    public object Item { get; set; }
	    #endregion

		#region constructors
		/// <summary>
		/// Createa a new CacheItem object.
		/// </summary>
		internal CacheItem()
		{
			RequestCount = 0;
			Created = DateTime.Now;
		}
		#endregion
	}
}