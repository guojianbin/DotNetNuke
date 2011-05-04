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
using System.Web;

using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.HttpModules.Exceptions
{
    public class ExceptionModule : IHttpModule
    {
        public string ModuleName
        {
            get
            {
                return "ExceptionModule";
            }
        }

        #region IHttpModule Members

        public void Init(HttpApplication application)
        {
            application.Error += OnErrorRequest;
        }

        public void Dispose()
        {
        }

        #endregion

        public void OnErrorRequest(object s, EventArgs e)
        {
            try
            {
                HttpContext Context = HttpContext.Current;
                HttpServerUtility Server = Context.Server;
                HttpRequest Request = Context.Request;
                if (Request.Url.LocalPath.ToLower().EndsWith(".aspx") == false && Request.Url.LocalPath.ToLower().EndsWith(".asmx") == false &&
                    Request.Url.LocalPath.ToLower().EndsWith(".ashx") == false)
                {
                    return;
                }
                Exception lastException = Server.GetLastError();
                if (!(lastException is HttpException))
                {
                    var lex = new Exception("Unhandled Error: ", Server.GetLastError());
                    var objExceptionLog = new ExceptionLogController();
                    try
                    {
                        objExceptionLog.AddLog(lex);
                    }
					catch (Exception ex)
					{
						lex = new Exception(lex.Message, ex);
						DnnLog.Error(ex);
					}
                }
            }
            catch (Exception exc)
            {
                DnnLog.Error(exc);

            }
        }
    }
}