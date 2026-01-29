using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace PluginsTasks
{
    
  public class AccountUpdate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracing =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory factory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service = factory.CreateOrganizationService(context.UserId);

            try
            {
                if (!context.InputParameters.Contains("Target") ||
                    !(context.InputParameters["Target"] is Entity))
                    return;

                // Get updated account entity
                Entity account = (Entity)context.InputParameters["Target"];

                // Only proceed if address changed
                if (!account.Contains("address1_line1"))
                    return;

                // Query contacts linked to this account
                QueryExpression query = new QueryExpression("contact");
                query.ColumnSet = new ColumnSet("address1_line1");

                query.Criteria.AddCondition("parentcustomerid",
                    ConditionOperator.Equal, account.Id);

                EntityCollection contacts = service.RetrieveMultiple(query);

                // Loop through contacts
                foreach (Entity contact in contacts.Entities)
                {
                    // Copy account address into contact
                    contact["address1_line1"] = account["address1_line1"];

                    // Update contact
                    service.Update(contact);
                }

                tracing.Trace("Contacts updated successfully.");
            }
            catch (Exception ex)
            {
                tracing.Trace("AccountUpdatePlugin Error: {0}", ex.ToString());
                throw new InvalidPluginExecutionException("Update failed: " + ex.Message);
            }
        }
    }


}

