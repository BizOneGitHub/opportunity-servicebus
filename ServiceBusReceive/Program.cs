using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;

namespace ServiceBusReceive
{
    class Program
    {
        private readonly IHttpClientFactory _clientFactory;

        public Program(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }
       
        // connection string to your Service Bus namespace
        static string connectionString = "Endpoint=sb://sb-crm-opportunity-sea.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=sy6ty0sRDvJZiZnhIz+P9vvOFL4shfiHAjLdLPz2olM=";

        // name of your Service Bus topic
        static string topicName = "opportunity";

        // name of the subscription to the topic
        static string subscriptionName = "opp-test";

        // the client that owns the connection and can be used to create senders and receivers
        static ServiceBusClient client;

        // the processor that reads and processes messages from the subscription
        static ServiceBusProcessor processor;

        // handle received messages
        static async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            Console.WriteLine($"Received: {body} from subscription: {subscriptionName}");

            // complete the message. messages is deleted from the subscription. 
            await args.CompleteMessageAsync(args.Message);

            string baseInfo = "{\"opportunity_id\": 1, \"action_type\": \"update\"}";

            await SendMessageToStorageAsync(baseInfo);
        }

        // handle any errors when receiving messages
        static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        

        static async Task SendMessageToStorageAsync(string bodyMsg)
        {
            try
            {
                string connectionStringStorageAccount = "DefaultEndpointsProtocol=https;AccountName=stgoppsea;AccountKey=CsXAMQCw1+AzOs1KE3sgl74CX+7qU5cWx9zXZUNDLJEoWWR56JPh+eRfdNcxzObFnfGApJh44CVIoywjEetr7A==;EndpointSuffix=core.windows.net";
                string containerName = "fileoppurtunity";
             
                BlobContainerClient containerClient = new BlobContainerClient(connectionStringStorageAccount, containerName);

                //create file name
                var blobClient = containerClient.GetBlobClient("opportunity_" + Guid.NewGuid().ToString() + ".json");
                Console.WriteLine(Guid.NewGuid().ToString());

                // create Stream and upload file into storage account with containerClient
                byte[] byteArray = Encoding.ASCII.GetBytes(bodyMsg);
                using (MemoryStream strm = new MemoryStream(byteArray))
                {
                    await blobClient.UploadAsync(strm);
                }


            }
            catch (System.Exception ex)
            {
                throw new Exception("Could not send to file into storage account", ex);
            }
        }

        static async Task Main()
        {
            // The Service Bus client types are safe to cache and use as a singleton for the lifetime
            // of the application, which is best practice when messages are being published or read
            // regularly.
            //
            // Create the clients that we'll use for sending and processing messages.
            client = new ServiceBusClient(connectionString);

            // create a processor that we can use to process the messages
            processor = client.CreateProcessor(topicName, subscriptionName, new ServiceBusProcessorOptions());

            try
            {
                // add handler to process messages
                processor.ProcessMessageAsync += MessageHandler;
                // add handler to process any errors
                processor.ProcessErrorAsync += ErrorHandler;
                // start processing 
                await processor.StartProcessingAsync();
                Console.WriteLine("Wait for a minute and then press any key to end the processing");
                Console.ReadKey();

                // stop processing 
                Console.WriteLine("\nStopping the receiver...");
                await processor.StopProcessingAsync();
                Console.WriteLine("Stopped receiving messages");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await processor.DisposeAsync();
                await client.DisposeAsync();
            }
        }
    }
}
