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
using System.Collections.Generic;
using System.Xml.Serialization;

using DotNetNuke.Security.Roles;

#endregion

namespace DotNetNuke.Entities.Users.Social
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Users
    /// Class:      UserSocial
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The UserSocial is a high-level class describing social details of a user. 
    /// As an example, this class contains Friends, Followers, Follows lists.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class UserSocial
    {
        #region Private

        private IList<Relationship> _relationships;
        private IDictionary<string, IList<UserRelationship>> _userRelationships;
        private  IList<UserRoleInfo> _roles;
        private readonly UserInfo _userInfo;
        private readonly RelationshipController _relationshipController;
        private readonly RoleController _roleController;

        #endregion

        #region Constructor

        public UserSocial(UserInfo userInfo)
        {
            _relationshipController = new RelationshipController();
            _roleController = new RoleController();
            _userInfo = userInfo;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns the Friend Relationship (if it exists with the current User)
        /// </summary>
        public UserRelationship Friend
        {
            get { return RelationshipController.Instance.GetFriendRelationship(_userInfo); }
        }

        /// <summary>
        /// Returns the Follower Relationship (if it exists with the current User)
        /// </summary>
        public UserRelationship Follower
        {
            get { return RelationshipController.Instance.GetFollowerRelationship(_userInfo); }
        }

        /// <summary>
        /// Returns the Following Relationship (if it exists with the current User)
        /// </summary>
        public UserRelationship Following
        {
            get { return RelationshipController.Instance.GetFollowingRelationship(_userInfo); }
        }


        /// <summary>
        /// A collection of all the relationships the user is a member of.
        /// </summary>
        public IDictionary<string, IList<UserRelationship>> UserRelationships
        {
            get { return _userRelationships ?? (_userRelationships = GetUserRelationships()); }
        }

        /// <summary>
        /// List of Relationships for the User
        /// </summary>
        [XmlAttribute]
        public IList<Relationship> Relationships
        {
            get
            {
                if (_relationships == null)
                {
                    _relationships = RelationshipController.Instance.GetRelationshipsByPortalId(_userInfo.PortalID);

                    foreach (var r in RelationshipController.Instance.GetRelationshipsByUserId(_userInfo.UserID))
                    {
                        _relationships.Add(r);
                    }
                }

                return _relationships;
            }
        }

        /// <summary>
        /// List of Roles/Groups for the User
        /// </summary>
        [XmlAttribute]
        public IList<UserRoleInfo> Roles
        {
            get { return _roles ?? (_roles = _roleController.GetUserRoles(_userInfo, true)); }
        }

        #endregion

        private IDictionary<string, IList<UserRelationship>> GetUserRelationships()
        {
            var dictionary = new Dictionary<string, IList<UserRelationship>>();

            foreach (UserRelationship userRelationship in RelationshipController.Instance.GetUserRelationships(_userInfo))
            {
                Relationship relationship = RelationshipController.Instance.GetRelationship(userRelationship.RelationshipId);

                IList<UserRelationship> userRelationshipList;
                if (!dictionary.TryGetValue(relationship.RelationshipId.ToString(), out userRelationshipList))
                {
                    userRelationshipList = new List<UserRelationship>();
                    dictionary.Add(relationship.RelationshipId.ToString(), userRelationshipList);
                }

                userRelationshipList.Add(userRelationship);
            }

            return dictionary;
        }
    }
}
