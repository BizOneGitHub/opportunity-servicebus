using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Opportunity.Service;

namespace Opportunity.Function
{
    public class CrmOppotunityTrigger
    {
        private readonly ILogger<CrmOppotunityTrigger> _log;

        //private readonly ICustomerImport _customerImport;

        public CrmOppotunityTrigger(ILogger<CrmOppotunityTrigger> log)
        {
            _log = log;
           // _customerImport = customerImport;
        }

        [FunctionName("CrmOppotunityTrigger")]
        public async Task Run([ServiceBusTrigger("opportunity", "opp-test", Connection = "ServiceBusConnectionString", IsSessionsEnabled = false)] Message sbMessage
           , MessageReceiver messageReceiver
           // , string lockToken
           )
        {
            try
            {
                Console.WriteLine($"Process procpect message {Encoding.UTF8.GetString(sbMessage.Body)}");
                _log.LogInformation($"Process procpect message {sbMessage.Body}");
                //await _customerImport.HandleMessage(Encoding.UTF8.GetString(sbMessage.Body), sbMessage.UserProperties);
            }
            catch (System.Exception ex)
            {
                _log.LogError(ex, "Error");
                //await messageReceiver.DeadLetterAsync(lockToken);
                throw new System.Exception("Error handling prospect");
            }
        }
    }
}
