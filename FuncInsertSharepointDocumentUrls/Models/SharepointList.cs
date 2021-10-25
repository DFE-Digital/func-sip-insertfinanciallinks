using FuncInsertSharepointDocumentUrls.Enums;

namespace FuncInsertSharepointDocumentUrls.Models
{
    public record SharepointList
    {
        public SharepointDocumentType Type { get; init; }
        public string Name { get; init; }
        public int Year { get; init; }
    }
}