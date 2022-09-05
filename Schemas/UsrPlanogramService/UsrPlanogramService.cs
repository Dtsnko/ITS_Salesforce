namespace Terrasoft.Configuration.UsrPlanogramService
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
	using System.Net.Security;
    using System.Security.Cryptography;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;

    [ServiceContract]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class UsrPlanogramService
	{
		private static AppConnection appConnection;
		private static UserConnection userConnection;
		
		public UsrPlanogramService()
		{
			appConnection = HttpContext.Current.Application["AppConnection"] as AppConnection;
			userConnection = appConnection.SystemUserConnection;

        }
		
		public string InsertPhotoIntoAccountAttachments(DecryptedData data, byte [] photo)
		{
            var name = data.Path.Split('\\').Last().Trim();
			if (string.IsNullOrEmpty(name))
                name = "NoName.jpg";
            var AccountEntity = userConnection.EntitySchemaManager.GetInstanceByName("Account").CreateEntity(userConnection);
            Guid accountId;
            if(!AccountEntity.FetchFromDB("Code", data.CustomerCode))
            {
                return "Account not found";
            }
            accountId = (Guid)AccountEntity.GetColumnValue("Id");

            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("UsrPlanogramId", data.PlanogramId);
            conditions.Add("UsrAccount", accountId);
            var AttachmentEntity = userConnection.EntitySchemaManager.GetInstanceByName("UsrPlanograms").CreateEntity(userConnection);
            if (!AttachmentEntity.ExistInDB(conditions))
            {
                var AttachmentId = Guid.NewGuid();
                AttachmentEntity.SetDefColumnValues();
                AttachmentEntity.SetColumnValue("Id", AttachmentId);
                AttachmentEntity.SetColumnValue("UsrName", name);
                AttachmentEntity.SetColumnValue("UsrFile", photo);
                //AttachmentEntity.SetColumnValue("TypeId", "529BC2F8-0EE0-DF11-971B-001D60E938C6");
                //AttachmentEntity.SetColumnValue("Version", 1);
                AttachmentEntity.SetColumnValue("UsrAccountId", accountId);
                AttachmentEntity.SetColumnValue("UsrPlanogramId", data.PlanogramId);
                AttachmentEntity.SetColumnValue("UsrLink", data.Link);

                try
                {
                    AttachmentEntity.Save(false);
                }
                catch (System.Exception e)
                {
                    return e.Message;
                }
            }
            else
            {
                return "Planogram alredy exists";
            }
			return "Successfully added";
		}

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare,
        ResponseFormat = WebMessageFormat.Json)]
        public void SetPlanograms(RootObject data)
        {
            string decrypted = "";
            List<DecryptedData> decryptedList = new List<DecryptedData>();
            List<string> results = new List<string>();
            Terrasoft.Configuration.ITSLogger.Log(userConnection, "", "Method call", "", "UsrPlanogramService.SetPlanograms");
            try
            {
                decrypted = DecryptStringAES(data.uploadData.Data);
                //decrypted = DecryptStringFromBytes_Aes(Convert.FromBase64String(data.data));
                var temp = JsonConvert.DeserializeObject<Planogram>(decrypted);
                decryptedList = JsonConvert.DeserializeObject<List<DecryptedData>>(temp.Planograms);
            }
            catch(System.Exception ex)
            {
            	Terrasoft.Configuration.ITSLogger.Log(userConnection, "Error: decrypt and parse", "", ex.Message, "UsrPlanogramService.SetPlanograms");
                throw new Exception(ex.Message);
            }

            foreach(var a in decryptedList)
            {
                byte[] photo = GetImageFromUri(a.Link);
                results.Add($"PlanogramId: {a.PlanogramId}; res: {InsertPhotoIntoAccountAttachments(a, photo)}");
            }

            Terrasoft.Configuration.ITSLogger.Log(userConnection, "", "Success", string.Join("\n", results), "UsrPlanogramService.SetPlanograms");
        }

        private byte[] GetImageFromUri(string uri)
        {
        	//ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
        	//ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
        	//ServicePointManager.SecurityProtocol = (SecurityProtocolType)12288;
        	uri = uri.Replace("http://","https://");
        	string html = "";
            WebClient wc = new WebClient();
			
            byte[] imageBytes = wc.DownloadData(uri);
            return imageBytes;
        }
        
		public string DecryptStringAES(string cipherText)
        {
        	var password = Convert.ToString(Terrasoft.Core.Configuration.SysSettings.GetValue(userConnection, "UsrSalesforceSecret"));
        	var salt = Convert.ToString(Terrasoft.Core.Configuration.SysSettings.GetValue(userConnection, "UsrSalesforceSalt"));
            RijndaelManaged aesAlg = null;
            string plaintext = null;
            try
            {
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt));
                byte[] bytes = Convert.FromBase64String(cipherText);
                using (MemoryStream msDecrypt = new MemoryStream(bytes))
                {
                    aesAlg = new RijndaelManaged();
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                    aesAlg.IV = ReadByteArray(msDecrypt);
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (CryptoStream csDeCrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDeCrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            finally
            {
                if(aesAlg != null)
                    aesAlg.Clear();
            }

            return plaintext;
        }

        private byte[] ReadByteArray(Stream msDecrypt)
        {
            byte[] rawLength = new byte[sizeof(int)];
            if(msDecrypt.Read(rawLength,0,rawLength.Length) != rawLength.Length)
                throw new SystemException("Stream did not contain properly formatted byte array");
            byte[] buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if(msDecrypt.Read(buffer, 0, buffer.Length) != buffer.Length)
                throw new SystemException("Did not read byte array properly");
            return buffer;
        }
        
        private string DecryptStringFromBytes_Aes(byte[] cipherText)
        {
            var Key = Convert.FromBase64String(Convert.ToString(Terrasoft.Core.Configuration.SysSettings.GetValue(userConnection, "UsrDecryptionKey")));
            var IV = Convert.FromBase64String(Convert.ToString(Terrasoft.Core.Configuration.SysSettings.GetValue(userConnection, "UsrInitializationVector")));
            string plaintext = null;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }

        // public class EncryptedData
        // {
        //     [DataMember]
        //     [JsonProperty("data")]
        //     public string data { get; set; }
        // }
        
        public class UploadData
		{
			[DataMember]
            [JsonProperty("Data")]
		    public string Data { get; set; }
		}
		
		public class RootObject
		{
			[DataMember]
            [JsonProperty("uploadData")]
		    public UploadData uploadData { get; set; }
		    [DataMember]
            [JsonProperty("delete")]
		    public bool delete { get; set; }
		}
		
		public class Planogram
        {
            public string Planograms { get; set; }
        }
		
        public class DecryptedData
        {
            [DataMember]
            [JsonProperty("PlanogramId")]
            public int PlanogramId { get; set; }
            [DataMember]
            [JsonProperty("CustomerCode")]
            public string CustomerCode { get; set; }
            [DataMember]
            [JsonProperty("Size")]
            public float Size { get; set; }
            [DataMember]
            [JsonProperty("Created")]
            public DateTime Created { get; set; }
            [DataMember]
            [JsonProperty("Updated")]
            public DateTime? Updated { get; set; }
            [DataMember]
            [JsonProperty("Path")]
            public string Path { get; set; }
            [DataMember]
            [JsonProperty("Link")]
            public string Link { get; set; }

        }
    }
}
