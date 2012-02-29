#region Copyright
// 
// DotNetNukeŽ - http://www.dotnetnuke.com
// Copyright (c) 2002-2012
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
using System.Data;
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Services.Social.Messaging.Views
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Messaging.Views
    /// Class:      MessageFileView
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The MessageFileView class contains details about the attachment
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class MessageFileView
    {
        /// <summary>
        /// The name of the file with extension
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }

        private long _size;

        /// <summary>
        /// Size of the File with Unit, e.g. 100 B, 12 KB, 200 MB, etc.
        /// </summary>
        [XmlAttribute]
        public string Size
        {
            get { return FormatBytes(_size); }
            set { long.TryParse(value, out _size); }
        }

        /// <summary>
        /// Url of the file to download
        /// </summary>
        [XmlAttribute]
        public string Url { get; set; }

        private static string FormatBytes(long bytes)
        {
            const int scale = 1024;
            var orders = new[] { "GB", "MB", "KB", "B" };
            var max = (long)Math.Pow(scale, orders.Length - 1);

            foreach (var order in orders)
            {
                if (bytes > max)
                    return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);

                max /= scale;
            }
            return "0 B";
        }
    }
}
