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
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Installer;

#endregion

namespace DotNetNuke.UI.Skins
{
    public enum SkinParser
    {
        Localized,
        Portable
    }

    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Class	 : SkinFileProcessor
    ///
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///     Handles processing of a list of uploaded skin files into a working skin.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[willhsc]	3/3/2004	Created
    /// </history>
    /// -----------------------------------------------------------------------------
    public class SkinFileProcessor
    {
        private readonly string DUPLICATE_DETAIL = Util.GetLocalizedString("DuplicateSkinObject.Detail");
        private readonly string DUPLICATE_ERROR = Util.GetLocalizedString("DuplicateSkinObject.Error");
        private readonly string FILES_END = Util.GetLocalizedString("EndSkinFiles");
        private readonly string FILE_BEGIN = Util.GetLocalizedString("BeginSkinFile");
        private readonly string FILE_END = Util.GetLocalizedString("EndSkinFile");
        private readonly string INITIALIZE_PROCESSOR = Util.GetLocalizedString("StartProcessor");
        private readonly string LOAD_SKIN_TOKEN = Util.GetLocalizedString("LoadingSkinToken");
        private readonly string PACKAGE_LOAD = Util.GetLocalizedString("PackageLoad");
        private readonly string PACKAGE_LOAD_ERROR = Util.GetLocalizedString("PackageLoad.Error");
        private readonly ControlParser m_ControlFactory;
        private readonly Hashtable m_ControlList = new Hashtable();
        private readonly ObjectParser m_ObjectFactory;
        private readonly PathParser m_PathFactory = new PathParser();
        private readonly XmlDocument m_SkinAttributes = new XmlDocument();
        private readonly string m_SkinName;
        private readonly string m_SkinPath;
        private readonly string m_SkinRoot;
        private string m_Message = "";

        public SkinFileProcessor(string ControlKey, string ControlSrc)
        {
            m_ControlList.Add(ControlKey, ControlSrc);
            m_ControlFactory = new ControlParser(m_ControlList);
            m_ObjectFactory = new ObjectParser(m_ControlList);
        }

        public SkinFileProcessor(string SkinPath, string SkinRoot, string SkinName)
        {
            Message += SkinController.FormatMessage(INITIALIZE_PROCESSOR, SkinRoot + " :: " + SkinName, 0, false);
            m_SkinRoot = SkinRoot;
            m_SkinPath = SkinPath;
            m_SkinName = SkinName;
            string FileName = this.SkinPath + this.SkinRoot + "\\" + this.SkinName + "\\" + SkinRoot.Substring(0, SkinRoot.Length - 1) + ".xml";
            if (File.Exists(FileName))
            {
                try
                {
                    SkinAttributes.Load(FileName);
                    Message += SkinController.FormatMessage(PACKAGE_LOAD, Path.GetFileName(FileName), 2, false);
                }
                catch (Exception ex)
                {
                    DnnLog.Error(ex);
                    Message += SkinController.FormatMessage(string.Format(PACKAGE_LOAD_ERROR, ex.Message), Path.GetFileName(FileName), 2, true);
                }
            }
            string Token;
            foreach (SkinControlInfo objSkinControl in SkinControlController.GetSkinControls().Values)
            {
                Token = objSkinControl.ControlKey.ToUpper();
                if (m_ControlList.ContainsKey(Token))
                {
                    Message += SkinController.FormatMessage(string.Format(DUPLICATE_ERROR, objSkinControl.ControlKey.ToUpper()),
                                                            string.Format(DUPLICATE_DETAIL, m_ControlList[Token], objSkinControl.ControlSrc),
                                                            2,
                                                            true);
                }
                else
                {
                    Message += SkinController.FormatMessage(string.Format(LOAD_SKIN_TOKEN, objSkinControl.ControlKey.ToUpper()), objSkinControl.ControlSrc, 2, false);
                    m_ControlList.Add(Token, objSkinControl.ControlSrc);
                }
            }
            m_ControlFactory = new ControlParser(m_ControlList);
            m_ObjectFactory = new ObjectParser(m_ControlList);
        }

