using System;
using FuncInsertSharepointDocumentUrls.Enums;
using FuncInsertSharepointDocumentUrls.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;

namespace FuncInsertSharepointDocumentUrls.Factories
{
    public class FinancialLinkEntityFactory : IFinancialLinkEntityFactory
    {
        private const string SipDocumentType = "sip_documenttype";
        private const string SipEstablishment = "sip_establishment";
        private const string SipFinancialLink = "sip_financiallink";
        private const string SipUrl = "sip_url";
        private const string SipYear = "sip_year";
        
        private readonly ILogger<FinancialLinkEntityFactory> _log;

        public FinancialLinkEntityFactory(ILogger<FinancialLinkEntityFactory> log)
        {
            _log = log;
        }
        
        public Entity Create(SharepointRecord record, EntityReference establishmentReference) 
        {
            if (Enum.TryParse<SharepointDocumentType>(record.DynamicsDocType, true, out var enumValue))
            {
                return new(SipFinancialLink)
                {
                    [SipUrl] = record.DynamicsFileUrl,
                    [SipEstablishment] = establishmentReference,
                    [SipDocumentType] = new OptionSetValue((int)enumValue),
                    [SipYear] = record.DynamicsYear
                };
            }

            _log.LogWarning("Could not parse DocumentType {Type}", record.DynamicsDocType);
            return null;
        }
    }
}