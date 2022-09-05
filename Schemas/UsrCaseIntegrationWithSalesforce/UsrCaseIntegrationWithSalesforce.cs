namespace Terrasoft.Configuration
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

    public class UsrCaseIntegrationWithSalesforce
    {
        private AppConnection appConnection;
        private UserConnection userConnection;
        private Response response;
        private string url;
        private string forceExePath;
        private string logIn;
        private string pass;
        private string tok;
        private string forceExeQueryCase;
        private string ver;
        private string ExMessage = "";
        private string ResponseString = "";

        public UsrCaseIntegrationWithSalesforce()
        {
            appConnection = HttpContext.Current.Application["AppConnection"] as AppConnection;
            userConnection = appConnection.SystemUserConnection;
        }

        public UsrCaseIntegrationWithSalesforce(UserConnection userConnection)
        {
            this.userConnection = userConnection;
        }

        public Response GetInformationCase()
        {
             Response response = new Response();
            response.Success = true;
            try
            {
                url = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceCaseUrl", "");
                forceExePath = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceExePath", "");
                logIn = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceLogin", "");
                pass = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforcePassword", "");
                tok = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceToken", "");
                forceExeQueryCase = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceCaseQuery", "");
                ver = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceVersion", "");
            }
            catch (Exception ex)
            {
                response.Success = false;
                Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrCaseIntegrationWithSalesforce: ", "Error:", ex.Message, "GetInformationCase Syssettings");
                response.Error = ex.Message;
                return response;
            }

            response = SendJsonToWebApiCase();
            return response;
        }

        public Response SendJsonToWebApiCase()
        {
            Response response = new Response();
            HttpWebResponse webResponse;
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.Timeout = 100000000;
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                User obj = new User()
                {
                    login = logIn,
                    password = pass,
                    token = tok,
                    version = ver,
                    select = forceExeQueryCase,
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
                    Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrCaseIntegrationWithSalesforce: ", "Error:", ResponseString, "SendJsonToWebApiCase ProtocolError");
                    throw new System.Exception(ex.Message);
                    webResponse = (HttpWebResponse)ex.Response;
                    ResponseString = "Some error occured: " + webResponse.StatusCode.ToString();
                    Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrCaseIntegrationWithSalesforce: ", "Error:", ResponseString, "SendJsonToWebApiCase ProtocolError");
                }
                else
                {
                    response.Success = false;
                    Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrCaseIntegrationWithSalesforce: ", "Error:", ResponseString, "SendJsonToWebApiCase error");
                    throw new System.Exception(ex.Message);
                    ResponseString = "Some error occured: " + ex.Status.ToString();
                    Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrCaseIntegrationWithSalesforce: ", "Error:", ResponseString, "SendJsonToWebApiCase error");
                }
            }

            var json = UpdateCaseInformation();
            return response;
        }

        public Case ConvertJsonFromSFDCCase()
        {
            Case list = new Case();
            var json = "{\"CaseItem\":[" + ResponseString + "]}";
            json = json.Replace("}{", "},{");
            try
            {
                JavaScriptSerializer jsJson = new JavaScriptSerializer();
                jsJson.MaxJsonLength = int.MaxValue;
                list = jsJson.Deserialize<Case>(json);
            }
            catch (Exception ex)
            {
                Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrCaseIntegrationWithSalesforce: ", "Error:", json, "ConvertJsonFromSFDCCase parse error");
                throw new System.Exception(ex.Message);
                return list;
            }

            return list;
        }

        public bool UpdateCaseInformation()
        {
            Response response = new Response();
            response.Success = true;

            var obj = ConvertJsonFromSFDCCase();

            foreach (var item in obj.CaseItem)
            {
                var ownerId = GetLookupBpmIdByString("Contact", "UsrContactID", item.DTE_JobList__r.DTE_MC_Manager_Contact__c, "OwnerId");
                var statusId = GetLookupBpmIdByString("CaseStatus", "Name", "New", "Id");
                var priorityId = GetLookupBpmIdByString("CasePriority", "Name", "Medium", "Id");
                var originId = GetLookupBpmIdByString("CaseOrigin", "Name", "SFDC", "Id");
                var accountId = GetLookupBpmIdByString("Account", "Code", item.DTE_JobList__r.Account__r.DTE_AccountNumber__c, "Id");
                var contactId = GetLookupBpmIdByString("Contact", "UsrContactID", item.DTE_JobList__r.DTE_MC_Manager_Contact__c, "Id");
                var categoryId = GetLookupBpmIdByString("CaseCategory", "Name", "Заявка контакта", "Id");
                var activityId = GetLookupBpmIdByString("Activity", "UsrSfdcId", item.DTE_JobList__c, "Id");
                var caseId = GetLookupBpmIdByString("Case", "UsrJobListId", item.DTE_JobList__c, "Id");

                var entityCase = userConnection.EntitySchemaManager.GetInstanceByName("Case").CreateEntity(userConnection);

                if (originId == Guid.Empty)
                {
                    originId = Guid.NewGuid();

                    var entityCaseOriginId = userConnection.EntitySchemaManager.GetInstanceByName("CaseOrigin").CreateEntity(userConnection);

                    entityCaseOriginId.SetDefColumnValues();

                    entityCaseOriginId.SetColumnValue("Name", "SFDC");
                    entityCaseOriginId.SetColumnValue("Id", originId);

                    try
                    {
                        entityCaseOriginId.Save(false);
                    }
                    catch (Exception ex)
                    {
                        response.Success = false;
                        Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrCaseIntegrationWithSalesforce", "Error:", ex.Message, "UpdateCaseInformation entityCaseOriginId save");
                        throw new System.Exception(ex.Message);
                    }
                }

                if ((contactId == Guid.Empty || contactId == null) && item.DTE_JobList__r.DTE_MC_Manager_Contact__c != null)
                {
                    contactId = Guid.NewGuid();

                    var entityContactId = userConnection.EntitySchemaManager.GetInstanceByName("Contact").CreateEntity(userConnection);

                    entityContactId.SetDefColumnValues();

                    entityContactId.SetColumnValue("UsrContactID", item.DTE_JobList__r.DTE_MC_Manager_Contact__c);
                    entityContactId.SetColumnValue("Id", contactId);

                    try
                    {
                        entityContactId.Save(false);
                    }
                    catch (Exception ex)
                    {
                        response.Success = false;
                        Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrCaseIntegrationWithSalesforce: ", "Error:", ex.Message, "UpdateCaseInformation entityContactId save");
                        throw new System.Exception(ex.Message);
                    }
                }

                var encutf8 = Encoding.GetEncoding("utf-8");
                var encibm866 = Encoding.GetEncoding("IBM866");
                byte[] bytes;
                bytes = encibm866.GetBytes(item.DTE_Question__c);
                item.DTE_Question__c = encutf8.GetString(bytes);

                var encUtf8 = Encoding.GetEncoding("utf-8");
                var encIbm866 = Encoding.GetEncoding("IBM866");
                byte[] bytess;
                bytess = encIbm866.GetBytes(item.DTE_Value__c);
                item.DTE_Value__c = encUtf8.GetString(bytess);

                if (item.DTE_Question__c == "Будь ласка, обери, з якою складністю ти стикнувся:")
                {
                    if (entityCase.FetchFromDB(caseId))
                    {
                        var symptoms = GetSymptoms("Case", "Id", caseId, "Symptoms");

                        if (!symptoms.Contains(item.DTE_Value__c))
                        {
                            entityCase.SetDefColumnValues();
                            entityCase.SetColumnValue("Symptoms", symptoms + "_" + item.DTE_Value__c);

                            try
                            {
                                entityCase.Save(false);
                            }
                            catch (Exception ex)
                            {
                                response.Success = false;
                                Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrCaseIntegrationWithSalesforce: ", "Error:", ex.Message, "UpdateCaseInformation entityCase save");
                                throw new System.Exception(ex.Message);
                            }
                        }
                    }
                    else
                    {
                        caseId = Guid.NewGuid();

                        entityCase.SetDefColumnValues();

                        entityCase.SetColumnValue("Id", caseId);
                        entityCase.SetColumnValue("UsrSfdcId", item.Id);
                        entityCase.SetColumnValue("CreatedOn", item.CreatedDate);
                        entityCase.SetColumnValue("Subject", " ");
                        entityCase.SetColumnValue("Symptoms", item.DTE_Value__c);
                        entityCase.SetColumnValue("OwnerId", ownerId != Guid.Empty ? ownerId : (Guid?)null);
                        entityCase.SetColumnValue("StatusId", statusId != Guid.Empty ? statusId : (Guid?)null);
                        entityCase.SetColumnValue("PriorityId", priorityId != Guid.Empty ? priorityId : (Guid?)null);
                        entityCase.SetColumnValue("OriginId", originId != Guid.Empty ? originId : (Guid?)null);
                        entityCase.SetColumnValue("AccountId", accountId != Guid.Empty ? accountId : (Guid?)null);
                        entityCase.SetColumnValue("ContactId", contactId != Guid.Empty ? contactId : (Guid?)null);
                        entityCase.SetColumnValue("CategoryId", categoryId != Guid.Empty ? categoryId : (Guid?)null);
                        entityCase.SetColumnValue("UsrJobListId", item.DTE_JobList__c);
                        entityCase.SetColumnValue("RegisteredOn", DateTime.Now);

                        try
                        {
                            entityCase.Save(false);
                        }
                        catch (Exception ex)
                        {
                            response.Success = false;
                            Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrCaseIntegrationWithSalesforce: ", "Error:", ex.Message, "UpdateCaseInformation entityCase save");
                            throw new System.Exception(ex.Message);
                        }
                    }
                }

                if (item.DTE_Question__c == "Розкажи нам детальніше про свою ситуацію:")
                {
                    if (entityCase.FetchFromDB(caseId))
                    {
                        var symptoms = GetSymptoms("Case", "Id", caseId, "Symptoms");

                        if (!symptoms.Contains(item.DTE_Value__c))
                        {
                            entityCase.SetDefColumnValues();
                            entityCase.SetColumnValue("Symptoms", symptoms + "_" + item.DTE_Value__c);

                            try
                            {
                                entityCase.Save(false);
                            }
                            catch (Exception ex)
                            {
                                response.Success = false;
                                Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrCaseIntegrationWithSalesforce: ", "Error:", ex.Message, "UpdateCaseInformation entityCase save");
                                throw new System.Exception(ex.Message);
                            }
                        }
                    }
                    else
                    {
                        caseId = Guid.NewGuid();

                        entityCase.SetDefColumnValues();

                        entityCase.SetColumnValue("Id", caseId);
                        entityCase.SetColumnValue("UsrSfdcId", item.Id);
                        entityCase.SetColumnValue("CreatedOn", item.CreatedDate);
                        entityCase.SetColumnValue("Subject", " ");
                        entityCase.SetColumnValue("Symptoms", item.DTE_Value__c);
                        entityCase.SetColumnValue("OwnerId", ownerId != Guid.Empty ? ownerId : (Guid?)null);
                        entityCase.SetColumnValue("StatusId", statusId != Guid.Empty ? statusId : (Guid?)null);
                        entityCase.SetColumnValue("PriorityId", priorityId != Guid.Empty ? priorityId : (Guid?)null);
                        entityCase.SetColumnValue("OriginId", originId != Guid.Empty ? originId : (Guid?)null);
                        entityCase.SetColumnValue("AccountId", accountId != Guid.Empty ? accountId : (Guid?)null);
                        entityCase.SetColumnValue("ContactId", contactId != Guid.Empty ? contactId : (Guid?)null);
                        entityCase.SetColumnValue("CategoryId", categoryId != Guid.Empty ? categoryId : (Guid?)null);
                        entityCase.SetColumnValue("UsrJobListId", item.DTE_JobList__c);
                        entityCase.SetColumnValue("RegisteredOn", DateTime.Now);

                        try
                        {
                            entityCase.Save(false);
                        }
                        catch (Exception ex)
                        {
                            response.Success = false;
                            Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrCaseIntegrationWithSalesforce: ", "Error:", ex.Message, "UpdateCaseInformation entityCase save");
                            throw new System.Exception(ex.Message);
                        }
                    }
                }

                if (item.DTE_Question__c == "Будь ласка, обери, щодо якого питання ти хочеш з нами зв’язатися:")
                {
                    if (entityCase.FetchFromDB(caseId))
                    {
                        var serviceItemId = GetLookupBpmIdByString("ServiceItem", "Name", item.DTE_Value__c, "Id");

                        entityCase.SetDefColumnValues();
                        entityCase.SetColumnValue("ServiceItemId", serviceItemId != Guid.Empty ? serviceItemId : (Guid?)null);

                        try
                        {
                            entityCase.Save(false);
                        }
                        catch (Exception ex)
                        {
                            response.Success = false;
                            Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrCaseIntegrationWithSalesforce: ", "Error:", ex.Message, "UpdateCaseInformation entityCase save");
                            throw new System.Exception(ex.Message);
                        }
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

        public string GetSymptoms(string table, string column, Guid value, string columnReturn)
        {
            if (value == Guid.Empty || value == null)
            {
                return string.Empty;
            }

            var Symptoms = (new Select(userConnection).Top(1)
                .Column(columnReturn)
                .From(table)
                .Where(column).IsEqual(Column.Parameter(value)) as Select).ExecuteScalar<String>();
            return Symptoms;
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

        public class Case
        {
            [JsonProperty("CaseItem")]
            public List<CaseItems> CaseItem { get; set; }
        }

        public class CaseItems
        {
            [JsonProperty("CreatedDate")]
            public DateTime? CreatedDate { get; set; }

            [JsonProperty("DTE_JobList__c")]
            public string DTE_JobList__c { get; set; }

            [JsonProperty("DTE_JobList__r")]
            public DTE_Joblist__R DTE_JobList__r { get; set; }

            [JsonProperty("DTE_Question__c")]
            public string DTE_Question__c { get; set; }

            [JsonProperty("DTE_Value__c")]
            public string DTE_Value__c { get; set; }

            [JsonProperty("Id")]
            public string Id { get; set; }
        }

        public class DTE_Joblist__R
        {
            [JsonProperty("Account__r")]
            public Account__R Account__r { get; set; }

            [JsonProperty("DTE_MC_Manager_Contact__c")]
            public string DTE_MC_Manager_Contact__c { get; set; }
        }

        public class Account__R
        {
            [JsonProperty("DTE_AccountNumber__c")]
            public string DTE_AccountNumber__c { get; set; }
        }
    }
}