        private PathParser PathFactory
        {
            get
            {
                return m_PathFactory;
            }
        }

        private ControlParser ControlFactory
        {
            get
            {
                return m_ControlFactory;
            }
        }

        private ObjectParser ObjectFactory
        {
            get
            {
                return m_ObjectFactory;
            }
        }

        private XmlDocument SkinAttributes
        {
            get
            {
                return m_SkinAttributes;
            }
        }

        private string Message
        {
            get
            {
                return m_Message;
            }
            set
            {
                m_Message = value;
            }
        }

        public string SkinRoot
        {
            get
            {
                return m_SkinRoot;
            }
        }

        public string SkinPath
        {
            get
            {
                return m_SkinPath;
            }
        }

        public string SkinName
        {
            get
            {
                return m_SkinName;
            }
        }

        public string ProcessFile(string FileName, SkinParser ParseOption)
        {
            string strMessage = SkinController.FormatMessage(FILE_BEGIN, Path.GetFileName(FileName), 0, false);
            var objSkinFile = new SkinFile(SkinRoot, FileName, SkinAttributes);
            switch (objSkinFile.FileExtension)
            {
                case ".htm":
                case ".html":
                    string contents = objSkinFile.Contents;
                    strMessage += ObjectFactory.Parse(ref contents);
                    strMessage += PathFactory.Parse(ref contents, PathFactory.HTMLList, objSkinFile.SkinRootPath, ParseOption);
                    strMessage += ControlFactory.Parse(ref contents, objSkinFile.Attributes);
                    objSkinFile.Contents = contents;
                    var Registrations = new ArrayList();
                    Registrations.AddRange(ControlFactory.Registrations);
                    Registrations.AddRange(ObjectFactory.Registrations);
                    strMessage += objSkinFile.PrependASCXDirectives(Registrations);
                    break;
            }
            objSkinFile.Write();
            strMessage += objSkinFile.Messages;
            strMessage += SkinController.FormatMessage(FILE_END, Path.GetFileName(FileName), 1, false);
            return strMessage;
        }

        public string ProcessList(ArrayList FileList)
        {
            return ProcessList(FileList, SkinParser.Localized);
        }

        public string ProcessList(ArrayList FileList, SkinParser ParseOption)
        {
            foreach (string FileName in FileList)
            {
                Message += ProcessFile(FileName, ParseOption);
            }
            Message += SkinController.FormatMessage(FILES_END, SkinRoot + " :: " + SkinName, 0, false);
            return Message;
        }

        public string ProcessSkin(string SkinSource, XmlDocument SkinAttributes, SkinParser ParseOption)
        {
            var objSkinFile = new SkinFile(SkinSource, SkinAttributes);
            string contents = objSkinFile.Contents;
            Message += ControlFactory.Parse(ref contents, objSkinFile.Attributes);
            Message += objSkinFile.PrependASCXDirectives(ControlFactory.Registrations);
            return contents;
        }

        #region Nested type: ControlParser

        private class ControlParser
        {
            private readonly Hashtable m_ControlList = new Hashtable();
            private readonly string m_InitMessages = "";
            private XmlDocument m_Attributes = new XmlDocument();
            private string m_ParseMessages = "";
            private ArrayList m_RegisterList = new ArrayList();

            public ControlParser(Hashtable ControlList)
            {
                m_ControlList = (Hashtable) ControlList.Clone();
            }

            internal ArrayList Registrations
            {
                get
                {
                    return m_RegisterList;
                }
            }

            private MatchEvaluator Handler
            {
                get
                {
                    return TokenMatchHandler;
                }
            }

            private ArrayList RegisterList
            {
                get
                {
                    return m_RegisterList;
                }
                set
                {
                    m_RegisterList = value;
                }
            }

