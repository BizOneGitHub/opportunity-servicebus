using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Crm.Service
{
    public class CrmImport : ICrmImport
    {
        private readonly ILogger<CrmImport> _log;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly HttpClient _client;

        public CrmImport(ILogger<CrmImport> log, IHttpClientFactory httpClientFactory, ServiceBusClient serviceBusClient)
        {
            _log = log;
            _serviceBusClient = serviceBusClient;
            _client = httpClientFactory.CreateClient("Crm");

        }

        public async Task HandleMessage(string bodyMsg)
        {
            
            // Get from queue
            dynamic message = JToken.Parse(bodyMsg);
            string urlToCusomerBase = message?.Message?.Url + message?.Message?.RecordID;
           
            if (String.IsNullOrEmpty(urlToCusomerBase)) return;

            // Get data from url
            var baseInfo = await GetMetadataInfoOfAPI(urlToCusomerBase);
            string jsonSBSMetatdata = "{\"SBSMetadata\":" + bodyMsg + "," + baseInfo.Trim().Substring(1);
            await SendMessageToStorageAsync(jsonSBSMetatdata);
        }

       private async Task SendMessageToStorageAsync(string bodyMsg)
        {
            try
            {
                //connection storage account
                var connectionStringStorageAccount = Environment.GetEnvironmentVariable("ConnectionStringStorageAccount");
                var containerName = Environment.GetEnvironmentVariable("ContainerName");

                BlobContainerClient containerClient = new BlobContainerClient(connectionStringStorageAccount, containerName);

                string fileName = Environment.GetEnvironmentVariable("folderFileStoreMetadata") + "/" + Environment.GetEnvironmentVariable("TopicName") + Guid.NewGuid().ToString() + ".json";
                //create file name
                var blobClient = containerClient.GetBlobClient(fileName);

                // create Stream and upload file into storage account with containerClient
                byte[] byteArray = Encoding.ASCII.GetBytes(bodyMsg);
                using (MemoryStream strm = new MemoryStream(byteArray))
                {
                    await blobClient.UploadAsync(strm);
                }                         

            }
            catch (System.Exception ex)
            {
                _log.LogError($"Could not send to file into storage account", ex);
                throw new Exception("Could not send to file into storage account", ex);
            }
        }
      
        private async Task<string> GetMetadataInfoOfAPI(string urlToOpportunityBase)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, urlToOpportunityBase);

            var response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
               if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _log.LogError("Unauthorized get addresses");
                    throw new UnauthorizedAccessException();
                }
                else
                {
                    // Handle failure
                    _log.LogError("Could not read addresses");
                    throw new Exception("Could not read addresses");
                }
            }
            var baseInfo = await response.Content.ReadAsStringAsync();
            return baseInfo;
        }
    }
}
