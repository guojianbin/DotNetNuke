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
using System.Collections.Generic;
using System.IO;
using System.Xml;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Installer.Packages;

#endregion

namespace DotNetNuke.Services.Installer.Writers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ScriptComponentWriter class handles creating the manifest for Script
    /// Component(s)
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	02/11/2008	created
    /// </history>
    /// -----------------------------------------------------------------------------
    public class ScriptComponentWriter : FileComponentWriter
    {
        public ScriptComponentWriter(string basePath, Dictionary<string, InstallFile> scripts, PackageInfo package) : base(basePath, scripts, package)
        {
        }

        protected override string CollectionNodeName
        {
            get
            {
                return "scripts";
            }
        }

        protected override string ComponentType
        {
            get
            {
                return "Script";
            }
        }

        protected override string ItemNodeName
        {
            get
            {
                return "script";
            }
        }

        protected override void WriteFileElement(XmlWriter writer, InstallFile file)
        {
            Log.AddInfo(string.Format(Util.WRITER_AddFileToManifest, file.Name));
            string type = "Install";
            string version = Null.NullString;
            string fileName = Path.GetFileNameWithoutExtension(file.Name);
            if (fileName.ToLower() == "uninstall")
            {
                type = "UnInstall";
                version = Package.Version.ToString(3);
            }
            else if (fileName.ToLower() == "install")
            {
                type = "Install";
                version = new Version(0, 0, 0).ToString(3);
            }
            else if (fileName.StartsWith("Install"))
            {
                type = "Install";
                version = fileName.Replace("Install.", "");
            }
            else
            {
                type = "Install";
                version = fileName;
            }
            writer.WriteStartElement(ItemNodeName);
            writer.WriteAttributeString("type", type);
            if (!string.IsNullOrEmpty(file.Path))
            {
                writer.WriteElementString("path", file.Path);
            }
            writer.WriteElementString("name", file.Name);
            if (!string.IsNullOrEmpty(file.SourceFileName))
            {
                writer.WriteElementString("sourceFileName", file.SourceFileName);
            }
            writer.WriteElementString("version", version);
            writer.WriteEndElement();
        }
    }
}