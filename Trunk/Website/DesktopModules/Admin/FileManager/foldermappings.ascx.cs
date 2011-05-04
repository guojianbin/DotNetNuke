﻿#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
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

using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Utilities;
using DotNetNuke.UI.WebControls;

using Telerik.Web.UI;

using Globals = DotNetNuke.Common.Globals;

namespace DotNetNuke.Modules.Admin.FileManager
{
    public partial class FolderMappings : PortalModuleBase
    {
        #region Private Variables

        private readonly IFolderMappingController _folderMappingController = FolderMappingController.Instance;


        #endregion

        #region Properties
        public int FolderPortalID
        {
            get
            {
                return IsHostMenu ? Null.NullInteger : PortalId;
            }
        }

        protected List<FolderMappingInfo> FolderMappingsList
        {
            get
            {
                try
                {
                    object obj = Session["FolderMappingsList"];
                    if (obj == null)
                    {
                        obj = _folderMappingController.GetFolderMappings(FolderPortalID);
                        if (obj != null)
                        {
                            Session["FolderMappingsList"] = obj;
                        }
                        else
                        {
                            obj = new List<FolderMappingInfo>();
                        }
                    }
                    return (List<FolderMappingInfo>)obj;
                }
                catch
                {
                    Session["FolderMappingsList"] = null;
                }
                return new List<FolderMappingInfo>();
            }
            set { Session["FolderMappingsList"] = value; }
        }
        #endregion

        #region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdCancel.Click += cmdCancel_Click;
            cmdNewMapping.Click += cmdNewMapping_Click;

            if (!IsPostBack)
            {
                Session["FolderMappingsList"] = null;
                
                if (ModuleConfiguration.ModuleControl.SupportsPopUps)
                {
                    grdMappings.Rebind();
                }
            }
        }

        protected void grdMappings_ItemCommand(object source, GridCommandEventArgs e)
        {
            if (e.CommandName == "Edit")
            {
                Response.Redirect(EditUrl("ItemID", e.CommandArgument.ToString(), "EditFolderMapping"));
            }
            else
            {
                var folderMappingsList = FolderMappingsList;
                var folderMapping = folderMappingsList.Find(f => f.FolderMappingID == int.Parse(e.CommandArgument.ToString()));

                switch (e.CommandName)
                {
                    case "Delete":
                        _folderMappingController.DeleteFolderMapping(folderMapping.FolderMappingID);
                        folderMappingsList.Remove(folderMapping);
                        break;
                    case "ChangeAvailability":
                        folderMapping.IsEnabled = !folderMapping.IsEnabled;
                        _folderMappingController.UpdateFolderMapping(folderMapping);
                        break;
                    default:
                        break;
                }

                FolderMappingsList = folderMappingsList;
                grdMappings.Rebind();
            }
        }

        protected void grdMappings_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.Item ||
                e.Item.ItemType == GridItemType.AlternatingItem)
            {
                var folderMapping = (e.Item.DataItem as FolderMappingInfo);

                if (folderMapping.IsEditable)
                {
                    CommandButton cmdDeleteMapping = (e.Item.FindControl("cmdDeleteMapping") as CommandButton);
                    var deleteMessage = string.Format(Localization.GetString("DeleteConfirm", LocalResourceFile), folderMapping.MappingName);
                    cmdDeleteMapping.OnClientClick = "return confirm(\"" + ClientAPI.GetSafeJSString(deleteMessage) + "\");";

                    Button btnChangeAvailability = (e.Item.FindControl("btnChangeAvailability") as Button);

                    CheckBox chkEnabled = (e.Item.FindControl("chkEnabled") as CheckBox);
                    chkEnabled.Attributes.Add("onclick", "javascript:document.getElementById('" + btnChangeAvailability.ClientID + "').click();");
                }
            }
        }

        protected void grdMappings_OnRowDrop(object sender, GridDragDropEventArgs e)
        {
            if (string.IsNullOrEmpty(e.HtmlElement))
            {
                if (e.DraggedItems[0].OwnerGridID == grdMappings.ClientID)
                {
                    var folderMappingsList = FolderMappingsList;
                    var folderMapping = folderMappingsList.Find(f => f.FolderMappingID == (int)e.DestDataItem.GetDataKeyValue("FolderMappingID"));

                    var destinationIndex = folderMappingsList.IndexOf(folderMapping);

                    if (e.DropPosition == GridItemDropPosition.Above && e.DestDataItem.ItemIndex > e.DraggedItems[0].ItemIndex)
                    {
                        destinationIndex -= 1;
                    }
                    if (e.DropPosition == GridItemDropPosition.Below && e.DestDataItem.ItemIndex < e.DraggedItems[0].ItemIndex)
                    {
                        destinationIndex += 1;
                    }

                    var folderMappingsToMove = new List<FolderMappingInfo>();
                    foreach (GridDataItem item in e.DraggedItems)
                    {
                        var tmpFolderMapping = folderMappingsList.Find(f => f.FolderMappingID == (int)item.GetDataKeyValue("FolderMappingID"));
                        if (tmpFolderMapping != null)
                        {
                            folderMappingsToMove.Add(tmpFolderMapping);
                        }
                    }

                    foreach (FolderMappingInfo folderMappingToMove in folderMappingsToMove)
                    {
                        folderMappingsList.Remove(folderMappingToMove);
                        folderMappingsList.Insert(destinationIndex, folderMappingToMove);
                    }

                    UpdateFolderMappings(folderMappingsList);
                    FolderMappingsList = folderMappingsList;

                    grdMappings.Rebind();

                    int destinationItemIndex = destinationIndex - (grdMappings.PageSize * grdMappings.CurrentPageIndex);
                    e.DestinationTableView.Items[destinationItemIndex].Selected = true;
                }
            }
        }

        protected void grdMappings_NeedDataSource(object source, GridNeedDataSourceEventArgs e)
        {
            grdMappings.DataSource = FolderMappingsList;
        }

        private void cmdNewMapping_Click(object sender, EventArgs e)
        {
            try
            {
                Response.Redirect(EditUrl("EditFolderMapping"));
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                Response.Redirect(Globals.NavigateURL());
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion

        #region Private Methods

        private void UpdateFolderMappings(List<FolderMappingInfo> folderMappingsList)
        {
            for (var i = 3; i < folderMappingsList.Count; i++)
            {
                folderMappingsList[i].Priority = i + 1;
                _folderMappingController.UpdateFolderMapping(folderMappingsList[i]);
            }
        }

        #endregion
    }
}