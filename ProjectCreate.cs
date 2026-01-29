using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace PluginsTasks
{

        public class ProjectCreate : IPlugin
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
                if (context.MessageName != "Create")
                    return;

                Guid projectId = context.PrimaryEntityId;

                // Array of default task names
                string[] taskNames = { "Planning", "Execution", "Closure" };

                foreach (string name in taskNames)
                {
                    // Create task entity
                    Entity task = new Entity("task");

                    // Set subject
                    task["subject"] = name + " Task";

                    // Link task to project
                    task["regardingobjectid"] =
                        new EntityReference("new_project", projectId);

                    // Create task record
                    service.Create(task);
                }

                tracing.Trace("Default tasks created successfully.");
            }
            catch (Exception ex)
            {
                tracing.Trace("ProjectCreatePlugin Error: {0}", ex.ToString());
                throw new InvalidPluginExecutionException("Plugin error: " + ex.Message);
            }
        }
    }

}

