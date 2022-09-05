namespace Terrasoft.Configuration
{
	using System;
    using Terrasoft.Core;
	using Terrasoft.Core.DB;
	using Terrasoft.Core.Entities;
	using System.Threading;
    using System.Threading.Tasks;
    
	public class UsrITSSalesforceLogger
	{
		public static bool Log(UserConnection userConnection, string errors, string success, string body, string methodAndServiceName)
		{
			var ins = new Insert(userConnection).Into("UsrITS_SalesforceLogger")
							.Set("UsrITSSalesforceErrors", Column.Parameter(errors))
							.Set("UsrITSSalesforceRequestTime", Column.Parameter(DateTime.Now))
							.Set("UsrITSSalesforceSuccess", Column.Parameter(success))
							.Set("UsrITSSalesforceServiceDetails", Column.Parameter(methodAndServiceName))
							.Set("UsrITSSalesforceRequestBody", Column.Parameter(body));
			try
			{
				UsrITSSalesforceLogger lg = new UsrITSSalesforceLogger();
				var result = lg.CompleteAsync(ins);
				return true;
			}
			catch(Exception)
			{
				return false;
			}
		}
		public async System.Threading.Tasks.Task CompleteAsync(Insert insAsync)
		{
		    await Task.Factory.StartNew(()=> insAsync.Execute());
		}
	}
}