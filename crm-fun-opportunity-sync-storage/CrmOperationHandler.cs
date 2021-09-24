using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Crm
{
    public class CrmOperationHandler : DelegatingHandler
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private static string _token;
        public CrmOperationHandler(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        protected async override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(_token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            }

            var response = await base.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _token = await PerformAuthorization();
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                }
            }

            return response;
        }

        private async Task<string> PerformAuthorization()
        {
            var clientId = Environment.GetEnvironmentVariable("clientId");
            var clientSecret = Environment.GetEnvironmentVariable("clientSecret");
            var scope = Environment.GetEnvironmentVariable("scope");
            var apiOauth2 = Environment.GetEnvironmentVariable("apiOauth2");
            var grantType = Environment.GetEnvironmentVariable("grantType");


            var dict = new Dictionary<string, string>();
            dict.Add("grant_type", grantType);
            dict.Add("scope", scope);
            dict.Add("client_id", clientId);
            dict.Add("client_secret", clientSecret);

            var client = _httpClientFactory.CreateClient();

            var req = new HttpRequestMessage(HttpMethod.Post, $"{apiOauth2}") { Content = new FormUrlEncodedContent(dict) };
            var res = await client.SendAsync(req);

            if (res.IsSuccessStatusCode)
            {
                string content = await res.Content.ReadAsStringAsync();
                dynamic dynamicContent = JToken.Parse(content);
                string token = dynamicContent?.access_token;
                return token;
            }
            return string.Empty;
        }
    }
}