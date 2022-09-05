namespace Terrasoft.Configuration.UsrDecryptionPostRequest
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
    using Quartz.Impl;
    using Quartz;
    using Quartz.Impl.Triggers;
    using Terrasoft.Core.Scheduler;


    /*Test Data
 	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
 	*/
	[ServiceContract]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class UsrDecryptionPostRequest
    {
        private AppConnection appConnection;
        private UserConnection userConnection;

        public UsrDecryptionPostRequest()
        {
            appConnection = HttpContext.Current.Application["AppConnection"] as AppConnection;
            userConnection = appConnection.SystemUserConnection;
        }

        public UsrDecryptionPostRequest(UserConnection userConnection)
        {
            this.userConnection = userConnection;
        }
        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare,
        ResponseFormat = WebMessageFormat.Json)]
        public void InsertData(RootObject data)
        {
            string decrypted = "";
            List<Response> responses = new List<Response>();
            responses.Add(new Response(){IsSuccess = true, ErrorMessage = data.uploadData.Data.Substring(0,10)});
            Terrasoft.Configuration.ITSLogger.Log(userConnection, "", "Request data", "DecryptedData:" + decrypted, "UsrDecryptionPostRequest.InsertData");
            try
            {
                decrypted = DecryptStringAES(data.uploadData.Data);
                responses.Add(new Response(){IsSuccess = true, ErrorMessage = "Success decrypted"});
                Root root = DeserializeJson(decrypted);
                 responses.Add(new Response(){IsSuccess = true, ErrorMessage = "Success deserialize"});
                responses.AddRange(UpdateBPMData(root));
                responses.Add(new Response(){IsSuccess = true, ErrorMessage = "Success updated data"});
            }
            catch (System.Exception ex)
            {
                responses.Add(new Response() { IsSuccess = false, ErrorMessage = ex.Message });
                Terrasoft.Configuration.ITSLogger.Log(userConnection, "Error", "", string.Join("\n",responses.Select(x=>x.ErrorMessage)), "UsrDecryptionPostRequest.InsertData");
                throw new Exception(string.Join("\n",responses.Select(x=>x.ErrorMessage)));
            }
            
            ExecuteProcess();
            // if(!IsActiveProccess()){
            // 	Terrasoft.Configuration.ITSLogger.Log(userConnection, "", "Success", "Start process", "UsrDecryptionPostRequest.InsertData");
            // 	ExecuteProcess();
            // 	Terrasoft.Configuration.ITSLogger.Log(userConnection, "", "Success", "End process", "UsrDecryptionPostRequest.InsertData");
            // }
            // Terrasoft.Configuration.ITSLogger.Log(userConnection, "Errors", "", string.Join("\n",responses.Where(y=>!y.IsSuccess).Select(x=>x.ErrorMessage)), "UsrDecryptionPostRequest.InsertData");
            // Terrasoft.Configuration.ITSLogger.Log(userConnection, "", "Success", string.Join("\n",responses.Where(y=>y.IsSuccess).Select(x=>x.Result)), "UsrDecryptionPostRequest.InsertData");
        }
        
        private void ExecuteProcess(){
   			string processName = "UsrProccessTargetNew";
			string jobName = processName + "Job";
			string jobGroupName = jobName + "Group";
			 
			double startOffset = 70; // 80
			AppScheduler.RemoveJob(jobName, jobGroupName);
			var job = AppScheduler.CreateProcessJob(jobName, jobGroupName, processName, userConnection.Workspace.Name, userConnection.CurrentUser.Name);
			var trigger = new SimpleTriggerImpl(jobName + "Trigger", jobGroupName, DateTime.UtcNow.AddMinutes(startOffset));
			AppScheduler.Instance.ScheduleJob(job, trigger);
   //     	ProcessSchemaManager processSchemaManager = (ProcessSchemaManager)userConnection.GetSchemaManager(@"ProcessSchemaManager");
			// ProcessSchema processSchema = processSchemaManager.GetInstanceByName(@"UsrStartProcessTarget"); //вместо GetManager вставить название БП
			// Terrasoft.Core.Process.Process process = processSchema.CreateProcess(userConnection);
			// process.Execute(userConnection);
        }
        
        private bool IsActiveProccess(){
			var processEngine = userConnection.ProcessEngine;
			IProcessExecutor processExecutor = processEngine.ProcessExecutor;
			string schemaName = "UsrProccessTargetNew";
			System.Diagnostics.Debugger.Break();
			var select =
				(Select)new Select(userConnection).Top(10)
					.Column("spl", "Id")
				.From("SysProcessLog").As("spl").WithHints(Hints.NoLock)
				.InnerJoin("SysSchema").As("ss").WithHints(Hints.NoLock)
					.On("ss", "Id").IsEqual("spl", "SysSchemaId")
				.InnerJoin("SysProcessStatus").As("sps").WithHints(Hints.NoLock)
					.On("sps", "Id").IsEqual("spl", "StatusId")
				.Where("ss", "Name").IsEqual(Column.Parameter(schemaName))
				.And("sps", "Value").IsEqual(Column.Parameter("1"))
				.And("spl", "ParentId").IsNull();
			var processIds = new HashSet<Guid>();
			using (var dbExecutor = userConnection.EnsureDBConnection()) {
				using (IDataReader dataReader = select.ExecuteReader(dbExecutor)) {
					while (dataReader.Read()) {
						Guid sysProcessDataId = dataReader.GetColumnValue<Guid>("Id");
						processIds.Add(sysProcessDataId);
					}
				}
			}
			return processIds.Count == 1 ? true : false;
        }

        private List<Response> UpdateBPMData(Root root)
        {
        	Terrasoft.Configuration.ITSLogger.Log(userConnection, "", "Info count", $"Touchpoint count: {root.Touchpoints.Count()}; Brands count: {root.Brands.Count()}; Targets count: {root.Targets.Count()}; Contracts count: {root.Contracts.Count()}" , "UsrDecryptionPostRequest.UpdateBPMData");
            List<Response> response = new List<Response>();
            foreach (var touchpoint in root.Touchpoints)
            {
            	try{
                	response.Add(InsUpdTouchpoint(touchpoint));
            	}
            	catch(System.Exception e){
					Terrasoft.Configuration.ITSLogger.Log(userConnection, "Error. touchpoint", "", e.Message, "UsrDecryptionPostRequest.UpdateBPMData");
            	}
            }
            foreach (var brand in root.Brands)
            {
            	try{
                	response.Add(InsUpdBrand(brand));
            	}
            	catch(System.Exception e){
            		Terrasoft.Configuration.ITSLogger.Log(userConnection, "Error. brand", "", e.Message, "UsrDecryptionPostRequest.UpdateBPMData");
            	}
            }
            foreach (var target in root.Targets)
            {
            	try{
                	response.Add(InsUpdTarget(target));
            	}
            	catch(System.Exception e){
            		Terrasoft.Configuration.ITSLogger.Log(userConnection, "Error. target", "", e.Message, "UsrDecryptionPostRequest.UpdateBPMData");
            	}
            }

            foreach (var contract in root.Contracts)
            {
            	try{
                	response.Add(InsUpdContract(contract));
            	}
            	catch(System.Exception e){
            		Terrasoft.Configuration.ITSLogger.Log(userConnection, "Error. contract", "", e.Message, "UsrDecryptionPostRequest.UpdateBPMData");
            	}
            }
            return response;
        }

        //Update/insert brand
        private Response InsUpdBrand(Brand brand)
        {
            // var BrandEntity = userConnection.EntitySchemaManager.GetInstanceByName("UsrBrands").CreateEntity(userConnection);
            var entitySchema = new EntitySchemaQuery(userConnection.EntitySchemaManager, "UsrBrands");
			
			entitySchema.AddColumn("Id");
            entitySchema.AddColumn("Name");
            entitySchema.AddColumn("UsrBrandId");
            entitySchema.AddColumn("UsrLongBrand");
            entitySchema.AddColumn("UsrSort");
            
			entitySchema.Filters.Add(entitySchema.CreateFilterWithParameters(FilterComparisonType.Equal, "UsrBrandId", brand.BrandId));
			
			var brandCollection = entitySchema.GetEntityCollection(userConnection);
			if(brandCollection.Count() > 0){
				var BrandEntity = brandCollection[0];
				BrandEntity.SetColumnValue("Name", brand.ShortBrand);
                BrandEntity.SetColumnValue("UsrBrandId", brand.BrandId);
                BrandEntity.SetColumnValue("UsrLongBrand", brand.LongBrand);
                BrandEntity.SetColumnValue("UsrSort", brand.Sort);
                try
                {
                    BrandEntity.Save(false);
                    return new Response()
                    {
                        IsSuccess = true,
                        Result = "Brand successfuly updated"
                    };
                }
                catch (System.Exception e)
                {
                    return new Response()
                    {
                        IsSuccess = false,
                        ErrorMessage = "InsUpdBrand: " + e.Message
                    };
                }
			}
			else{
				var BrandEntity = userConnection.EntitySchemaManager.GetInstanceByName("UsrBrands").CreateEntity(userConnection);
				var BrandId = Guid.NewGuid();
                BrandEntity.SetDefColumnValues();
                BrandEntity.SetColumnValue("Id", BrandId);
                BrandEntity.SetColumnValue("Name", brand.ShortBrand);
                BrandEntity.SetColumnValue("UsrBrandId", brand.BrandId);
                BrandEntity.SetColumnValue("UsrLongBrand", brand.LongBrand);
                BrandEntity.SetColumnValue("UsrSort", brand.Sort);

                try
                {
                    BrandEntity.Save(false);
                    return new Response()
                    {
                        IsSuccess = true,
                        Result = "Brand successfuly inserted: " + BrandId
                    };
                }
                catch (System.Exception e)
                {
                    return new Response()
                    {
                        IsSuccess = false,
                        ErrorMessage = "InsUpdBrand: " + e.Message
                    };
                }
			}
        }

        //Update/insert Target
        private Response InsUpdTarget(Target target)
        {
        	var TempTargetEntity = userConnection.EntitySchemaManager.GetInstanceByName("UsrTargetTemp").CreateEntity(userConnection);
        	
        	TempTargetEntity.SetDefColumnValues();
            TempTargetEntity.SetColumnValue("UsrTargetId", target.TargetID);
            TempTargetEntity.SetColumnValue("UsrAccountCode", target.CustomerCode);
            TempTargetEntity.SetColumnValue("UsrSalesforceId", target.BrandId);
            TempTargetEntity.SetColumnValue("UsrAmount", target.Amount);
        	
        	
        	try
            {
                TempTargetEntity.Save(false);
                //Terrasoft.Configuration.ITSLogger.Log(userConnection, "", "Success insert", "Inseert TargetId: " + target.TargetID.ToString(), "UsrDecryptionPostRequest.InsertData.InsUpdTarget");
                return new Response()
                {
                    IsSuccess = true,
                    Result = "Target successfuly inserted"
                };
            }
            catch (System.Exception e)
            {
            	Terrasoft.Configuration.ITSLogger.Log(userConnection, "Error insert", "", "Inseert TargetId: " + target.TargetID.ToString() + "\nErrorMessage: " + e.Message, "UsrDecryptionPostRequest.InsertData.InsUpdTarget");
                return new Response()
                {
                    IsSuccess = false,
                    ErrorMessage = "InsUpdTarget: " + e.Message
                };
            }
            // var TargetEntity = userConnection.EntitySchemaManager.GetInstanceByName("UsrTargets").CreateEntity(userConnection);

            // Guid AccountId = GetLookupBpmIdByString("Account", "Code", target.CustomerCode, "Id"); 
            // if(AccountId == Guid.Empty || AccountId == null)
            // {
            //     return new Response()
            //     {
            //         IsSuccess = false,
            //         ErrorMessage = $"Account with ID {target.CustomerCode} not found!"
            //     };
            // }
            // // Guid BrandId = GetLookupBpmIdByString("UsrBrands", "UsrBrandId", target.BrandId, "Id");
            // // if (BrandId == Guid.Empty || BrandId == null)
            // // {
            // //     return new Response()
            // //     {
            // //         IsSuccess = false,
            // //         ErrorMessage = $"Brand with id {target.BrandId} not found!"
            // //     };
            // // }
            // var conditions = new Dictionary<string, object>();
            // conditions.Add("UsrTargetID", target.TargetID);
            // conditions.Add("UsrAccount", AccountId);

            // if (!TargetEntity.FetchFromDB(conditions))
            // {
            //     TargetEntity.SetDefColumnValues();
            //     TargetEntity.SetColumnValue("UsrTargetID", target.TargetID);
            //     TargetEntity.SetColumnValue("UsrAccountId", AccountId);
            //     TargetEntity.SetColumnValue("UsrSalesforceId", target.BrandId);
            //     TargetEntity.SetColumnValue("UsrAmount", target.Amount);

            //     try
            //     {
            //         TargetEntity.Save(false);
            //         Terrasoft.Configuration.ITSLogger.Log(userConnection, "", "Success insert", "Inseert TargetId: " + target.TargetID.ToString(), "UsrDecryptionPostRequest.InsertData.InsUpdTarget");
            //         return new Response()
            //         {
            //             IsSuccess = true,
            //             Result = "Target successfuly inserted"
            //         };
            //     }
            //     catch (System.Exception e)
            //     {
            //     	Terrasoft.Configuration.ITSLogger.Log(userConnection, "Error insert", "", "Inseert TargetId: " + target.TargetID.ToString() + "\nErrorMessage: " + e.Message, "UsrDecryptionPostRequest.InsertData.InsUpdTarget");
            //         return new Response()
            //         {
            //             IsSuccess = false,
            //             ErrorMessage = "InsUpdTarget: " + e.Message
            //         };
            //     }
            // }
            // else
            // {
            //     TargetEntity.SetColumnValue("UsrTargetID", target.TargetID);
            //     TargetEntity.SetColumnValue("UsrSalesforceId", target.BrandId);
            //     TargetEntity.SetColumnValue("UsrAmount", target.Amount);

            //     try
            //     {
            //         TargetEntity.Save(false);
            //         Terrasoft.Configuration.ITSLogger.Log(userConnection, "", "Success update", "Inseert TargetId: " + target.TargetID.ToString(), "UsrDecryptionPostRequest.InsertData.InsUpdTarget");
            //         return new Response()
            //         {
            //             IsSuccess = true,
            //             Result = "Target successfuly updated"
            //         };
            //     }
            //     catch (System.Exception e)
            //     {
            //     	Terrasoft.Configuration.ITSLogger.Log(userConnection, "Error update", "", "Inseert TargetId: " + target.TargetID.ToString() + "\nErrorMessage: " + e.Message, "UsrDecryptionPostRequest.InsertData.InsUpdTarget");
            //         return new Response()
            //         {
            //             IsSuccess = false,
            //             ErrorMessage = "InsUpdTarget: " + e.Message
            //         };
            //     }
            // }
        }

        //Update/insert Contract
        private Response InsUpdContract(Contract contract)
        {
            // var ContractEntity = userConnection.EntitySchemaManager.GetInstanceByName("UsrContracts").CreateEntity(userConnection);
            Guid AccountId = GetLookupBpmIdByString("Account", "UsrIDPOS", contract.TouchPointID, "Id");
            if (AccountId == Guid.Empty)
            {
                return new Response()
                {
                    IsSuccess = false,
                    ErrorMessage = $"Account with ID POS {contract.TouchPointID} not found!"
                };
            }
            
            var entitySchema = new EntitySchemaQuery(userConnection.EntitySchemaManager, "UsrContracts");
			
			entitySchema.AddColumn("Id");
            entitySchema.AddColumn("UsrTouchPointId");
            entitySchema.AddColumn("UsrValue");
            
			entitySchema.Filters.Add(entitySchema.CreateFilterWithParameters(FilterComparisonType.Equal, "Id", contract.Id));
			
			var contractCollection = entitySchema.GetEntityCollection(userConnection);
			if(contractCollection.Count() > 0){
				var ContractEntity = contractCollection[0];
				
				ContractEntity.SetColumnValue("UsrTouchPointId", AccountId);
                ContractEntity.SetColumnValue("UsrValue", contract.Value);

                try
                {
                    ContractEntity.Save(false);
                    return new Response()
                    {
                        IsSuccess = true,
                        Result = "Contract successfuly updated"
                    };
                }
                catch (System.Exception e)
                {
                    return new Response()
                    {
                        IsSuccess = false,
                        ErrorMessage = "InsUpdContract: " + e.Message
                    };
                }
			}
			else{
				var ContractEntity = userConnection.EntitySchemaManager.GetInstanceByName("UsrContracts").CreateEntity(userConnection);
				
				ContractEntity.SetDefColumnValues();
                ContractEntity.SetColumnValue("Id", contract.Id);
                ContractEntity.SetColumnValue("UsrTouchPointId", AccountId);
                ContractEntity.SetColumnValue("UsrValue", contract.Value);

                try
                {
                    ContractEntity.Save(false);
                    return new Response()
                    {
                        IsSuccess = true,
                        Result = "Contract successfuly inserted: " + contract.Id
                    };
                }
                catch (System.Exception e)
                {
                    return new Response()
                    {
                        IsSuccess = false,
                        ErrorMessage = "InsUpdContract: " + e.Message
                    };
                }
			}
        }

        //Update/insert touchpoint
        private Response InsUpdTouchpoint(Touchpoint touchpoint)
        {
            
            Response response = new Response();
            Guid RegionId = new Guid();
            Guid CityId = new Guid();
            Guid DisrictId = new Guid();
            Guid AccountCategoryId = new Guid();
            Guid SettlementId = new Guid();

            response = GetRegionId(touchpoint.TouchPointOblast, ref RegionId);
            if (!response.IsSuccess)
            {
                return response;
            }

            response = GetCityId(touchpoint.TouchPointCity, RegionId, ref CityId);
            if (!response.IsSuccess)
            {
                return response;
            }

            response = GetDistrictId(touchpoint.CustomerDisrict, ref DisrictId);
            if (!response.IsSuccess)
            {
                return response;
            }

            response = GetAccountCategoryId(touchpoint.CustomerTradeCategory, ref AccountCategoryId);
            if (!response.IsSuccess)
            {
                return response;
            }

            response = GetSettlementId(touchpoint.Settlement, ref SettlementId);
            if (!response.IsSuccess)
            {
                return response;
            }
			string ValidCode = "UA" + touchpoint.TouchPointCode;
			
			var entitySchema = new EntitySchemaQuery(userConnection.EntitySchemaManager, "Account");
			
			entitySchema.AddColumn("Id");
			entitySchema.AddColumn("Code");
			entitySchema.AddColumn("UsrRRPActivated");
            entitySchema.AddColumn("UsrRRPCity");
            entitySchema.AddColumn("UsrArea");
            entitySchema.AddColumn("UsrRegion");
            entitySchema.AddColumn("UsrSPV");
            entitySchema.AddColumn("UsrBusinessBuilder");
            entitySchema.AddColumn("UsrStrata");
            entitySchema.AddColumn("AccountCategory");
            entitySchema.AddColumn("Name");
            entitySchema.AddColumn("UsrParent");
            entitySchema.AddColumn("UsrGroupParent");
            entitySchema.AddColumn("UsrPOSCoverage");
            entitySchema.AddColumn("UsrPOSTSECoverage");
            entitySchema.AddColumn("UsrDTEPOSStatus");
            entitySchema.AddColumn("UsrDTECompliance");
            entitySchema.AddColumn("UsrVideoRAPActivated");
            entitySchema.AddColumn("UsrRetailIndustrySalesPpw");
            entitySchema.AddColumn("UsrPmCcRrpSalesPpw");
            entitySchema.AddColumn("UsrContractTypeCode");
            entitySchema.AddColumn("UsrContractActualStartDate");
            entitySchema.AddColumn("UsrContractMaxValue");
            
			entitySchema.Filters.Add(entitySchema.CreateFilterWithParameters(FilterComparisonType.Equal, "Code", ValidCode));
			
			var accountCollection = entitySchema.GetEntityCollection(userConnection);
			if(accountCollection.Count() > 0){
				var AccountEntity = accountCollection[0];
				
				var AccountId = (Guid)AccountEntity.GetColumnValue("Id1");
                AccountEntity.SetColumnValue("Code", ValidCode);
                AccountEntity.SetColumnValue("UsrRRPActivated", touchpoint.RRP_Activated);
                AccountEntity.SetColumnValue("UsrRRPCity", touchpoint.Matched_City);
                AccountEntity.SetColumnValue("UsrArea", touchpoint.CustomerArea);
                AccountEntity.SetColumnValue("UsrRegion", touchpoint.TouchPointRegion);
                AccountEntity.SetColumnValue("UsrSPV", touchpoint.SPV);
                AccountEntity.SetColumnValue("UsrBusinessBuilder", touchpoint.TouchPointTerritory);
                AccountEntity.SetColumnValue("UsrStrata", touchpoint.CustomerStrata);
                AccountEntity.SetColumnValue("AccountCategoryId", AccountCategoryId);
                AccountEntity.SetColumnValue("Name", touchpoint.TouchPointName);
                AccountEntity.SetColumnValue("UsrParent", touchpoint.Parent);
                AccountEntity.SetColumnValue("UsrGroupParent", touchpoint.GroupParent);
                AccountEntity.SetColumnValue("UsrPOSCoverage", touchpoint.Customer_in_Coverage);
                AccountEntity.SetColumnValue("UsrPOSTSECoverage", touchpoint.TSE_RegularCoverage);
                AccountEntity.SetColumnValue("UsrDTEPOSStatus", touchpoint.DTE_POS_Status);
                AccountEntity.SetColumnValue("UsrDTECompliance", touchpoint.DTE_Compliance_Last_4_weeks);
                AccountEntity.SetColumnValue("UsrVideoRAPActivated", touchpoint.VideoRAP_Activated);
                AccountEntity.SetColumnValue("UsrRetailIndustrySalesPpw", touchpoint.RetailIndustrySalesPPW);
                AccountEntity.SetColumnValue("UsrPmCcRrpSalesPpw", touchpoint.PMCCandRRPSalesPPW);
                AccountEntity.SetColumnValue("UsrContractTypeCode", touchpoint.AgreementLevel);
                AccountEntity.SetColumnValue("UsrContractActualStartDate", touchpoint.Contract_Actual_StartDate);
                AccountEntity.SetColumnValue("UsrContractMaxValue", touchpoint.ContractMaxValue);

                try
                {
                    AccountEntity.Save(false);
                }
                catch (System.Exception e)
                {
                    return new Response()
                    {
                        IsSuccess = false,
                        ErrorMessage = "InsUpdTouchpoint: " + e.Message
                    };
                }

                response = InsertIntoAccountAddresses(AccountId, RegionId, CityId, touchpoint.TouchPointStreet, DisrictId, SettlementId);
                if (!response.IsSuccess)
                {
                    return response;
                }

                return new Response()
                {
                    IsSuccess = true,
                    Result = $"Account succesfully updated: {AccountId}"
                };
			}
			else{
				var AccountEntity = userConnection.EntitySchemaManager.GetInstanceByName("Account").CreateEntity(userConnection);
				
				var AccountId = Guid.NewGuid();
                AccountEntity.SetDefColumnValues();
                AccountEntity.SetColumnValue("Id", AccountId);
                AccountEntity.SetColumnValue("Code", ValidCode);
                AccountEntity.SetColumnValue("UsrRRPActivated", touchpoint.RRP_Activated);
                AccountEntity.SetColumnValue("UsrRRPCity", touchpoint.Matched_City);
                AccountEntity.SetColumnValue("UsrArea", touchpoint.CustomerArea);
                AccountEntity.SetColumnValue("UsrRegion", touchpoint.TouchPointRegion);
                AccountEntity.SetColumnValue("UsrSPV", touchpoint.SPV);
                AccountEntity.SetColumnValue("UsrBusinessBuilder", touchpoint.TouchPointTerritory);
                AccountEntity.SetColumnValue("UsrStrata", touchpoint.CustomerStrata);
                AccountEntity.SetColumnValue("AccountCategoryId", AccountCategoryId);
                AccountEntity.SetColumnValue("Name", touchpoint.TouchPointName);
                AccountEntity.SetColumnValue("UsrParent", touchpoint.Parent);
                AccountEntity.SetColumnValue("UsrGroupParent", touchpoint.GroupParent);
                AccountEntity.SetColumnValue("UsrPOSCoverage", touchpoint.Customer_in_Coverage);
                AccountEntity.SetColumnValue("UsrPOSTSECoverage", touchpoint.TSE_RegularCoverage);
                AccountEntity.SetColumnValue("UsrDTEPOSStatus", touchpoint.DTE_POS_Status);
                AccountEntity.SetColumnValue("UsrDTECompliance", touchpoint.DTE_Compliance_Last_4_weeks);
                AccountEntity.SetColumnValue("UsrVideoRAPActivated", touchpoint.VideoRAP_Activated);
                AccountEntity.SetColumnValue("UsrRetailIndustrySalesPpw", touchpoint.RetailIndustrySalesPPW);
                AccountEntity.SetColumnValue("UsrPmCcRrpSalesPpw", touchpoint.PMCCandRRPSalesPPW);
                AccountEntity.SetColumnValue("UsrContractTypeCode", touchpoint.AgreementLevel);
                AccountEntity.SetColumnValue("UsrContractActualStartDate", touchpoint.Contract_Actual_StartDate);
                AccountEntity.SetColumnValue("UsrContractMaxValue", touchpoint.ContractMaxValue);

                try
                {
                    AccountEntity.Save(false);
                }
                catch (System.Exception e)
                {
                    return new Response()
                    {
                        IsSuccess = false,
                        ErrorMessage = "InsUpdTouchpoint: " + e.Message
                    };
                }

                response = InsertIntoAccountAddresses(AccountId, RegionId, CityId, touchpoint.TouchPointStreet, DisrictId, SettlementId);
                if (!response.IsSuccess)
                {
                    return response;
                }

                return new Response()
                {
                    IsSuccess = true,
                    Result = $"Account succesfully insereted: {AccountId}"
                };
			}
        }

        private Response InsertIntoAccountAddresses(Guid accountId, Guid RegionId, Guid CityId, string Address, Guid DistrictId, Guid SettlementId)
        {
            
            var CountryId = GetLookupBpmIdByString("Country", "Name", "Ukraine", "Id");
            var AddressTypeId = GetLookupBpmIdByString("AddressType", "Name", "Actual", "Id");
            Dictionary<string, object> Conditions = new Dictionary<string, object>
            {
                { "Account", accountId },
                { "Country", CountryId },
                { "Region", RegionId },
                { "City", CityId },
                { "Address", Address },
                { "UsrDistrict", DistrictId },
                { "UsrSettlement", SettlementId }
            };
            
            var entitySchema = new EntitySchemaQuery(userConnection.EntitySchemaManager, "AccountAddress");
			
			entitySchema.AddColumn("Account");
            entitySchema.AddColumn("Country");
            entitySchema.AddColumn("Region");
            entitySchema.AddColumn("City");
            entitySchema.AddColumn("Address");
            entitySchema.AddColumn("UsrDistrict");
            entitySchema.AddColumn("UsrSettlement");
            
			entitySchema.Filters.Add(entitySchema.CreateFilterWithParameters(FilterComparisonType.Equal, "Account", accountId));
			entitySchema.Filters.Add(entitySchema.CreateFilterWithParameters(FilterComparisonType.Equal, "Country", CountryId));
			entitySchema.Filters.Add(entitySchema.CreateFilterWithParameters(FilterComparisonType.Equal, "Region", RegionId));
			entitySchema.Filters.Add(entitySchema.CreateFilterWithParameters(FilterComparisonType.Equal, "City", CityId));
			entitySchema.Filters.Add(entitySchema.CreateFilterWithParameters(FilterComparisonType.Equal, "Address", Address));
			entitySchema.Filters.Add(entitySchema.CreateFilterWithParameters(FilterComparisonType.Equal, "UsrDistrict", DistrictId));
			entitySchema.Filters.Add(entitySchema.CreateFilterWithParameters(FilterComparisonType.Equal, "UsrSettlement", SettlementId));
			
			var accountAddressCollection = entitySchema.GetEntityCollection(userConnection);
			if(accountAddressCollection.Count() == 0){
				var AccountAddressEntity = userConnection.EntitySchemaManager.GetInstanceByName("AccountAddress").CreateEntity(userConnection);
				
				var AccountAddressId = Guid.NewGuid();
                AccountAddressEntity.SetDefColumnValues();
                AccountAddressEntity.SetColumnValue("Id", AccountAddressId);
                AccountAddressEntity.SetColumnValue("AddressTypeId", AddressTypeId);
                AccountAddressEntity.SetColumnValue("CountryId", CountryId);
                AccountAddressEntity.SetColumnValue("RegionId", RegionId);
                AccountAddressEntity.SetColumnValue("CityId", CityId);
                AccountAddressEntity.SetColumnValue("Address", Address);
                AccountAddressEntity.SetColumnValue("UsrDistrictId", DistrictId);
                AccountAddressEntity.SetColumnValue("UsrSettlementId", SettlementId);
                AccountAddressEntity.SetColumnValue("AccountId", accountId);
                try
                {
                    AccountAddressEntity.Save(false);
                    return new Response()
                    {
                        IsSuccess = true,
                        Result = AccountAddressId
                    };
                }
                catch(System.Exception e)
                {
                    return new Response()
                    {
                        IsSuccess = false,
                        Result = $"CountryId = {CountryId}; ",
                        ErrorMessage = "InsertIntoAccountAddresses: " + e.Message
                    };
                }
			}
            else
            {
                return new Response() {
                    IsSuccess = true,
                    Result = "Account Address alredy exist"
                };
            }
        }

        private Response GetSettlementId(string SettlementName, ref Guid settlementId)
        {
        	
        	var entitySchema = new EntitySchemaQuery(userConnection.EntitySchemaManager, "UsrSettlement");
			
			entitySchema.AddColumn("Id");
            entitySchema.AddColumn("Name");
            
			entitySchema.Filters.Add(entitySchema.CreateFilterWithParameters(FilterComparisonType.Equal, "Name", SettlementName));
			
			var settlementCollection = entitySchema.GetEntityCollection(userConnection);
			if(settlementCollection.Count() > 0){
				var SettlementEntity = settlementCollection[0];
        		var SettlementId = (Guid)SettlementEntity.GetColumnValue("Id1");
                settlementId = SettlementId;
                return new Response()
                {
                    IsSuccess = true,
                    Result = SettlementEntity.GetColumnValue("Id1")
                };
			}
			else{
				var SettlementEntity = userConnection.EntitySchemaManager.GetInstanceByName("UsrSettlement").CreateEntity(userConnection);
				
				var SettlementId = Guid.NewGuid();
                SettlementEntity.SetDefColumnValues();
                SettlementEntity.SetColumnValue("Id", SettlementId);
                SettlementEntity.SetColumnValue("Name", SettlementName);

                try
                {
                    SettlementEntity.Save(false);
                    settlementId = SettlementId;
                    return new Response()
                    {
                        IsSuccess = true,
                        Result = SettlementId
                    };
                }
                catch (System.Exception e)
                {
                    return new Response()
                    {
                        IsSuccess = false,
                        ErrorMessage = "GetSettlementId: " + e.Message
                    };
                }
			}

            return new Response()
            {
                IsSuccess = false,
                ErrorMessage = "GetSettlementId: Something went wrong"
            };
        }

        private Response GetAccountCategoryId(string AccountCategoryName, ref Guid accountCategoryId)
        {
        	
        	var entitySchema = new EntitySchemaQuery(userConnection.EntitySchemaManager, "AccountCategory");
			
			entitySchema.AddColumn("Id");
            entitySchema.AddColumn("Name");
            
			entitySchema.Filters.Add(entitySchema.CreateFilterWithParameters(FilterComparisonType.Equal, "Name", AccountCategoryName));
			
			var accountCategoryCollection = entitySchema.GetEntityCollection(userConnection);
			if(accountCategoryCollection.Count() > 0){
				var AccountCategoryEntity = accountCategoryCollection[0];
        		var AccountCategoryId = (Guid)AccountCategoryEntity.GetColumnValue("Id1");
                accountCategoryId = AccountCategoryId;
                return new Response()
                {
                    IsSuccess = true,
                    Result = AccountCategoryEntity.GetColumnValue("Id1")
                };
			}
			else{
				var AccountCategoryEntity = userConnection.EntitySchemaManager.GetInstanceByName("AccountCategory").CreateEntity(userConnection);
				var AccountCategoryId = Guid.NewGuid();
                AccountCategoryEntity.SetDefColumnValues();
                AccountCategoryEntity.SetColumnValue("Id", AccountCategoryId);
                AccountCategoryEntity.SetColumnValue("Name", AccountCategoryName);

                try
                {
                    AccountCategoryEntity.Save(false);
                    accountCategoryId = AccountCategoryId;
                    return new Response()
                    {
                        IsSuccess = true,
                        Result = AccountCategoryId
                    };
                }
                catch (System.Exception e)
                {
                    return new Response()
                    {
                        IsSuccess = false,
                        ErrorMessage = "GetAccountCategoryId: " + e.Message
                    };
                }
			}

            return new Response()
            {
                IsSuccess = false,
                ErrorMessage = "GetAccountCategoryId: Something went wrong"
            };
        }

        private Response GetDistrictId(string DistrictName, ref Guid districtId)
        {
        	var entitySchema = new EntitySchemaQuery(userConnection.EntitySchemaManager, "UsrDistrict");
			
			entitySchema.AddColumn("Id");
            entitySchema.AddColumn("Name");
            
			entitySchema.Filters.Add(entitySchema.CreateFilterWithParameters(FilterComparisonType.Equal, "Name", DistrictName));
			
			var DostrictCollection = entitySchema.GetEntityCollection(userConnection);
			if(DostrictCollection.Count() > 0){
				var DistrictEntity = DostrictCollection[0];
        		var DistrictId = (Guid)DistrictEntity.GetColumnValue("Id1");
                districtId = DistrictId;
                return new Response()
                {
                    IsSuccess = true,
                    Result = DistrictEntity.GetColumnValue("Id1")
                };
			}
			else{
            	var DistrictEntity = userConnection.EntitySchemaManager.GetInstanceByName("UsrDistrict").CreateEntity(userConnection);
                var DistrictId = Guid.NewGuid();
                DistrictEntity.SetDefColumnValues();
                DistrictEntity.SetColumnValue("Id", DistrictId);
                DistrictEntity.SetColumnValue("Name", DistrictName);

                try
                {
                    DistrictEntity.Save(false);
                    districtId = DistrictId;
                    return new Response()
                    {
                        IsSuccess = true,
                        Result = DistrictId
                    };
                }
                catch (System.Exception e)
                {
                    return new Response()
                    {
                        IsSuccess = false,
                        ErrorMessage = "GetDistrictId: " + e.Message
                    };
                }
			}

            return new Response()
            {
                IsSuccess = false,
                ErrorMessage = "GetDistrictId: Something went wrong"
            };
        }

        private Response GetRegionId(string RegionName, ref Guid regionId)
        {
        	var entitySchema = new EntitySchemaQuery(userConnection.EntitySchemaManager, "Region");
			
			entitySchema.AddColumn("Id");
            entitySchema.AddColumn("Name");
            
			entitySchema.Filters.Add(entitySchema.CreateFilterWithParameters(FilterComparisonType.Equal, "Name", RegionName));
			
			var RegionCollection = entitySchema.GetEntityCollection(userConnection);
			if(RegionCollection.Count() > 0){
				var RegionEntity = RegionCollection[0];
				var RegionId = (Guid)RegionEntity.GetColumnValue("Id1");
                regionId = RegionId;
                return new Response()
                {
                    IsSuccess = true,
                    Result = RegionEntity.GetColumnValue("Id1")
                };
			}
			else{
				var RegionEntity = userConnection.EntitySchemaManager.GetInstanceByName("Region").CreateEntity(userConnection);
            	var CountryId = GetLookupBpmIdByString("Country", "Name", "Ukraine", "Id");
            	var TimeZoneId = GetLookupBpmIdByString("Country", "Name", "Ukraine", "TimeZoneId");
				
				var RegionId = Guid.NewGuid();
                RegionEntity.SetDefColumnValues();
                RegionEntity.SetColumnValue("Id", RegionId);
                RegionEntity.SetColumnValue("Name", RegionName);
                RegionEntity.SetColumnValue("CountryId", CountryId);
                RegionEntity.SetColumnValue("TimeZoneId", TimeZoneId);

                try
                {
                    RegionEntity.Save(false);
                    regionId = RegionId;
                    return new Response()
                    {
                        IsSuccess = true,
                        Result = RegionId
                    };
                }
                catch (System.Exception e)
                {
                    return new Response()
                    {
                        IsSuccess = false,
                        ErrorMessage = "GetRegionId: " + e.Message
                    };
                }
			}
        	

            return new Response()
            {
                IsSuccess = false,
                ErrorMessage = "GetRegionId: Something went wrong"
            };
        }

        /// <summary>
        /// Get CityId if city exist. Else create new City. Return CityId
        /// </summary>
        /// <param name="CityName"></param>
        /// <param name="RegionId"></param>
        /// <returns>CityId</returns>
        private Response GetCityId(string CityName, Guid RegionId, ref Guid cityId)
        {
        	var entitySchema = new EntitySchemaQuery(userConnection.EntitySchemaManager, "City");
			
			entitySchema.AddColumn("Id");
            entitySchema.AddColumn("Name");
            
			entitySchema.Filters.Add(entitySchema.CreateFilterWithParameters(FilterComparisonType.Equal, "Name", CityName));
			
			var CityCollection = entitySchema.GetEntityCollection(userConnection);
			if(CityCollection.Count() > 0)
			{
				var CityEntity = CityCollection[0];
				var CityId = (Guid)CityEntity.GetColumnValue("Id1");
                cityId = CityId;
                return new Response()
                {
                    IsSuccess = true,
                    Result = CityId
                };
			}
			else
			{
				var CityEntity = userConnection.EntitySchemaManager.GetInstanceByName("City").CreateEntity(userConnection);
	            var CountryId = GetLookupBpmIdByString("Country", "Name", "Ukraine", "Id");
	            var TimeZoneId = GetLookupBpmIdByString("Country", "Name", "Ukraine", "TimeZoneId");
	            
	            var CityId = Guid.NewGuid();
                CityEntity.SetDefColumnValues();
                CityEntity.SetColumnValue("Id", CityId);
                CityEntity.SetColumnValue("Name", CityName);
                CityEntity.SetColumnValue("CountryId", CountryId);
                CityEntity.SetColumnValue("RegionId", RegionId);
                CityEntity.SetColumnValue("TimeZoneId", TimeZoneId);

                try
                {
                    CityEntity.Save(false);
                    cityId = CityId;
                    return new Response()
                    {
                        IsSuccess = true,
                        Result = CityId
                    };
                }
                catch (System.Exception e)
                {
                    return new Response()
                    {
                        IsSuccess = false,
                        ErrorMessage = "GetCityId: " + e.Message
                    };
                }

			}

            return new Response()
            {
                IsSuccess = false,
                ErrorMessage = "GetCityId: Something went wrong"
            };
        }

        private Root DeserializeJson(string json)
        {
            //var test = json.Split(new string[] { "]\r\n}" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x + "]\r\n}").ToArray();
            Root root = new Root();
            StringRoot strRoot = new StringRoot();
            strRoot = JsonConvert.DeserializeObject<StringRoot>(json);
            if(!string.IsNullOrEmpty(strRoot.Brands))
            	root.Brands = JsonConvert.DeserializeObject<List<Brand>>(strRoot.Brands);
        	if(!string.IsNullOrEmpty(strRoot.Targets))
            	root.Targets = JsonConvert.DeserializeObject<List<Target>>(strRoot.Targets);
            if(!string.IsNullOrEmpty(strRoot.Contracts))
            	root.Contracts = JsonConvert.DeserializeObject<List<Contract>>(strRoot.Contracts);
            if(!string.IsNullOrEmpty(strRoot.Touchpoints))
            	root.Touchpoints = JsonConvert.DeserializeObject<List<Touchpoint>>(strRoot.Touchpoints);
            // var obj = test.SingleOrDefault(x => x.Contains("Brands"));
            // if(!string.IsNullOrEmpty(obj))
            //     root.Brands = JsonConvert.DeserializeObject<Root>(obj).Brands;
            // obj = test.SingleOrDefault(x => x.Contains("Targets"));
            // if (!string.IsNullOrEmpty(obj))
            //     root.Targets = JsonConvert.DeserializeObject<Root>(obj).Targets;
            // obj = test.SingleOrDefault(x => x.Contains("Contracts"));
            // if (!string.IsNullOrEmpty(obj))
            //     root.Contracts = JsonConvert.DeserializeObject<Root>(obj).Contracts;
            // obj = test.SingleOrDefault(x => x.Contains("Touchpoints"));
            // if (!string.IsNullOrEmpty(obj))
            //     root.Touchpoints = JsonConvert.DeserializeObject<Root>(obj).Touchpoints;
            return root;
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

        public class Response
        {
            public string ErrorMessage { get; set; }
            public bool IsSuccess { get; set; }
            public object Result { get; set; }
        }

        #region public classes

        public class EncryptedData
        {
            [JsonProperty("Data")]
            public string Data { get; set; }
        }

        public class RootObject
        {
            [JsonProperty("uploadData")]
            public EncryptedData uploadData { get; set; }
        }

		public class StringRoot
		{
			[JsonProperty("Brands")]
            public string Brands { get; set; }

            [JsonProperty("Targets")]
            public string Targets { get; set; }

            [JsonProperty("Contracts")]
            public string Contracts { get; set; }

            [JsonProperty("Touchpoints")]
            public string Touchpoints { get; set; }
		}

        public class Root
        {
			public Root()
            {
                Brands = new List<Brand>();
                Targets = new List<Target>();
                Contracts = new List<Contract>();
                Touchpoints = new List<Touchpoint>();
            }
            [JsonProperty("Brands")]
            public List<Brand> Brands { get; set; }

            [JsonProperty("Targets")]
            public List<Target> Targets { get; set; }

            [JsonProperty("Contracts")]
            public List<Contract> Contracts { get; set; }

            [JsonProperty("Touchpoints")]
            public List<Touchpoint> Touchpoints { get; set; }
        }

        public class Brand
        {
            [JsonProperty("BrandId")]
            public string BrandId { get; set; }

            [JsonProperty("ShortBrand")]
            public string ShortBrand { get; set; }

            [JsonProperty("LongBrand")]
            public string LongBrand { get; set; }

            [JsonProperty("Sort")]
            public string Sort { get; set; }
        }

        public class Target
        {
            [JsonProperty("TargetID")]
            public string TargetID { get; set; }

            [JsonProperty("CustomerCode")]
            public string CustomerCode { get; set; }

            [JsonProperty("BrandId")]
            public string BrandId { get; set; }

            [JsonProperty("Amount")]
            public string Amount { get; set; }
        }

        public class Contract
        {
            [JsonProperty("Id")]
            public string Id { get; set; }

            [JsonProperty("TouchPointID")]
            public string TouchPointID { get; set; }

            [JsonProperty("Value")]
            public string Value { get; set; }
        }

        public class Touchpoint
        {
            [JsonProperty("TouchPointCode")]
            public string TouchPointCode { get; set; }
            [JsonProperty("RRP_Activated")]
            public string RRP_Activated { get; set; }
            [JsonProperty("Matched_City")]
            public string Matched_City { get; set; }
            [JsonProperty("CustomerArea")]
            public string CustomerArea { get; set; }
            [JsonProperty("TouchPointRegion")]
            public string TouchPointRegion { get; set; }
            [JsonProperty("SPV")]
            public string SPV { get; set; }
            [JsonProperty("TouchPointTerritory")]
            public string TouchPointTerritory { get; set; }
            [JsonProperty("TouchPointOblast")]
            public string TouchPointOblast { get; set; }
            [JsonProperty("CustomerDisrict")]
            public string CustomerDisrict { get; set; }
            [JsonProperty("Settlement")]
            public string Settlement { get; set; }
            [JsonProperty("CustomerStrata")]
            public string CustomerStrata { get; set; }
            [JsonProperty("TouchPointStreet")]
            public string TouchPointStreet { get; set; }
            [JsonProperty("CustomerTradeCategory")]
            public string CustomerTradeCategory { get; set; }
            [JsonProperty("TouchPointName")]
            public string TouchPointName { get; set; }
            [JsonProperty("Parent")]
            public string Parent { get; set; }
            [JsonProperty("GroupParent")]
            public string GroupParent { get; set; }
            [JsonProperty("Customer_in_Coverage")]
            public string Customer_in_Coverage { get; set; }
            [JsonProperty("TSE_RegularCoverage")]
            public string TSE_RegularCoverage { get; set; }
            [JsonProperty("DTE_POS_Status")]
            public string DTE_POS_Status { get; set; }
            [JsonProperty("DTE_Compliance_Last_4_weeks")]
            public string DTE_Compliance_Last_4_weeks { get; set; }
            [JsonProperty("VideoRAP_Activated")]
            public string VideoRAP_Activated { get; set; }
            [JsonProperty("Retail Industry Sales, ppw")]
            public string RetailIndustrySalesPPW { get; set; }
            [JsonProperty("PM CC&RRP Sales, ppw")]
            public string PMCCandRRPSalesPPW { get; set; }
            [JsonProperty("AgreementLevel")]
            public string AgreementLevel { get; set; }
            [JsonProperty("Contract_Actual_StartDate")]
            public string Contract_Actual_StartDate { get; set; }
            [JsonProperty("ContractMaxValue")]
            public string ContractMaxValue { get; set; }
            [JsonProperty("TouchPointCity")]
            public string TouchPointCity { get; set; }
        }
        #endregion
    }
}