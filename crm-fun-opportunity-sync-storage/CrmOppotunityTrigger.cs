using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Crm.Service;

namespace Crm.Function
{
    public class CrmOppotunityTrigger
    {
        private readonly ILogger<CrmOppotunityTrigger> _log;

        private readonly ICrmImport _crmImport;

        public CrmOppotunityTrigger(ILogger<CrmOppotunityTrigger> log, ICrmImport crmImport)
        {
            _log = log;
            _crmImport = crmImport;
        }

        [FunctionName("CrmOppotunityTrigger")]
        public async Task Run([ServiceBusTrigger("opportunity", "opp-test", Connection = "ServiceBusConnectionString", IsSessionsEnabled = false)] Message sbMessage
           , MessageReceiver messageReceiver
           // , string lockToken
           )
        {
            try
            {              
                await _crmImport.HandleMessage(Encoding.UTF8.GetString(sbMessage.Body), sbMessage.UserProperties);
            }
            catch (System.Exception ex)
            {
                _log.LogError(ex, "Error");
               // await messageReceiver.DeadLetterAsync();
                throw new System.Exception("Error handling prospect");
            }
        }
    }
}
