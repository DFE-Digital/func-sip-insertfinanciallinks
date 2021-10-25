using Microsoft.SharePoint.Client;

namespace FuncInsertSharepointDocumentUrls.Clients
{
    public interface ISharepointClient
    {
        ClientContext GetContext();
    }
}