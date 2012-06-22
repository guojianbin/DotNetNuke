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
using System.Globalization;
using System.Web;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System.IO;
using DotNetNuke.Services.Localization.Internal;

#endregion

namespace DotNetNuke.Services.UserProfile
{
    public class UserProfilePicHandler : IHttpHandler
    {
        #region IHttpHandler Members

        public void ProcessRequest(HttpContext context)
        {
            SetupCulture();

            var photoUrl = String.Empty;
            if (!String.IsNullOrEmpty(context.Request.QueryString["photoUrl"]))
            {
                photoUrl = HttpUtility.UrlDecode(context.Request.QueryString["photoUrl"]);
            }

            var userId = -1;
            if (!String.IsNullOrEmpty(context.Request.QueryString["userid"]))
            {
                userId = Convert.ToInt32(context.Request.QueryString["userid"]);
            }

            var width = 55;
            if (!String.IsNullOrEmpty(context.Request.QueryString["w"]))
            {
                width = Convert.ToInt32(context.Request.QueryString["w"]);
            }

            var height = 55;
            if (!String.IsNullOrEmpty(context.Request.QueryString["h"]))
            {
                height = Convert.ToInt32(context.Request.QueryString["h"]);
            }

            if (String.IsNullOrEmpty(photoUrl))
            {
                PortalSettings settings = PortalController.GetCurrentPortalSettings();
                var userController = new UserController();
                var user = userController.GetUser(settings.PortalId, userId);
                photoUrl = user.Profile.PhotoURL;
            }
            var fileInfo = new FileInfo(context.Request.MapPath(photoUrl));
            string ext;
            if (fileInfo.Exists)
            {
                ext = fileInfo.Extension;
            }
            else
            {
                fileInfo = new FileInfo(context.Request.MapPath("~/images/no_avatar.gif"));
                ext = "gif";
            }

            string sizedPhoto = fileInfo.FullName.Replace(ext, width.ToString(CultureInfo.InvariantCulture) + "x" + height.ToString(CultureInfo.InvariantCulture) + ext);

            if (!File.Exists(sizedPhoto))
            {
                //need to create the photo
                File.Copy(fileInfo.FullName, sizedPhoto);
                sizedPhoto = ImageUtils.CreateImage(sizedPhoto, height, width);
            }

            switch (ext)
            {
                case ".png":
                    context.Response.ContentType = "image/png";
                    break;
                case ".jpeg":
                case ".jpg":
                    context.Response.ContentType = "image/jpeg";
                    break;
                case ".gif":
                    context.Response.ContentType = "image/gif";
                    break;
            }
            context.Response.WriteFile(sizedPhoto);
            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetExpires(DateTime.Now.AddMinutes(1));
            context.Response.Cache.SetMaxAge(new TimeSpan(0, 1, 0));
            context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
            context.Response.End();

        }
        
        private void SetupCulture()
        {
            PortalSettings settings = PortalController.GetCurrentPortalSettings();
            if (settings == null) return;

            CultureInfo pageLocale = TestableLocalization.Instance.GetPageLocale(settings);
            if (pageLocale != null)
            {
                TestableLocalization.Instance.SetThreadCultures(pageLocale, settings);
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        #endregion
       
    }
}
