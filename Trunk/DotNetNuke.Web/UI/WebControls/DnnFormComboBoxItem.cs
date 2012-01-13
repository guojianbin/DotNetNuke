﻿#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
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
using System.Collections;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Web.UI.WebControls.Extensions;

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnFormComboBoxItem : DnnFormListItemBase
    {
        private DropDownList _comboBox;

        private void IndexChanged(object sender, EventArgs e)
        {
            UpdateDataSource(Value, _comboBox.SelectedValue, DataField);
        }

        protected override void BindList()
        {
            BindListInternal(_comboBox, Value, ListSource, ListTextField, ListValueField);
        }

        internal static void BindListInternal(DropDownList comboBox, object value, IEnumerable listSource, string textField, string valueField)
        {
            if (comboBox != null)
            {
                string selectedValue = !comboBox.Page.IsPostBack ? Convert.ToString(value) : comboBox.SelectedValue;

                if (listSource is Dictionary<string, string>)
                {
                    var items = listSource as Dictionary<string, string>;
                    foreach (var item in items)
                    {
                        comboBox.Items.Add(new ListItem(item.Key, item.Value));
                    }
                }
                else
                {
                    comboBox.DataTextField = textField;
                    comboBox.DataValueField = valueField;
                    comboBox.DataSource = listSource;

                    comboBox.DataBind();
                }

                //Reset SelectedValue
                comboBox.Select(selectedValue);
            }
        }

        protected override WebControl CreateControlInternal(Control container)
        {
            _comboBox = new DropDownList { ID = ID + "_ComboBox" };
            _comboBox.SelectedIndexChanged += IndexChanged;
            container.Controls.Add(_comboBox);

            if (ListSource != null)
            {
                BindListInternal(_comboBox, Value, ListSource, ListTextField, ListValueField);
            }

            return _comboBox;
        }
    }
}