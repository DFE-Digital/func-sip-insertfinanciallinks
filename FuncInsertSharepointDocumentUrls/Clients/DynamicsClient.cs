using System;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace FuncInsertSharepointDocumentUrls.Clients
{
    public class DynamicsClient : IDynamicsClient
    {
        public ServiceClient Client { get; }
        
        public DynamicsClient()
        {
            var organizationUrl = Environment.GetEnvironmentVariable("DynamicsURL");
            var clientId = Environment.GetEnvironmentVariable("ClientID");
            var secret = Environment.GetEnvironmentVariable("Secret");

            var connectionString = $"Url={organizationUrl};AuthType=ClientSecret;ClientId={clientId};ClientSecret={secret};RequireNewInstance=true";
            Client = new(connectionString);
        }
    }
}