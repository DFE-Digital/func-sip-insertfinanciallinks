using Microsoft.PowerPlatform.Dataverse.Client;

namespace FuncInsertSharepointDocumentUrls.Clients
{
    public interface IDynamicsClient
    {
        ServiceClient Client { get; }
    }
}