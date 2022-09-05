namespace Terrasoft.Configuration.UsrPhotoService
{
	using System;
	using System.Linq;
	using System.Data;
	using System.Collections.Generic;
	using System.Web;
	using System.ServiceModel;
	using System.ServiceModel.Web;
	using System.ServiceModel.Activation;
	using System.Runtime.Serialization;
	using System.Net;
	using System.Text;
	using System.Web;
	using CoreSysSettings = Terrasoft.Core.Configuration.SysSettings;
	using System.IO;
	using Terrasoft.Common;
	using Terrasoft.Core;
	using Terrasoft.Core.DB;
	using Column = Terrasoft.Core.DB.Column;
	using Terrasoft.Core.Entities;
	using Newtonsoft;
	using Newtonsoft.Json;
	using System.ComponentModel;
	using System.Reflection;
	using System.Net.Http.Headers;
	using System.Net.Http;
	using System.Threading.Tasks;
	using System.Globalization;
	using System.Text.RegularExpressions;
	using Terrasoft.Core.Process;
	using Terrasoft.Mail.Sender;
	using Terrasoft.Mail;
	using Terrasoft.Core.Factories;
	using System.Security.Cryptography;
	using System.Xml;
	using System.Xml.Linq;

    [ServiceContract]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class UsrPhotoService
	{
		private  AppConnection appConnection;
		private  UserConnection userConnection;
		
		public UsrPhotoService()
		{
			this.appConnection = HttpContext.Current.Application["AppConnection"] as AppConnection;
			this.userConnection = appConnection.SystemUserConnection;
        }
        
        public UsrPhotoService(UserConnection _userConnection)
		{
			userConnection = _userConnection;
        }
        
        
        [OperationContract] 
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json,
        ResponseFormat = WebMessageFormat.Json)]
        public void RefreshPhoto(string IDs)
        {
        	var stringID = IDs.Split(',');
        	string idImg="", url = "";
			string token = Terrasoft.Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrAuthorizationToken", string.Empty);
			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
			foreach(var sID in stringID){
				Guid ID;
				if(Guid.TryParse(sID, out ID)){
					idImg = GetStringByGuid("UsrPhotos", "Id", ID, "UsrSfdcId");
					url = $"https://pmi.my.salesforce.com/services/data/v20.0/sobjects/Attachment/{idImg}/Body";
					try
					{
						HttpResponseMessage response = client.GetAsync(url).Result;
						var result = response.Content.ReadAsStreamAsync().Result;
						
						var photoEntity = userConnection.EntitySchemaManager.GetInstanceByName("UsrPhotos").CreateEntity(userConnection);
						if (photoEntity.FetchFromDB(ID))
						{
							photoEntity.SetStreamValue("UsrFile", result);
							photoEntity.Save(false);
							var newid = Guid.NewGuid();
							var update = new Update(userConnection, "UsrPhotos")
								.Set("Id", Column.Parameter(newid))
								.Where ("Id").IsEqual(Column.Parameter(ID));
						 	update.Execute();
						}
						
					}
					catch (AggregateException ex)
					{
						string err = "";
						foreach (var x in ex.InnerExceptions) {
							err += x.Message;
						}
					Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "", "Error:", err+$" Id inBPM:{ID}, IdPhoto:{idImg}", "SetPhoto");
					}
					catch (Exception ex)
					{
						string err = ex.Message;

						 if (ex is WebException)
						 {
							 var wex = (WebException)ex;
							  err = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();
						 }
						Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "", "Error:", err+$" Id inBPM:{ID}, IdPhoto:{idImg}", "SetPhoto");
						
					}
				}
			}
        }

    	[OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare,
        ResponseFormat = WebMessageFormat.Json)]
        public bool SetPhoto(Guid ID)
        {
        	string idImg = GetStringByGuid("UsrPhotos", "Id", ID, "UsrSfdcId");
        	string token = Terrasoft.Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrAuthorizationToken", string.Empty);
            string url = $"https://pmi.my.salesforce.com/services/data/v20.0/sobjects/Attachment/{idImg}/Body";
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(15);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            try
	        {
		        HttpResponseMessage response = client.GetAsync(url).Result;
		        var result = response.Content.ReadAsStreamAsync().Result;
	            
	            var photoEntity = userConnection.EntitySchemaManager.GetInstanceByName("UsrPhotos").CreateEntity(userConnection);
	            if (photoEntity.FetchFromDB(ID))
				{
					photoEntity.SetStreamValue("UsrFile", result);
		            photoEntity.Save(false);
				}
	        }
	        catch (AggregateException ex)
            {
                string err = "";
                foreach (var x in ex.InnerExceptions) {
                    err += x.Message;
                }
            Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "", "Error:", err+$" Id inBPM:{ID}, IdPhoto:{idImg}", "SetPhoto");
            }
	        catch (Exception ex)
	        {
	        	string err = ex.Message;

                 if (ex is WebException)
                 {
                     var wex = (WebException)ex;
                      err = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();
                 }
	        	Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "", "Error:", err+$" Id inBPM:{ID}, IdPhoto:{idImg}", "SetPhoto");
	        	return false;
	        }
	        return true;
        }

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare,
        ResponseFormat = WebMessageFormat.Json)]
        public void TryLogin()
        {
        	string url = "https://login.salesforce.com/services/Soap/u/43.0";
        	try
	        {
	            string log = Terrasoft.Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceLogin", string.Empty);
	            string pas = Terrasoft.Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforcePassword", string.Empty);
	            string tok = Terrasoft.Core.Configuration.SysSettings.GetValue<string>(userConnection, "UsrSalesforceToken", string.Empty);
	            
	            string data = getXMLString(log, pas+tok);
	
	            var request = (HttpWebRequest)WebRequest.Create(url);
	            request.Method = "POST";
	            byte[] byteArray = Encoding.UTF8.GetBytes(data);
	            request.ContentType = "text/xml";
	            request.Headers.Add("SOAPAction", "Wololo");
	            request.ContentLength = byteArray.Length;
	            using (var dataStream = request.GetRequestStream())
	            {
	                dataStream.Write(byteArray, 0, byteArray.Length);
	                dataStream.Close();
	            }
	            using (var response = (HttpWebResponse)request.GetResponse())
	            {
	                using (StreamReader rd = new StreamReader(response.GetResponseStream()))
	                {
	                    XDocument mydoc = XDocument.Load(rd);
	                    XNamespace ns = "urn:partner.soap.sforce.com";
	                    var info = (from o in mydoc.Root.Descendants(ns + "result")
	                                   select new
	                                   {
	                                       sid = (string)o.Element(ns + "sessionId"),
	                                       //uid = (string)o.Element(ns + "userId"),
	                                   }).FirstOrDefault();
	                    CoreSysSettings.SetDefValue(userConnection, "UsrAuthorizationToken", info.sid);
	                }
	            }
			}
			catch (Exception ex)
			{
				string err = ex.Message;

                 if (ex is WebException)
                 {
                     var wex = (WebException)ex;
                      err = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();
                 }
				Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "", "Error:", err, "UsrPhotoService.TryLogin");
			}
        }
        
        public string getXMLString(string log, string pas){
            using (var sw = new StringWriter())
            {
                using (var xw = new XmlTextWriter(sw))
                {
                    xw.WriteStartElement("soapenv:Envelope");
                    xw.WriteAttributeString("xmlns:soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
                    xw.WriteAttributeString("xmlns:urn", "urn:partner.soap.sforce.com");
                    xw.WriteStartElement("soapenv:Body");
                    xw.WriteStartElement("urn:login");
                    xw.WriteStartElement("urn:username");
                    xw.WriteString(log);
                    xw.WriteEndElement();
                    xw.WriteStartElement("urn:password");
                    xw.WriteString(pas);
                    xw.WriteEndElement();
                    xw.WriteEndElement();
                    xw.WriteEndElement();
                    xw.WriteEndElement();
                }
                return sw.ToString();
            }
        }

        public string GetStringByGuid(string table, string column, Guid value, string columnReturn)
		{
			if (value == Guid.Empty || value == null)
			{
				return String.Empty;
			}
			var lookupBPMId = (new Select(userConnection).Top(1)
				.Column(columnReturn)
				.From(table)
				.Where(column).IsEqual(Terrasoft.Core.DB.Column.Parameter(value)) as Select).ExecuteScalar<string>();
			return lookupBPMId;
		}
    }
}
