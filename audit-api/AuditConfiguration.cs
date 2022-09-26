using Audit.Core;
using Audit.WebApi;
using Microsoft.AspNetCore.Mvc;

public static class AuditConfiguration
{
    public static void AddAudit(MvcOptions mvcOptions)
    {
        mvcOptions.AddAuditFilter(config => config
            .LogAllActions()
            .WithEventType("{verb} {controller}.{action}")
            .IncludeHeaders()
            .IncludeRequestBody()
            .IncludeResponseHeaders()
        );
    }

    public static void ConfigureAudit(IServiceCollection serviceCollection)
    {
        //To Mongo
        Audit.Core.Configuration.Setup()
                                 .UseMongoDB(config => config
                                 .ConnectionString("mongodb://localhost:27017")
                                 .Database("Audit")
                                 .Collection("Logs"));

        //To Console
        //Audit.Core.Configuration.Setup()
        //      .UseDynamicAsyncProvider(config => config
        //          .OnInsert(async ev => Console.WriteLine(ev.ToJson())));

        Audit.Core.Configuration.AddCustomAction(ActionType.OnEventSaving, scope =>
        {
            var auditAction = scope.Event.GetWebApiAuditAction();
            if (auditAction == null)
            {
                return;
            }

            auditAction.Headers.Remove("Authorization");

            scope.Event.CustomFields.Add("User", new { Name = "UserName", Id = "123456" });
            scope.Event.CustomFields.Add("Application", new { Name = "AuditAPI", Node = "1" });

            if (auditAction.HttpMethod.Equals("DELETE"))
            {
                auditAction.RequestBody = null;
            }
        });
    }


}