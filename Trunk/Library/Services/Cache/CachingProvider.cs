#region Copyright

// 
// DotNetNuke� - http://www.dotnetnuke.com
// Copyright (c) 2002-2011
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;

using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Services.Cache
{
	/// <summary>
	/// CachingProvider provides basic component of cache system, by default it will use HttpRuntime.Cache.
	/// </summary>
	/// <remarks>
	/// <para>Using cache will speed up the application to a great degree, we recommend to use cache for whole modules,
	/// but sometimes cache also make confuse for user, if we didn't take care of how to make cache expired when needed,
	/// such as if a data has already been deleted but the cache arn't clear, it will cause un expected errors.
	/// so you should choose a correct performance setting type when you trying to cache some stuff, and always remember
	/// update cache immediately after the data changed.</para>
	/// </remarks>
	/// <example>
	/// <code lang="C#">
	/// public static void ClearCache(string cachePrefix)
    /// {
    ///     CachingProvider.Instance().Clear("Prefix", GetDnnCacheKey(cachePrefix));
	/// }
	/// </code>
	/// </example>
    public abstract class CachingProvider
    {
        private static System.Web.Caching.Cache _objCache;
        private const string CachePrefix = "DNN_";

		/// <summary>
		/// Gets the default cache provider.
		/// </summary>
		/// <value>HttpRuntime.Cache</value>
        protected static System.Web.Caching.Cache Cache
        {
            get
            {
                if (_objCache == null)
                {
                    _objCache = HttpRuntime.Cache;
                }
                return _objCache;
            }
        }

		/// <summary>
		/// Cleans the cache key by remove cache key prefix.
		/// </summary>
		/// <param name="CacheKey">The cache key.</param>
		/// <returns>cache key without prefix.</returns>
		/// <exception cref="ArgumentException">cache key is empty.</exception>
        public static string CleanCacheKey(string CacheKey)
        {
            if (String.IsNullOrEmpty(CacheKey))
            {
                throw new ArgumentException("Argument cannot be null or an empty string", "CacheKey");
            }
            return CacheKey.Substring(CachePrefix.Length);
        }

		/// <summary>
		/// Gets the cache key with key prefix.
		/// </summary>
		/// <param name="CacheKey">The cache key.</param>
		/// <returns>CachePrefix + CacheKey</returns>
		/// <exception cref="ArgumentException">Cache key is empty.</exception>
        public static string GetCacheKey(string CacheKey)
        {
            if (string.IsNullOrEmpty(CacheKey))
            {
                throw new ArgumentException("Argument cannot be null or an empty string", "CacheKey");
            }
            return CachePrefix + CacheKey;
        }

		/// <summary>
		/// Instances of  caching provider.
		/// </summary>
		/// <returns>The Implemments provider of cache system defind in web.config.</returns>
        public static CachingProvider Instance()
        {
            return ComponentFactory.GetComponent<CachingProvider>();
        }

        private void ClearCacheInternal(string prefix, bool clearRuntime)
        {
            foreach (DictionaryEntry objDictionaryEntry in HttpRuntime.Cache)
            {
                if (Convert.ToString(objDictionaryEntry.Key).StartsWith(prefix))
                {
                    if (clearRuntime)
                    {
                        RemoveInternal(Convert.ToString(objDictionaryEntry.Key));
                    }
                    else
                    {
                        Remove(Convert.ToString(objDictionaryEntry.Key));
                    }
                }
            }
        }

        private void ClearCacheKeysByPortalInternal(int portalId, bool clearRuntime)
        {
            RemoveFormattedCacheKey(DataCache.PortalCacheKey, clearRuntime, Null.NullInteger, string.Empty);
            RemoveFormattedCacheKey(DataCache.LocalesCacheKey, clearRuntime, portalId);
            RemoveFormattedCacheKey(DataCache.ProfileDefinitionsCacheKey, clearRuntime, portalId);
            RemoveFormattedCacheKey(DataCache.ListsCacheKey, clearRuntime, portalId);
            RemoveFormattedCacheKey(DataCache.SkinsCacheKey, clearRuntime, portalId);
        }

        private void ClearDesktopModuleCacheInternal(int portalId, bool clearRuntime)
        {
            RemoveFormattedCacheKey(DataCache.DesktopModuleCacheKey, clearRuntime, portalId);
            RemoveFormattedCacheKey(DataCache.PortalDesktopModuleCacheKey, clearRuntime, portalId);
            RemoveCacheKey(DataCache.ModuleDefinitionCacheKey, clearRuntime);
            RemoveCacheKey(DataCache.ModuleControlsCacheKey, clearRuntime);
        }

        private void ClearFolderCacheInternal(int portalId, bool clearRuntime)
        {
            RemoveFormattedCacheKey(DataCache.FolderCacheKey, clearRuntime, portalId);
            RemoveFormattedCacheKey(DataCache.FolderPermissionCacheKey, clearRuntime, portalId);
        }

        private void ClearHostCacheInternal(bool clearRuntime)
        {
            RemoveCacheKey(DataCache.HostSettingsCacheKey, clearRuntime);
            RemoveCacheKey(DataCache.SecureHostSettingsCacheKey, clearRuntime);
            RemoveCacheKey(DataCache.PortalAliasCacheKey, clearRuntime);
            RemoveCacheKey("CSS", clearRuntime);
            RemoveCacheKey(DataCache.DesktopModulePermissionCacheKey, clearRuntime);
            RemoveCacheKey("GetRoles", clearRuntime);
            RemoveCacheKey("CompressionConfig", clearRuntime);
            ClearFolderCacheInternal(-1, clearRuntime);
            ClearDesktopModuleCacheInternal(-1, clearRuntime);
            ClearCacheKeysByPortalInternal(-1, clearRuntime);
        }

        private void ClearModuleCacheInternal(int tabId, bool clearRuntime)
        {
            RemoveFormattedCacheKey(DataCache.TabModuleCacheKey, clearRuntime, tabId);
            RemoveFormattedCacheKey(DataCache.ModulePermissionCacheKey, clearRuntime, tabId);
        }

        private void ClearModulePermissionsCachesByPortalInternal(int portalId, bool clearRuntime)
        {
            var objTabs = new TabController();
            foreach (KeyValuePair<int, TabInfo> tabPair in objTabs.GetTabsByPortal(portalId))
            {
                RemoveFormattedCacheKey(DataCache.ModulePermissionCacheKey, clearRuntime, tabPair.Value.TabID);
            }
        }

        private void ClearPortalCacheInternal(int portalId, bool cascade, bool clearRuntime)
        {
            RemoveFormattedCacheKey(DataCache.PortalSettingsCacheKey, clearRuntime, portalId);
            Dictionary<string, Locale> locales = LocaleController.Instance.GetLocales(portalId);
            if (locales == null || locales.Count == 0)
            {
                //At least attempt to remove default locale
                string defaultLocale = PortalController.GetPortalDefaultLanguage(portalId);
                RemoveCacheKey(String.Format(DataCache.PortalCacheKey, portalId, defaultLocale), clearRuntime);
            }
            else
            {
                foreach (Locale portalLocale in LocaleController.Instance.GetLocales(portalId).Values)
                {
                    RemoveCacheKey(String.Format(DataCache.PortalCacheKey, portalId, portalLocale.Code), clearRuntime);
                }
            }
            if (cascade)
            {
                var objTabs = new TabController();
                foreach (KeyValuePair<int, TabInfo> tabPair in objTabs.GetTabsByPortal(portalId))
                {
                    ClearModuleCacheInternal(tabPair.Value.TabID, clearRuntime);
                }
                var moduleController = new ModuleController();
                foreach (ModuleInfo moduleInfo in moduleController.GetModules(portalId))
                {
                    RemoveCacheKey("GetModuleSettings" + moduleInfo.ModuleID, clearRuntime);
                }
            }
            ClearFolderCacheInternal(portalId, clearRuntime);
            ClearCacheKeysByPortalInternal(portalId, clearRuntime);
            ClearDesktopModuleCacheInternal(portalId, clearRuntime);
            ClearTabCacheInternal(portalId, clearRuntime);
        }

        private void ClearTabCacheInternal(int portalId, bool clearRuntime)
        {
            RemoveFormattedCacheKey(DataCache.TabCacheKey, clearRuntime, portalId);
            RemoveFormattedCacheKey(DataCache.TabPermissionCacheKey, clearRuntime, portalId);
            Dictionary<string, Locale> locales = LocaleController.Instance.GetLocales(portalId);
            if (locales == null || locales.Count == 0)
            {
                //At least attempt to remove default locale
                string defaultLocale = PortalController.GetPortalDefaultLanguage(portalId);
                RemoveCacheKey(string.Format(DataCache.TabPathCacheKey, defaultLocale, portalId), clearRuntime);
            }
            else
            {
                foreach (Locale portalLocale in LocaleController.Instance.GetLocales(portalId).Values)
                {
                    RemoveCacheKey(string.Format(DataCache.TabPathCacheKey, portalLocale.Code, portalId), clearRuntime);
                }
            }

            RemoveCacheKey(string.Format(DataCache.TabPathCacheKey, Null.NullString, portalId), clearRuntime);
        }

        private void RemoveCacheKey(string CacheKey, bool clearRuntime)
        {
            if (clearRuntime)
            {
                RemoveInternal(GetCacheKey(CacheKey));
            }
            else
            {
                Remove(GetCacheKey(CacheKey));
            }
        }

        private void RemoveFormattedCacheKey(string CacheKeyBase, bool clearRuntime, params object[] parameters)
        {
            if (clearRuntime)
            {
                RemoveInternal(string.Format(GetCacheKey(CacheKeyBase), parameters));
            }
            else
            {
                Remove(string.Format(GetCacheKey(CacheKeyBase), parameters));
            }
        }

		/// <summary>
		/// Clears the cache internal.
		/// </summary>
		/// <param name="cacheType">Type of the cache.</param>
		/// <param name="data">The data.</param>
		/// <param name="clearRuntime">if set to <c>true</c> clear runtime cache.</param>
        protected void ClearCacheInternal(string cacheType, string data, bool clearRuntime)
        {
            switch (cacheType)
            {
                case "Prefix":
                    ClearCacheInternal(data, clearRuntime);
                    break;
                case "Host":
                    ClearHostCacheInternal(clearRuntime);
                    break;
                case "Folder":
                    ClearFolderCacheInternal(int.Parse(data), clearRuntime);
                    break;
                case "Module":
                    ClearModuleCacheInternal(int.Parse(data), clearRuntime);
                    break;
                case "ModulePermissionsByPortal":
                    ClearModulePermissionsCachesByPortalInternal(int.Parse(data), clearRuntime);
                    break;
                case "Portal":
                    ClearPortalCacheInternal(int.Parse(data), false, clearRuntime);
                    break;
                case "PortalCascade":
                    ClearPortalCacheInternal(int.Parse(data), true, clearRuntime);
                    break;
                case "Tab":
                    ClearTabCacheInternal(int.Parse(data), clearRuntime);
                    break;
            }
        }

		/// <summary>
		/// Removes the internal.
		/// </summary>
		/// <param name="CacheKey">The cache key.</param>
        protected void RemoveInternal(string CacheKey)
        {
            DataCache.RemoveFromPrivateDictionary(CacheKey);
            if (Cache[CacheKey] != null)
            {
                Cache.Remove(CacheKey);
            }
        }

		/// <summary>
		/// Clears the specified type.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="data">The data.</param>
        public virtual void Clear(string type, string data)
        {
            ClearCacheInternal(type, data, false);
        }

        public virtual IDictionaryEnumerator GetEnumerator()
        {
            return Cache.GetEnumerator();
        }

		/// <summary>
		/// Gets the item.
		/// </summary>
		/// <param name="CacheKey">The cache key.</param>
		/// <returns>cache content</returns>
        public virtual object GetItem(string CacheKey)
        {
            return Cache[CacheKey];
        }

		/// <summary>
		/// Inserts the specified cache key.
		/// </summary>
		/// <param name="CacheKey">The cache key.</param>
		/// <param name="objObject">The object.</param>
        public virtual void Insert(string CacheKey, object objObject)
        {
            DNNCacheDependency objDependency = null;
            Insert(CacheKey, objObject, objDependency, System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
        }

		/// <summary>
		/// Inserts the specified cache key.
		/// </summary>
		/// <param name="CacheKey">The cache key.</param>
		/// <param name="objObject">The object.</param>
		/// <param name="objDependency">The dependency.</param>
        public virtual void Insert(string CacheKey, object objObject, DNNCacheDependency objDependency)
        {
            Insert(CacheKey, objObject, objDependency, System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
        }

		/// <summary>
		/// Inserts the specified cache key.
		/// </summary>
		/// <param name="CacheKey">The cache key.</param>
		/// <param name="objObject">The object.</param>
		/// <param name="objDependency">The dependency.</param>
		/// <param name="AbsoluteExpiration">The absolute expiration.</param>
		/// <param name="SlidingExpiration">The sliding expiration.</param>
        public virtual void Insert(string CacheKey, object objObject, DNNCacheDependency objDependency, DateTime AbsoluteExpiration, TimeSpan SlidingExpiration)
        {
            Insert(CacheKey, objObject, objDependency, AbsoluteExpiration, SlidingExpiration, CacheItemPriority.Default, null);
        }

		/// <summary>
		/// Inserts the specified cache key.
		/// </summary>
		/// <param name="CacheKey">The cache key.</param>
		/// <param name="Value">The value.</param>
		/// <param name="objDependency">The dependency.</param>
		/// <param name="AbsoluteExpiration">The absolute expiration.</param>
		/// <param name="SlidingExpiration">The sliding expiration.</param>
		/// <param name="Priority">The priority.</param>
		/// <param name="OnRemoveCallback">The on remove callback.</param>
        public virtual void Insert(string CacheKey, object Value, DNNCacheDependency objDependency, DateTime AbsoluteExpiration, TimeSpan SlidingExpiration, CacheItemPriority Priority,
                                   CacheItemRemovedCallback OnRemoveCallback)
        {
            if (objDependency == null)
            {
                Cache.Insert(CacheKey, Value, null, AbsoluteExpiration, SlidingExpiration, Priority, OnRemoveCallback);
            }
            else
            {
                Cache.Insert(CacheKey, Value, objDependency.SystemCacheDependency, AbsoluteExpiration, SlidingExpiration, Priority, OnRemoveCallback);
            }
        }

		/// <summary>
		/// Determines whether is web farm.
		/// </summary>
		/// <returns>
		///   <c>true</c> if is web farm; otherwise, <c>false</c>.
		/// </returns>
        public virtual bool IsWebFarm()
        {
            return (ServerController.GetEnabledServers().Count > 1);
        }

		/// <summary>
		/// Purges the cache.
		/// </summary>
		/// <returns></returns>
        public virtual string PurgeCache()
        {
            return Localization.Localization.GetString("PurgeCacheUnsupported.Text", Localization.Localization.GlobalResourceFile);
        }

		/// <summary>
		/// Removes the specified cache key.
		/// </summary>
		/// <param name="CacheKey">The cache key.</param>
        public virtual void Remove(string CacheKey)
        {
            RemoveInternal(CacheKey);
        }
    }
}