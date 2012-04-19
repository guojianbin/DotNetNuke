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

using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Social.Messaging;
using DotNetNuke.Services.Scheduling;

#endregion

namespace DotNetNuke.Services.Social.Messaging.Scheduler
{
    public class CoreMessagingScheduler : SchedulerClient
    {
      
        private readonly PortalController _pController = new PortalController();

        private readonly UserController _uController = new UserController();

        public CoreMessagingScheduler(ScheduleHistoryItem objScheduleHistoryItem)
        {
            ScheduleHistoryItem = objScheduleHistoryItem;
        }

        public override void DoWork()
        {
            try
            {
                Guid _schedulerInstance = Guid.NewGuid();
                ScheduleHistoryItem.AddLogNote("MessagingScheduler DoWork Starting " + _schedulerInstance);

                if ((string.IsNullOrEmpty(Host.SMTPServer)))
                {
                    ScheduleHistoryItem.AddLogNote("No SMTP Servers have been configured for this host. Terminating task.");
                    ScheduleHistoryItem.Succeeded = true;
                    //'Return
                }
                else
                {
                    Hashtable settings = ScheduleHistoryItem.GetSettings();

                    bool _messageLeft = true;
                    int _messagesSent = 0;

                   
                    while (_messageLeft)
                    {
                        IList<MessageRecipient> batchMessages = MessagingController.Instance.GetNextMessagesForDispatch(_schedulerInstance, Convert.ToInt32(Entities.Host.Host.MessageSchedulerBatchSize.ToString()));
                        
                        if ((batchMessages != null))
                        {
                            try
                            {
                                foreach (var messageRecipient in batchMessages)
                                {
                                    SendMessage(messageRecipient);
                                    _messagesSent = _messagesSent + 1;   
                                }
                              
                            }
                            catch (Exception e)
                            {
                                Errored(ref e);
                            }
                        }
                        else
                        {
                            _messageLeft = false;
                        }
                    }

                    ScheduleHistoryItem.AddLogNote(string.Format("Message Scheduler '{0}' sent a total of {1} message(s)", _schedulerInstance, _messagesSent));
                    ScheduleHistoryItem.Succeeded = true;
                }
            }
            catch (Exception ex)
            {
                ScheduleHistoryItem.Succeeded = false;
                ScheduleHistoryItem.AddLogNote("MessagingScheduler Failed: " + ex);
                Errored(ref ex);
            }
        }

        private void SendMessage(MessageRecipient objMessage)
        {
            //MessagingController.Instance.g
            //todo: check if host user can send to multiple portals...
            Message messageDetails =MessagingController.Instance.GetMessage(objMessage.MessageID);

            string senderAddress = UserController.GetUserById(messageDetails.PortalID, PortalSettings.Current.AdministratorId).Email;
            string fromAddress = _pController.GetPortal(messageDetails.PortalID).Email;
            string toAddress = _uController.GetUser(messageDetails.PortalID, objMessage.RecipientID).Email;


            Mail.Mail.SendEmail(fromAddress, senderAddress, toAddress, messageDetails.Subject, messageDetails.Body);


            MessagingController.Instance.MarkMessageAsDispatched(objMessage.MessageID, objMessage.RecipientID);
        }
    }
}