            private Hashtable ControlList
            {
                get
                {
                    return m_ControlList;
                }
            }

            private XmlDocument Attributes
            {
                get
                {
                    return m_Attributes;
                }
                set
                {
                    m_Attributes = value;
                }
            }

            private string Messages
            {
                get
                {
                    return m_ParseMessages;
                }
                set
                {
                    m_ParseMessages = value;
                }
            }

            public string Parse(ref string Source, XmlDocument Attributes)
            {
                Messages = m_InitMessages;
                this.Attributes = Attributes;
                RegisterList.Clear();
                var FindTokenInstance = new Regex("\\[\\s*(?<token>\\w*)\\s*:?\\s*(?<instance>\\w*)\\s*]", RegexOptions.IgnoreCase);
                Source = FindTokenInstance.Replace(Source, Handler);
                return Messages;
            }

            private string TokenMatchHandler(Match m)
            {
                string TOKEN_PROC = Util.GetLocalizedString("ProcessToken");
                string TOKEN_SKIN = Util.GetLocalizedString("SkinToken");
                string TOKEN_PANE = Util.GetLocalizedString("PaneToken");
                string TOKEN_FOUND = Util.GetLocalizedString("TokenFound");
                string TOKEN_FORMAT = Util.GetLocalizedString("TokenFormat");
                string TOKEN_NOTFOUND_INFILE = Util.GetLocalizedString("TokenNotFoundInFile");
                string CONTROL_FORMAT = Util.GetLocalizedString("ControlFormat");
                string TOKEN_NOTFOUND = Util.GetLocalizedString("TokenNotFound");
                string Token = m.Groups["token"].Value.ToUpper();
                string ControlName = Token + m.Groups["instance"].Value;
                string AttributeNode = Token + (String.IsNullOrEmpty(m.Groups["instance"].Value) ? "" : ":" + m.Groups["instance"].Value);
                Messages += SkinController.FormatMessage(TOKEN_PROC, "[" + AttributeNode + "]", 2, false);
                if (ControlList.ContainsKey(Token) || Token.IndexOf("CONTENTPANE") != -1)
                {
                    string SkinControl = "";
                    if (ControlList.ContainsKey(Token))
                    {
                        Messages += SkinController.FormatMessage(TOKEN_SKIN, (string) ControlList[Token], 2, false);
                    }
                    else
                    {
                        Messages += SkinController.FormatMessage(TOKEN_PANE, Token, 2, false);
                    }
                    if (Attributes.DocumentElement != null)
                    {
                        XmlNode xmlSkinAttributeRoot = Attributes.DocumentElement.SelectSingleNode("descendant::Object[Token='[" + AttributeNode + "]']");
                        if (xmlSkinAttributeRoot != null)
                        {
                            Messages += SkinController.FormatMessage(TOKEN_FOUND, "[" + AttributeNode + "]", 2, false);
                            foreach (XmlNode xmlSkinAttribute in xmlSkinAttributeRoot.SelectNodes(".//Settings/Setting"))
                            {
                                if (!String.IsNullOrEmpty(xmlSkinAttribute.SelectSingleNode("Value").InnerText))
                                {
                                    Messages += SkinController.FormatMessage(TOKEN_FORMAT,
                                                                             xmlSkinAttribute.SelectSingleNode("Name").InnerText + "=\"" + xmlSkinAttribute.SelectSingleNode("Value").InnerText + "\"",
                                                                             2,
                                                                             false);
                                    SkinControl += " " + xmlSkinAttribute.SelectSingleNode("Name").InnerText + "=\"" + xmlSkinAttribute.SelectSingleNode("Value").InnerText.Replace("\"", "&quot;") +
                                                   "\"";
                                }
                            }
                        }
                        else
                        {
                            Messages += SkinController.FormatMessage(TOKEN_NOTFOUND_INFILE, "[" + AttributeNode + "]", 2, false);
                        }
                    }
                    if (ControlList.ContainsKey(Token))
                    {
                        SkinControl = "dnn:" + Token + " runat=\"server\" id=\"dnn" + ControlName + "\"" + SkinControl;
                        string ControlRegistration = "<%@ Register TagPrefix=\"dnn\" TagName=\"" + Token + "\" Src=\"~/" + (string) ControlList[Token] + "\" %>" + Environment.NewLine;
                        if (RegisterList.Contains(ControlRegistration) == false)
                        {
                            RegisterList.Add(ControlRegistration);
                        }
                        Messages += SkinController.FormatMessage(CONTROL_FORMAT, "&lt;" + SkinControl + " /&gt;", 2, false);
                        SkinControl = "<" + SkinControl + " />";
                    }
                    else
                    {
                        if (SkinControl.ToLower().IndexOf("id=") == -1)
                        {
                            SkinControl = " id=\"ContentPane\"";
                        }
                        SkinControl = "div runat=\"server\"" + SkinControl + "></div";
                        Messages += SkinController.FormatMessage(CONTROL_FORMAT, "&lt;" + SkinControl + "&gt;", 2, false);
                        SkinControl = "<" + SkinControl + ">";
                    }
                    return SkinControl;
                }
                else
                {
                    Messages += SkinController.FormatMessage(TOKEN_NOTFOUND, "[" + m.Groups["token"].Value + "]", 2, false);
                    return "[" + m.Groups["token"].Value + "]";
                }
            }
        }

