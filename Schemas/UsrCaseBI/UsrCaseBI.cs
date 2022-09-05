namespace Terrasoft.Configuration.UsrCaseBI
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
	using System.Data;

	[ServiceContract]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class UsrCaseBI
	{
		private AppConnection appConnection;
		private UserConnection userConnection;
		private Response response;

		public UsrCaseBI()
		{
			appConnection = HttpContext.Current.Application["AppConnection"] as AppConnection;
			userConnection = appConnection.SystemUserConnection;
		}

		public UsrCaseBI(UserConnection userConnection)
		{
			this.userConnection = userConnection;
		}

		[OperationContract]
		[WebInvoke(Method = "GET", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare,
		ResponseFormat = WebMessageFormat.Json)]
		public List<ResponseCase> GetCase(DateTime date_from, DateTime date_to)
		{
			Response response = new Response();

			response.Success = true;

			List<ResponseCase> responseCasesInfo = new List<ResponseCase>();
			ResponseCase responseCaseInfo = new ResponseCase();

			try
			{
				if (date_from == null && date_to == null)
				{
					response.Success = false;
					Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrCaseBI:", "Error:", "InvalidIncomingData", "GetCase()");
					return responseCasesInfo;
				}

				Select select = new Select(userConnection)

				.Column("Case", "Id").As("CaseId")
				.Column("Case", "CreatedOn").As("CreatedOn")
				.Column("CreatedBy", "Id").As("CreatedById")
				.Column("CreatedBy", "Name").As("CreatedByName")
				.Column("Case", "ModifiedOn").As("ModifiedOn")
				.Column("ModifiedBy", "Id").As("ModifiedById")
				.Column("ModifiedBy", "Name").As("ModifiedByName")
				.Column("Case", "Number").As("Number")
				.Column("Case", "RegisteredOn").As("RegisteredOn")
				.Column("Case", "Subject").As("Subject")
				.Column("Case", "Symptoms").As("Symptoms")
				.Column("Owner", "Id").As("OwnerId")
				.Column("Owner", "Name").As("OwnerName")
				.Column("Case", "ResponseDate").As("ResponseDate")
				.Column("Case", "SolutionDate").As("SolutionDate")
				.Column("Status", "Id").As("StatusId")
				.Column("Status", "Name").As("StatusName")
				.Column("Case", "Notes").As("Notes")
				.Column("Priority", "Id").As("PriorityId")
				.Column("Priority", "Name").As("PriorityName")
				.Column("Origin", "Id").As("OriginId")
				.Column("Origin", "Name").As("OriginName")
				.Column("Account", "Id").As("AccountId")
				.Column("Account", "Name").As("AccountName")
				.Column("Case", "ContactId").As("ContactId")
				.Column("Case", "RespondedOn").As("RespondedOn")
				.Column("Case", "SolutionProvidedOn").As("SolutionProvidedOn")
				.Column("Case", "ClosureDate").As("ClosureDate")
				.Column("ClosureCode", "Id").As("ClosureCodeId")
				.Column("ClosureCode", "Name").As("ClosureCodeName")
				.Column("Category", "Id").As("CategoryId")
				.Column("Category", "Name").As("CategoryName")
				.Column("Case", "ResponseOverdue").As("ResponseOverdue")
				.Column("Case", "SolutionOverdue").As("SolutionOverdue")
				.Column("Case", "SolutionRemains").As("SolutionRemains")
				.Column("ServiceItem", "Id").As("ServiceItemId")
				.Column("ServiceItem", "Name").As("ServiceItemName")
				.Column("Case", "FirstSolutionProvidedOn").As("FirstSolutionProvidedOn")

				.From("Case").As("Case")

				.Join(JoinType.LeftOuter, "Contact").As("CreatedBy")
				.On("Case", "CreatedById").IsEqual("CreatedBy", "Id")
				.Join(JoinType.LeftOuter, "Contact").As("ModifiedBy")
				.On("Case", "ModifiedById").IsEqual("ModifiedBy", "Id")
				.Join(JoinType.LeftOuter, "Contact").As("Owner")
				.On("Case", "OwnerId").IsEqual("Owner", "Id")
				.Join(JoinType.LeftOuter, "CaseStatus").As("Status")
				.On("Case", "StatusId").IsEqual("Status", "Id")
				.Join(JoinType.LeftOuter, "CasePriority").As("Priority")
				.On("Case", "PriorityId").IsEqual("Priority", "Id")
				.Join(JoinType.LeftOuter, "CaseOrigin").As("Origin")
				.On("Case", "OriginId").IsEqual("Origin", "Id")
				.Join(JoinType.LeftOuter, "Account").As("Account")
				.On("Case", "AccountId").IsEqual("Account", "Id")
				.Join(JoinType.LeftOuter, "ClosureCode").As("ClosureCode")
				.On("Case", "ClosureCodeId").IsEqual("ClosureCode", "Id")
				.Join(JoinType.LeftOuter, "CaseCategory").As("Category")
				.On("Case", "CategoryId").IsEqual("Category", "Id")
				.Join(JoinType.LeftOuter, "ServiceItem").As("ServiceItem")
				.On("Case", "ServiceItemId").IsEqual("ServiceItem", "Id")

				.Where("Case", "ModifiedOn").IsGreaterOrEqual(Terrasoft.Core.DB.Column.Parameter(date_from))
				.And("Case", "ModifiedOn").IsLessOrEqual(Terrasoft.Core.DB.Column.Parameter(date_to))
				.OrderBy(Terrasoft.Common.OrderDirectionStrict.Ascending, "ModifiedOn")

				as Select;

				using (DBExecutor dbExecutor = userConnection.EnsureDBConnection())
				{
					using (IDataReader reader = select.ExecuteReader(dbExecutor))
					{
						while (reader.Read())
						{
							responseCaseInfo = new ResponseCase();
							responseCaseInfo.Id = reader.GetColumnValue<Guid>("CaseId");
							responseCaseInfo.CreatedOn = reader.GetColumnValue<string>("CreatedOn") ?? String.Empty;
							if (reader.GetColumnValue<Guid>("CreatedById") != Guid.Empty)	
							{
								responseCaseInfo.CreatedById = getIdAndName(reader.GetGuid(reader.GetOrdinal("CreatedById")), reader.GetString(reader.GetOrdinal("CreatedByName"))) ?? null;
							}
							responseCaseInfo.ModifiedOn = reader.GetColumnValue<string>("ModifiedOn") ?? String.Empty;
							if (reader.GetColumnValue<Guid>("ModifiedById") != Guid.Empty)
							{
								responseCaseInfo.ModifiedById = getIdAndName(reader.GetGuid(reader.GetOrdinal("ModifiedById")), reader.GetString(reader.GetOrdinal("ModifiedByName"))) ?? null;
							}
							responseCaseInfo.Number = reader.GetColumnValue<string>("Number") ?? String.Empty;
							responseCaseInfo.RegisteredOn = reader.GetColumnValue<string>("RegisteredOn") ?? String.Empty;
							responseCaseInfo.Subject = reader.GetColumnValue<string>("Subject") ?? String.Empty;
							responseCaseInfo.Symptoms = reader.GetColumnValue<string>("Symptoms") ?? String.Empty;
							if (reader.GetColumnValue<Guid>("OwnerId") != Guid.Empty)
							{
								responseCaseInfo.OwnerId = getIdAndName(reader.GetGuid(reader.GetOrdinal("OwnerId")), reader.GetString(reader.GetOrdinal("OwnerName"))) ?? null;
							}
							responseCaseInfo.ResponseDate = reader.GetColumnValue<string>("ResponseDate") ?? String.Empty;
							responseCaseInfo.SolutionDate = reader.GetColumnValue<string>("SolutionDate") ?? String.Empty;
							if (reader.GetColumnValue<Guid>("StatusId") != Guid.Empty)
							{
								responseCaseInfo.StatusId = getIdAndName(reader.GetGuid(reader.GetOrdinal("StatusId")), reader.GetString(reader.GetOrdinal("StatusName"))) ?? null;
							}
							responseCaseInfo.Notes = reader.GetColumnValue<string>("Notes") ?? String.Empty;
							if (reader.GetColumnValue<Guid>("PriorityId") != Guid.Empty)
							{
								responseCaseInfo.PriorityId = getIdAndName(reader.GetGuid(reader.GetOrdinal("PriorityId")), reader.GetString(reader.GetOrdinal("PriorityName"))) ?? null;
							}
							if (reader.GetColumnValue<Guid>("OriginId") != Guid.Empty)
							{
								responseCaseInfo.OriginId = getIdAndName(reader.GetGuid(reader.GetOrdinal("OriginId")), reader.GetString(reader.GetOrdinal("OriginName"))) ?? null;
							}
							if (reader.GetColumnValue<Guid>("AccountId") != Guid.Empty)
							{
								responseCaseInfo.AccountId = getIdAndName(reader.GetGuid(reader.GetOrdinal("AccountId")), reader.GetString(reader.GetOrdinal("AccountName"))) ?? null;
							}
							responseCaseInfo.ContactId = reader.GetColumnValue<string>("ContactId") ?? String.Empty;
							responseCaseInfo.RespondedOn = reader.GetColumnValue<string>("RespondedOn") ?? String.Empty;
							responseCaseInfo.SolutionProvidedOn = reader.GetColumnValue<string>("SolutionProvidedOn") ?? String.Empty;
							responseCaseInfo.ClosureDate = reader.GetColumnValue<string>("ClosureDate") ?? String.Empty;
							if (reader.GetColumnValue<Guid>("ClosureCodeId") != Guid.Empty)
							{
								responseCaseInfo.ClosureCodeId = getIdAndName(reader.GetGuid(reader.GetOrdinal("ClosureCodeId")), reader.GetString(reader.GetOrdinal("ClosureCodeName"))) ?? null;
							}
							if (reader.GetColumnValue<Guid>("CategoryId") != Guid.Empty)
							{
								responseCaseInfo.CategoryId = getIdAndName(reader.GetGuid(reader.GetOrdinal("CategoryId")), reader.GetString(reader.GetOrdinal("CategoryName"))) ?? null;
							}
							responseCaseInfo.ResponseOverdue = reader.GetColumnValue<string>("ResponseOverdue") ?? String.Empty;
							responseCaseInfo.SolutionOverdue = reader.GetColumnValue<string>("SolutionOverdue") ?? String.Empty;
							responseCaseInfo.SolutionRemains = reader.GetColumnValue<string>("SolutionRemains") ?? String.Empty;
							if (reader.GetColumnValue<Guid>("ServiceItemId") != Guid.Empty)
							{
								responseCaseInfo.ServiceItemId = getIdAndName(reader.GetGuid(reader.GetOrdinal("ServiceItemId")), reader.GetString(reader.GetOrdinal("ServiceItemName"))) ?? null;
							}
							responseCaseInfo.FirstSolutionProvidedOn = reader.GetColumnValue<string>("FirstSolutionProvidedOn") ?? String.Empty;
							responseCasesInfo.Add(responseCaseInfo);
						}
					}
				}
			}

			catch (Exception ex)
			{
				response.Success = false;
				response.Error = ex.Message;
				Terrasoft.Configuration.UsrITSSalesforceLogger.Log(userConnection, "UsrCaseBI: ", "Error:", ex.Message, "GetCase()");
				throw new System.Exception(ex.Message);
				return responseCasesInfo;
			}

			return responseCasesInfo;
		}

		public Object getIdAndName(Guid id, string name)
		{
			return new Object
			{
				Id = id,
				Name = name
			};
        }

		public class ResponseCase
		{
			[DataMember(Name = "Id")]
			[JsonProperty("Id")]
			public Guid? Id { get; set; }
			
			[DataMember(Name = "CreatedOn")]
			[JsonProperty("CreatedOn")]
			public string CreatedOn { get; set; }

			[DataMember(Name = "CreatedById")]
			[JsonProperty("CreatedById")]
			public Object CreatedById { get; set; }

			[DataMember(Name = "ModifiedOn")]
			[JsonProperty("ModifiedOn")]
			public string ModifiedOn { get; set; }
			
			[DataMember(Name = "ModifiedById")]
			[JsonProperty("ModifiedById")]
			public Object ModifiedById { get; set; }

			[DataMember(Name = "Number")]
			[JsonProperty("Number")]
			public string Number { get; set; }
			
			[DataMember(Name = "RegisteredOn")]
			[JsonProperty("RegisteredOn")]
			public string RegisteredOn { get; set; }
			
			[DataMember(Name = "Subject")]
			[JsonProperty("Subject")]
			public string Subject { get; set; }
			
			[DataMember(Name = "Symptoms")]
			[JsonProperty("Symptoms")]
			public string Symptoms { get; set; }
			
			[DataMember(Name = "OwnerId")]
			[JsonProperty("OwnerId")]
			public Object OwnerId { get; set; }

			[DataMember(Name = "ResponseDate")]
			[JsonProperty("ResponseDate")]
			public string ResponseDate { get; set; }
			
			[DataMember(Name = "SolutionDate")]
			[JsonProperty("SolutionDate")]
			public string SolutionDate { get; set; }
			
			[DataMember(Name = "StatusId")]
			[JsonProperty("StatusId")]
			public Object StatusId { get; set; }

			[DataMember(Name = "Notes")]
			[JsonProperty("Notes")]
			public string Notes { get; set; }
			
			[DataMember(Name = "PriorityId")]
			[JsonProperty("PriorityId")]
			public Object PriorityId { get; set; }

			[DataMember(Name = "OriginId")]
			[JsonProperty("OriginId")]
			public Object OriginId { get; set; }

			[DataMember(Name = "AccountId")]
			[JsonProperty("AccountId")]
			public Object AccountId { get; set; }

			[DataMember(Name = "ContactId")]
			[JsonProperty("ContactId")]
			public string ContactId { get; set; }
			
			[DataMember(Name = "RespondedOn")]
			[JsonProperty("RespondedOn")]
			public string RespondedOn { get; set; }
			
			[DataMember(Name = "SolutionProvidedOn")]
			[JsonProperty("SolutionProvidedOn")]
			public string SolutionProvidedOn { get; set; }
			
			[DataMember(Name = "ClosureDate")]
			[JsonProperty("ClosureDate")]
			public string ClosureDate { get; set; }
			
			[DataMember(Name = "ClosureCodeId")]
			[JsonProperty("ClosureCodeId")]
			public Object ClosureCodeId { get; set; }

			[DataMember(Name = "CategoryId")]
			[JsonProperty("CategoryId")]
			public Object CategoryId { get; set; }

			[DataMember(Name = "ResponseOverdue")]
			[JsonProperty("ResponseOverdue")]
			public string ResponseOverdue { get; set; }
			
			[DataMember(Name = "SolutionOverdue")]
			[JsonProperty("SolutionOverdue")]
			public string SolutionOverdue { get; set; }
			
			[DataMember(Name = "SolutionRemains")]
			[JsonProperty("SolutionRemains")]
			public string SolutionRemains { get; set; }
			
			[DataMember(Name = "ServiceItemId")]
			[JsonProperty("ServiceItemId")]
			public Object ServiceItemId { get; set; }

			[DataMember(Name = "FirstSolutionProvidedOn")]
			[JsonProperty("FirstSolutionProvidedOn")]
			public string FirstSolutionProvidedOn { get; set; }
		}

		[DataContract]
		public class Object		
		{
			[DataMember(Name = "Id")]
			[JsonProperty("Id")]
			public Guid Id { get; set; }

			[DataMember(Name = "Name")]
			[JsonProperty("Name")]
			public string Name { get; set; }
		}
		
		public class Response
        {
            public string Message { get; set; }

            public bool Success { get; set; }

            public string Error { get; set; }
        }
	}
 }