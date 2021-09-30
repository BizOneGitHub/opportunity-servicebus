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

        private readonly ICrmDeadLetterHandler _crmDeadLetterHandler;

        public CrmOppotunityTrigger(ILogger<CrmOppotunityTrigger> log, ICrmImport crmImport, ICrmDeadLetterHandler crmDeadLetterHandler)
        {
            _log = log;
            _crmImport = crmImport;
            _crmDeadLetterHandler = crmDeadLetterHandler;
        }

        [FunctionName("CrmOppotunityTrigger")]
        public async Task Run([ServiceBusTrigger("opportunity", "opp-test", Connection = "ServiceBusConnectionString", IsSessionsEnabled = false)] Message sbMessage
           , MessageReceiver messageReceiver
            , string lockToken
           )
        {
            try
            {
                // process message to push metadata into storage account
                await _crmImport.HandleMessage(Encoding.UTF8.GetString(sbMessage.Body));

                // execute HandleDeadletter during startTime to endTime
                int startDeadLetter = Int16.Parse(Environment.GetEnvironmentVariable("startDeadLetter"));
                int endDeadLetter = Int16.Parse(Environment.GetEnvironmentVariable("endDeadLetter"));
                var date = DateTime.Now;
                int minute = date.Minute;
                if (minute >= startDeadLetter && minute <= endDeadLetter)
                {
                    await _crmDeadLetterHandler.HandleDeadletter();
                }
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
