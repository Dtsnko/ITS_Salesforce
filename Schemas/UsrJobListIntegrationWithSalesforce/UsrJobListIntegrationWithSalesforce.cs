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

    public class UsrJobListIntegrationWithSalesforce
    {
        private AppConnection appConnection;
        private UserConnection userConnection;
        private Response response;

        private string url;
        private string forceExePath;
        private string logIn;
        private string pass;
        private string tok;
        private string forceExeQueryJobList;
        private string forceExeQueryResponseQuestions;
        private string ver;
        private string ExMessage = "";
        private string ResponseString = "";
        public static string date;

        public UsrJobListIntegrationWithSalesforce()
        {
            appConnection = HttpContext.Current.Application["AppConnection"] as AppConnection;
            userConnection = appConnection.SystemUserConnection;
        }

        public UsrJobListIntegrationWithSalesforce(UserConnection userConnection)
        {
            this.userConnection = userConnection;
        }

        public Response GetInformationJobList()
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

                forceExeQueryJobList = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceExeJobListQuery", "");

                var UsrLastModifiedDateSFDCJobLists = GetUsrLastModifiedDateSFDC("Activity", "UsrLastModifiedDateSFDC");
                var dateJoblists = Convert.ToDateTime(UsrLastModifiedDateSFDCJobLists).ToUniversalTime().ToString("O");
                forceExeQueryJobList = forceExeQueryJobList.Replace("UsrLastModifiedDateSFDC", dateJoblists);

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
                httpWebRequest.Timeout = 100000000;

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
                            entityActivity.SetColumnValue("UsrLastModifiedDateSFDC", item.LastModifiedDate);
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
                            entityActivity.SetColumnValue("UsrLastModifiedDateSFDC", item.LastModifiedDate);
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
                            entityActivity.SetColumnValue("UsrLastModifiedDateSFDC", item.LastModifiedDate);
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
                            entityActivity.SetColumnValue("UsrLastModifiedDateSFDC", item.LastModifiedDate);
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

        public Response GetInformationResponseQuestion()
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

                forceExeQueryResponseQuestions = Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceExeResponseQuestionQuery", "");
                
                var LastModifiedDateSFDCCreatioResponseQuestions = GetUsrLastModifiedDateSFDC("UsrResponseQuestion", "UsrLastModifiedDateSFDC");
                var dateCreatioResponseQuestions = Convert.ToDateTime(LastModifiedDateSFDCCreatioResponseQuestions).ToUniversalTime().ToString("O");
                var LastModifiedDateSFDCCreatioJobLists = GetUsrLastModifiedDateSFDC("Activity", "UsrLastModifiedDateSFDC");
                var dateCreatioJobLists = Convert.ToDateTime(LastModifiedDateSFDCCreatioJobLists).ToUniversalTime().ToString("O");
                forceExeQueryResponseQuestions = forceExeQueryResponseQuestions.Replace("UsrLastModifiedDateSFDC", dateCreatioResponseQuestions).Replace("LastModifiedDateSFDCCreatio", dateCreatioJobLists);

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
				httpWebRequest.Timeout = 100000000;

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

                try
                {
                    if (item.DTE_Question__c != null && item.DTE_Question__c != String.Empty)
                    {
                        var encutf8 = Encoding.GetEncoding("utf-8");
                        var encibm866 = Encoding.GetEncoding("IBM866");
                        byte[] bytes;
                        bytes = encibm866.GetBytes(item.DTE_Question__c);
                        item.DTE_Question__c = encutf8.GetString(bytes);
                    }
                }
                catch (Exception ex)
                {
                    response.Success = false;
                    Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", "Error:", ex.Message, "UpdateResponseQuestionInformation encoding DTE_Question__c");
                    throw new System.Exception(ex.Message);
                }

                try
                {
                    if (item.DTE_Value__c != null && item.DTE_Value__c != String.Empty)
                    {
                        var encutf8 = Encoding.GetEncoding("utf-8");
                        var encibm866 = Encoding.GetEncoding("IBM866");
                        byte[] bytes;
                        bytes = encibm866.GetBytes(item.DTE_Value__c);
                        item.DTE_Value__c = encutf8.GetString(bytes);
                    }
                }
                catch (Exception ex)
                {
                    response.Success = false;
                    Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrJobListIntegrationWithSalesforce: ", "Error:", ex.Message, "UpdateResponseQuestionInformation encoding DTE_Value__c");
                    throw new System.Exception(ex.Message);
                }

                if ((activityId != Guid.Empty && activityId != null) && item.DTE_JobList__r != null)
                {
                    var entityResponseQuestion = userConnection.EntitySchemaManager.GetInstanceByName("UsrResponseQuestion").CreateEntity(userConnection);
                    
                    if (entityResponseQuestion.FetchFromDB(responseQuestionId))
                    {
                        entityResponseQuestion.SetColumnValue("UsrQuestion", !String.IsNullOrEmpty(item.DTE_Question__c) ? item.DTE_Question__c : String.Empty);
                        entityResponseQuestion.SetColumnValue("UsrAnsweredCorrectly", item.DTE_AnsweredCorrectly__c);
                        entityResponseQuestion.SetColumnValue("UsrValue", !String.IsNullOrEmpty(item.DTE_Value__c) ? item.DTE_Value__c : String.Empty);
                        entityResponseQuestion.SetColumnValue("UsrLastModifiedDateSFDC", item.LastModifiedDate);
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
                        entityResponseQuestion.SetColumnValue("UsrLastModifiedDateSFDC", item.LastModifiedDate);
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

        public string GetUsrLastModifiedDateSFDC(string table, string columnReturn)
        {
            if (columnReturn == String.Empty || columnReturn == null)
            {
                return string.Empty;
            }

            var LastModifiedDateSFDC = (new Select(userConnection).Top(1)
                .Column(columnReturn)
                .From(table)
                .OrderBy(Terrasoft.Common.OrderDirectionStrict.Descending, columnReturn) as Select).ExecuteScalar<String>();

            return LastModifiedDateSFDC.ToString();
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