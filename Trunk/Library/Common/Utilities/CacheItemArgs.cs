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

using System.Collections;
using System.Web.Caching;

using DotNetNuke.Services.Cache;

#endregion

namespace DotNetNuke.Common.Utilities
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Common.Utilities
    /// Class:      CacheItemArgs
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The CacheItemArgs class provides an EventArgs implementation for the
    /// CacheItemExpiredCallback delegate
    /// </summary>
    /// <history>
    ///     [cnurse]	01/12/2008	created
    /// </history>
    /// -----------------------------------------------------------------------------
    public class CacheItemArgs
    {
        private ArrayList _paramList;

        public CacheItemArgs(string key)
        {
            CacheKey = key;
            CacheTimeOut = 20;
            CachePriority = CacheItemPriority.Default;
            //_ParamList = new ArrayList();
        }

        public CacheItemArgs(string key, int timeout)
            : this(key)
        {
            CacheTimeOut = timeout;
            CachePriority = CacheItemPriority.Default;
            //_ParamList = new ArrayList();
        }

        public CacheItemArgs(string key, CacheItemPriority priority)
            : this(key)
        {
            CachePriority = priority;
            //_ParamList = new ArrayList();
        }

        public CacheItemArgs(string key, int timeout, CacheItemPriority priority)
            : this(key)
        {
            CacheTimeOut = timeout;
            CachePriority = priority;
        }

        public CacheItemArgs(string key, int timeout, CacheItemPriority priority, params object[] parameters)
            : this(key)
        {
            CacheTimeOut = timeout;
            CachePriority = priority;
            Params = parameters;
        }

        public CacheItemRemovedCallback CacheCallback { get; set; }

        public DNNCacheDependency CacheDependency { get; set; }

        public string CacheKey { get; private set; }

        public CacheItemPriority CachePriority { get; set; }

        public int CacheTimeOut { get; set; }

        public ArrayList ParamList
        {
            get
            {
                if (_paramList == null)
                {
                    _paramList = new ArrayList();
                    foreach (object param in Params)
                    {
                        _paramList.Add(param);
                    }
                }

                return _paramList;
            }
        }

        public object[] Params { get; private set; }

        public string ProcedureName { get; set; }
    }
}