        #endregion

        #region Nested type: ObjectParser

        private class ObjectParser
        {
            private readonly Hashtable m_ControlList = new Hashtable();
            private readonly string m_InitMessages = "";
            private string m_ParseMessages = "";
            private ArrayList m_RegisterList = new ArrayList();

            public ObjectParser(Hashtable ControlList)
            {
                m_ControlList = (Hashtable) ControlList.Clone();
            }

            internal ArrayList Registrations
            {
                get
                {
                    return m_RegisterList;
                }
            }

            private MatchEvaluator Handler
            {
                get
                {
                    return ObjectMatchHandler;
                }
            }

            private ArrayList RegisterList
            {
                get
                {
                    return m_RegisterList;
                }
                set
                {
                    m_RegisterList = value;
                }
            }

            private Hashtable ControlList
            {
                get
                {
                    return m_ControlList;
                }
            }

            private string Messages
            {
                get
                {
                    return m_ParseMessages;
                }
                set
                {
                    m_ParseMessages = value;
                }
            }

            public string Parse(ref string Source)
            {
                Messages = m_InitMessages;
                RegisterList.Clear();
                var FindObjectInstance = new Regex("\\<object(?<token>.*?)</object>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                Source = FindObjectInstance.Replace(Source, Handler);
                return Messages;
            }

            private string ObjectMatchHandler(Match m)
            {
                string OBJECT_PROC = Util.GetLocalizedString("ProcessObject");
                string OBJECT_SKIN = Util.GetLocalizedString("SkinObject");
                string OBJECT_PANE = Util.GetLocalizedString("PaneObject");
                string OBJECT_FOUND = Util.GetLocalizedString("ObjectFound");
                string CONTROL_FORMAT = Util.GetLocalizedString("ControlFormat");
                string OBJECT_NOTFOUND = Util.GetLocalizedString("ObjectNotFound");
                string EmbeddedObjectAttributes = m.Groups["token"].Value.Substring(0, m.Groups["token"].Value.IndexOf(">"));
                string[] Attributes = EmbeddedObjectAttributes.Split(' ');
                string AttributeNode = "";
                string Token = "";
                string ControlName = "";
                string[] Attribute;
                string AttributeName;
                string AttributeValue;
                foreach (string strAttribute in Attributes)
                {
                    if (strAttribute != string.Empty)
                    {
                        Attribute = strAttribute.Split('=');
                        AttributeName = Attribute[0].Trim();
                        AttributeValue = Attribute[1].Trim().Replace("\"", "");
                        switch (AttributeName.ToLower())
                        {
                            case "id":
                                ControlName = AttributeValue;
                                break;
                            case "codetype":
                                AttributeNode = AttributeValue;
                                break;
                            case "codebase":
                                Token = AttributeValue.ToUpper();
                                break;
                        }
                    }
                }
                if (AttributeNode.ToLower() == "dotnetnuke/server")
                {
                    Messages += SkinController.FormatMessage(OBJECT_PROC, Token, 2, false);
                    if (ControlList.ContainsKey(Token) || Token == "CONTENTPANE")
                    {
                        string SkinControl = "";
                        if (ControlList.ContainsKey(Token))
                        {
                            Messages += SkinController.FormatMessage(OBJECT_SKIN, (string) ControlList[Token], 2, false);
                        }
                        else
                        {
                            Messages += SkinController.FormatMessage(OBJECT_PANE, Token, 2, false);
                        }
                        string Parameters = m.Groups["token"].Value.Substring(m.Groups["token"].Value.IndexOf(">") + 1);
                        Parameters = Parameters.Replace("<param name=\"", "");
                        Parameters = Parameters.Replace("\" value", "");
                        Parameters = Parameters.Replace("/>", "");
                        Parameters = Regex.Replace(Parameters, "\\s+", " ");
                        if (ControlList.ContainsKey(Token))
                        {
                            SkinControl = "dnn:" + Token + " runat=\"server\" ";
                            if (!String.IsNullOrEmpty(ControlName))
                            {
                                SkinControl += "id=\"" + ControlName + "\" ";
                            }
                            SkinControl += Parameters;
                            string ControlRegistration = "<%@ Register TagPrefix=\"dnn\" TagName=\"" + Token + "\" Src=\"~/" + (string) ControlList[Token] + "\" %>" + Environment.NewLine;
                            if (RegisterList.Contains(ControlRegistration) == false)
                            {
                                RegisterList.Add(ControlRegistration);
                            }
                            Messages += SkinController.FormatMessage(CONTROL_FORMAT, "&lt;" + SkinControl + " /&gt;", 2, false);
                            SkinControl = "<" + SkinControl + "/>";
                        }
                        else
                        {
                            SkinControl = "div runat=\"server\" ";
                            if (!String.IsNullOrEmpty(ControlName))
                            {
                                SkinControl += "id=\"" + ControlName + "\" ";
                            }
                            else
                            {
                                SkinControl += "id=\"ContentPane\" ";
                            }
                            SkinControl += Parameters + "></div";
                            Messages += SkinController.FormatMessage(CONTROL_FORMAT, "&lt;" + SkinControl + "&gt;", 2, false);
                            SkinControl = "<" + SkinControl + ">";
                        }
                        return SkinControl;
                    }
                    else
                    {
                        Messages += SkinController.FormatMessage(OBJECT_NOTFOUND, Token, 2, false);
                        return "<object" + m.Groups["token"].Value + "</object>";
                    }
                }
                else
                {
                    return "<object" + m.Groups["token"].Value + "</object>";
                }
            }
        }

