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
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using DotNetNuke.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Upgrade;
using DotNetNuke.UI.Modules;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.Modules.Admin.Extensions
{
    public partial class Extensions : ModuleUserControlBase, IActionable
    {
        #region Protected Methods

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
 
            jQuery.RequestDnnPluginsRegistration();

            installedExtensionsControl.LocalResourceFile = LocalResourceFile;
            installedExtensionsControl.ModuleContext.Configuration = ModuleContext.Configuration;

            if (ModuleContext.PortalSettings.UserInfo.IsSuperUser)
            {
                avalableExtensionsControl.LocalResourceFile = LocalResourceFile;
                avalableExtensionsControl.ModuleContext.Configuration = ModuleContext.Configuration;
                avalableExtensionsTab.Visible = true;
                avalableExtensionsControl.Visible = true;           
            }

            string appGalleryUri = Localization.GetString("appgalleryEndpoint", "~/DesktopModules/Admin/Extensions/App_LocalResources/SharedResources.resx");

            if (!String.IsNullOrEmpty(appGalleryUri) && ModuleContext.PortalSettings.UserInfo.IsSuperUser)
            {
                moreExtensionsControl.LocalResourceFile = LocalResourceFile;
                moreExtensionsControl.ModuleContext.Configuration = ModuleContext.Configuration;
                moreExtensionsTab.Visible = true;
                moreExtensionsControl.Visible = true;
            }

        }

        #endregion

        #region IActionable Members

        public ModuleActionCollection ModuleActions
        {
            get
            {
                var actions = new ModuleActionCollection();
                if (ModuleContext.IsHostMenu)
                {
                    actions.Add(ModuleContext.GetNextActionID(),
                                Localization.GetString("ExtensionInstall.Action", LocalResourceFile),
                                ModuleActionType.AddContent,
                                "",
                                "action_import.gif",
                                Util.InstallURL(ModuleContext.TabId, ""),
                                false,
                                SecurityAccessLevel.Host,
                                true,
                                false);
                    actions.Add(ModuleContext.GetNextActionID(),
                                Localization.GetString("CreateExtension.Action", LocalResourceFile),
                                ModuleActionType.AddContent,
                                "",
                                "add.gif",
                                ModuleContext.EditUrl("NewExtension"),
                                false,
                                SecurityAccessLevel.Host,
                                true,
                                false);
                    actions.Add(ModuleContext.GetNextActionID(),
                                Localization.GetString("CreateModule.Action", LocalResourceFile),
                                ModuleActionType.AddContent,
                                "",
                                "add.gif",
                                ModuleContext.EditUrl("EditModuleDefinition"),
                                false,
                                SecurityAccessLevel.Host,
                                true,
                                false);
                }
                return actions;
            }
        }

        #endregion

    }
}