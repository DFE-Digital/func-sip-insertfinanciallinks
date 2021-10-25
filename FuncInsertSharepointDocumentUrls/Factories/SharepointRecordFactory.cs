using System.Linq;
using FuncInsertSharepointDocumentUrls.Enums;
using FuncInsertSharepointDocumentUrls.Models;

namespace FuncInsertSharepointDocumentUrls.Factories
{
    public class SharepointRecordFactory : ISharepointRecordFactory
    {
        public SharepointRecord Create(string domain, string relativeUrl, int year, SharepointDocumentType type)
        {
            return new()
            {
                CompaniesHouseNumber = relativeUrl.Split('/').Last().Split('_').First(),
                DynamicsDocType = type.ToString().ToUpper(),
                DynamicsFileUrl = $"{domain}{relativeUrl}",
                DynamicsYear = $"{year - 1}-{year}"
            };
        }
    }
}