        #endregion

        #region Nested type: PathParser

        private class PathParser
        {
            private readonly string SUBST = Util.GetLocalizedString("Substituting");
            private readonly string SUBST_DETAIL = Util.GetLocalizedString("Substituting.Detail");
            private readonly ArrayList m_CSSPatterns = new ArrayList();
            private readonly ArrayList m_HTMLPatterns = new ArrayList();
            private string m_Messages = "";
            private string m_SkinPath = "";

            public ArrayList HTMLList
            {
                get
                {
                    if (m_HTMLPatterns.Count == 0)
                    {
                        string[] arrPattern = {
                                                  "(?<tag><head[^>]*?\\sprofile\\s*=\\s*\")(?!https://|http://|\\\\|[~/])(?<content>[^\"]*)(?<endtag>\"[^>]*>)",
                                                  "(?<tag><object[^>]*?\\s(?:codebase|data|usemap)\\s*=\\s*\")(?!https://|http://|\\\\|[~/])(?<content>[^\"]*)(?<endtag>\"[^>]*>)",
                                                  "(?<tag><img[^>]*?\\s(?:src|longdesc|usemap)\\s*=\\s*\")(?!https://|http://|\\\\|[~/])(?<content>[^\"]*)(?<endtag>\"[^>]*>)",
                                                  "(?<tag><input[^>]*?\\s(?:src|usemap)\\s*=\\s*\")(?!https://|http://|\\\\|[~/])(?<content>[^\"]*)(?<endtag>\"[^>]*>)",
                                                  "(?<tag><iframe[^>]*?\\s(?:src|longdesc)\\s*=\\s*\")(?!https://|http://|\\\\|[~/])(?<content>[^\"]*)(?<endtag>\"[^>]*>)",
                                                  "(?<tag><(?:td|th|table|body)[^>]*?\\sbackground\\s*=\\s*\")(?!https://|http://|\\\\|[~/])(?<content>[^\"]*)(?<endtag>\"[^>]*>)",
                                                  "(?<tag><(?:script|bgsound|embed|xml|frame)[^>]*?\\ssrc\\s*=\\s*\")(?!https://|http://|\\\\|[~/])(?<content>[^\"]*)(?<endtag>\"[^>]*>)",
                                                  "(?<tag><(?:base|link|a|area)[^>]*?\\shref\\s*=\\s*\")(?!https://|http://|\\\\|[~/]|javascript:|mailto:)(?<content>[^\"]*)(?<endtag>\"[^>]*>)",
                                                  "(?<tag><(?:blockquote|ins|del|q)[^>]*?\\scite\\s*=\\s*\")(?!https://|http://|\\\\|[~/])(?<content>[^\"]*)(?<endtag>\"[^>]*>)",
                                                  "(?<tag><(?:param\\s+name\\s*=\\s*\"(?:movie|src|base)\")[^>]*?\\svalue\\s*=\\s*\")(?!https://|http://|\\\\|[~/])(?<content>[^\"]*)(?<endtag>\"[^>]*>)",
                                                  "(?<tag><embed[^>]*?\\s(?:src)\\s*=\\s*\")(?!https://|http://|\\\\|[~/])(?<content>[^\"]*)(?<endtag>\"[^>]*>)"
                                              };
                        int i;
                        for (i = 0; i <= arrPattern.GetLength(0) - 1; i++)
                        {
                            var re = new Regex(arrPattern[i], RegexOptions.Multiline | RegexOptions.IgnoreCase);
                            m_HTMLPatterns.Add(re);
                        }
                        m_HTMLPatterns.TrimToSize();
                    }
                    return m_HTMLPatterns;
                }
            }

