#region Copyright

// 
// DotNetNukeŽ - http://www.dotnetnuke.com
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
using System.Text;
using System.Web.Caching;
using System.Web.UI;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Cache;

#endregion

namespace DotNetNuke.Framework
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Framework
    /// Project:    DotNetNuke
    /// Class:      CachePageStatePersister
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// CachePageStatePersister provides a cache based page state peristence mechanism
    /// </summary>
    /// <history>
    ///		[cnurse]	11/30/2006	documented
    /// </history>
    /// -----------------------------------------------------------------------------
    public class CachePageStatePersister : PageStatePersister
    {
        private const string VIEW_STATE_CACHEKEY = "__VIEWSTATE_CACHEKEY";

        public CachePageStatePersister(Page page) : base(page)
        {
        }

        public override void Load()
        {
            string key = Page.Request.Params[VIEW_STATE_CACHEKEY];
            if (string.IsNullOrEmpty(key) || !key.StartsWith("VS_"))
            {
                throw new ApplicationException("Missing valid " + VIEW_STATE_CACHEKEY);
            }
            var state = DataCache.GetCache<Pair>(key);
            if (state != null)
            {
                ViewState = state.First;
                ControlState = state.Second;
            }
            DataCache.RemoveCache(key);
        }

        public override void Save()
        {
            if (ViewState == null && ControlState == null)
            {
                return;
            }
            var key = new StringBuilder();
            {
                key.Append("VS_");
                key.Append(Page.Session == null ? Guid.NewGuid().ToString() : Page.Session.SessionID);
                key.Append("_");
                key.Append(DateTime.Now.Ticks.ToString());
            }
            var state = new Pair(ViewState, ControlState);
            DNNCacheDependency objDependency = null;
            DataCache.SetCache(key.ToString(), state, objDependency, DateTime.Now.AddMinutes(Page.Session.Timeout), Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);
            Page.ClientScript.RegisterHiddenField(VIEW_STATE_CACHEKEY, key.ToString());
        }
    }
}
