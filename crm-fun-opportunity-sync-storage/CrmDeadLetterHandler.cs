using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Crm.Service
{
    public class CrmDeadLetterHandler : ICrmDeadLetterHandler
    {
        private readonly ILogger<CrmDeadLetterHandler> _log;

        private readonly ICrmImport _crmImport;
        private readonly string topicName = "";
        private readonly string subscriptionName = "";
        private readonly ServiceBusClient _serviceBusClient;

        public CrmDeadLetterHandler(ILogger<CrmDeadLetterHandler> log, ServiceBusClient serviceBusClient, ICrmImport crmImport)
        {
            _log = log;
            _serviceBusClient = serviceBusClient;
            _crmImport = crmImport;
             topicName = Environment.GetEnvironmentVariable("TopicName");
             subscriptionName = Environment.GetEnvironmentVariable("SubscripionName") + "/$deadletterqueue";
    }
        
        public async Task HandleDeadletter()
        {

            try
            {
                // create a receiver that we can use to receive the messages
                var options = new ServiceBusReceiverOptions()
                {
                    ReceiveMode = ServiceBusReceiveMode.PeekLock
                };
                ServiceBusReceiver receiver = _serviceBusClient.CreateReceiver(topicName, subscriptionName, options);
                List<ServiceBusReceivedMessage> listOfTasks = new List<ServiceBusReceivedMessage>();

                //Counter Deadletter
                int msgCount = 0;
                for (var i = 0; i < 1000; i += 250)
                {
                    var peekMessages = await receiver.PeekMessagesAsync(250, i);
                   
                    msgCount += peekMessages.Count;
                    if (peekMessages.Count < 250)
                    {
                        break;
                    }
                }

                //add serviceBusReceivedMessages for listOfTasks
                while (msgCount > 0)
                {
                    var serviceBusReceivedMessages = await receiver.ReceiveMessageAsync();
                    listOfTasks.Add(serviceBusReceivedMessages);
                    msgCount--;
                }

                // execute storage acount
                listOfTasks.ForEach(async (message) => { 
                    await _crmImport.HandleMessage(Encoding.UTF8.GetString(message.Body));
                    await receiver.CompleteMessageAsync(message);
                });

            }
            catch (System.Exception ex)
            {
                _log.LogError(ex, "Error");

                throw new System.Exception("HandleDeadletter", ex);
            }
        }
    }
}
