namespace FuncInsertSharepointDocumentUrls.Models
{
    public record SharepointRecord
    {
        public string DynamicsFileUrl { get; init; }
        public string DynamicsYear { get; init; }
        public string DynamicsDocType { get; init; }
        public string CompaniesHouseNumber { get; init; }
    }
}