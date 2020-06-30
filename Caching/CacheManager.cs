using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace MPN.Framework.Caching
{
	/// <summary>
	/// Provides functionality to cache domain objects in memory.
	/// </summary>
	public class CacheManager
	{
		#region members
		private static int _itemCeiling;
        private static readonly List<CacheItem> _cache;
		#endregion

		#region accessors
		/// <summary>
		/// Controls the maximum number of items that should be kept in the cache at any one time.
		/// </summary>
		public static int ItemCeiling { get { return _itemCeiling; } set { _itemCeiling = value; } }
		/// <summary>
		/// The number of items currently in the Cache.
		/// </summary>
		public static int ItemCount { get { return _cache.Count; } }
        public static decimal CacheCapacityUsed { get { return CacheManager.CalculateCapacityUsed(); } }
		#endregion

		#region constructors
		/// <summary>
		/// Creates a new CacheManager object.
		/// </summary>
		static CacheManager()
		{
			// set a default ceiling.
			_itemCeiling = 10000;
			if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["MediaPanther.Framework.Caching.MaxItems"]))
				_itemCeiling = Convert.ToInt32(ConfigurationManager.AppSettings["MediaPanther.Framework.Caching.MaxItems"]);

            _cache = new List<CacheItem>();
		}
		#endregion

		#region public methods
		/// <summary>
		/// Adds a new object to the cache.
		/// </summary>
		/// <param name="item">The actual object to store in the cache.</param>
        /// <param name="typeIdentifier">The type identifier for the object being cached, i.e. the object type name. Forms a compound key with the primary or secondary key.</param>
		/// <param name="primaryKey">The numeric primary key for the object.</param>
        /// <param name="secondaryKey">An optional secondary key for the object, e.g. a textual name.</param>
		public static void AddItem(object item, string typeIdentifier, long primaryKey, string secondaryKey)
		{
			if (string.IsNullOrEmpty(typeIdentifier))
			{
				Logger.LogWarning(string.Format("CacheManager.AddItem() - typeIdentifier is null - item: '{0}', primaryKey: '{1}', secondaryKey: '{2}'.", item.ToString(), primaryKey, secondaryKey));
				throw new ArgumentNullException();
			}

            lock (_cache)
            {
			    var itemCount = primaryKey > 0 ? _cache.Count(ci => ci.PrimaryKey == primaryKey && ci.TypeIdentifier == typeIdentifier) : _cache.Count(ci => ci.SecondaryKey == secondaryKey && ci.TypeIdentifier == typeIdentifier);
                if (itemCount == 0)
                {
                    var cacheItem = new CacheItem { TypeIdentifier = typeIdentifier, PrimaryKey = primaryKey, Item = item };
                    if (!string.IsNullOrEmpty(secondaryKey))
                        cacheItem.SecondaryKey = secondaryKey;

				    // is the cache full?
				    if (_cache.Count >= _itemCeiling)
					    RemoveUnpopularItem();

				    _cache.Add(cacheItem);
			    }
			    else
			    {
				    Logger.LogWarning(string.Format("CacheManager.AddItem() - New cache item already exists! - item: '{0}', typeIdentifier: '{1}', primaryKey: '{2}', secondaryKey: '{3}'.", item.ToString(), typeIdentifier, primaryKey, secondaryKey));
			    }
            }
		}

	    /// <summary>
	    /// Removes an object from the cache.
	    /// </summary>
        /// <param name="typeIdentifier">The type of identifier being used.</param>
        /// <param name="primaryKey">The unique-identifier for the item to be removed.</param>
	    public static void RemoveItem(string typeIdentifier, int primaryKey)
		{
			if (typeIdentifier == String.Empty)
			{
				Logger.LogWarning(string.Format("CacheManager.RemoveItem(string, int) - typeIdentifier is null - primaryKey: '{0}'.", primaryKey));
                throw new ArgumentNullException("typeIdentifier");
			}

			lock (_cache)
			{
				var itemToRemove = _cache.Find(ci => ci.PrimaryKey == primaryKey && ci.TypeIdentifier == typeIdentifier);
				_cache.Remove(itemToRemove);
			}
		}

        /// <summary>
        /// Removes an object from the cache.
        /// </summary>
        /// <param name="typeIdentifier">The type of identifier being used.</param>
        /// <param name="secondaryKey">The unique-identifier string.</param>
        public static void RemoveItem(string typeIdentifier, string secondaryKey)
        {
			if (typeIdentifier == string.Empty)
			{
				Logger.LogWarning(string.Format("CacheManager.RemoveItem(string, string) - typeIdentifier is null - secondaryKey: '{0}'.", secondaryKey));
                throw new ArgumentNullException("typeIdentifier");
			}

            if (string.IsNullOrEmpty(secondaryKey))
            {
                Logger.LogWarning(string.Format("CacheManager.RemoveItem(string, string) - secondaryKey is null - typeIdentifier: '{0}'.", typeIdentifier));
                throw new ArgumentNullException("secondaryKey");
            }

            lock (_cache)
			{
				var itemToRemove = _cache.Find(ci => ci.SecondaryKey == secondaryKey && ci.TypeIdentifier == typeIdentifier);
				_cache.Remove(itemToRemove);
			}
        }

	    /// <summary>
	    /// Collects an object that has been cached previously. Will return null if no such item found.
	    /// </summary>
	    /// <param name="typeIdentifier">The type of unique-identifier being used.</param>
	    /// <param name="primaryKey">The unique-identifier for the item to be found.</param>
	    /// <param name="secondaryKey">The secondary unique-identifier for the item to be found.</param>
	    public static object RetrieveItem(string typeIdentifier, int primaryKey, string secondaryKey)
		{
			if (primaryKey < 1 && secondaryKey == string.Empty)
			{
				Logger.LogWarning(string.Format("CacheManager.RetrieveItem(string, int, string) - both keys null - typeIdentifier: '{0}'.", typeIdentifier));
				throw new ArgumentNullException();
			}

	        if (typeIdentifier == string.Empty)
	        {
	            Logger.LogWarning(string.Format("CacheManager.RetrieveItem(string, int, string) - typeIdentifier is null - primaryKey: '{0}', secondaryKey: '{1}'.", primaryKey, secondaryKey));
	            throw new ArgumentNullException("typeIdentifier");
	        }

	        CacheItem item;
			lock (_cache)
			{
				item = primaryKey > 0 ? _cache.Find(ci => ci.PrimaryKey == primaryKey && ci.TypeIdentifier == typeIdentifier) : _cache.Find(ci => ci.SecondaryKey == secondaryKey && ci.TypeIdentifier == typeIdentifier);
			}

	        if (item == null)
	            return null;

	        item.RequestCount++;
	        return item.Item;
		}

		/// <summary>
		/// Empties the cache of all items.
		/// </summary>
		public static void FlushCache()
		{
			lock (_cache)
				_cache.Clear();
		}

		/// <summary>
		/// Retrieves the top X amount of most popular CacheItems.
		/// </summary>
		/// <param name="count">The number of items to retrieve.</param>
		public static List<CacheItem> RetrieveTopItems(int count)
		{
			if (count >= _cache.Count)
				count = _cache.Count;

			List<CacheItem> items = null;

			lock (_cache)
			{
				items = (from ci in _cache
						 orderby ci.RequestCount descending
						 select ci).Take(count).ToList();
			}

			return items;
		}
		#endregion

		#region private methods
		/// <summary>
		/// Removes the first unpopular item from the Cache to make room.
		/// </summary>
		private static void RemoveUnpopularItem()
		{
			lock (_cache)
			{
				var item = (from ci in _cache
							orderby ci.RequestCount ascending
							select ci).Take(1).FirstOrDefault();

				_cache.Remove(item);
			}
		}

        private static decimal CalculateCapacityUsed()
        {
            if (CacheManager.ItemCeiling == 0 || CacheManager.ItemCount == 0)
                return 0;

            var ret = (decimal)CacheManager.ItemCount / (decimal)CacheManager.ItemCeiling;
            ret = ret * (decimal)100;

            return ret;
        }
		#endregion
	}
}