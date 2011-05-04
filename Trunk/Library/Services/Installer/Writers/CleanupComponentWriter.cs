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

using System.Collections.Generic;
using System.IO;
using System.Xml;

#endregion

namespace DotNetNuke.Services.Installer.Writers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The CleanupComponentWriter class handles creating the manifest for Cleanup
    /// Component(s)
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	02/21/2008	created
    /// </history>
    /// -----------------------------------------------------------------------------
    public class CleanupComponentWriter
    {
        private readonly SortedList<string, InstallFile> _Files;
        private string _BasePath;

        public CleanupComponentWriter(string basePath, SortedList<string, InstallFile> files)
        {
            _Files = files;
            _BasePath = basePath;
        }

        public virtual void WriteManifest(XmlWriter writer)
        {
            foreach (KeyValuePair<string, InstallFile> kvp in _Files)
            {
                writer.WriteStartElement("component");
                writer.WriteAttributeString("type", "Cleanup");
                writer.WriteAttributeString("fileName", kvp.Value.Name);
                writer.WriteAttributeString("version", Path.GetFileNameWithoutExtension(kvp.Value.Name));
                writer.WriteEndElement();
            }
        }
    }
}