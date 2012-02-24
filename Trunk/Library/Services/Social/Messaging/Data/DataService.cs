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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Services.Social.Messaging.Views;

#endregion

namespace DotNetNuke.Services.Social.Messaging.Data
{
    internal class DataService : ComponentBase<IDataService, DataService>, IDataService
        
    {
        private readonly DataProvider _provider = DataProvider.Instance();

        #region Messages CRUD

        public int SaveSocialMessage(Message message, int createUpdateUserId)
        {
            //need to fix groupmail
            return _provider.ExecuteScalar<int>("SaveSocialMessage", message.MessageID, message.To, message.From, message.Subject, message.Body, message.ParentMessageID, message.ReplyAllAllowed, message.SenderUserID, createUpdateUserId);
        }

        public IDataReader GetSocialMessage()
        {
            return _provider.ExecuteReader("GetSocialMessage");
        }

        public IDataReader GetSocialMessagesBySender()
        {
            return _provider.ExecuteReader("GetSocialMessagesBySender");
        }

        public void DeleteSocialMessage(int messageId)
        {
            _provider.ExecuteNonQuery("DeleteSocialMessage", messageId);
        }

        public int CreateMessageReply(int parentMessageId, string body, int senderUserId, string from, int createUpdateUserId)
        {
            return _provider.ExecuteScalar<int>("CreateSocialMessageReply", parentMessageId, body, senderUserId, from, createUpdateUserId);
        }

        public IList<MessageItemView> GetMessageItems(int userId, int pageIndex, int pageSize, string sortColumn, bool sortAscending, MessageReadStatus readStatus, MessageArchivedStatus archivedStatus, MessageSentStatus sentStatus, ref int totalRecords)
        {
            object read = null;
            object archived = null;
            object sent = null;

            switch (readStatus)
            {
                case MessageReadStatus.Read:
                    read = true;
                    break;
                case MessageReadStatus.UnRead:
                    read = false;
                    break;
                case MessageReadStatus.Any:
                    read = null;
                    break;
            }

            switch (archivedStatus)
            {
                case MessageArchivedStatus.Archived:
                    archived = true;
                    break;
                case MessageArchivedStatus.UnArchived:
                    archived = false;
                    break;
                case MessageArchivedStatus.Any:
                    archived = null;
                    break;
            }

            switch (sentStatus)
            {
                case MessageSentStatus.Received:
                    sent = false;
                    break;
                case MessageSentStatus.Sent:
                    sent = true;
                    break;
                case MessageSentStatus.Any:
                    sent = null;
                    break;
            }

            IDataReader dr = _provider.ExecuteReader("GetMessageItems", userId, pageIndex, pageSize, sortColumn, sortAscending, read, archived, sent);

            try
            {
                while (dr.Read())
                {
                    totalRecords = Convert.ToInt32(dr["TotalRecords"]);
                }
                dr.NextResult();
                return CBO.FillCollection<MessageItemView>(dr);
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }
        }

        public IList<MessageItemView> GetMessageThread(int messageId, int userId, int pageIndex, int pageSize, string sortColumn, bool @sortAscending, ref int totalRecords)
        {
            IDataReader dr = _provider.ExecuteReader("GetMessageThread", messageId, userId, pageIndex, pageSize, sortColumn, sortAscending);

            try
            {
                while (dr.Read())
                {
                    totalRecords = Convert.ToInt32(dr["TotalRecords"]);
                }
                dr.NextResult();
                return CBO.FillCollection<MessageItemView>(dr);
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }
        }

        public void UpdateSocialMessageReadStatus(int parentMessageId, int userId, bool read)
        {
            _provider.ExecuteNonQuery("UpdateSocialMessageReadStatus", parentMessageId, userId, read);
        }

        public void UpdateSocialMessageArchivedStatus(int parentMessageId, int userId, bool archived)
        {
            _provider.ExecuteNonQuery("UpdateSocialMessageArchivedStatus", parentMessageId, userId, archived);
        }


        #endregion

        #region Message_Recipients CRUD

        public int SaveSocialMessageRecipient(MessageRecipient messageRecipient, int createUpdateUserId)
        {
            return _provider.ExecuteScalar<int>("SaveSocialMessageRecipient", messageRecipient.RecipientID, messageRecipient.MessageID, messageRecipient.UserID, messageRecipient.Read, messageRecipient.Archived, messageRecipient.Sent, createUpdateUserId);
        }

        public void CreateSocialMessageRecipientsForRole(int messageId, string roleIds, int createUpdateUserId)
        {
            _provider.ExecuteNonQuery("CreateSocialMessageRecipientsForRole", messageId, roleIds, createUpdateUserId);
        }

        public IDataReader GetSocialMessageRecipient(int messageRecipientId)
        {
            return _provider.ExecuteReader("GetSocialMessageRecipient", messageRecipientId);
        }

        public IDataReader GetSocialMessageRecipientsByUser(int userId)
        {
            return _provider.ExecuteReader("GetSocialMessageRecipientsByUser", userId);
        }

        public IDataReader GetSocialMessageRecipientsByMessage(int messageId)
        {
            return _provider.ExecuteReader("GetSocialMessageRecipientsByMessage", messageId);
        }

        public IDataReader GetSocialMessageRecipientByMessageAndUser(int messageId, int userId, MessageSentStatus sentStatus)
        {
            object sent = null;

            switch (sentStatus)
            {
                case MessageSentStatus.Received:
                    sent = false;
                    break;
                case MessageSentStatus.Sent:
                    sent = true;
                    break;
                case MessageSentStatus.Any:
                    sent = null;
                    break;
            }

            return _provider.ExecuteReader("GetSocialMessageRecipientsByMessageAndUser", messageId, userId, sent);
        }

        public void DeleteSocialMessageRecipient(int messageRecipientId)
        {
            _provider.ExecuteNonQuery("DeleteSocialMessageRecipient", messageRecipientId);
        }

        #endregion

        #region Message_Attachments CRUD

        public int SaveSocialMessageAttachment(MessageAttachment messageAttachment, int createUpdateUserId)
        {
            return _provider.ExecuteScalar<int>("SaveSocialMessageAttachment", messageAttachment.MessageAttachmentID, messageAttachment.MessageID, messageAttachment.FileID, createUpdateUserId);
        }

        public IDataReader GetSocialMessageAttachment()
        {
            return _provider.ExecuteReader("GetSocialMessageAttachment");
        }

        public IDataReader GetSocialMessageAttachmentsByMessage()
        {
            return _provider.ExecuteReader("GetSocialMessageAttachmentsByMessage");
        }

        public void DeleteSocialMessageAttachment(int messageAttachmentID)
        {
            _provider.ExecuteNonQuery("DeleteSocialMessageAttachment", messageAttachmentID);
        }

        #endregion
    }
}