            public ArrayList CSSList
            {
                get
                {
                    if (m_CSSPatterns.Count == 0)
                    {
                        string[] arrPattern = {"(?<tag>\\surl\\u0028)(?<content>[^\\u0029]*)(?<endtag>\\u0029.*;)"};
                        int i;
                        for (i = 0; i <= arrPattern.GetLength(0) - 1; i++)
                        {
                            var re = new Regex(arrPattern[i], RegexOptions.Multiline | RegexOptions.IgnoreCase);
                            m_CSSPatterns.Add(re);
                        }
                        m_CSSPatterns.TrimToSize();
                    }
                    return m_CSSPatterns;
                }
            }

            private MatchEvaluator Handler
            {
                get
                {
                    return MatchHandler;
                }
            }

            private string SkinPath
            {
                get
                {
                    return m_SkinPath;
                }
                set
                {
                    m_SkinPath = value;
                }
            }

            private SkinParser ParseOption { get; set; }

            public string Parse(ref string Source, ArrayList RegexList, string SkinPath, SkinParser ParseOption)
            {
                m_Messages = "";
                this.SkinPath = SkinPath;
                this.ParseOption = ParseOption;
                int i;
                for (i = 0; i <= RegexList.Count - 1; i++)
                {
                    Source = ((Regex) RegexList[i]).Replace(Source, Handler);
                }
                return m_Messages;
            }

