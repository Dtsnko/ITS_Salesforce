namespace Terrasoft.Configuration.UsrUpdateAccountOrContactResponseQuestions
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
    
    [ServiceContract]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class UsrUpdateAccountOrContactResponseQuestions
    {
        private AppConnection appConnection;
        private UserConnection userConnection;
        private Response response;

        private string url;
        private string forceExePath;
        private string logIn;
        private string pass;
        private string tok;
        private string forceExeQueryResponseQuestions;
        private string ver;
        private string ExMessage = "";
        private string ResponseString = "";

        public UsrUpdateAccountOrContactResponseQuestions()
        {
            appConnection = HttpContext.Current.Application["AppConnection"] as AppConnection;
            userConnection = appConnection.SystemUserConnection;
        }

        public UsrUpdateAccountOrContactResponseQuestions(UserConnection userConnection)
        {
            this.userConnection = userConnection;
        }
        
        [OperationContract] 
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json,
        ResponseFormat = WebMessageFormat.Json)]
        public Response GetInformationResponseQuestion(string JobListSfdcId)
        {
            Response response = new Response();

            response.Success = true;

            try
            {
                url = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceResponseQuestionUrl", "");

                forceExePath = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceExePath", "");

                logIn = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceLogin", "");

                pass = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforcePassword", "");

                tok = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceToken", "");

                forceExeQueryResponseQuestions = "SELECT DTE_ResponseQuestion__c.LastModifiedDate, DTE_JobList__r.Id, DTE_ResponseQuestion__c.Id, DTE_ResponseQuestion__c.DTE_Question__c, DTE_ResponseQuestion__c.DTE_AnsweredCorrectly__c, DTE_ResponseQuestion__c.DTE_Value__c FROM DTE_ResponseQuestion__c WHERE DTE_JobList__r.Id = " + "\'" + JobListSfdcId + "\'" + " ORDER BY DTE_ResponseQuestion__c.LastModifiedDate ASC";

                ver = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceVersion", "");
            }
            catch (Exception ex)
            {
                response.Success = false;
                Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", "Error:", ex.Message, "GetInformationResponseQuestion syssettings");
                response.Error = ex.Message;
                throw new System.Exception(ex.Message);
                return response;
            }

            response = SendJsonToWebApiResponseQuestion();
            return response;
        }

        public Response SendJsonToWebApiResponseQuestion()
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
                    select = forceExeQueryResponseQuestions,
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

                webResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                ResponseString = new StreamReader(webResponse.GetResponseStream()).ReadToEnd();
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    response.Success = false;
                    Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", "Error:", ResponseString, "SendJsonToWebApiResponseQuestion ProtocolError");
                    throw new System.Exception(ex.Message);
                    webResponse = (HttpWebResponse)ex.Response;
                    ResponseString = "Some error occured: " + webResponse.StatusCode.ToString();
                    Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", "Error:", ResponseString, "SendJsonToWebApiResponseQuestion ProtocolError");
                }
                else
                {
                    response.Success = false;
                    Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", "Error:", ResponseString, "SendJsonToWebApiResponseQuestion ToString()");
                    throw new System.Exception(ex.Message);
                    ResponseString = "Some error occured: " + ex.Status.ToString();
                    Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", "Error:", ResponseString, "SendJsonToWebApiResponseQuestion ToString()");
                }
            }

            var json = UpdateResponseQuestionInformation();

            return response;
        }

        public ResponseQuestion ConvertJsonFromSFDCResponseQuestion()
        {
            ResponseQuestion list = new ResponseQuestion();

            var json = "{\"ResponseQuestionItem\":[" + ResponseString + "]}";
            json = json.Replace("}{", "},{");

            try
            {
                JavaScriptSerializer jsJson = new JavaScriptSerializer();
                jsJson.MaxJsonLength = int.MaxValue;
                list = jsJson.Deserialize<ResponseQuestion>(json);
            }
            catch (Exception ex)
            {
                Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", "Error:", ResponseString, "ConvertJsonFromSFDCResponseQuestion parse error");
                throw new System.Exception(ex.Message);
                return list;
            }

            return list;
        }

        public bool UpdateResponseQuestionInformation()
        {
            var obj = ConvertJsonFromSFDCResponseQuestion();

            foreach (var item in obj.ResponseQuestionItem)
            {
                var responseQuestionId = GetLookupBpmIdByString("UsrResponseQuestion", "UsrSfdcId", item.Id, "Id");
                var activityId = GetLookupBpmIdByString("Activity", "UsrSfdcId", item.DTE_JobList__r?.Id, "Id");
                
                if(item.DTE_Question__c != null && item.DTE_Question__c != String.Empty)
                {
	                var encutf8 = Encoding.GetEncoding("utf-8");
			        var encibm866 = Encoding.GetEncoding("IBM866");
			        byte[] bytes;
			        bytes = encibm866.GetBytes(item.DTE_Question__c);
			        item.DTE_Question__c = encutf8.GetString(bytes);
                }
                
                if(item.DTE_Value__c != null && item.DTE_Value__c != String.Empty)
                {
	                var encutf8 = Encoding.GetEncoding("utf-8");
			        var encibm866 = Encoding.GetEncoding("IBM866");
			        byte[] bytes;
			        bytes = encibm866.GetBytes(item.DTE_Value__c);
			        item.DTE_Value__c = encutf8.GetString(bytes);
                }

                if ((activityId != Guid.Empty && activityId != null) && item.DTE_JobList__r != null)
                {
                    var entityResponseQuestion = userConnection.EntitySchemaManager.GetInstanceByName("UsrResponseQuestion").CreateEntity(userConnection);

                    if (entityResponseQuestion.FetchFromDB(responseQuestionId))
                    {
                        entityResponseQuestion.SetColumnValue("UsrQuestion", !String.IsNullOrEmpty(item.DTE_Question__c) ? item.DTE_Question__c : String.Empty);
                        entityResponseQuestion.SetColumnValue("UsrAnsweredCorrectly", item.DTE_AnsweredCorrectly__c);
                        entityResponseQuestion.SetColumnValue("UsrValue", !String.IsNullOrEmpty(item.DTE_Value__c) ? item.DTE_Value__c : String.Empty);
                        entityResponseQuestion.SetColumnValue("UsrActivityId", activityId != Guid.Empty ? activityId : (Guid?)null);
                    }
                    else
                    {
                        responseQuestionId = Guid.NewGuid();

                        entityResponseQuestion.SetDefColumnValues();

                        entityResponseQuestion.SetColumnValue("Id", responseQuestionId);
                        entityResponseQuestion.SetColumnValue("UsrSfdcId", item.Id);
                        entityResponseQuestion.SetColumnValue("UsrActivityId", activityId != Guid.Empty ? activityId : (Guid?)null);
                        entityResponseQuestion.SetColumnValue("UsrQuestion", !String.IsNullOrEmpty(item.DTE_Question__c) ? item.DTE_Question__c : String.Empty);
                        entityResponseQuestion.SetColumnValue("UsrAnsweredCorrectly", item.DTE_AnsweredCorrectly__c);
                        entityResponseQuestion.SetColumnValue("UsrValue", !String.IsNullOrEmpty(item.DTE_Value__c) ? item.DTE_Value__c : String.Empty);
                    }

                    try
                    {
                        entityResponseQuestion.Save(false);
                    }
                    catch (Exception ex)
                    {
                        response.Success = false;
                        Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", "Error:", ex.Message, "UpdateResponseQuestionInformation entityResponseQuestion save");
                        throw new System.Exception(ex.Message);
                    }
                }
            }

            return true;
        }
        
        public Guid GetLookupBpmIdByString(string table, string column, string value, string columnReturn)
        {
            if (value == String.Empty || value == null)
            {
                return Guid.Empty;
            }

            var lookupBPMId = (new Select(userConnection).Top(1)
                .Column(columnReturn)
                .From(table)
                .Where(column).IsEqual(Column.Parameter(value)) as Select).ExecuteScalar<Guid>();
            return lookupBPMId;
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
        
        public class ResponseQuestion
        {
            [JsonProperty("ResponseQuestionItem")]
            public List<ResponseQuestionItems> ResponseQuestionItem { get; set; }
        }

        public class ResponseQuestionItems
        {
            [JsonProperty("DTE_AnsweredCorrectly__c")]
            public bool DTE_AnsweredCorrectly__c { get; set; }

            [JsonProperty("DTE_JobList__r")]
            public DTE_Joblist__R DTE_JobList__r { get; set; }

            [JsonProperty("DTE_Question__c")]
            public string DTE_Question__c { get; set; }

            [JsonProperty("DTE_Value__c")]
            public string DTE_Value__c { get; set; }

            [JsonProperty("Id")]
            public string Id { get; set; }

            [JsonProperty("LastModifiedDate")]
            public DateTime? LastModifiedDate { get; set; }
        }

        public class DTE_Joblist__R
        {
            [JsonProperty("Id")]
            public string Id { get; set; }

            [JsonProperty("LastModifiedDate")]
            public DateTime? LastModifiedDate { get; set; }
        }
    }
}