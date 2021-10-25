using System.Collections.Generic;
using System.Threading.Tasks;
using FuncInsertSharepointDocumentUrls.Enums;
using FuncInsertSharepointDocumentUrls.Models;

namespace FuncInsertSharepointDocumentUrls.Repositories
{
    public interface ISharepointRepository
    {
        Task<List<SharepointList>> GetListsAsync(SharepointDocumentType documentType, int year);
        Task<IEnumerable<SharepointRecord>> GetRecordsAsync(SharepointList sharepointList);
    }
}