            private string MatchHandler(Match m)
            {
                string strOldTag = m.Groups["tag"].Value + m.Groups["content"].Value + m.Groups["endtag"].Value;
                string strNewTag = strOldTag;
                if (!m.Groups[0].Value.ToLower().Contains("codetype=\"dotnetnuke/client\""))
                {
                    switch (ParseOption)
                    {
                        case SkinParser.Localized:
                            if (strNewTag.IndexOf(SkinPath) == -1)
                            {
                                strNewTag = m.Groups["tag"].Value + SkinPath + m.Groups["content"].Value + m.Groups["endtag"].Value;
                            }
                            break;
                        case SkinParser.Portable:
                            if (strNewTag.ToLower().IndexOf("<%= skinpath %>") == -1)
                            {
                                strNewTag = m.Groups["tag"].Value + "<%= SkinPath %>" + m.Groups["content"].Value + m.Groups["endtag"].Value;
                            }
                            if (strNewTag.IndexOf(SkinPath) != -1)
                            {
                                strNewTag = strNewTag.Replace(SkinPath, "");
                            }
                            break;
                    }
                }
                m_Messages += SkinController.FormatMessage(SUBST, string.Format(SUBST_DETAIL, HttpUtility.HtmlEncode(strOldTag), HttpUtility.HtmlEncode(strNewTag)), 2, false);
                return strNewTag;
            }
        }

        #endregion

        #region Nested type: SkinFile

        private class SkinFile
        {
            private readonly string CONTROL_DIR = Util.GetLocalizedString("ControlDirective");
            private readonly string CONTROL_REG = Util.GetLocalizedString("ControlRegister");
            private readonly string FILE_FORMAT_ERROR = Util.GetLocalizedString("FileFormat.Error");
            private readonly string FILE_LOAD = Util.GetLocalizedString("SkinFileLoad");
            private readonly string FILE_LOAD_ERROR = Util.GetLocalizedString("SkinFileLoad.Error");
            private readonly string FILE_WRITE = Util.GetLocalizedString("FileWrite");
            private readonly XmlDocument m_FileAttributes;
            private readonly string m_FileExtension;
            private readonly string m_FileName;
            private readonly string m_SkinRoot;
            private readonly string m_SkinRootPath;
            private readonly string m_WriteFileName;
            private string FILE_FORMAT_DETAIL = Util.GetLocalizedString("FileFormat.Detail");
            private string m_Messages = "";

            public SkinFile(string SkinContents, XmlDocument SkinAttributes)
            {
                m_FileAttributes = SkinAttributes;
                Contents = SkinContents;
            }

