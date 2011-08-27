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
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Security;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Security;
using DotNetNuke.Security.Membership;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Mail;
using DotNetNuke.Services.Messaging.Data;

using MembershipProvider = DotNetNuke.Security.Membership.MembershipProvider;

#endregion

namespace DotNetNuke.Entities.Users
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Users
    /// Class:      UserController
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The UserController class provides Business Layer methods for Users
    /// </summary>
    /// <remarks>
	/// DotNetNuke user management is base on asp.net membership provider, but  the default implementation of these providers 
	/// do not satisfy the broad set of use cases which we need to support in DotNetNuke. so The dependency of DotNetNuke on the 
	/// MemberRole (ASP.NET 2 Membership) components will be abstracted into a DotNetNuke Membership Provider, in order to allow 
	/// developers complete flexibility in implementing alternate Membership approaches.
	/// <list type="bullet">
	/// <item>This will allow for a number of enhancements to be added</item>
	/// <item>Removal of dependence on the HttpContext</item>
	/// <item>Support for Hashed Passwords</item>
	/// <item>Support for Password Question and Answer</item>
	/// <item>Enforce Password Complexity</item>
	/// <item>Password Aging (Expiry)</item>
	/// <item>Force Password Update</item>
	/// <item>Enable/Disable Password Retrieval/Reset</item>
	/// <item>CAPTCHA Support</item>
	/// <item>Redirect after registration/login/logout</item>
	/// </list>
    /// </remarks>
    /// <seealso cref="DotNetNuke.Security.Membership.MembershipProvider"/>
    /// -----------------------------------------------------------------------------
    public class UserController
    {
        #region Private Members

        private static readonly MembershipProvider MemberProvider = MembershipProvider.Instance();

        #endregion

        #region Public Properties

        public string DisplayFormat { get; set; }

        public int PortalId { get; set; }

        #endregion

        #region Private Methods

        private static void AddEventLog(int portalId, string username, int userId, string portalName, string ip, UserLoginStatus loginStatus)
        {
            var objEventLog = new EventLogController();

            //initialize log record
            var objEventLogInfo = new LogInfo();
            var objSecurity = new PortalSecurity();
            objEventLogInfo.AddProperty("IP", ip);
            objEventLogInfo.LogPortalID = portalId;
            objEventLogInfo.LogPortalName = portalName;
            objEventLogInfo.LogUserName = objSecurity.InputFilter(username, PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);
            objEventLogInfo.LogUserID = userId;

            //create log record
            objEventLogInfo.LogTypeKey = loginStatus.ToString();
            objEventLog.AddLog(objEventLogInfo);
        }

        private static void AutoAssignUsersToPortalRoles(UserInfo user, int portalId)
        {
            var roleController = new RoleController();

            foreach (RoleInfo role in roleController.GetPortalRoles(portalId))
            {
                if (role.AutoAssignment)
                {
                    roleController.AddUserRole(portalId, user.UserID, role.RoleID, Null.NullDate, Null.NullDate);
                }
            }
        }

        private static void AutoAssignUsersToRoles(UserInfo user, int portalId)
        {
            if (IsMemberOfPortalGroup(portalId))
            {
                var portalController = new PortalController();
                PortalInfo thisPortal = portalController.GetPortal(portalId);

                foreach (PortalInfo portal in PortalGroupController.Instance.GetPortalsByGroup(thisPortal.PortalGroupID))
                {
                    AutoAssignUsersToPortalRoles(user, portal.PortalID);
                }
            }
            else
            {
                AutoAssignUsersToPortalRoles(user, portalId);
            }
        }

        private static bool CanDeteteAdministrator(UserInfo objUser, bool deleteAdmin)
        {
            var canDelete = true;
            IDataReader dr = null;
            try
            {
                dr = DataProvider.Instance().GetPortal(objUser.PortalID, PortalController.GetActivePortalLanguage(objUser.PortalID));
                if (dr.Read())
                {
                    if (objUser.UserID == Convert.ToInt32(dr["AdministratorId"]))
                    {
                        canDelete = deleteAdmin;
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
                canDelete = false;
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }
            return canDelete;
        }

        //TODO - Handle Portal Groups
        private static void DeleteUserPermissions(UserInfo user)
        {
            FolderPermissionController.DeleteFolderPermissionsByUser(user);

            //Delete Module Permissions
            ModulePermissionController.DeleteModulePermissionsByUser(user);

            //Delete Tab Permissions
            TabPermissionController.DeleteTabPermissionsByUser(user);
        }

        private static object GetCachedUserByPortalCallBack(CacheItemArgs cacheItemArgs)
        {
            var portalId = (int)cacheItemArgs.ParamList[0];
            var username = (string)cacheItemArgs.ParamList[1];
            return MemberProvider.GetUserByUserName(portalId, username);
        }

        private static int GetEffectivePortalId(int portalId)
        {
            return PortalController.GetEffectivePortalId(portalId);
        }

        private static object GetUserCountByPortalCallBack(CacheItemArgs cacheItemArgs)
        {
            var portalId = (int)cacheItemArgs.ParamList[0];
            var portalUserCount = MemberProvider.GetUserCountByPortal(portalId);
            DataCache.SetCache(cacheItemArgs.CacheKey, portalUserCount);
            return portalUserCount;
        }

        internal static Hashtable GetUserSettings(int portalId, Hashtable settings)
        {
            portalId = GetEffectivePortalId(portalId);

            if (settings["Column_FirstName"] == null)
            {
                settings["Column_FirstName"] = false;
            }
            if (settings["Column_LastName"] == null)
            {
                settings["Column_LastName"] = false;
            }
            if (settings["Column_DisplayName"] == null)
            {
                settings["Column_DisplayName"] = true;
            }
            if (settings["Column_Address"] == null)
            {
                settings["Column_Address"] = true;
            }
            if (settings["Column_Telephone"] == null)
            {
                settings["Column_Telephone"] = true;
            }
            if (settings["Column_Email"] == null)
            {
                settings["Column_Email"] = false;
            }
            if (settings["Column_CreatedDate"] == null)
            {
                settings["Column_CreatedDate"] = true;
            }
            if (settings["Column_LastLogin"] == null)
            {
                settings["Column_LastLogin"] = false;
            }
            if (settings["Column_Authorized"] == null)
            {
                settings["Column_Authorized"] = true;
            }
            if (settings["Display_Mode"] == null)
            {
                settings["Display_Mode"] = DisplayMode.All;
            }
            else
            {
                settings["Display_Mode"] = (DisplayMode)Convert.ToInt32(settings["Display_Mode"]);
            }
            if (settings["Display_SuppressPager"] == null)
            {
                settings["Display_SuppressPager"] = false;
            }
            if (settings["Records_PerPage"] == null)
            {
                settings["Records_PerPage"] = 10;
            }
            if (settings["Profile_DefaultVisibility"] == null)
            {
                settings["Profile_DefaultVisibility"] = UserVisibilityMode.AdminOnly;
            }
            else
            {
                settings["Profile_DefaultVisibility"] = (UserVisibilityMode)Convert.ToInt32(settings["Profile_DefaultVisibility"]);
            }
            if (settings["Profile_DisplayVisibility"] == null)
            {
                settings["Profile_DisplayVisibility"] = true;
            }
            if (settings["Profile_ManageServices"] == null)
            {
                settings["Profile_ManageServices"] = true;
            }
            if (settings["Redirect_AfterLogin"] == null)
            {
                settings["Redirect_AfterLogin"] = -1;
            }
            if (settings["Redirect_AfterRegistration"] == null)
            {
                settings["Redirect_AfterRegistration"] = -1;
            }
            if (settings["Redirect_AfterLogout"] == null)
            {
                settings["Redirect_AfterLogout"] = -1;
            }
            if (settings["Security_CaptchaLogin"] == null)
            {
                settings["Security_CaptchaLogin"] = false;
            }
            if (settings["Security_CaptchaRegister"] == null)
            {
                settings["Security_CaptchaRegister"] = false;
            }
            if (settings["Security_CaptchaRetrivePassword"] == null)
            {
                settings["Security_CaptchaRetrivePassword"] = false;
            }
            if (settings["Security_EmailValidation"] == null)
            {
                settings["Security_EmailValidation"] = Globals.glbEmailRegEx;
            }
            if (settings["Security_UserNameValidation"] == null)
            {
                settings["Security_UserNameValidation"] = Globals.glbUserNameRegEx;
            }
            //Forces a valid profile on registration
            if (settings["Security_RequireValidProfile"] == null)
            {
                settings["Security_RequireValidProfile"] = false;
            }
            //Forces a valid profile on login
            if (settings["Security_RequireValidProfileAtLogin"] == null)
            {
                settings["Security_RequireValidProfileAtLogin"] = true;
            }
            if (settings["Security_UsersControl"] == null)
            {
                var portal = new PortalController().GetPortal(portalId);

                if (portal != null && portal.Users > 1000)
                {
                    settings["Security_UsersControl"] = UsersControl.TextBox;
                }
                else
                {
                    settings["Security_UsersControl"] = UsersControl.Combo;
                }
            }
            else
            {
                settings["Security_UsersControl"] = (UsersControl)Convert.ToInt32(settings["Security_UsersControl"]);
            }
            //Display name format
            if (settings["Security_DisplayNameFormat"] == null)
            {
                settings["Security_DisplayNameFormat"] = "";
            }
            return settings;
        }

        private static bool IsMemberOfPortalGroup(int portalId)
        {
            var portalController = new PortalController();
            var portal = portalController.GetPortal(portalId);

            return (portal.PortalGroupID > Null.NullInteger);
        }

        private static void SendDeleteEmailNotifications(UserInfo user, PortalSettings portalSettings)
        {
            var message = new Message();
            message.FromUserID = portalSettings.AdministratorId;
            message.ToUserID = portalSettings.AdministratorId;
            message.Subject = Localization.GetSystemMessage(user.Profile.PreferredLocale,
                                                            portalSettings,
                                                            "EMAIL_USER_UNREGISTER_SUBJECT",
                                                            user,
                                                            Localization.GlobalResourceFile,
                                                            null,
                                                            "",
                                                            portalSettings.AdministratorId);
            message.Body = Localization.GetSystemMessage(user.Profile.PreferredLocale,
                                                         portalSettings,
                                                         "EMAIL_USER_UNREGISTER_BODY",
                                                         user,
                                                         Localization.GlobalResourceFile,
                                                         null,
                                                         "",
                                                         portalSettings.AdministratorId);
            message.Status = MessageStatusType.Unread;
            Mail.SendEmail(portalSettings.Email, portalSettings.Email, message.Subject, message.Body);
        }

        private static void SetAuthenticationCookie(UserInfo user, bool createPersistentCookie)
        {
            FormsAuthentication.SetAuthCookie(user.Username, createPersistentCookie);

            //check if cookie is persistent, and user has supplied custom value for expiration
            var persistentCookieTimeout = Config.GetPersistentCookieTimeout();
            if (createPersistentCookie)
            {
                //manually create authentication cookie    
                //first, create the authentication ticket     
                var authenticationTicket = new FormsAuthenticationTicket(user.Username, true, persistentCookieTimeout);
                //encrypt it     
                var encryptedAuthTicket = FormsAuthentication.Encrypt(authenticationTicket);
                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedAuthTicket)
                {
                    Expires = authenticationTicket.Expiration,
                    Domain = FormsAuthentication.CookieDomain,
                    Path = FormsAuthentication.FormsCookiePath
                };
                //set cookie expiration to correspond with ticket expiration.  
                //set cookie domain to be consistent with domain specification in web.config
                //set cookie path to be consistent with path specification in web.config
                HttpContext.Current.Response.Cookies.Set(authCookie);
            }
        }

        #endregion

        #region Public Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ChangePassword attempts to change the users password
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="user">The user to update.</param>
        /// <param name="oldPassword">The old password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <returns>A Boolean indicating success or failure.</returns>
        /// -----------------------------------------------------------------------------
        public static bool ChangePassword(UserInfo user, string oldPassword, string newPassword)
        {
            bool retValue;

            //Although we would hope that the caller has already validated the password,
            //Validate the new Password
            if (ValidatePassword(newPassword))
            {
                retValue = MemberProvider.ChangePassword(user, oldPassword, newPassword);

                //Update User
                user.Membership.UpdatePassword = false;
                UpdateUser(user.PortalID, user);
            }
            else
            {
                throw new Exception("Invalid Password");
            }
            return retValue;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ChangePasswordQuestionAndAnswer attempts to change the users password Question
        /// and PasswordAnswer
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="user">The user to update.</param>
        /// <param name="password">The password.</param>
        /// <param name="passwordQuestion">The new password question.</param>
        /// <param name="passwordAnswer">The new password answer.</param>
        /// <returns>A Boolean indicating success or failure.</returns>
        /// -----------------------------------------------------------------------------
        public static bool ChangePasswordQuestionAndAnswer(UserInfo user, string password, string passwordQuestion, string passwordAnswer)
        {
            var objEventLog = new EventLogController();
            objEventLog.AddLog(user, PortalController.GetCurrentPortalSettings(), GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.USER_UPDATED);
            return MemberProvider.ChangePasswordQuestionAndAnswer(user, password, passwordQuestion, passwordAnswer);
        }

        public static void CheckInsecurePassword(string username, string password, ref UserLoginStatus loginStatus)
        {
            if (username == "admin" && (password == "admin" || password == "dnnadmin"))
            {
                loginStatus = UserLoginStatus.LOGIN_INSECUREADMINPASSWORD;
            }
            if (username == "host" && (password == "host" || password == "dnnhost"))
            {
                loginStatus = UserLoginStatus.LOGIN_INSECUREHOSTPASSWORD;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Creates a new User in the Data Store
        /// </summary>
        /// <remarks></remarks>
        /// <param name="user">The userInfo object to persist to the Database</param>
        /// <returns>The Created status ot the User</returns>
        /// -----------------------------------------------------------------------------
        public static UserCreateStatus CreateUser(ref UserInfo user)
        {
            int portalId = GetEffectivePortalId(user.PortalID);
            user.PortalID = portalId;

            //Create the User
            var createStatus = MemberProvider.CreateUser(ref user);

            if (createStatus == UserCreateStatus.Success)
            {
                var objEventLog = new EventLogController();
                objEventLog.AddLog(user, PortalController.GetCurrentPortalSettings(), GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.USER_CREATED);
                DataCache.ClearPortalCache(portalId, false);
                if (!user.IsSuperUser)
                {
                    //autoassign user to portal roles
                    AutoAssignUsersToRoles(user, portalId);
                }
            }
            return createStatus;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Deletes all Unauthorized Users for a Portal
        /// </summary>
        /// <remarks></remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// -----------------------------------------------------------------------------
        public static void DeleteUnauthorizedUsers(int portalId)
        {
            var arrUsers = GetUsers(portalId);
            for (int i = 0; i < arrUsers.Count; i++)
            {
                var user = arrUsers[i] as UserInfo;
                if (user != null)
                {
                    if (user.Membership.Approved == false || user.Membership.LastLoginDate == Null.NullDate)
                    {
                        DeleteUser(ref user, true, false);
                    }
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Deletes an existing User from the Data Store
        /// </summary>
        /// <remarks></remarks>
        /// <param name="user">The userInfo object to delete from the Database</param>
        /// <param name="notify">A flag that indicates whether an email notification should be sent</param>
        /// <param name="deleteAdmin">A flag that indicates whether the Portal Administrator should be deleted</param>
        /// <returns>A Boolean value that indicates whether the User was successfully deleted</returns>
        /// -----------------------------------------------------------------------------
        public static bool DeleteUser(ref UserInfo user, bool notify, bool deleteAdmin)
        {
            int portalId = GetEffectivePortalId(user.PortalID);
            user.PortalID = portalId;
            
            var canDelete = CanDeteteAdministrator(user, deleteAdmin);

            if (canDelete)
            {
                //Delete Permissions
                DeleteUserPermissions(user);
                canDelete = MemberProvider.DeleteUser(user);
            }
            if (canDelete)
            {
                //Obtain PortalSettings from Current Context
                var portalSettings = PortalController.GetCurrentPortalSettings();
                var objEventLog = new EventLogController();
                objEventLog.AddLog("Username", user.Username, portalSettings, user.UserID, EventLogController.EventLogType.USER_DELETED);
                if (notify && !user.IsSuperUser)
                {
                    //send email notification to portal administrator that the user was removed from the portal
                    SendDeleteEmailNotifications(user, portalSettings);
                }
                DataCache.ClearPortalCache(portalId, false);
                DataCache.ClearUserCache(portalId, user.Username);
            }
            return canDelete;
        }


        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Deletes all Users for a Portal
        /// </summary>
        /// <remarks></remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="notify">A flag that indicates whether an email notification should be sent</param>
        /// <param name="deleteAdmin">A flag that indicates whether the Portal Administrator should be deleted</param>
        /// -----------------------------------------------------------------------------
        public static void DeleteUsers(int portalId, bool notify, bool deleteAdmin)
        {
            var arrUsers = GetUsers(portalId);
            for (int i = 0; i < arrUsers.Count; i++)
            {
                var objUser = arrUsers[i] as UserInfo;
                DeleteUser(ref objUser, notify, deleteAdmin);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generates a new random password (Length = Minimum Length + 4)
        /// </summary>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        public static string GeneratePassword()
        {
            return GeneratePassword(MembershipProviderConfig.MinPasswordLength + 4);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generates a new random password
        /// </summary>
        /// <param name="length">The length of password to generate.</param>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        public static string GeneratePassword(int length)
        {
            return MemberProvider.GeneratePassword(length);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetCachedUser retrieves the User from the Cache, or fetches a fresh copy if 
        /// not in cache or if Cache settings not set to HeavyCaching
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="username">The username of the user being retrieved.</param>
        /// <returns>The User as a UserInfo object</returns>
        /// -----------------------------------------------------------------------------
        public static UserInfo GetCachedUser(int portalId, string username)
        {
            //Get the User cache key
            portalId = GetEffectivePortalId(portalId);
            var cacheKey = string.Format(DataCache.UserCacheKey, portalId, username);
            return CBO.GetCachedObject<UserInfo>(new CacheItemArgs(cacheKey, DataCache.UserCacheTimeOut, DataCache.UserCachePriority, portalId, username), GetCachedUserByPortalCallBack);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get the current UserInfo object
        /// </summary>
        /// <returns>The current UserInfo if authenticated, oherwise an empty user</returns>
        /// -----------------------------------------------------------------------------
        public static UserInfo GetCurrentUserInfo()
        {
            UserInfo user;
            if ((HttpContext.Current == null))
            {
                if (!Thread.CurrentPrincipal.Identity.IsAuthenticated)
                {
                    return new UserInfo();
                }
                var portalSettings = PortalController.GetCurrentPortalSettings();
                if (portalSettings != null)
                {
                    user = GetCachedUser(portalSettings.PortalId, Thread.CurrentPrincipal.Identity.Name);
                    return user ?? new UserInfo();
                }
                return new UserInfo();
            }
            user = (UserInfo)HttpContext.Current.Items["UserInfo"];
            return user ?? new UserInfo();
        }

        public static ArrayList GetDeletedUsers(int portalId)
        {
            return MemberProvider.GetDeletedUsers(GetEffectivePortalId(portalId));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a collection of Online Users
        /// </summary>
        /// <param name="portalId">The Id of the Portal</param>
        /// <returns>An ArrayList of UserInfo objects</returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetOnlineUsers(int portalId)
        {
            return MemberProvider.GetOnlineUsers(GetEffectivePortalId(portalId));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Current Password Information for the User 
        /// </summary>
        /// <remarks>This method will only return the password if the memberProvider supports
        /// and is using a password encryption method that supports decryption.</remarks>
        /// <param name="user">The user whose Password information we are retrieving.</param>
        /// <param name="passwordAnswer">The answer to the "user's" password Question.</param>
        /// -----------------------------------------------------------------------------
        public static string GetPassword(ref UserInfo user, string passwordAnswer)
        {
            if (MembershipProviderConfig.PasswordRetrievalEnabled)
            {
                user.Membership.Password = MemberProvider.GetPassword(user, passwordAnswer);
            }
            else
            {
				//Throw a configuration exception as password retrieval is not enabled
                throw new ConfigurationErrorsException("Password Retrieval is not enabled");
            }
            return user.Membership.Password;
        }

        public static ArrayList GetUnAuthorizedUsers(int portalId, bool includeDeleted, bool superUsersOnly)
        {
            return MemberProvider.GetUnAuthorizedUsers(GetEffectivePortalId(portalId), includeDeleted, superUsersOnly);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUnAuthorizedUsers gets all the users of the portal, that are not authorized
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetUnAuthorizedUsers(int portalId)
        {
            return GetUnAuthorizedUsers(portalId, false, false);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUser retrieves a User from the DataStore
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="userId">The Id of the user being retrieved from the Data Store.</param>
        /// <returns>The User as a UserInfo object</returns>
        /// -----------------------------------------------------------------------------
        public static UserInfo GetUserById(int portalId, int userId)
        {
            return MemberProvider.GetUser(GetEffectivePortalId(portalId), userId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUserByUserName retrieves a User from the DataStore
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="username">The username of the user being retrieved from the Data Store.</param>
        /// <returns>The User as a UserInfo object</returns>
        /// -----------------------------------------------------------------------------
        public static UserInfo GetUserByName(int portalId, string username)
        {
            return GetCachedUser(portalId, username);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUserCountByPortal gets the number of users in the portal
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <returns>The no of users</returns>
        /// -----------------------------------------------------------------------------
        public static int GetUserCountByPortal(int portalId)
        {
            portalId = GetEffectivePortalId(portalId);
            var cacheKey = string.Format(DataCache.PortalUserCountCacheKey, portalId);
            return CBO.GetCachedObject<int>(new CacheItemArgs(cacheKey, DataCache.PortalUserCountCacheTimeOut, DataCache.PortalUserCountCachePriority, portalId), GetUserCountByPortalCallBack);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Retruns a String corresponding to the Registration Status of the User
        /// </summary>
		/// <param name="userRegistrationStatus">The AUserCreateStatus</param>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        public static string GetUserCreateStatus(UserCreateStatus userRegistrationStatus)
        {
            switch (userRegistrationStatus)
            {
                case UserCreateStatus.DuplicateEmail:
                    return Localization.GetString("UserEmailExists");
                case UserCreateStatus.InvalidAnswer:
                    return Localization.GetString("InvalidAnswer");
                case UserCreateStatus.InvalidEmail:
                    return Localization.GetString("InvalidEmail");
                case UserCreateStatus.InvalidPassword:
                    string strInvalidPassword = Localization.GetString("InvalidPassword");
                    strInvalidPassword = strInvalidPassword.Replace("[PasswordLength]", MembershipProviderConfig.MinPasswordLength.ToString());
                    strInvalidPassword = strInvalidPassword.Replace("[NoneAlphabet]", MembershipProviderConfig.MinNonAlphanumericCharacters.ToString());
                    return strInvalidPassword;
                case UserCreateStatus.PasswordMismatch:
                    return Localization.GetString("PasswordMismatch");
                case UserCreateStatus.InvalidQuestion:
                    return Localization.GetString("InvalidQuestion");
                case UserCreateStatus.InvalidUserName:
                    return Localization.GetString("InvalidUserName");
                case UserCreateStatus.UserRejected:
                    return Localization.GetString("UserRejected");
                case UserCreateStatus.DuplicateUserName:
                case UserCreateStatus.UserAlreadyRegistered:
                case UserCreateStatus.UsernameAlreadyExists:
                    return Localization.GetString("UserNameExists");
                case UserCreateStatus.ProviderError:
                case UserCreateStatus.DuplicateProviderUserKey:
                case UserCreateStatus.InvalidProviderUserKey:
                    return Localization.GetString("RegError");
                default:
                    throw new ArgumentException("Unknown UserCreateStatus value encountered", "userRegistrationStatus");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Membership Information for the User
        /// </summary>
        /// <remarks></remarks>
        /// <param name="user">The user whose Membership information we are retrieving.</param>
        /// -----------------------------------------------------------------------------
        public static void GetUserMembership(UserInfo user)
        {
            int portalId = GetEffectivePortalId(user.PortalID);
            user.PortalID = portalId;
            MemberProvider.GetUserMembership(ref user);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Default Settings for the Module
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static Hashtable GetDefaultUserSettings()
        {
            var portalId = -1; 
            var portalSettings = PortalController.GetCurrentPortalSettings();

            if (portalSettings!= null)
            {
                portalId = portalSettings.PortalId;
            }
            return GetUserSettings(portalId, new Hashtable());
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUser retrieves a User from the DataStore
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="userId">The Id of the user being retrieved from the Data Store.</param>
        /// <returns>The User as a UserInfo object</returns>
        /// -----------------------------------------------------------------------------
        public UserInfo GetUser(int portalId, int userId)
        {
            return GetUserById(portalId, userId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUserSettings retrieves the UserSettings from the User
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <returns>The Settings Hashtable</returns>
        /// -----------------------------------------------------------------------------
        public static Hashtable GetUserSettings(int portalId)
        {
            var settings = GetDefaultUserSettings();
            Dictionary<string, string> settingsDictionary = (portalId == Null.NullInteger)
                                                            ? HostController.Instance.GetSettingsDictionary() 
                                                            : PortalController.GetPortalSettingsDictionary(GetEffectivePortalId(portalId));
            if (settingsDictionary != null)
            {
                foreach (KeyValuePair<string, string> kvp in settingsDictionary)
                {
                    int index = kvp.Key.IndexOf("_");
                    if (index > 0)
                    {
                        //Get the prefix
                        string prefix = kvp.Key.Substring(0, index + 1);
                        switch (prefix)
                        {
                            case "Column_":
                            case "Display_":
                            case "Profile_":
                            case "Records_":
                            case "Redirect_":
                            case "Security_":
                                switch(kvp.Key)
                                {
                                    case "Display_Mode":
                                        settings[kvp.Key] = (DisplayMode) Convert.ToInt32(kvp.Value);
                                        break;
                                    case "Profile_DefaultVisibility":
                                        settings[kvp.Key] = (UserVisibilityMode)Convert.ToInt32(kvp.Value);
                                        break;
                                    case "Security_UsersControl":
                                        settings[kvp.Key] = (UsersControl)Convert.ToInt32(kvp.Value);
                                        break;
                                    default:
                                        //update value or add any new values
                                        settings[kvp.Key] = kvp.Value;
                                        break;
                                }
                                break;
                        }
                    }
                }
            }
            return settings;
        }
		
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUsers gets all the users of the portal
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetUsers(int portalId)
        {
            return GetUsers(false, false, portalId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUsers gets all the users of the portal
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="includeDeleted">Include Deleted Users.</param>
        /// <param name="superUsersOnly">Only get super users.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetUsers(bool includeDeleted, bool superUsersOnly, int portalId)
        {
            var totalrecords = -1;
            return GetUsers(portalId, -1, -1, ref totalrecords, includeDeleted, superUsersOnly);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUsers gets all the users of the portal, by page
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetUsers(int portalId, int pageIndex, int pageSize, ref int totalRecords)
        {
            return GetUsers(portalId, pageIndex, pageSize, ref totalRecords, false, false);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUsers gets all the users of the portal, by page
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
        /// <param name="includeDeleted">Include Deleted Users.</param>
        /// <param name="superUsersOnly">Only get super users.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetUsers(int portalId, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
        {
            return MemberProvider.GetUsers(GetEffectivePortalId(portalId), pageIndex, pageSize, ref totalRecords, includeDeleted, superUsersOnly);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUsersByEmail gets all the users of the portal whose email matches a provided
        /// filter expression
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="emailToMatch">The email address to use to find a match.</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetUsersByEmail(int portalId, string emailToMatch, int pageIndex, int pageSize, ref int totalRecords)
        {
            return GetUsersByEmail(portalId, emailToMatch, pageIndex, pageSize, ref totalRecords, false, false);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUsersByEmail gets all the users of the portal whose email matches a provided
        /// filter expression
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="emailToMatch">The email address to use to find a match.</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
		/// <param name="includeDeleted">Include Deleted Users.</param>
		/// <param name="superUsersOnly">Only get super users.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetUsersByEmail(int portalId, string emailToMatch, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
        {
            return MemberProvider.GetUsersByEmail(GetEffectivePortalId(portalId), emailToMatch, pageIndex, pageSize, ref totalRecords, includeDeleted, superUsersOnly);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUsersByProfileProperty gets all the users of the portal whose profile matches
        /// the profile property pased as a parameter
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="propertyName">The name of the property being matched.</param>
        /// <param name="propertyValue">The value of the property being matched.</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetUsersByProfileProperty(int portalId, string propertyName, string propertyValue, int pageIndex, int pageSize, ref int totalRecords)
        {
            return GetUsersByProfileProperty(portalId, propertyName, propertyValue, pageIndex, pageSize, ref totalRecords, false, false);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUsersByProfileProperty gets all the users of the portal whose profile matches
        /// the profile property pased as a parameter
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="propertyName">The name of the property being matched.</param>
        /// <param name="propertyValue">The value of the property being matched.</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
        /// <param name="includeDeleted">Include Deleted Users.</param>
        /// <param name="superUsersOnly">Only get super users.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetUsersByProfileProperty(int portalId, string propertyName, string propertyValue, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
        {
            return MemberProvider.GetUsersByProfileProperty(GetEffectivePortalId(portalId), propertyName, propertyValue, pageIndex, pageSize, ref totalRecords, includeDeleted, superUsersOnly);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUsersByUserName gets all the users of the portal whose username matches a provided
        /// filter expression
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="userNameToMatch">The username to use to find a match.</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetUsersByUserName(int portalId, string userNameToMatch, int pageIndex, int pageSize, ref int totalRecords)
        {
            return GetUsersByUserName(portalId, userNameToMatch, pageIndex, pageSize, ref totalRecords, false, false);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUsersByUserName gets all the users of the portal whose username matches a provided
        /// filter expression
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="userNameToMatch">The username to use to find a match.</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
		/// <param name="includeDeleted">Include Deleted Users.</param>
		/// <param name="superUsersOnly">Only get super users.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetUsersByUserName(int portalId, string userNameToMatch, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
        {
            return MemberProvider.GetUsersByUserName(GetEffectivePortalId(portalId), userNameToMatch, pageIndex, pageSize, ref totalRecords, includeDeleted, superUsersOnly);
        }

        public static void RemoveDeletedUsers(int portalId)
        {
            var arrUsers = GetUsers(true, false, portalId);

            foreach (UserInfo objUser in arrUsers)
            {
                if (objUser.IsDeleted)
                {
                    RemoveUser(objUser);
                }
            }
        }

        public static bool RemoveUser(UserInfo user)
        {
            int portalId = GetEffectivePortalId(user.PortalID);
            user.PortalID = portalId;
            
            //Restore the User
            var retValue = MemberProvider.RemoveUser(user);

            if ((retValue))
            {
                // Obtain PortalSettings from Current Context
                var portalSettings = PortalController.GetCurrentPortalSettings();

                //Log event
                var objEventLog = new EventLogController();
                objEventLog.AddLog("Username", user.Username, portalSettings, user.UserID, EventLogController.EventLogType.USER_REMOVED);

                DataCache.ClearPortalCache(portalId, false);
                DataCache.ClearUserCache(portalId, user.Username);
            }

            return retValue;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Resets the password for the specified user
        /// </summary>
        /// <remarks>Resets the user's password</remarks>
        /// <param name="user">The user whose Password information we are resetting.</param>
        /// <param name="passwordAnswer">The answer to the "user's" password Question.</param>
        /// -----------------------------------------------------------------------------
        public static string ResetPassword(UserInfo user, string passwordAnswer)
        {
            if (MembershipProviderConfig.PasswordResetEnabled)
            {
                user.Membership.Password = MemberProvider.ResetPassword(user, passwordAnswer);
            }
            else
            {
				//Throw a configuration exception as password reset is not enabled
                throw new ConfigurationErrorsException("Password Reset is not enabled");
            }
            return user.Membership.Password;
        }

        public static bool RestoreUser(ref UserInfo user)
        {
            int portalId = GetEffectivePortalId(user.PortalID);
            user.PortalID = portalId;
            
            //Restore the User
            var retValue = MemberProvider.RestoreUser(user);

            if ((retValue))
            {
                // Obtain PortalSettings from Current Context
                var portalSettings = PortalController.GetCurrentPortalSettings();

                //Log event
                var objEventLog = new EventLogController();
                objEventLog.AddLog("Username", user.Username, portalSettings, user.UserID, EventLogController.EventLogType.USER_RESTORED);

                DataCache.ClearPortalCache(portalId, false);
                DataCache.ClearUserCache(portalId, user.Username);
            }

            return retValue;
        }


        public static string SettingsKey(int portalId)
        {
            return "UserSettings|" + portalId;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Unlocks the User's Account
        /// </summary>
        /// <remarks></remarks>
        /// <param name="user">The user whose account is being Unlocked.</param>
        /// -----------------------------------------------------------------------------
        public static bool UnLockUser(UserInfo user)
        {
            int portalId = GetEffectivePortalId(user.PortalID);

            //Unlock the User
            var retValue = MemberProvider.UnLockUser(user);
            DataCache.ClearUserCache(portalId, user.Username);
            return retValue;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Update all the Users Display Names
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void UpdateDisplayNames()
        {
            int portalId = GetEffectivePortalId(PortalId);

            var arrUsers = GetUsers(PortalId);
            foreach (UserInfo objUser in arrUsers)
            {
                objUser.UpdateDisplayName(DisplayFormat);
                UpdateUser(portalId, objUser);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Updates a User
        /// </summary>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="user">The use to update</param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static void UpdateUser(int portalId, UserInfo user)
        {
            UpdateUser(portalId, user, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   updates a user
        /// </summary>
        /// <param name = "portalId">the portalid of the user</param>
        /// <param name = "user">the user object</param>
        /// <param name = "loggedAction">whether or not the update calls the eventlog - the eventlogtype must still be enabled for logging to occur</param>
        /// <remarks>
        /// </remarks>
        public static void UpdateUser(int portalId, UserInfo user, bool loggedAction)
        {
            portalId = GetEffectivePortalId(portalId);
            user.PortalID = portalId;
            
            //Update the User
            MemberProvider.UpdateUser(user);
            if (loggedAction)
            {
                var objEventLog = new EventLogController();
                objEventLog.AddLog(user, PortalController.GetCurrentPortalSettings(), GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.USER_UPDATED);
            }
            //Remove the UserInfo from the Cache, as it has been modified
            DataCache.ClearUserCache(portalId, user.Username);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Validates a User's credentials against the Data Store, and sets the Forms Authentication
        /// Ticket
        /// </summary>
        /// <param name="portalId">The Id of the Portal the user belongs to</param>
		/// <param name="username">The user name of the User attempting to log in</param>
		/// <param name="password">The password of the User attempting to log in</param>
		/// <param name="verificationCode">The verification code of the User attempting to log in</param>
		/// <param name="portalName">The name of the Portal</param>
        /// <param name="ip">The IP Address of the user attempting to log in</param>
        /// <param name="loginStatus">A UserLoginStatus enumeration that indicates the status of the 
        /// Login attempt.  This value is returned by reference.</param>
		/// <param name="createPersistentCookie">A flag that indicates whether the login credentials 
        /// should be persisted.</param>
        /// <returns>The UserInfo object representing a successful login</returns>
        /// -----------------------------------------------------------------------------
        public static UserInfo UserLogin(int portalId, string username, string password, string verificationCode, string portalName, string ip, ref UserLoginStatus loginStatus, bool createPersistentCookie)
        {
            portalId = GetEffectivePortalId(portalId);
            
            loginStatus = UserLoginStatus.LOGIN_FAILURE;

            //Validate the user
            var objUser = ValidateUser(portalId, username, password, verificationCode, portalName, ip, ref loginStatus);
            if (objUser != null)
            {
				//Call UserLogin overload
                UserLogin(portalId, objUser, portalName, ip, createPersistentCookie);
            }
            else
            {
                AddEventLog(portalId, username, Null.NullInteger, portalName, ip, loginStatus);
            }
			
            //return the User object
            return objUser;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Logs a Validated User in
        /// </summary>
        /// <param name="portalId">The Id of the Portal the user belongs to</param>
        /// <param name="user">The validated User</param>
		/// <param name="portalName">The name of the Portal</param>
        /// <param name="ip">The IP Address of the user attempting to log in</param>
		/// <param name="createPersistentCookie">A flag that indicates whether the login credentials should be persisted.</param>
        /// -----------------------------------------------------------------------------
        public static void UserLogin(int portalId, UserInfo user, string portalName, string ip, bool createPersistentCookie)
        {
            portalId = GetEffectivePortalId(portalId);

            AddEventLog(portalId, user.Username, user.UserID, portalName, ip, user.IsSuperUser ? UserLoginStatus.LOGIN_SUPERUSER : UserLoginStatus.LOGIN_SUCCESS);

            //Update User in Database with Last IP used
            user.LastIPAddress = ip;
            UpdateUser(portalId, user, false);

            //set the forms authentication cookie ( log the user in )
            SetAuthenticationCookie(user, createPersistentCookie);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Validates a Password
        /// </summary>
        /// <param name="password">The password to Validate</param>
        /// <returns>A boolean</returns>
        /// -----------------------------------------------------------------------------
        public static bool ValidatePassword(string password)
        {
            var isValid = true;

            //Valid Length
            if (password.Length < MembershipProviderConfig.MinPasswordLength)
            {
                isValid = false;
            }
			
            //Validate NonAlphaChars
            var rx = new Regex("[^0-9a-zA-Z]");
            if (rx.Matches(password).Count < MembershipProviderConfig.MinNonAlphanumericCharacters)
            {
                isValid = false;
            }
            //Validate Regex
            if (!String.IsNullOrEmpty(MembershipProviderConfig.PasswordStrengthRegularExpression) && isValid)
            {
                rx = new Regex(MembershipProviderConfig.PasswordStrengthRegularExpression);
                isValid = rx.IsMatch(password);
            }
            return isValid;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Validates a User's credentials against the Data Store
        /// </summary>
        /// <param name="portalId">The Id of the Portal the user belongs to</param>
		/// <param name="username">The user name of the User attempting to log in</param>
		/// <param name="password">The password of the User attempting to log in</param>
		/// <param name="verificationCode">The verification code of the User attempting to log in</param>
		/// <param name="portalName">The name of the Portal</param>
        /// <param name="ip">The IP Address of the user attempting to log in</param>
        /// <param name="loginStatus">A UserLoginStatus enumeration that indicates the status of the 
        /// Login attempt.  This value is returned by reference.</param>
        /// <returns>The UserInfo object representing a valid user</returns>
        /// -----------------------------------------------------------------------------
        public static UserInfo ValidateUser(int portalId, string username, string password, string verificationCode, string portalName, string ip, ref UserLoginStatus loginStatus)
        {
            return ValidateUser(portalId, username, password, "DNN", verificationCode, portalName, ip, ref loginStatus);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Validates a User's credentials against the Data Store
        /// </summary>
        /// <param name="portalId">The Id of the Portal the user belongs to</param>
		/// <param name="username">The user name of the User attempting to log in</param>
		/// <param name="password">The password of the User attempting to log in</param>
        /// <param name="authType">The type of Authentication Used</param>
		/// <param name="verificationCode">The verification code of the User attempting to log in</param>
		/// <param name="portalName">The name of the Portal</param>
        /// <param name="ip">The IP Address of the user attempting to log in</param>
        /// <param name="loginStatus">A UserLoginStatus enumeration that indicates the status of the 
        /// Login attempt.  This value is returned by reference.</param>
        /// <returns>The UserInfo object representing a valid user</returns>
        /// -----------------------------------------------------------------------------
        public static UserInfo ValidateUser(int portalId, string username, string password, string authType, string verificationCode, string portalName, string ip, ref UserLoginStatus loginStatus)
        {
            portalId = GetEffectivePortalId(portalId);

            loginStatus = UserLoginStatus.LOGIN_FAILURE;

            //Try and Log the user in
            var objUser = MemberProvider.UserLogin(portalId, username, password, authType, verificationCode, ref loginStatus);
            if (loginStatus == UserLoginStatus.LOGIN_USERLOCKEDOUT || loginStatus == UserLoginStatus.LOGIN_FAILURE)
            {
				//User Locked Out so log to event log
                AddEventLog(portalId, username, Null.NullInteger, portalName, ip, loginStatus);
            }
			
            //Check Default Accounts
            if (loginStatus == UserLoginStatus.LOGIN_SUCCESS || loginStatus == UserLoginStatus.LOGIN_SUPERUSER)
            {
                CheckInsecurePassword(username, password, ref loginStatus);
            }
			
            //return the User object
            return objUser;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Validates a User's Password and Profile
        /// </summary>
        /// <remarks>This overload takes a valid User (Credentials check out) and check whether the Password and Profile need updating</remarks>
        /// <param name="portalId">The Id of the Portal the user belongs to</param>
        /// <param name="objUser">The user attempting to log in</param>
        /// <param name="ignoreExpiring">Ingore expired user.</param>
        /// <returns>The UserLoginStatus</returns>
        /// -----------------------------------------------------------------------------
        public static UserValidStatus ValidateUser(UserInfo objUser, int portalId, bool ignoreExpiring)
        {
            portalId = GetEffectivePortalId(portalId);
            
            var validStatus = UserValidStatus.VALID;

            //Check if Password needs to be updated
            if (objUser.Membership.UpdatePassword)
            {
				//Admin has forced password update
                validStatus = UserValidStatus.UPDATEPASSWORD;
            }
            else if (PasswordConfig.PasswordExpiry > 0)
            {
                var expiryDate = objUser.Membership.LastPasswordChangeDate.AddDays(PasswordConfig.PasswordExpiry);
                if (expiryDate < DateTime.Now)
                {
					//Password Expired
                    validStatus = UserValidStatus.PASSWORDEXPIRED;
                }
                else if (expiryDate < DateTime.Now.AddDays(PasswordConfig.PasswordExpiryReminder) && (!ignoreExpiring))
                {
					//Password update reminder
                    validStatus = UserValidStatus.PASSWORDEXPIRING;
                }
            }
			
            //Check if Profile needs updating
            if (validStatus == UserValidStatus.VALID)
            {
                var validProfile = Convert.ToBoolean(UserModuleBase.GetSetting(portalId, "Security_RequireValidProfileAtLogin"));
                if (validProfile && (!ProfileController.ValidateProfile(portalId, objUser.Profile)))
                {
                    validStatus = UserValidStatus.UPDATEPROFILE;
                }
            }
            return validStatus;
        }

        #endregion

        #region "Obsoleted Methods, retained for Binary Compatability"

        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.CreateUser")]
        public int AddUser(UserInfo objUser)
        {
            CreateUser(ref objUser);
            return objUser.UserID;
        }

        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.CreateUser")]
        public int AddUser(UserInfo objUser, bool addToMembershipProvider)
        {
            CreateUser(ref objUser);
            return objUser.UserID;
        }

        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.DeleteUsers")]
        public void DeleteAllUsers(int portalId)
        {
            DeleteUsers(portalId, false, true);
        }

        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.DeleteUser")]
        public bool DeleteUser(int portalId, int userId)
        {
            var objUser = GetUser(portalId, userId);

            //Call Shared method with notify=true, deleteAdmin=false
            return DeleteUser(ref objUser, true, false);
        }

        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.DeleteUsers")]
        public void DeleteUsers(int portalId)
        {
            DeleteUsers(portalId, true, false);
        }

        [Obsolete("Deprecated in DNN 6.1.")]
        public static ArrayList FillUserCollection(int portalId, IDataReader dr, ref int totalRecords)
        {
            //Note:  the DataReader returned from this method should contain 2 result sets.  The first set
            //       contains the TotalRecords, that satisfy the filter, the second contains the page
            //       of data
            var arrUsers = new ArrayList();
            try
            {
                while (dr.Read())
                {
                    //fill business object
                    UserInfo user = FillUserInfo(portalId, dr, false);
                    //add to collection
                    arrUsers.Add(user);
                }
                //Get the next result (containing the total)
                dr.NextResult();

                //Get the total no of records from the second result
                totalRecords = Globals.GetTotalRecords(ref dr);
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                //close datareader
                CBO.CloseDataReader(dr, true);
            }
            return arrUsers;
        }

        [Obsolete("Deprecated in DNN 6.1.")]
        public static ArrayList FillUserCollection(int portalId, IDataReader dr)
        {
            //Note:  the DataReader returned from this method should contain 2 result sets.  The first set
            //       contains the TotalRecords, that satisfy the filter, the second contains the page
            //       of data
            var arrUsers = new ArrayList();
            try
            {
                while (dr.Read())
                {
                    //fill business object
                    UserInfo user = FillUserInfo(portalId, dr, false);
                    //add to collection
                    arrUsers.Add(user);
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                //close datareader
                CBO.CloseDataReader(dr, true);
            }
            return arrUsers;
        }

        [Obsolete("Deprecated in DNN 6.1.")]
        public static UserInfo FillUserInfo(int portalId, IDataReader dr, bool closeDataReader)
        {
            UserInfo objUserInfo = null;
            try
            {
                //read datareader
                var bContinue = true;
                if (closeDataReader)
                {
                    bContinue = false;
                    if (dr.Read())
                    {
                        //Ensure the data reader returned is valid
                        if (string.Equals(dr.GetName(0), "UserID", StringComparison.InvariantCultureIgnoreCase))
                        {
                            bContinue = true;
                        }
                    }
                }
                if (bContinue)
                {
                    objUserInfo = new UserInfo
                    {
                        PortalID = portalId,
                        IsSuperUser = Null.SetNullBoolean(dr["IsSuperUser"]),
                        IsDeleted = Null.SetNullBoolean(dr["IsDeleted"]),
                        UserID = Null.SetNullInteger(dr["UserID"]),
                        FirstName = Null.SetNullString(dr["FirstName"]),
                        LastName = Null.SetNullString(dr["LastName"]),
                        RefreshRoles = Null.SetNullBoolean(dr["RefreshRoles"]),
                        DisplayName = Null.SetNullString(dr["DisplayName"])
                    };
                    objUserInfo.AffiliateID = Null.SetNullInteger(Null.SetNull(dr["AffiliateID"], objUserInfo.AffiliateID));
                    objUserInfo.Username = Null.SetNullString(dr["Username"]);
                    GetUserMembership(objUserInfo);
                    objUserInfo.Email = Null.SetNullString(dr["Email"]);
                    objUserInfo.Membership.UpdatePassword = Null.SetNullBoolean(dr["UpdatePassword"]);
                    if (!objUserInfo.IsSuperUser)
                    {
                        objUserInfo.Membership.Approved = Null.SetNullBoolean(dr["Authorised"]);
                    }
                }
            }
            finally
            {
                CBO.CloseDataReader(dr, closeDataReader);
            }
            return objUserInfo;
        }

        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.GetUserByName")]
        public UserInfo FillUserInfo(int portalID, string username)
        {
            return GetCachedUser(portalID, username);
        }

        [Obsolete("Deprecated in DNN 5.1. This function should be replaced by String.Format(DataCache.UserCacheKey, portalId, username)")]
        public string GetCacheKey(int portalID, string username)
        {
            return string.Format(DataCache.UserCacheKey, portalID, username);
        }

        [Obsolete("Deprecated in DNN 5.1. This function should be replaced by String.Format(DataCache.UserCacheKey, portalId, username)")]
        public static string CacheKey(int portalId, string username)
        {
            return string.Format(DataCache.UserCacheKey, portalId, username);
        }

        [Obsolete("Deprecated in DNN 5.1. Not needed any longer for due to autohydration")]
        public static ArrayList GetUnAuthorizedUsers(int portalId, bool isHydrated)
        {
            return GetUnAuthorizedUsers(portalId);
        }

        [Obsolete("Deprecated in DNN 5.1. Not needed any longer for due to autohydration")]
        public static UserInfo GetUser(int portalId, int userId, bool isHydrated)
        {
            return GetUserById(portalId, userId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetUser retrieves a User from the DataStore
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "portalId">The Id of the Portal</param>
        /// <param name = "userId">The Id of the user being retrieved from the Data Store.</param>
        /// <param name = "isHydrated">A flag that determines whether the user is hydrated.</param>
        /// <param name = "hydrateRoles">A flag that instructs the method to automatically hydrate the roles</param>
        /// <returns>The User as a UserInfo object</returns>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        [Obsolete("Deprecated in DNN 5.1. Not needed any longer for single users due to autohydration")]
        public static UserInfo GetUser(int portalId, int userId, bool isHydrated, bool hydrateRoles)
        {
            var user = GetUserById(portalId, userId);

            if (hydrateRoles)
            {
                var controller = new RoleController();
                user.Roles = controller.GetRolesByUser(userId, portalId);
            }

            return user;
        }

        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.GetUserByName")]
        public UserInfo GetUserByUsername(int portalID, string username)
        {
            return GetCachedUser(portalID, username);
        }

        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.GetUserByName")]
        public UserInfo GetUserByUsername(int portalID, string username, bool synchronizeUsers)
        {
            return GetCachedUser(portalID, username);
        }

        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.GetUserByName")]
        public static UserInfo GetUserByName(int portalId, string username, bool isHydrated)
        {
            return GetCachedUser(portalId, username);
        }

        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.GetUsers")]
        public ArrayList GetSuperUsers()
        {
            return GetUsers(Null.NullInteger);
        }

        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.GetUsers")]
        public ArrayList GetUsers(bool synchronizeUsers, bool progressiveHydration)
        {
            return GetUsers(Null.NullInteger);
        }

        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.GetUsers")]
        public ArrayList GetUsers(int portalId, bool synchronizeUsers, bool progressiveHydration)
        {
            var totalRecords = -1;
            return GetUsers(portalId, -1, -1, ref totalRecords);
        }

        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.GetUsers")]
        public static ArrayList GetUsers(int portalId, bool isHydrated)
        {
            var totalRecords = -1;
            return GetUsers(portalId, -1, -1, ref totalRecords);
        }

        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.GetUsers")]
        public static ArrayList GetUsers(int portalId, bool isHydrated, int pageIndex, int pageSize, ref int totalRecords)
        {
            return GetUsers(portalId, pageIndex, pageSize, ref totalRecords);
        }

        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.GetUsersByEmail")]
        public static ArrayList GetUsersByEmail(int portalId, bool isHydrated, string emailToMatch, int pageIndex, int pageSize, ref int totalRecords)
        {
            return GetUsersByEmail(portalId, emailToMatch, pageIndex, pageSize, ref totalRecords);
        }

        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.GetUsersByUserName")]
        public static ArrayList GetUsersByUserName(int portalId, bool isHydrated, string userNameToMatch, int pageIndex, int pageSize, ref int totalRecords)
        {
            return GetUsersByUserName(portalId, userNameToMatch, pageIndex, pageSize, ref totalRecords);
        }

        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.GetUsersByProfileProperty")]
        public static ArrayList GetUsersByProfileProperty(int portalId, bool isHydrated, string propertyName, string propertyValue, int pageIndex, int pageSize, ref int totalRecords)
        {
            return GetUsersByProfileProperty(portalId, propertyName, propertyValue, pageIndex, pageSize, ref totalRecords);
        }

        [Obsolete("Deprecated in DNN 6.1. The method had no implementation !!!")]
        public static void SetAuthCookie(string username, bool createPersistentCookie)
        {
        }

        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.ChangePassword")]
        public bool SetPassword(UserInfo objUser, string newPassword)
        {
            return ChangePassword(objUser, Null.NullString, newPassword);
        }

        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.ChangePassword")]
        public bool SetPassword(UserInfo objUser, string oldPassword, string newPassword)
        {
            return ChangePassword(objUser, oldPassword, newPassword);
        }

        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.UnlockUserAccount")]
        public void UnlockUserAccount(UserInfo objUser)
        {
            UnLockUser(objUser);
        }

        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.UpdateUser")]
        public void UpdateUser(UserInfo objUser)
        {
            UpdateUser(objUser.PortalID, objUser);
        }

        #endregion
    }
}
