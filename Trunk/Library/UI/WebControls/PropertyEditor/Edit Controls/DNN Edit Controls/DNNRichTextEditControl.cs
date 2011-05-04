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
using System.Web.UI.WebControls;

using DotNetNuke.Modules.HTMLEditorProvider;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      DNNRichTextEditControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The DNNRichTextEditControl control provides a standard UI component for editing
    /// RichText
    /// </summary>
    /// <history>
    ///     [cnurse]	03/31/2006	created
    /// </history>
    /// -----------------------------------------------------------------------------
    [Obsolete("Deprecated in DNN 5.6.2.  Replaced by control of same name in DotNetNuke.Web assembly")]
    [ToolboxData("<{0}:DNNRichTextEditControl runat=server></{0}:DNNRichTextEditControl>")]
    public class DNNRichTextEditControl : TextEditControl
    {
        private HtmlEditorProvider RichTextEditor;

        protected override void CreateChildControls()
        {
            if (EditMode == PropertyEditorMode.Edit)
            {
                RichTextEditor = HtmlEditorProvider.Instance();
                RichTextEditor.ControlID = ID + "edit";
                RichTextEditor.Initialize();
                RichTextEditor.Height = ControlStyle.Height;
                RichTextEditor.Width = ControlStyle.Width;
                if (RichTextEditor.Height.IsEmpty)
                {
                    RichTextEditor.Height = new Unit(300);
                }
                if (RichTextEditor.Width.IsEmpty)
                {
                    RichTextEditor.Width = new Unit(450);
                }
                Controls.Clear();
                Controls.Add(RichTextEditor.HtmlEditorControl);
            }
            base.CreateChildControls();
        }

        public override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            bool dataChanged = false;
            string presentValue = StringValue;
            string postedValue = RichTextEditor.Text;
            if (!presentValue.Equals(postedValue))
            {
                Value = postedValue;
                dataChanged = true;
            }
            return dataChanged;
        }

        protected override void OnDataChanged(EventArgs e)
        {
            string strValue = Convert.ToString(Value);
            string strOldValue = Convert.ToString(OldValue);
            var args = new PropertyEditorEventArgs(Name);
            args.Value = Page.Server.HtmlEncode(strValue);
            args.OldValue = Page.Server.HtmlEncode(strOldValue);
            args.StringValue = Page.Server.HtmlEncode(StringValue);
            base.OnValueChanged(args);
        }

        protected override void OnInit(EventArgs e)
        {
            EnsureChildControls();
            base.OnInit(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (EditMode == PropertyEditorMode.Edit)
            {
                RichTextEditor.Text = Page.Server.HtmlDecode(Convert.ToString(Value));
            }
            if (Page != null && EditMode == PropertyEditorMode.Edit)
            {
                Page.RegisterRequiresPostBack(this);
            }
        }

        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            RenderChildren(writer);
        }

        protected override void RenderViewMode(HtmlTextWriter writer)
        {
            string propValue = Page.Server.HtmlDecode(Convert.ToString(Value));
            ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write(propValue);
            writer.RenderEndTag();
        }
    }
}
