using FuncInsertSharepointDocumentUrls.Models;
using Microsoft.Xrm.Sdk;

namespace FuncInsertSharepointDocumentUrls.Factories
{
    public interface IFinancialLinkEntityFactory
    {
        Entity Create(SharepointRecord record, EntityReference establishmentReference);
    }
}