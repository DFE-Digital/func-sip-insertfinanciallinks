using FuncInsertSharepointDocumentUrls.Enums;
using FuncInsertSharepointDocumentUrls.Models;

namespace FuncInsertSharepointDocumentUrls.Factories
{
    public interface ISharepointRecordFactory
    {
        SharepointRecord Create(string domain, string relativeUrl, int year, SharepointDocumentType type);
    }
}