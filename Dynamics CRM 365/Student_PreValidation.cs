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
    public class Student_PreValidation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the tracing service
            ITracingService tracingService =
            (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Obtain the IOrganizationService instance which you will need for  
            // web service calls.  
            IOrganizationServiceFactory serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                if(context.MessageName.ToLower() != "delete" && context.Stage != 10)
                {
                    return;
                }

                EntityReference erStudent = (EntityReference)context.InputParameters["Target"];
                QueryExpression qeCourse = new QueryExpression()
                {
                    EntityName = "crb9e_course",
                    ColumnSet = new ColumnSet("crb9e_name")
                };

                qeCourse.Criteria.AddCondition("crb9e_studentenrolled", ConditionOperator.Equal, erStudent.Id);

                EntityCollection ecCourse = service.RetrieveMultiple(qeCourse);

                if(ecCourse.Entities.Count > 0)
                {
                    throw new Exception("Child record of student course found. can not delete this record.");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
    }
}
