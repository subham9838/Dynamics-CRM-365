using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using System.ServiceModel;
using System.IdentityModel.Tokens;
using Microsoft.Xrm.Sdk.Query;

namespace Dynamics_CRM_365
{
    public class PreValidation : IPlugin
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

                // Obtain the IOrganizationService instance which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    if(context.MessageName.ToLower() != "create" && context.Stage != 10)
                    {
                        return;
                    }

                    string autoNumber = string.Empty, prefix = string.Empty, suffix = string.Empty, seperator = string.Empty,
                        currentNumber = string.Empty, year = string.Empty, month = string.Empty, day = string.Empty;
                    DateTime today = DateTime.Now;
                    year = today.Year.ToString();
                    month = today.Month.ToString("00");
                    day = today.Day.ToString("00");

                    QueryExpression qeAutoNumberConfig = new QueryExpression()
                    {
                        EntityName = "crb9e_autonumberconfiguration",
                        ColumnSet = new ColumnSet("crb9e_prefix", "crb9e_suffix", "crb9e_seperator", "crb9e_currentnumber", "crb9e_name")
                    };

                    qeAutoNumberConfig.Criteria.AddCondition("crb9e_name", ConditionOperator.Equal, "ApplicationAutoNumber");

                    EntityCollection ecAutoNumberConfig = service.RetrieveMultiple(qeAutoNumberConfig);
                    if(ecAutoNumberConfig.Entities.Count > 0)
                    {
                        Entity applicationAutoNumber = ecAutoNumberConfig.Entities[0];
                        prefix = (string)applicationAutoNumber["crb9e_prefix"];
                        suffix = (string)applicationAutoNumber["crb9e_suffix"];
                        seperator = (string)applicationAutoNumber["crb9e_seperator"];
                        currentNumber = (string)applicationAutoNumber["crb9e_currentnumber"];
                        int tempCurrentNumber = int.Parse(currentNumber);
                        tempCurrentNumber++;
                        currentNumber = tempCurrentNumber.ToString("000000");
                        autoNumber = prefix + seperator + year + month + day + seperator + suffix + seperator + currentNumber;

                        QueryExpression qeApplication = new QueryExpression()
                        {
                            EntityName = "crb9e_application",
                            ColumnSet = new ColumnSet("crb9e_applicationnumber")
                        };

                        qeApplication.Criteria.AddCondition("crb9e_applicationnumber", ConditionOperator.Equal, autoNumber);
                        EntityCollection ecApplication = service.RetrieveMultiple(qeApplication);

                        if( ecApplication.Entities.Count > 0 )
                        {
                            throw new Exception("Duplicate application found with application Id: " + autoNumber);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException(ex.Message);
                }
            }
        }
    }
}
