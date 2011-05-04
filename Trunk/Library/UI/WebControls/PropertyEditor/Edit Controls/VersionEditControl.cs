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
using System.Collections.Specialized;
using System.Web.UI;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      VersionEditControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The VersionEditControl control provides a standard UI component for editing
    /// System.Version properties.
    /// </summary>
    /// <history>
    ///     [cnurse]	02/21/2008	created
    /// </history>
    /// -----------------------------------------------------------------------------
    [ToolboxData("<{0}:VersionEditControl runat=server></{0}:VersionEditControl>")]
    public class VersionEditControl : EditControl
    {
        protected override string StringValue
        {
            get
            {
                return Value.ToString();
            }
            set
            {
                Value = new Version(value);
            }
        }

        protected Version Version
        {
            get
            {
                return Value as Version;
            }
        }

        protected void RenderDropDownList(HtmlTextWriter writer, string type, int val)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID + "_" + type);
            writer.AddStyleAttribute("width", "40px");
            writer.RenderBeginTag(HtmlTextWriterTag.Select);
            for (int i = 0; i <= 99; i++)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Value, i.ToString());
                if (val == i)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Option);
                writer.Write(i.ToString("00"));
                writer.RenderEndTag();
            }
            writer.RenderEndTag();
        }

        protected override void OnDataChanged(EventArgs e)
        {
            var args = new PropertyEditorEventArgs(Name);
            args.Value = Value;
            args.OldValue = OldValue;
            args.StringValue = StringValue;
            base.OnValueChanged(args);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (Page != null && EditMode == PropertyEditorMode.Edit)
            {
                Page.RegisterRequiresPostBack(this);
            }
        }

        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            RenderDropDownList(writer, "Major", Version.Major);
            writer.Write("&nbsp;");
            RenderDropDownList(writer, "Minor", Version.Minor);
            writer.Write("&nbsp;");
            RenderDropDownList(writer, "Build", Version.Build);
            writer.RenderEndTag();
        }

        protected override void RenderViewMode(HtmlTextWriter writer)
        {
            ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            if (Version != null)
            {
                writer.Write(Version.ToString(3));
            }
            writer.RenderEndTag();
        }

        public override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            string majorVersion = postCollection[postDataKey + "_Major"];
            string minorVersion = postCollection[postDataKey + "_Minor"];
            string buildVersion = postCollection[postDataKey + "_Build"];
            bool dataChanged = false;
            Version presentValue = Version;
            var postedValue = new Version(majorVersion + "." + minorVersion + "." + buildVersion);
            if (!presentValue.Equals(postedValue))
            {
                Value = postedValue;
                dataChanged = true;
            }
            return dataChanged;
        }
    }
}