            public SkinFile(string SkinRoot, string FileName, XmlDocument SkinAttributes)
            {
                m_FileName = FileName;
                m_FileExtension = Path.GetExtension(FileName);
                m_SkinRoot = SkinRoot;
                m_FileAttributes = SkinAttributes;
                string strTemp = FileName.Replace(Path.GetFileName(FileName), "");
                strTemp = strTemp.Replace("\\", "/");
                m_SkinRootPath = Globals.ApplicationPath + strTemp.Substring(0, strTemp.ToUpper().IndexOf("/PORTALS"));
                Contents = Read(FileName);
                switch (FileExtension)
                {
                    case ".htm":
                    case ".html":
                        m_WriteFileName = FileName.Replace(Path.GetExtension(FileName), ".ascx");
                        var PaneCheck1 = new Regex("\\s*id\\s*=\\s*\"" + Globals.glbDefaultPane + "\"", RegexOptions.IgnoreCase);
                        var PaneCheck2 = new Regex("\\s*[" + Globals.glbDefaultPane + "]", RegexOptions.IgnoreCase);
                        if (PaneCheck1.IsMatch(Contents) == false && PaneCheck2.IsMatch(Contents) == false)
                        {
                            m_Messages += SkinController.FormatMessage(FILE_FORMAT_ERROR, string.Format(FILE_FORMAT_ERROR, FileName), 2, true);
                        }
                        if (File.Exists(FileName.Replace(FileExtension, ".xml")))
                        {
                            try
                            {
                                m_FileAttributes.Load(FileName.Replace(FileExtension, ".xml"));
                                m_Messages += SkinController.FormatMessage(FILE_LOAD, FileName, 2, false);
                            }
                            catch (Exception exc)
                            {
                                DnnLog.Error(exc);
                                m_FileAttributes = SkinAttributes;
                                m_Messages += SkinController.FormatMessage(FILE_LOAD_ERROR, FileName, 2, true);
                            }
                        }
                        break;
                    default:
                        m_WriteFileName = FileName;
                        break;
                }
            }

            public string SkinRoot
            {
                get
                {
                    return m_SkinRoot;
                }
            }

            public XmlDocument Attributes
            {
                get
                {
                    return m_FileAttributes;
                }
            }

            public string Messages
            {
                get
                {
                    return m_Messages;
                }
            }

            public string FileName
            {
                get
                {
                    return m_FileName;
                }
            }

            public string WriteFileName
            {
                get
                {
                    return m_WriteFileName;
                }
            }

            public string FileExtension
            {
                get
                {
                    return m_FileExtension;
                }
            }

            public string SkinRootPath
            {
                get
                {
                    return m_SkinRootPath;
                }
            }

            public string Contents { get; set; }

            private string Read(string FileName)
            {
                var objStreamReader = new StreamReader(FileName);
                string strFileContents = objStreamReader.ReadToEnd();
                objStreamReader.Close();
                return strFileContents;
            }

            public void Write()
            {
                if (File.Exists(WriteFileName))
                {
                    File.Delete(WriteFileName);
                }
                m_Messages += SkinController.FormatMessage(FILE_WRITE, Path.GetFileName(WriteFileName), 2, false);
                var objStreamWriter = new StreamWriter(WriteFileName);
                objStreamWriter.WriteLine(Contents);
                objStreamWriter.Flush();
                objStreamWriter.Close();
            }

            public string PrependASCXDirectives(ArrayList Registrations)
            {
                string Messages = "";
                string Prefix = "";
                string strPattern = "<\\s*body[^>]*>(?<skin>.*)<\\s*/\\s*body\\s*>";
                Match objMatch;
                objMatch = Regex.Match(Contents, strPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (!String.IsNullOrEmpty(objMatch.Groups[1].Value))
                {
                    Contents = objMatch.Groups[1].Value;
                }
                if (SkinRoot == SkinController.RootSkin)
                {
                    Prefix += "<%@ Control language=\"vb\" AutoEventWireup=\"false\" Explicit=\"True\" Inherits=\"DotNetNuke.UI.Skins.Skin\" %>" + Environment.NewLine;
                }
                else if (SkinRoot == SkinController.RootContainer)
                {
                    Prefix += "<%@ Control language=\"vb\" AutoEventWireup=\"false\" Explicit=\"True\" Inherits=\"DotNetNuke.UI.Containers.Container\" %>" + Environment.NewLine;
                }
                Messages += SkinController.FormatMessage(CONTROL_DIR, HttpUtility.HtmlEncode(Prefix), 2, false);
                foreach (string Item in Registrations)
                {
                    Messages += SkinController.FormatMessage(CONTROL_REG, HttpUtility.HtmlEncode(Item), 2, false);
                    Prefix += Item;
                }
                Contents = Prefix + Contents;
                return Messages;
            }
        }

        #endregion
    }
}
