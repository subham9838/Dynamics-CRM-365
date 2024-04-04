using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using System.ServiceModel;
using Microsoft.Xrm.Sdk.Query;

namespace Dynamics_CRM_365
{
    public class AutoNumber : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the tracing service
            ITracingService tracingService =
            (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            if(context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity entity = (Entity)context.InputParameters["Target"];
                Entity updatedAutoNumberConfiguartion = new Entity("crb9e_autonumberconfiguration");

                // Obtain the IOrganizationService instance which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    if(context.MessageName.ToLower() != "create" && context.Stage != 20)
                    {
                        return;
                    }

                    StringBuilder autoNumber = new StringBuilder();
                    string prefix, seperator, suffix, currentNumber, year, month, day;
                    DateTime today = DateTime.Now;
                    day = today.Day.ToString("00");
                    month = today.Month.ToString("00");
                    year = today.Year.ToString();

                    QueryExpression autoNumberConfiguration = new QueryExpression()
                    {
                        EntityName = "crb9e_autonumberconfiguration",
                        ColumnSet = new ColumnSet("crb9e_prefix", "crb9e_suffix", "crb9e_seperator", "crb9e_currentnumber", "crb9e_name") 
                    };

                    EntityCollection entityCollection = service.RetrieveMultiple(autoNumberConfiguration);

                    if(entityCollection.Entities.Count == 0)
                    {
                        return;
                    }

                    foreach(Entity autoNumberEntity in entityCollection.Entities)
                    {
                        if (autoNumberEntity.Attributes["crb9e_name"].ToString().ToLower() == "applicationautonumber")
                        {
                            prefix = autoNumberEntity.GetAttributeValue<string>("crb9e_prefix");
                            suffix = autoNumberEntity.GetAttributeValue<string>("crb9e_suffix");
                            seperator = autoNumberEntity.GetAttributeValue<string>("crb9e_seperator");
                            currentNumber = autoNumberEntity.GetAttributeValue<string>("crb9e_currentnumber");
                            int tempCurrentNumber = int.Parse(currentNumber);
                            tempCurrentNumber++;
                            currentNumber = tempCurrentNumber.ToString("000000");
                            updatedAutoNumberConfiguartion.Id = autoNumberEntity.Id;
                            updatedAutoNumberConfiguartion["crb9e_currentnumber"] = currentNumber;
                            service.Update(updatedAutoNumberConfiguartion);
                            autoNumber.Append(prefix + seperator + year + month + day + seperator + suffix + seperator + currentNumber);
                            break;
                        }
                    }
                    entity["crb9e_applicationnumber"] = autoNumber.ToString();
                }

                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException("Error occured", ex);
                }
            }
        }
    }
}
