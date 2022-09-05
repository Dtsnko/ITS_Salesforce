namespace Terrasoft.Configuration.UsrProcessTargetService
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
    using System.IO.Compression;
    using System.Collections;
    using Terrasoft.Core.Process.Configuration;
    using System.Net.Security;
    using System.Diagnostics;

    public class UsrProcessTargetService
   	{
   		private AppConnection appConnection;
        private UserConnection userConnection;
        private Dictionary<string, Guid> AccountIds = new Dictionary<string, Guid>();
   		
   		public UsrProcessTargetService(UserConnection connection){
   //			appConnection = HttpContext.Current.Application["AppConnection"] as AppConnection;
			// userConnection = appConnection.SystemUserConnection;
			userConnection = connection;
   		}
   		
   		public bool SelectByAccountCode(string AccountCode)
   		{
   			List<Target> Targets = new List<Target>();
   			Select selectQuery = new Select(userConnection)
	            .Column("Id")
	            .Column("UsrTargetID")
	            .Column("UsrAccountId")
	            .Column("UsrSalesforceId")
	            .Column("UsrAmount")
	            .From("UsrTargetTemp")
	            .Where("UsrAccountId").IsEqual(Column.Parameter(AccountCode)) as Select;
	            
	            
			// Выполнение запроса к базе данных и получение результирующего набора данных.
			using (DBExecutor dbExecutor = userConnection.EnsureDBConnection())
			{
			    using (IDataReader reader = selectQuery.ExecuteReader(dbExecutor))
			    {
			        while (reader.Read())
			        {
			            Targets.Add(new Target
			            {
			            	UsrTargetID = reader.GetColumnValue<string>("UsrTargetId"),
			            	UsrAccountId = reader.GetColumnValue<string>("UsrAccountId"),
			            	UsrSalesforceId = reader.GetColumnValue<string>("UsrSalesforceId"),
			            	UsrAmount = reader.GetColumnValue<string>("UsrAmount")
			            });
			        }
			    }
			}
			foreach(var target in Targets){
				SelectByParams(target);
			}
			DeleteTempTargets(AccountCode);
			return true;
   		}
   		
   		public void SelectByParams(Target newTarget)
   		{
   			var accId = GetAccountIdByPosCode(newTarget.UsrAccountId);
   			List<Target> Targets = new List<Target>();
   			Select selectQuery = new Select(userConnection)
	            .Column("Id")
	            .Column("UsrTargetID")
	            .Column("UsrAccountId")
	            .Column("UsrSalesforceId")
	            .Column("UsrAmount")
	            .From("UsrTargets")
	            .Where("UsrTargetID").IsEqual(Column.Parameter(newTarget.UsrTargetID))
	            .And("UsrAccountId").IsEqual(Column.Parameter(accId)) as Select;
	            
	            
			// Выполнение запроса к базе данных и получение результирующего набора данных.
			using (DBExecutor dbExecutor = userConnection.EnsureDBConnection())
			{
			    using (IDataReader reader = selectQuery.ExecuteReader(dbExecutor))
			    {
			        while (reader.Read())
			        {
			            Targets.Add(new Target{
			            	Id = reader.GetColumnValue<Guid>("Id"),
			            	UsrTargetID = reader.GetColumnValue<string>("UsrTargetID"),
			            	UsrAccountId = reader.GetColumnValue<string>("UsrAccountId"),
			            	UsrSalesforceId = reader.GetColumnValue<string>("UsrSalesforceId"),
			            	UsrAmount = reader.GetColumnValue<string>("UsrAmount")
			            });
			        }
			    }
			}
			
			if(Targets.Count == 0){
				InsertTarget(newTarget);
			}
			else if(Targets.Count == 1){
				UpdateTarget(newTarget);
			}
			else if(Targets.Count > 1)
			{
				DeleteTargetDouble(newTarget.UsrAccountId, newTarget.UsrTargetID);
				InsertTarget(newTarget);
			}
			
   		}
   		
   		public int InsertTarget(Target newTarget)
   		{
   			var AccountId = GetAccountIdByPosCode(newTarget.UsrAccountId);
   			//var AccountId = newTarget.UsrAccountId;
   			var ins = new Insert(userConnection)
        	.Into("UsrTargets")
        	.Set("UsrTargetID", Column.Parameter(newTarget.UsrTargetID))
        	.Set("UsrAccountId", Column.Parameter(AccountId))
        	.Set("UsrSalesforceId", Column.Parameter(newTarget.UsrSalesforceId))
        	.Set("UsrAmount", Column.Parameter(newTarget.UsrAmount));
    		var affectedRows = ins.Execute();
    		return affectedRows;
   		}
   		
   		public int UpdateTarget(Target newTarget)
   		{
   			var AccountId = GetAccountIdByPosCode(newTarget.UsrAccountId);
   			//var AccountId = newTarget.UsrAccountId;
   			var update = new Update(userConnection, "UsrTargets")
        	.Set("UsrTargetID", Column.Parameter(newTarget.UsrTargetID))
        	.Set("UsrAccountId", Column.Parameter(AccountId))
        	.Set("UsrSalesforceId", Column.Parameter(newTarget.UsrSalesforceId))
        	.Set("UsrAmount", Column.Parameter(newTarget.UsrAmount))
        	.Where ("Id").IsEqual(Column.Parameter(newTarget.Id));
    		var cnt = update.Execute();
    		return cnt;
   		}
   		
   		public int DeleteTargetDouble(string AccountCode, string TargetId)
   		{
   			var AccountId = GetAccountIdByPosCode(AccountCode);
   			var delete = new Delete(userConnection)
   			.From("UsrTargets")
   			.Where("UsrAccountId").IsEqual(Column.Parameter(AccountId))
   			.And("UsrTargetID").IsEqual(Column.Parameter(TargetId));
   			var cnt = delete.Execute();
   			return cnt;
   		}
   		
   		public void DeleteTempTargets(string AccountCode)
   		{
   			var delete = new Delete(userConnection)
   			.From("UsrTargetTemp")
   			.Where("UsrAccountId").IsEqual(Column.Parameter(AccountCode));
   			var cnt = delete.Execute();
   		}
   		
   		public Guid GetAccountIdByPosCode(string PosCode)
   		{
   			if(AccountIds.ContainsKey(PosCode)){
   				return AccountIds[PosCode];
   			}
   			var sel = new Select(userConnection)
   			.Top(1)
            .Column("Id")
        	.From("Account")
        	.Where("Code").IsEqual(Column.Parameter(PosCode)) as Select;
        	var accId =  sel.ExecuteScalar<Guid>();
        	if(accId == Guid.Empty || accId == null)
        	{
        		accId = InsertAccount(PosCode);
        	}
        	AccountIds.Add(PosCode, accId);
        	return accId;
   		}
   		
   		public Guid InsertAccount(string PosCode)
   		{	
   			var AccountId = Guid.NewGuid();
   			var ins = new Insert(userConnection)
        	.Into("Account")
        	.Set("Id", Column.Parameter(AccountId))
        	.Set("Code", Column.Parameter(PosCode));
    		var affectedRows = ins.Execute();
    		return AccountId;
   		}
   		
   		public class Target{
   			public Guid Id {get;set;}
   			public string UsrTargetID{get;set;}
   			public string UsrAccountId{get;set;}
   			public string UsrSalesforceId{get;set;}
   			public string UsrAmount{get;set;}
   		}
   	}
}