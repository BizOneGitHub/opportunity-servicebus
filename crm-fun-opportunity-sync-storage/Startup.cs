using System;
using System.Net.Http.Headers;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Azure.Messaging.ServiceBus;
using System.Net;
using Crm.Service;

[assembly: FunctionsStartup(typeof(Crm.Startup))]

namespace Crm
{
    public class Startup : FunctionsStartup
    {

       

        public override void Configure(IFunctionsHostBuilder builder)
        {
           

            builder.Services.AddSingleton<ICrmImport, CrmImport>();

          var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(response => response.StatusCode == HttpStatusCode.Unauthorized)
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10)
                });

             builder.Services.AddHttpClient("Opportunity", client =>
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }).AddPolicyHandler(retryPolicy)
            .AddHttpMessageHandler<CrmOperationHandler>();
            builder.Services.AddTransient<CrmOperationHandler>();
            // Add ServiceBus Client, can be done better....
            var connectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
            builder.Services.AddSingleton<ServiceBusClient>(new ServiceBusClient(connectionString));

        }
    }
}
