using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;
using System;

namespace PluginsTasks
{
    public class CaseCreate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
           
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service =
                serviceFactory.CreateOrganizationService(context.UserId);

            ITracingService tracing =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            
            if (!context.InputParameters.Contains("Target") ||
                !(context.InputParameters["Target"] is Entity))
                return;

            Entity caseEntity = (Entity)context.InputParameters["Target"];

            if (caseEntity.LogicalName != "incident")
                return;

            /* PRE-OPERATION : VALIDATION
               Prevent Case creation if Contact has > 3 open cases */

            if (context.Stage == 20) 
            {
                if (!caseEntity.Contains("customerid"))
                    return;

                EntityReference customerRef =
                    (EntityReference)caseEntity["customerid"];

                // Only validate for Contact
                if (customerRef.LogicalName != "contact")
                    return;

                QueryExpression query = new QueryExpression("incident");
                query.ColumnSet = new ColumnSet(false);
                query.Criteria.AddCondition(
                    "customerid", ConditionOperator.Equal, customerRef.Id);
                query.Criteria.AddCondition(
                    "statecode", ConditionOperator.Equal, 0); // Open cases

                EntityCollection openCases = service.RetrieveMultiple(query);

                if (openCases.Entities.Count >= 3)
                {
                    throw new InvalidPluginExecutionException(
                        "This contact already has more than 3 open cases. Case creation is not allowed.");
                }
            }

            /* POST-OPERATION : TASK + EMAIL */

            if (context.Stage == 40) 
            {
                // Retrieve the created Case to get CreatedOn
                Entity createdCase = service.Retrieve(
                    "incident",
                    context.PrimaryEntityId,
                    new ColumnSet("createdon", "title", "description", "customerid")
                );

                DateTime createdOn =
                    createdCase.GetAttributeValue<DateTime>("createdon");

                /*  Create Follow-up Task */
                Entity followUpTask = new Entity("task");
                followUpTask["subject"] = "Follow up on Case";
                followUpTask["scheduledend"] = createdOn.AddDays(1);
                followUpTask["regardingobjectid"] =
                    new EntityReference("incident", context.PrimaryEntityId);

                service.Create(followUpTask);

                /* Send Email to Contact */
                if (createdCase.Contains("customerid"))
                {
                    EntityReference contactRef =
                        (EntityReference)createdCase["customerid"];

                    if (contactRef.LogicalName == "contact")
                    {
                        Entity email = new Entity("email");
                        email["subject"] =
                            createdCase.GetAttributeValue<string>("title");
                        email["description"] =
                            createdCase.GetAttributeValue<string>("description");

                        email["to"] = new EntityCollection
                        {
                            Entities =
                            {
                                new Entity("activityparty")
                                {
                                    ["partyid"] = contactRef
                                }
                            }
                        };

                        Guid emailId = service.Create(email);

                        SendEmailRequest sendEmailRequest =
                            new SendEmailRequest
                            {
                                EmailId = emailId,
                                IssueSend = true
                            };

                        service.Execute(sendEmailRequest);
                    }
                }
            }
        }
    }
}
