namespace Terrasoft.Configuration.UsrUpdateJobStatusSFDC
{
    using System;
    using System.IO;
    using System.Net;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Web;
    using System.Collections;
    using System.Web.UI;
    using System.Web;
    using System.Web.Script.Serialization;
    using System.Collections;
    using Terrasoft.Common;
    using Terrasoft.Core;
    using System.Collections.Generic;
    using Terrasoft.Core.DB;
    using Terrasoft.Core.Entities;
    using Newtonsoft.Json;
    using System.Runtime.Serialization;
    using Newtonsoft;
    using System.ComponentModel;
    using System.Reflection;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    public class UsrUpdateJobStatusSFDC
    {
        private AppConnection appConnection;
        private UserConnection userConnection;
        private Response response;

        private string url;
        private string forceExePath;
        private string logIn;
        private string pass;
        private string tok;
        private string forceExeQuery;
        private string ver;
        private string ExMessage = "";
        private string ResponseString = "";

        public UsrUpdateJobStatusSFDC()
        {
            appConnection = HttpContext.Current.Application["AppConnection"] as AppConnection;
            userConnection = appConnection.SystemUserConnection;
        }

        public UsrUpdateJobStatusSFDC(UserConnection userConnection)
        {
            this.userConnection = userConnection;
        }

        public Response GetInformationJobStatusSFDC(string sfdcId, string state, string result, string modifiedDate, string detailedResult)
        {
            Response response = new Response();

            response.Success = true;

            try
            {
                url = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceJLStatusUrl", "");

                forceExePath = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceExePath", "");

                logIn = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceLogin", "");

                pass = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforcePassword", "");

                tok = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceToken", "");

                if (state == "Completed - Approved")
                {
                    forceExeQuery = sfdcId + " DTE_State__c:" + "\"" + state + "\"";
                }
                if (state == "Completed - Rejected")
                {
                	modifiedDate = Convert.ToDateTime(modifiedDate).ToUniversalTime().ToString("O");
                    forceExeQuery = sfdcId + " DTE_Approval_or_Rejection_Date__c:" + "\"" + modifiedDate + "\"" + " DTE_Approval_or_Rejection_Date_Time__c:" + "\"" + modifiedDate + "\"" + " DTE_Rejection_Reason__c:" + "\"" + detailedResult + "\"" + " DTE_State__c:" + "\"" + state + "\"";
                }

                ver = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceVersion", "");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Error = ex.Message;
                throw new System.Exception(ex.Message);
                Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrUpdateJobStatusSFDC: ", "Error:", ex.Message, "GetInformationJobStatusSFDC Syssettings");
                return response;
            }

            response = SendJsonToWebApiJobStatusSFDC();
            return response;
        }

        public Response SendJsonToWebApiJobStatusSFDC()
        {
            Response response = new Response();
            HttpWebResponse webResponse;

            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                User obj = new User()
                {
                    login = logIn,
                    password = pass,
                    token = tok,
                    version = ver,
                    select = forceExeQuery,
                    pathToForce = forceExePath
                };

                string jsonresult = JsonConvert.SerializeObject(obj);

                var data = Encoding.UTF8.GetBytes(jsonresult);

                httpWebRequest.ContentType = "application/json";
                httpWebRequest.ContentLength = data.Length;

                using (var stream = httpWebRequest.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                	response.Success = false;
                	Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrUpdateJobStatusSFDC: ", "Error:", ex.Message, "SendJsonToWebApiJobStatusSFDC ProtocolError");
                    throw new System.Exception(ex.Message);
                }
                else
                {
                	response.Success = false;
                	Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrUpdateJobStatusSFDC: ", "Error:", ex.Message, "SendJsonToWebApiJobStatusSFDC error ToString()");
                    throw new System.Exception(ex.Message);
                }
            }

            return response;
        }

        public bool AcceptAllCertificatePolicy(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public class User
        {
            public string login { get; set; }

            public string password { get; set; }

            public string token { get; set; }

            public string version { get; set; }

            public string select { get; set; }

            public string pathToForce { get; set; }
        }

        public class Response
        {
            public string Message { get; set; }

            public bool Success { get; set; }

            public string Error { get; set; }
        }
    }
}