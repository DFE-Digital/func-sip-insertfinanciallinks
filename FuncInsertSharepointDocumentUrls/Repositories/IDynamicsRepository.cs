using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace FuncInsertSharepointDocumentUrls.Repositories
{
    public interface IDynamicsRepository
    {
        void EnqueueEntity(Entity entity);
        Task ExecuteMultipleRequestsAsync();
        Task<EntityReference> GetEstablishmentEntityReferenceAsync(string companiesHouseNumber);
        Task<bool> CheckFinancialLinkExistsAsync(EntityReference establishment, string year, string url);
    }
}