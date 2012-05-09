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

using System;
using System.Collections.Generic;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;

namespace DotNetNuke.Services.Social.Messaging
{
    public interface IMessagingController
    {
        #region CRUD APIs

        Message GetMessage(int messageId);
        MessageRecipient GetMessageRecipient(int messageId, int userId);
        IList<MessageRecipient> GetMessageRecipients(int messageId);
        void DeleteMessageRecipient(int messageId, int userId);


        void MarkRead(int conversationId, int userId);
        void MarkUnRead(int conversationId, int userId);
        void MarkArchived(int conversationId, int userId);
        void MarkUnArchived(int conversationId, int userId);

        #endregion

        #region Admin Settings APIs

        ///<summary>How long a user needs to wait before user is allowed sending the next message</summary>
        ///<returns>Time in seconds. Returns zero if user has never sent a message</returns>
        /// <param name="sender">Sender's UserInfo</param>        
        int WaitTimeForNextMessage(UserInfo sender);

        ///<summary>Last message sent by the User</summary>
        ///<returns>Message. Null when no message was sent</returns>
        /// <param name="sender">Sender's UserInfo</param>        
        Message GetLastSentMessage(UserInfo sender);

        ///<summary>Are attachments allowed</summary>        
        ///<returns>True or False</returns>
        /// <param name="portalId">Portal Id</param>        
        bool AttachmentsAllowed(int portalId);

        ///<summary>Maximum number of Recipients allowed</summary>        
        ///<returns>Count. Message to a Role is considered a single Recipient. Each User in the To list is counted as one User each.</returns>
        /// <param name="portalId">Portal Id</param>        
        int RecipientLimit(int portalId);

        #endregion

        #region Create and Reply APIs

        void SendMessage(Message message, IList<RoleInfo> roles, IList<UserInfo> users, IList<int> fileIDs);

        void SendMessage(Message message, IList<RoleInfo> roles, IList<UserInfo> users, IList<int> fileIDs, UserInfo sender);

        int ReplyMessage(int conversationId, string body, IList<int> fileIDs);

        int ReplyMessage(int conversationId, string body, IList<int> fileIDs, UserInfo sender);

        #endregion        

    }
}