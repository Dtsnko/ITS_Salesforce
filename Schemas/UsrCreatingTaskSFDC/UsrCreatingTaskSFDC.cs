namespace Terrasoft.Configuration.UsrCreatingTaskSFDC
{
    using System;
    using System.Data;
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
    using Terrasoft;
    using Terrasoft.Common;
    using Terrasoft.Core;
    using System.Collections.Generic;
    using Terrasoft.Core.DB;
    using Terrasoft.Core.Entities;
    using Terrasoft.Configuration;
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

    public class UsrCreatingTaskSFDC
    {
        private AppConnection appConnection;
        private UserConnection userConnection;
        private Response response;

        private string url;
        private string forceExePath;
        private string logIn;
        private string pass;
        private string tok;
        private Activity forceExeQuery;
        private string ver;
        private string ExMessage = "";
        private string ResponseString = "";
        private Guid activityId;

        public UsrCreatingTaskSFDC()
        {
            appConnection = HttpContext.Current.Application["AppConnection"] as AppConnection;
            userConnection = appConnection.SystemUserConnection;
        }

        public UsrCreatingTaskSFDC(UserConnection userConnection)
        {
            this.userConnection = userConnection;
        }

        public Response GetInformationCreatingTaskSFDC(Guid Id)
        {
            activityId = Id;

            Response response = new Response();

            response.Success = true;

            try
            {
                url = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceJDLCreateUrl", "");

                forceExePath = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceExePath", "");

                logIn = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceLogin", "");

                pass = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforcePassword", "");

                tok = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceToken", "");

                ver = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceVersion", "");
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Error = ex.Message;
                Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrCreatingTaskSFDC: ", "Error:", ex.Message, "GetInformationCreatingTaskSFDC Syssettings");
                throw new System.Exception(ex.Message);
                return response;
            }

            response = GetActivityInfo();
            return response;
        }

        public Response GetActivityInfo()
        {
            Response response = new Response();

            response.Success = true;

            Activity activityobjects = new Activity();

            forceExeQuery = activityobjects;

            try
            {
                Select select = new Select(userConnection)

                .Column("Account", "UsrIDPOS").As("IDPOS")
                .Column("Account", "Code").As("PosCode")               
                .Column("OriginalActivity", "Title").As("JobListName")
                .Column("UsrActivityMarket", "Name").As("Market")
                .Column("UsrActivityAudienceType", "Name").As("AudienceType")
                .Column("Contact", "Name").As("FullName")
                .Column("Activity", "Title").As("Title")
                .Column("UsrActivityCurrencyIsoCode", "Name").As("CurrencyIsoCode")
                .Column("Activity", "DueDate").As("DueDate")
                .Column("Activity", "StartDate").As("StartDate")
                .Column("Activity", "UsrJobDefinitionListTemplate").As("JobDefinitionListTemplate")

                .From("Activity").As("Activity")

                .Join(JoinType.LeftOuter, "Account")
                .On("Activity", "AccountId").IsEqual("Account", "Id")
                .Join(JoinType.LeftOuter, "Activity").As("OriginalActivity")
                .On("Activity", "UsrOriginalActivityId").IsEqual("OriginalActivity", "Id")
                .Join(JoinType.LeftOuter, "UsrActivityMarket")
                .On("Activity", "UsrMarketId").IsEqual("UsrActivityMarket", "Id")
                .Join(JoinType.LeftOuter, "UsrActivityAudienceType")
                .On("Activity", "UsrAudienceTypeId").IsEqual("UsrActivityAudienceType", "Id")
                .Join(JoinType.LeftOuter, "Contact")
                .On("Activity", "ContactId").IsEqual("Contact", "Id")
                .Join(JoinType.LeftOuter, "UsrActivityCurrencyIsoCode")
                .On("Activity", "UsrCurrencyIsoCodeId").IsEqual("UsrActivityCurrencyIsoCode", "Id")

                .Where("Activity", "Id").IsEqual(Terrasoft.Core.DB.Column.Parameter(activityId))

                as Select;

                using (DBExecutor dbExecutor = userConnection.EnsureDBConnection())
                {
                    using (IDataReader reader = select.ExecuteReader(dbExecutor))
                    {
                        while (reader.Read())
                        {
                            activityobjects.IDPOS = reader.GetColumnValue<string>("IDPOS") ?? String.Empty;
                            activityobjects.PosCode = reader.GetColumnValue<string>("PosCode") ?? String.Empty;
                            activityobjects.JobListName = reader.GetColumnValue<string>("JobListName") ?? String.Empty;
                            activityobjects.Market = reader.GetColumnValue<string>("Market") ?? String.Empty;
                            activityobjects.AudienceType = reader.GetColumnValue<string>("AudienceType") ?? String.Empty;
                            activityobjects.FullName = reader.GetColumnValue<string>("FullName") ?? String.Empty;
                            activityobjects.Title = reader.GetColumnValue<string>("Title") ?? String.Empty;
                            activityobjects.CurrencyIsoCode = reader.GetColumnValue<string>("CurrencyIsoCode") ?? String.Empty;
                            activityobjects.DueDate = reader.GetColumnValue<string>("DueDate") ?? String.Empty;
                            activityobjects.DueDate = Convert.ToDateTime(activityobjects.DueDate).ToString("O");
                            activityobjects.StartDate = reader.GetColumnValue<string>("StartDate") ?? String.Empty;
                            activityobjects.StartDate = Convert.ToDateTime(activityobjects.StartDate).ToString("O");
                            activityobjects.JobDefinitionListTemplate = reader.GetColumnValue<string>("JobDefinitionListTemplate") ?? String.Empty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrCreatingTaskSFDC()", "Error:", ex.Message, "GetActivityInfo()");
                throw new System.Exception(ex.Message);
            }

            response = SendJsonToWebApiCreatingTaskSFDC();
            return response;
        }

        public Response SendJsonToWebApiCreatingTaskSFDC()
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
                    objects = forceExeQuery,
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
                    Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrCreatingTaskSFDC: ", "Error:", ex.Message, "SendJsonToWebApiCreatingTaskSFDC ProtocolError");
                    throw new System.Exception(ex.Message);
                }
                else
                {
                    response.Success = false;
                    Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrCreatingTaskSFDC: ", "Error:", ex.Message, "SendJsonToWebApiCreatingTaskSFDC error ToString()");
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

            public string pathToForce { get; set; }

            public Activity objects { get; set; }
        }

        public class Activity
        {
            [JsonProperty("IDPOS")]
            public string IDPOS { get; set; }

            [JsonProperty("PosCode")]
            public string PosCode { get; set; }

            [JsonProperty("JobListName")]
            public string JobListName { get; set; }

            [JsonProperty("Market")]
            public string Market { get; set; }

            [JsonProperty("AudienceType")]
            public string AudienceType { get; set; }

            [JsonProperty("FullName")]
            public string FullName { get; set; }

            [JsonProperty("Title")]
            public string Title { get; set; }

            [JsonProperty("CurrencyIsoCode")]
            public string CurrencyIsoCode { get; set; }

            [JsonProperty("DueDate")]
            public string DueDate { get; set; }

            [JsonProperty("StartDate")]
            public string StartDate { get; set; }

            [JsonProperty("JobDefinitionListTemplate")]
            public string JobDefinitionListTemplate { get; set; }
        }

        public class Response
        {
            public string Message { get; set; }

            public bool Success { get; set; }

            public string Error { get; set; }
        }
    }
}