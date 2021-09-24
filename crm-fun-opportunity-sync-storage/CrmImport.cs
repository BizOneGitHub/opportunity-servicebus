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

        public async Task HandleMessage(string bodyMsg, System.Collections.Generic.IDictionary<string, object> userProperties)
        {
            // Get from queue
            dynamic message = JToken.Parse(bodyMsg);
            string urlToCusomerBase = message?.Message?.Url + message?.Message?.RecordID;

            if (String.IsNullOrEmpty(urlToCusomerBase)) return;

            // Get data from urlToCusomerBase
            var baseInfo = await GetCustomerBaseInfo(urlToCusomerBase);
            string jsonSBSMetatdata = "{ \n\"SBSMetatdata\":" + bodyMsg + ",\n" + baseInfo.Trim().Substring(1);
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

                //create file name
                var blobClient = containerClient.GetBlobClient("opportunity_" + Guid.NewGuid().ToString() + ".json");

                // create Stream and upload file into storage account with containerClient
                byte[] byteArray = Encoding.ASCII.GetBytes(bodyMsg);
                using (MemoryStream strm = new MemoryStream(byteArray))
                {
                    await blobClient.UploadAsync(strm);
                }                         

            }
            catch (System.Exception ex)
            {
                _log.LogError($"Could not send to Change parent Topic", ex);
               // throw new Exception("Could not send to file into storage account", ex);
            }
        }
      
        private async Task<string> GetCustomerBaseInfo(string urlToCusomerBase)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, urlToCusomerBase);

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
