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

    public class UsrContactIntegrationWithSalesforce
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
        private string ResponseString = "";

        public UsrContactIntegrationWithSalesforce()
        {
            appConnection = HttpContext.Current.Application["AppConnection"] as AppConnection;
            userConnection = appConnection.SystemUserConnection;
        }

        public UsrContactIntegrationWithSalesforce(UserConnection userConnection)
        {
            this.userConnection = userConnection;
        }

        public Response GetInformation()
        {
            Response response = new Response();

            response.Success = true;

            try
            {
                url = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceContactUrl", "");

                forceExePath = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceExePath", "");

                logIn = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceLogin", "");

                pass = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforcePassword", "");

                tok = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceToken", "");

                forceExeQuery = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceExeContactQuery", "");

                ver = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceVersion", "");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Error = ex.Message;
                Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrContactIntegrationWithSalesforce", "Error:", ex.Message, "GetInformationJobList Syssettings");
                return response;
            }

            response = SendJsonToWebApi();
            return response;
        }

        public Response SendJsonToWebApi()
        {
            Response response = new Response();
            response.Success = true;
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

                webResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                ResponseString = new StreamReader(webResponse.GetResponseStream()).ReadToEnd();
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    response.Success = false;
                    Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrContactIntegrationWithSalesforce: ", "Error:", ResponseString, "SendJsonToWebApiContact ProtocolError");
                    throw new System.Exception(ex.Message);
                    webResponse = (HttpWebResponse)ex.Response;
                    ResponseString = "Some error occured: " + webResponse.StatusCode.ToString();
                    Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrContactIntegrationWithSalesforce: ", "Error:", ResponseString, "SendJsonToWebApiContact ProtocolError");
                }
                else
                {
                    response.Success = false;
                    Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrContactIntegrationWithSalesforce: ", "Error:", ResponseString, "SendJsonToWebApiContact error ToString()");
                    throw new System.Exception(ex.Message);
                    ResponseString = "Some error occured: " + ex.Status.ToString();
                    Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrContactIntegrationWithSalesforce: ", "Error:", ResponseString, "SendJsonToWebApiContact error ToString()");
                }
            }

            var json = UpdateContactInformation();

            return response;
        }

        public Root ConvertJsonFromSFDC()
        {
            Root list = new Root();

            var json = "{\"Item\":[" + ResponseString + "]}";
            json = json.Replace("}{", "},{");

            try
            {
                JavaScriptSerializer jsJson = new JavaScriptSerializer();
                jsJson.MaxJsonLength = int.MaxValue;
                list = jsJson.Deserialize<Root>(json);
            }
            catch (Exception ex)
            {
                response.Success = false;
                throw new System.Exception(ex.Message);
                Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrContactIntegrationWithSalesforce: ", "Error:", ex.Message, "ConvertJsonFromSFDC");
                return list;
            }

            return list;
        }

        public bool UpdateContactInformation()
        {
            var obj = ConvertJsonFromSFDC();
            foreach (var item in obj.Item)
            {
                foreach (var i in item.AccountContactRelations.records)
                {
                    if (i.Account != null)
                    {
                        var contactId = GetLookupBpmIdByString("Contact", "UsrContactID", item.Id, "Id");
                        var statusId = GetLookupBpmIdByString("UsrStatus", "Name", item.DTE_Status__c, "Id");
                        var accountId = GetLookupBpmIdByString("Account", "UsrIDPOS", i.Account.Id, "Id");

                        var entityContact = userConnection.EntitySchemaManager.GetInstanceByName("Contact").CreateEntity(userConnection);

                        var entityAccount = userConnection.EntitySchemaManager.GetInstanceByName("Account").CreateEntity(userConnection);
                        
                        if(item.Name != null && item.Name != String.Empty)
                        {
	                        var encutf8 = Encoding.GetEncoding("utf-8");
			                var encibm866 = Encoding.GetEncoding("IBM866");
			                byte[] bytes;
			                bytes = encibm866.GetBytes(item.Name);
			                item.Name = encutf8.GetString(bytes);
                        }
    
                        if (entityContact.FetchFromDB(contactId))
                        {
                            if (i.IsDirect == true)
                            {
                                entityContact.SetColumnValue("UsrPOSCode", i.Account.DTE_AccountNumber__c);
                            }
                            if (i.IsDirect == false)
                            {
                                var relatedPOS = GetUsrRelatedPOS("Contact", "Id", contactId, "UsrRelatedPOS");
                                relatedPOS = relatedPOS.Replace(i.Account.DTE_AccountNumber__c + "; ", "");
                                entityContact.SetColumnValue("UsrRelatedPOS", relatedPOS + i.Account.DTE_AccountNumber__c + "; ");
                            }
                            entityContact.SetColumnValue("UsrAddressPOS", i.Account.BillingStreet);
                            entityContact.SetColumnValue("Name", item.Id);
                            entityContact.SetColumnValue("Email", item.Email);
                            entityContact.SetColumnValue("MobilePhone", item.MobilePhone);
							entityContact.SetColumnValue("SFDCPhone", item.SFDCPhone);
                            entityContact.SetColumnValue("UsrStringTransactionPoints", item.DTE_TransactionPoints__c);
                            entityContact.SetColumnValue("UsrStatusId", statusId != Guid.Empty ? statusId : (Guid?)null);
                            entityContact.SetColumnValue("UsrRole", item.DTE_Role__c);
                            entityContact.SetColumnValue("UsrDateOfBirth", item.DTE_DateOfBirth__c);
                            entityContact.SetColumnValue("UsrLastLogin", item.DTE_Last_Login_Time__c);
                            entityContact.SetColumnValue("UsrRSPFullName", item.Name);
                            entityContact.SetColumnValue("AccountId", accountId != Guid.Empty ? accountId : (Guid?)null);
                        }
                        else
                        {
                            entityContact.SetDefColumnValues();

                            if (i.IsDirect == true)
                            {
                                entityContact.SetColumnValue("UsrPOSCode", i.Account.DTE_AccountNumber__c);
                            }
                            if (i.IsDirect == false)
                            {
                                var relatedPOS = GetUsrRelatedPOS("Contact", "Id", contactId, "UsrRelatedPOS");
                                relatedPOS = relatedPOS.Replace(i.Account.DTE_AccountNumber__c + "; ", "");
                                entityContact.SetColumnValue("UsrRelatedPOS", relatedPOS + i.Account.DTE_AccountNumber__c + "; ");
                            }
                            entityContact.SetColumnValue("UsrAddressPOS", i.Account.BillingStreet);
                            entityContact.SetColumnValue("Name", item.Id);
                            entityContact.SetColumnValue("Email", item.Email);
                            entityContact.SetColumnValue("MobilePhone", item.MobilePhone);
							entityContact.SetColumnValue("SFDCPhone", item.SFDCPhone);
                            entityContact.SetColumnValue("UsrStringTransactionPoints", item.DTE_TransactionPoints__c);
                            entityContact.SetColumnValue("UsrStatusId", statusId != Guid.Empty ? statusId : (Guid?)null);
                            entityContact.SetColumnValue("UsrRole", item.DTE_Role__c);
                            entityContact.SetColumnValue("UsrDateOfBirth", item.DTE_DateOfBirth__c);
                            entityContact.SetColumnValue("UsrLastLogin", item.DTE_Last_Login_Time__c);
                            entityContact.SetColumnValue("UsrRSPFullName", item.Name);
                            entityContact.SetColumnValue("AccountId", accountId != Guid.Empty ? accountId : (Guid?)null);
                            entityContact.SetColumnValue("UsrContactID", item.Id);
                        }

                        try
                        {
                            entityContact.Save(false);
                        }
                        catch (Exception ex)
                        {
                            response.Success = false;
                            throw new System.Exception(ex.Message);
                            Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrContactIntegrationWithSalesforce: ", "Error:", ex.Message, "UpdateContactInformation entityContact save");
                        }

                        if (entityAccount.FetchFromDB(accountId))
                        {
                            if (i.IsDirect == true)
                            {
                                entityAccount.SetColumnValue("Code", i.Account.DTE_AccountNumber__c);
                            }
                            entityAccount.SetColumnValue("UsrSFDCPOSStatus", i.Account.DTE_Status__c);
                        }

                        else
                        {
                            entityAccount.SetDefColumnValues();

                            if (i.IsDirect == true)
                            {
                                entityAccount.SetColumnValue("Code", i.Account.DTE_AccountNumber__c);
                            }
                            entityAccount.SetColumnValue("UsrSFDCPOSStatus", i.Account.DTE_Status__c);
                            entityAccount.SetColumnValue("UsrIDPOS", i.Account.Id);
                        }

                        try
                        {
                            entityAccount.Save(false);
                        }
                        catch (Exception ex)
                        {
                            response.Success = false;
                            throw new System.Exception(ex.Message);
                            Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrContactIntegrationWithSalesforce: ", "Error:", ex.Message, "UpdateContactInformation entityAccount save");
                        }
                    }

                    if (i.Account == null)
                    {
                        var contactId = GetLookupBpmIdByString("Contact", "UsrContactID", item.Id, "Id");
                        var statusId = GetLookupBpmIdByString("UsrStatus", "Name", item.DTE_Status__c, "Id");

                        var entityContact = userConnection.EntitySchemaManager.GetInstanceByName("Contact").CreateEntity(userConnection);
                        
                        if(item.Name != null && item.Name != String.Empty)
                        {
	                        var encutf8 = Encoding.GetEncoding("utf-8");
			                var encibm866 = Encoding.GetEncoding("IBM866");
			                byte[] bytes;
			                bytes = encibm866.GetBytes(item.Name);
			                item.Name = encutf8.GetString(bytes);
                        }

                        if (entityContact.FetchFromDB(contactId))
                        {
                            entityContact.SetColumnValue("Name", item.Id);
                            entityContact.SetColumnValue("Email", item.Email);
                            entityContact.SetColumnValue("MobilePhone", item.MobilePhone);
							entityContact.SetColumnValue("SFDCPhone", item.SFDCPhone);
                            entityContact.SetColumnValue("UsrStringTransactionPoints", item.DTE_TransactionPoints__c);
                            entityContact.SetColumnValue("UsrStatusId", statusId != Guid.Empty ? statusId : (Guid?)null);
                            entityContact.SetColumnValue("UsrRole", item.DTE_Role__c);
                            entityContact.SetColumnValue("UsrDateOfBirth", item.DTE_DateOfBirth__c);
                            entityContact.SetColumnValue("UsrLastLogin", item.DTE_Last_Login_Time__c);
                            entityContact.SetColumnValue("UsrRSPFullName", item.Name);
                        }
                        else
                        {
                            entityContact.SetDefColumnValues();

                            entityContact.SetColumnValue("Name", item.Id);
                            entityContact.SetColumnValue("Email", item.Email);
                            entityContact.SetColumnValue("MobilePhone", item.MobilePhone);
							entityContact.SetColumnValue("SFDCPhone", item.SFDCPhone);
                            entityContact.SetColumnValue("UsrStringTransactionPoints", item.DTE_TransactionPoints__c);
                            entityContact.SetColumnValue("UsrStatusId", statusId != Guid.Empty ? statusId : (Guid?)null);
                            entityContact.SetColumnValue("UsrRole", item.DTE_Role__c);
                            entityContact.SetColumnValue("UsrDateOfBirth", item.DTE_DateOfBirth__c);
                            entityContact.SetColumnValue("UsrLastLogin", item.DTE_Last_Login_Time__c);
                            entityContact.SetColumnValue("UsrRSPFullName", item.Name);
                            entityContact.SetColumnValue("UsrContactID", item.Id);
                        }

                        try
                        {
                            entityContact.Save(false);
                        }
                        catch (Exception ex)
                        {
                            response.Success = false;
                            throw new System.Exception(ex.Message);
                            Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrContactIntegrationWithSalesforce: ", "Error:", ex.Message, "UpdateContactInformation entityContact save");
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

        public string GetUsrRelatedPOS(string table, string column, Guid value, string columnReturn)
        {
            if (value == Guid.Empty || value == null)
            {
                return string.Empty;
            }

            var UsrRelatedPOS = (new Select(userConnection).Top(1)
                .Column(columnReturn)
                .From(table)
                .Where(column).IsEqual(Column.Parameter(value)) as Select).ExecuteScalar<String>();
            return UsrRelatedPOS;
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

        public class Root
        {
            [JsonProperty("Item")]
            public List<Items> Item { get; set; }
        }

        public class Items
        {
            [JsonProperty("AccountContactRelations")]
            public AccountContactRelations AccountContactRelations { get; set; }

            [JsonProperty("DTE_DateOfBirth__c")]
            public DateTime? DTE_DateOfBirth__c { get; set; }

            [JsonProperty("DTE_Last_Login_Time__c")]
            public DateTime? DTE_Last_Login_Time__c { get; set; }

            [JsonProperty("DTE_Role__c")]
            public string DTE_Role__c { get; set; }

            [JsonProperty("DTE_Status__c")]
            public string DTE_Status__c { get; set; }

            [JsonProperty("DTE_TransactionPoints__c")]
            public string DTE_TransactionPoints__c { get; set; }

            [JsonProperty("Email")]
            public string Email { get; set; }

            [JsonProperty("Id")]
            public string Id { get; set; }

            [JsonProperty("MobilePhone")]
            public string MobilePhone { get; set; }
			
			[JsonProperty("Phone")]
            public string SFDCPhone { get; set; }

            [JsonProperty("Name")]
            public string Name { get; set; }
			
			
        }

        public class AccountContactRelations
        {
            [JsonProperty("records")]
            public Record[] records { get; set; }
        }

        public class Record
        {
            [JsonProperty("Account")]
            public Account Account { get; set; }

            [JsonProperty("IsDirect")]
            public bool IsDirect { get; set; }
        }

        public class Account
        {
            [JsonProperty("BillingStreet")]
            public string BillingStreet { get; set; }

            [JsonProperty("DTE_AccountNumber__c")]
            public string DTE_AccountNumber__c { get; set; }

            [JsonProperty("DTE_Status__c")]
            public string DTE_Status__c { get; set; }

            [JsonProperty("Id")]
            public string Id { get; set; }
        }
    }
}