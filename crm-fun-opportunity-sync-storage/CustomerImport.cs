/*using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Opportunity.Service
{
    public class CustomerImport : ICustomerImport
    {
        private readonly ILogger<CustomerImport> _log;
        private readonly ServiceBusClient _serviceBusClient;
        public CustomerImport(ILogger<CustomerImport> log, ServiceBusClient serviceBusClient)
        {
            _log = log;
            //_config = config;
            _serviceBusClient = serviceBusClient;
           // _httpClientOpportunity = httpClientFactory.CreateClient("Crm");
        }

        public async Task HandleMessage(string bodyMsg, System.Collections.Generic.IDictionary<string, object> userProperties)
        {

            if (String.IsNullOrEmpty(bodyMsg))
                return;
            //var api = Environment.GetEnvironmentVariable("Web_Service_API");

            // Get from queue
             dynamic message = JToken.Parse(bodyMsg);
             string urlToCusomerBase = api + message?.Message?.opportunity_id;
             string action = message?.action_type;

            Console.WriteLine("eeee----" + bodyMsg);
            // Get data from urlToCusomerBase
           // var baseInfo = await GetCustomerBaseInfo(api);
           // Console.WriteLine(baseInfo);
            //string connectionStringStorageAccount = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            //string containerName = Environment.GetEnvironmentVariable("CONTAINER_NAME");
            // Console.WriteLine("connectionStringStorageAccount ---" + connectionStringStorageAccount);
            //Console.WriteLine("containerName ---" + containerName);

            //await SendMessageToStorageAsync(bodyMsg);

        }

       private async Task SendMessageToStorageAsync(string bodyMsg)
        {
            try
            {
                //connection storage account
                string connectionStringStorageAccount = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
                string containerName =  Environment.GetEnvironmentVariable("CONTAINER_NAME");
  
               BlobContainerClient containerClient = new BlobContainerClient(connectionStringStorageAccount, containerName);

                //create file name
                var blobClient = containerClient.GetBlobClient("opportunity_" + Guid.NewGuid().ToString() + ".json");
                Console.WriteLine(Guid.NewGuid().ToString());

                // create Stream and upload file into storage account with containerClient
                byte[] byteArray = Encoding.ASCII.GetBytes(bodyMsg);
                using (MemoryStream strm = new MemoryStream())
                {
                    strm.Write(byteArray, 0, byteArray.Length);
                    await blobClient.UploadAsync(strm);
                }
                         

            }
            catch (System.Exception ex)
            {
                _log.LogError($"Could not send to Change parent Topic");
                throw new Exception("Could not send to file into storage account", ex);
            }
        }
      
        private async Task<dynamic> GetCustomerBaseInfo(string urlToCusomerBase)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, urlToCusomerBase);
            var response = await _httpClientOpportunity.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _log.LogError($"Unauthorized get {urlToCusomerBase}");
                throw new UnauthorizedAccessException();
            }
            else if (!response.IsSuccessStatusCode)
            {
                {
                    // Handle failure
                    _log.LogError($"Can't read CustomerBaseInfo - {response.StatusCode}");
                    throw new Exception("Can't read CustomerBaseInfo");
                }
            }

            var baseInfoString = await response.Content.ReadAsStringAsync();
            dynamic baseInfo = JToken.Parse(baseInfoString);
            return baseInfo;
        }
    }
}
*/