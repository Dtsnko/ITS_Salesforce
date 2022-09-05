namespace Terrasoft.Configuration.UsrUpdateAccountOrContactJobList
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

    public class UsrUpdateAccountOrContactJobList
    {
        private AppConnection appConnection;
        private UserConnection userConnection;
        private Response response;

        private string url;
        private string forceExePath;
        private string logIn;
        private string pass;
        private string tok;
        private StringBuilder forceExeQuery = new StringBuilder();
        private string forceExeQueryJobList;
        private string ver;
        private string ExMessage = "";
        private string ResponseString = "";

        public UsrUpdateAccountOrContactJobList()
        {
            appConnection = HttpContext.Current.Application["AppConnection"] as AppConnection;
            userConnection = appConnection.SystemUserConnection;
        }

        public UsrUpdateAccountOrContactJobList(UserConnection userConnection)
        {
            this.userConnection = userConnection;
        }

        public Response GetInformationJobList(string contactSfdcId, string accountSfdcId, string taskName, string createdDateFrom, string createdDateTill, bool assigned, bool completedApproved, bool completedRejected, bool completedByOther, bool submitted)
        {
            Response response = new Response();

            response.Success = true;

            try
            {
                url = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceJobListUrl", "");

                forceExePath = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceExePath", "");

                logIn = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceLogin", "");

                pass = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforcePassword", "");

                tok = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceToken", "");

                forceExeQuery.Append("SELECT Job_List__c.CreatedDate, Job_List__c.LastModifiedDate, Job_List__c.Id, Job_List__c.Name,  Job_List__c.DTE_State__c,  Job_List__c.DTE_Submission_Date__c,  Job_List__c.Valid_From__c,  Job_List__c.Valid_Thru__c, Job_Definition_List__r.Name, Job_Definition_List__r.Job_Definition_List_Template__c, Account__r.Id,  DTE_MC_Manager_Contact__r.Id, (SELECT Attachment.Id FROM attachments) FROM Job_List__c ");

                if (contactSfdcId != null)
                {
                    forceExeQuery.Append("WHERE DTE_MC_Manager_Contact__r.Id = " + "\'" + contactSfdcId + "\' AND ");
                }
                if (accountSfdcId != null)
                {
                    forceExeQuery.Append("WHERE Account__r.Id = " + "\'" + accountSfdcId + "\' AND ");
                }
                if (taskName != null)
                {
                    forceExeQuery.Append("Job_Definition_List__r.Name LIKE " + "\'%" + taskName + "%\' AND ");
                }
                if (createdDateFrom != null)
                {
                    forceExeQuery.Append("Job_List__c.CreatedDate >= " + createdDateFrom + " AND ");
                }
                if (createdDateTill != null)
                {
                    forceExeQuery.Append("Job_List__c.CreatedDate <= " + createdDateTill + " AND ");
                }

                forceExeQuery.Append("(");

                if (assigned == true)
                {
                    forceExeQuery.Append("Job_List__c.DTE_State__c = \'Assigned\' OR ");
                }
                if (completedApproved == true)
                {
                    forceExeQuery.Append("Job_List__c.DTE_State__c = \'Completed - Approved\' OR ");
                }
                if (completedRejected == true)
                {
                    forceExeQuery.Append("Job_List__c.DTE_State__c = \'Completed - Rejected\' OR ");
                }
                if (completedByOther == true)
                {
                    forceExeQuery.Append("Job_List__c.DTE_State__c = \'Completed by other\' OR ");
                }
                if (submitted == true)
                {
                    forceExeQuery.Append("Job_List__c.DTE_State__c = \'Submitted\' OR ");
                }

                forceExeQuery.Append(")").Replace("OR )", ")").Replace("AND ()", "").Append(" ORDER BY Job_List__c.LastModifiedDate ASC");
                forceExeQueryJobList = forceExeQuery.ToString();

                ver = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceVersion", "");
            }
            catch (Exception ex)
            {
                response.Success = false;
                Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", "Error:", ex.Message, "GetInformationJobList Syssettings");
                response.Error = ex.Message;
                return response;
            }

            response = SendJsonToWebApiJobList();
            return response;
        }

        public Response SendJsonToWebApiJobList()
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
                    select = forceExeQueryJobList,
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
                    Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", "Error:", ResponseString, "SendJsonToWebApiJobList ProtocolError");
                    throw new System.Exception(ex.Message);
                    webResponse = (HttpWebResponse)ex.Response;
                    ResponseString = "Some error occured: " + webResponse.StatusCode.ToString();
                    Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", "Error:", ResponseString, "SendJsonToWebApiJobList ProtocolError");
                }
                else
                {
                    response.Success = false;
                    Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", "Error:", ResponseString, "SendJsonToWebApiJobList error");
                    throw new System.Exception(ex.Message);
                    ResponseString = "Some error occured: " + ex.Status.ToString();
                    Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", "Error:", ResponseString, "SendJsonToWebApiJobList error");
                }
            }

            var json = UpdateJobListInformation();

            return response;
        }

        public JobList ConvertJsonFromSFDCJobList()
        {
            JobList list = new JobList();

            var json = "{\"JobListItem\":[" + ResponseString + "]}";
            json = json.Replace("}{", "},{");

            try
            {
                JavaScriptSerializer jsJson = new JavaScriptSerializer();
                jsJson.MaxJsonLength = int.MaxValue;
                list = jsJson.Deserialize<JobList>(json);
            }
            catch (Exception ex)
            {
                Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", "Error:", json, "ConvertJsonFromSFDCJobList parse error");
                throw new System.Exception(ex.Message);
                return list;
            }

            return list;
        }

        public bool UpdateJobListInformation()
        {
            Response response = new Response();

            response.Success = true;

            var obj = ConvertJsonFromSFDCJobList();

            foreach (var item in obj.JobListItem)
            {
                if (item.Attachments != null)
                {
                    foreach (var i in item.Attachments.records)
                    {
                        var activityId = GetLookupBpmIdByString("Activity", "Title", item.Name, "Id");
                        var stateId = GetLookupBpmIdByString("UsrSfdcState", "Name", item.DTE_State__c, "Id");
                        var ActivityCategoryId = GetLookupBpmIdByString("ActivityCategory", "Name", "Job List", "Id");
                        var accountId = GetLookupBpmIdByString("Account", "UsrIDPOS", item.Account__r?.Id, "Id");
                        var contactId = GetLookupBpmIdByString("Contact", "UsrContactID", item.DTE_MC_Manager_Contact__r?.Id, "Id");
                        var photosId = GetLookupBpmIdByString("UsrPhotos", "UsrSfdcId", i.Id, "Id");
                        var JobDefinitionListId = GetLookupBpmIdByString("Activity", "Title", item.Job_Definition_List__r.Name, "Id");

                        var entityActivity = userConnection.EntitySchemaManager.GetInstanceByName("Activity").CreateEntity(userConnection);

                        if ((stateId == Guid.Empty || stateId == null) && item.DTE_State__c != null)
                        {
                            stateId = Guid.NewGuid();

                            var entityUsrSfdcStateId = userConnection.EntitySchemaManager.GetInstanceByName("UsrSfdcState").CreateEntity(userConnection);

                            entityUsrSfdcStateId.SetDefColumnValues();

                            entityUsrSfdcStateId.SetColumnValue("Name", item.DTE_State__c);
                            entityUsrSfdcStateId.SetColumnValue("Id", stateId);

                            try
                            {
                                entityUsrSfdcStateId.Save(false);
                            }
                            catch (Exception ex)
                            {
                                response.Success = false;
                                Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", "Error:", ex.Message, "UpdateJobListInformation entityUsrSfdcStateId save");
                                throw new System.Exception(ex.Message);
                            }
                        }

                        if (ActivityCategoryId == Guid.Empty || ActivityCategoryId == null)
                        {
                            ActivityCategoryId = Guid.NewGuid();

                            var entityActivityCategoryId = userConnection.EntitySchemaManager.GetInstanceByName("ActivityCategory").CreateEntity(userConnection);

                            entityActivityCategoryId.SetDefColumnValues();

                            entityActivityCategoryId.SetColumnValue("Name", "Job List");
                            entityActivityCategoryId.SetColumnValue("Id", ActivityCategoryId);

                            try
                            {
                                entityActivityCategoryId.Save(false);
                            }
                            catch (Exception ex)
                            {
                                response.Success = false;
                                Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", "Error:", ex.Message, "UpdateJobListInformation entityActivityCategoryId save");
                                throw new System.Exception(ex.Message);
                            }
                        }

                        if ((accountId == Guid.Empty || accountId == null) && item.Account__r != null)
                        {
                            accountId = Guid.NewGuid();

                            var entityAccountId = userConnection.EntitySchemaManager.GetInstanceByName("Account").CreateEntity(userConnection);

                            entityAccountId.SetDefColumnValues();

                            entityAccountId.SetColumnValue("UsrIDPOS", item.Account__r.Id);
                            entityAccountId.SetColumnValue("Id", accountId);

                            try
                            {
                                entityAccountId.Save(false);
                            }
                            catch (Exception ex)
                            {
                                response.Success = false;
                                Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", "Error:", ex.Message, "UpdateJobListInformation entityAccountId save");
                                throw new System.Exception(ex.Message);
                            }
                        }

                        if ((contactId == Guid.Empty || contactId == null) && item.DTE_MC_Manager_Contact__r != null)
                        {
                            contactId = Guid.NewGuid();

                            var entityContactId = userConnection.EntitySchemaManager.GetInstanceByName("Contact").CreateEntity(userConnection);

                            entityContactId.SetDefColumnValues();

                            entityContactId.SetColumnValue("UsrContactID", item.DTE_MC_Manager_Contact__r.Id);
                            entityContactId.SetColumnValue("Id", contactId);

                            try
                            {
                                entityContactId.Save(false);
                            }
                            catch (Exception ex)
                            {
                                response.Success = false;
                                Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", "Error:", ex.Message, "UpdateJobListInformation entityContactId save");
                                throw new System.Exception(ex.Message);
                            }
                        }

                        if (entityActivity.FetchFromDB(activityId))
                        {
                            entityActivity.SetColumnValue("UsrSfdcId", item.Id);
                            entityActivity.SetColumnValue("UsrJobListName", item.Name);
                            entityActivity.SetColumnValue("UsrSfdcStateId", item.DTE_State__c);
                            entityActivity.SetColumnValue("ActivityCategoryId", ActivityCategoryId != Guid.Empty ? ActivityCategoryId : (Guid?)null);
                            entityActivity.SetColumnValue("UsrSubmissionDate", item.DTE_Submission_Date__c);
                            entityActivity.SetColumnValue("StartDate", item.Valid_From__c);
                            entityActivity.SetColumnValue("DueDate", item.Valid_Thru__c);
                            entityActivity.SetColumnValue("AccountId", accountId != Guid.Empty ? accountId : (Guid?)null);
                            entityActivity.SetColumnValue("ContactId", contactId != Guid.Empty ? contactId : (Guid?)null);
                            entityActivity.SetColumnValue("UsrJobDefinitionListName", item.Job_Definition_List__r.Name);
                            entityActivity.SetColumnValue("UsrSfdcStateId", stateId != Guid.Empty ? stateId : (Guid?)null);
                            entityActivity.SetColumnValue("UsrJobDefinitionListId", JobDefinitionListId != Guid.Empty ? JobDefinitionListId : (Guid?)null);
                            entityActivity.SetColumnValue("UsrJobDefinitionListTemplate", item.Job_Definition_List__r.Job_Definition_List_Template__c);
                        }
                        else
                        {
                            activityId = Guid.NewGuid();

                            entityActivity.SetDefColumnValues();

                            entityActivity.SetColumnValue("Id", activityId);
                            entityActivity.SetColumnValue("UsrSfdcId", item.Id);
                            entityActivity.SetColumnValue("Title", item.Name);
                            entityActivity.SetColumnValue("UsrJobListName", item.Name);
                            entityActivity.SetColumnValue("ActivityCategoryId", ActivityCategoryId != Guid.Empty ? ActivityCategoryId : (Guid?)null);
                            entityActivity.SetColumnValue("UsrSubmissionDate", item.DTE_Submission_Date__c);
                            entityActivity.SetColumnValue("StartDate", item.Valid_From__c);
                            entityActivity.SetColumnValue("DueDate", item.Valid_Thru__c);
                            entityActivity.SetColumnValue("AccountId", accountId != Guid.Empty ? accountId : (Guid?)null);
                            entityActivity.SetColumnValue("ContactId", contactId != Guid.Empty ? contactId : (Guid?)null);
                            entityActivity.SetColumnValue("UsrJobDefinitionListName", item.Job_Definition_List__r.Name);
                            entityActivity.SetColumnValue("UsrSfdcStateId", stateId != Guid.Empty ? stateId : (Guid?)null);
                            entityActivity.SetColumnValue("UsrJobDefinitionListId", JobDefinitionListId != Guid.Empty ? JobDefinitionListId : (Guid?)null);
                            entityActivity.SetColumnValue("UsrJobDefinitionListTemplate", item.Job_Definition_List__r.Job_Definition_List_Template__c);
                        }

                        try
                        {
                            entityActivity.Save(false);
                        }
                        catch (Exception ex)
                        {
                            response.Success = false;
                            Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", "Error:", ex.Message, "UpdateJobListInformation entityActivity save");
                            throw new System.Exception(ex.Message);
                        }

                        var entityUsrPhotos = userConnection.EntitySchemaManager.GetInstanceByName("UsrPhotos").CreateEntity(userConnection);

                        if (!entityUsrPhotos.FetchFromDB(photosId))
                        {
                            entityUsrPhotos.SetDefColumnValues();

                            entityUsrPhotos.SetColumnValue("UsrSfdcId", i.Id);
                            entityUsrPhotos.SetColumnValue("UsrActivityId", activityId);
                        }

                        try
                        {
                            entityUsrPhotos.Save(false);
                        }
                        catch (Exception ex)
                        {
                            response.Success = false;
                            Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", "Error:", ex.Message, "UpdateJobListInformation entityUsrPhotos save");
                            throw new System.Exception(ex.Message);
                        }
                    }
                }
                try
                {
                    if (item.Attachments == null)
                    {
                        var activityId = GetLookupBpmIdByString("Activity", "Title", item.Name, "Id");
                        var stateId = GetLookupBpmIdByString("UsrSfdcState", "Name", item.DTE_State__c, "Id");
                        var ActivityCategoryId = GetLookupBpmIdByString("ActivityCategory", "Name", "Job List", "Id");
                        var accountId = GetLookupBpmIdByString("Account", "UsrIDPOS", item.Account__r?.Id, "Id");
                        var contactId = GetLookupBpmIdByString("Contact", "UsrContactID", item.DTE_MC_Manager_Contact__r?.Id, "Id");
                        var JobDefinitionListId = GetLookupBpmIdByString("Activity", "Title", item.Job_Definition_List__r.Name, "Id");

                        var entityActivity = userConnection.EntitySchemaManager.GetInstanceByName("Activity").CreateEntity(userConnection);

                        if ((stateId == Guid.Empty || stateId == null) && item.DTE_State__c != null)
                        {
                            stateId = Guid.NewGuid();

                            var entityUsrSfdcStateId = userConnection.EntitySchemaManager.GetInstanceByName("UsrSfdcState").CreateEntity(userConnection);

                            entityUsrSfdcStateId.SetDefColumnValues();

                            entityUsrSfdcStateId.SetColumnValue("Name", item.DTE_State__c);
                            entityUsrSfdcStateId.SetColumnValue("Id", stateId);

                            try
                            {
                                entityUsrSfdcStateId.Save(false);
                            }
                            catch (Exception ex)
                            {
                                response.Success = false;
                                Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", "Error:", ex.Message, "UpdateJobListInformation entityUsrSfdcStateId save");
                                throw new System.Exception(ex.Message);
                            }
                        }

                        if (ActivityCategoryId == Guid.Empty || ActivityCategoryId == null)
                        {
                            ActivityCategoryId = Guid.NewGuid();

                            var entityActivityCategoryId = userConnection.EntitySchemaManager.GetInstanceByName("ActivityCategory").CreateEntity(userConnection);

                            entityActivityCategoryId.SetDefColumnValues();

                            entityActivityCategoryId.SetColumnValue("Name", "Job List");
                            entityActivityCategoryId.SetColumnValue("Id", ActivityCategoryId);

                            try
                            {
                                entityActivityCategoryId.Save(false);
                            }
                            catch (Exception ex)
                            {
                                response.Success = false;
                                Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", "Error:", ex.Message, "UpdateJobListInformation entityActivityCategoryId save");
                                throw new System.Exception(ex.Message);
                            }
                        }

                        if ((accountId == Guid.Empty || accountId == null) && item.Account__r != null)
                        {
                            accountId = Guid.NewGuid();

                            var entityAccountId = userConnection.EntitySchemaManager.GetInstanceByName("Account").CreateEntity(userConnection);

                            entityAccountId.SetDefColumnValues();

                            entityAccountId.SetColumnValue("UsrIDPOS", item.Account__r.Id);
                            entityAccountId.SetColumnValue("Id", accountId);

                            try
                            {
                                entityAccountId.Save(false);
                            }
                            catch (Exception ex)
                            {
                                response.Success = false;
                                Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", "Error:", ex.Message, "UpdateJobListInformation entityAccountId save");
                                throw new System.Exception(ex.Message);
                            }
                        }

                        if ((contactId == Guid.Empty || contactId == null) && item.DTE_MC_Manager_Contact__r != null)
                        {
                            contactId = Guid.NewGuid();

                            var entityContactId = userConnection.EntitySchemaManager.GetInstanceByName("Contact").CreateEntity(userConnection);

                            entityContactId.SetDefColumnValues();

                            entityContactId.SetColumnValue("UsrContactID", item.DTE_MC_Manager_Contact__r.Id);
                            entityContactId.SetColumnValue("Id", contactId);

                            try
                            {
                                entityContactId.Save(false);
                            }
                            catch (Exception ex)
                            {
                                response.Success = false;
                                Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", "Error:", ex.Message, "UpdateJobListInformation entityContactId save");
                                throw new System.Exception(ex.Message);
                            }
                        }

                        if (entityActivity.FetchFromDB(activityId))
                        {
                            entityActivity.SetColumnValue("UsrSfdcId", item.Id);
                            entityActivity.SetColumnValue("UsrJobListName", item.Name);
                            entityActivity.SetColumnValue("ActivityCategoryId", ActivityCategoryId != Guid.Empty ? ActivityCategoryId : (Guid?)null);
                            entityActivity.SetColumnValue("UsrSubmissionDate", item.DTE_Submission_Date__c);
                            entityActivity.SetColumnValue("StartDate", item.Valid_From__c);
                            entityActivity.SetColumnValue("DueDate", item.Valid_Thru__c);
                            entityActivity.SetColumnValue("AccountId", accountId != Guid.Empty ? accountId : (Guid?)null);
                            entityActivity.SetColumnValue("ContactId", contactId != Guid.Empty ? contactId : (Guid?)null);
                            entityActivity.SetColumnValue("UsrJobDefinitionListName", item.Job_Definition_List__r.Name);
                            entityActivity.SetColumnValue("UsrSfdcStateId", stateId != Guid.Empty ? stateId : (Guid?)null);
                            entityActivity.SetColumnValue("UsrJobDefinitionListId", JobDefinitionListId != Guid.Empty ? JobDefinitionListId : (Guid?)null);
                            entityActivity.SetColumnValue("UsrJobDefinitionListTemplate", item.Job_Definition_List__r.Job_Definition_List_Template__c);
                        }
                        else
                        {
                            activityId = Guid.NewGuid();

                            entityActivity.SetDefColumnValues();

                            entityActivity.SetColumnValue("Id", activityId);
                            entityActivity.SetColumnValue("UsrSfdcId", item.Id);
                            entityActivity.SetColumnValue("Title", item.Name);
                            entityActivity.SetColumnValue("UsrJobListName", item.Name);
                            entityActivity.SetColumnValue("ActivityCategoryId", ActivityCategoryId != Guid.Empty ? ActivityCategoryId : (Guid?)null);
                            entityActivity.SetColumnValue("UsrSubmissionDate", item.DTE_Submission_Date__c);
                            entityActivity.SetColumnValue("StartDate", item.Valid_From__c);
                            entityActivity.SetColumnValue("DueDate", item.Valid_Thru__c);
                            entityActivity.SetColumnValue("AccountId", accountId != Guid.Empty ? accountId : (Guid?)null);
                            entityActivity.SetColumnValue("ContactId", contactId != Guid.Empty ? contactId : (Guid?)null);
                            entityActivity.SetColumnValue("UsrJobDefinitionListName", item.Job_Definition_List__r.Name);
                            entityActivity.SetColumnValue("UsrSfdcStateId", stateId != Guid.Empty ? stateId : (Guid?)null);
                            entityActivity.SetColumnValue("UsrJobDefinitionListId", JobDefinitionListId != Guid.Empty ? JobDefinitionListId : (Guid?)null);
                            entityActivity.SetColumnValue("UsrJobDefinitionListTemplate", item.Job_Definition_List__r.Job_Definition_List_Template__c);
                        }

                        try
                        {
                            entityActivity.Save(false);
                        }
                        catch (Exception ex)
                        {
                            response.Success = false;
                            Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", "Error:", ex.Message, "UpdateJobListInformation entityActivity save");
                            throw new System.Exception(ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", ex.Message, ResponseString, "item.Attachments == null");
                    throw new System.Exception(ex.Message);
                }
            }

            return true;
        }

        public bool AcceptAllCertificatePolicy(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
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

        public class JobList
        {
            [JsonProperty("JobListItem")]
            public List<JobListItems> JobListItem { get; set; }
        }

        public class JobListItems
        {
            [JsonProperty("Account__r")]
            public Account__R Account__r { get; set; }

            [JsonProperty("Attachments")]
            public Attachments Attachments { get; set; }

            [JsonProperty("DTE_MC_Manager_Contact__r")]
            public DTE_MC_Manager_Contact__R DTE_MC_Manager_Contact__r { get; set; }

            [JsonProperty("DTE_State__c")]
            public string DTE_State__c { get; set; }

            [JsonProperty("DTE_Submission_Date__c")]
            public DateTime? DTE_Submission_Date__c { get; set; }

            [JsonProperty("Id")]
            public string Id { get; set; }

            [JsonProperty("Job_Definition_List__r")]
            public Job_Definition_List__R Job_Definition_List__r { get; set; }

            [JsonProperty("LastModifiedDate")]
            public DateTime? LastModifiedDate { get; set; }

            [JsonProperty("Name")]
            public string Name { get; set; }

            [JsonProperty("Valid_From__c")]
            public DateTime? Valid_From__c { get; set; }

            [JsonProperty("Valid_Thru__c")]
            public DateTime? Valid_Thru__c { get; set; }
        }

        public class Account__R
        {
            [JsonProperty("Id")]
            public string Id { get; set; }
        }

        public class Attachments
        {
            [JsonProperty("records")]
            public Record[] records { get; set; }
        }

        public class Record
        {
            [JsonProperty("Id")]
            public string Id { get; set; }
        }

        public class DTE_MC_Manager_Contact__R
        {
            [JsonProperty("Id")]
            public string Id { get; set; }
        }

        public class Job_Definition_List__R
        {
            [JsonProperty("Name")]
            public string Name { get; set; }

            [JsonProperty("Job_Definition_List_Template__c")]
            public string Job_Definition_List_Template__c { get; set; }
        }
    }
}