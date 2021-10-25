using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FuncInsertSharepointDocumentUrls.Enums;
using FuncInsertSharepointDocumentUrls.Factories;
using FuncInsertSharepointDocumentUrls.Models;
using FuncInsertSharepointDocumentUrls.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace FuncInsertSharepointDocumentUrls.Functions
{
    public class DownloadFinancialDocumentsFunction
    {
        private readonly IDynamicsRepository _dynamicsRepository;
        private readonly ISharepointRepository _sharepointRepository;
        private readonly IFinancialLinkEntityFactory _financialLinkEntityFactory;
        private readonly ILogger<DownloadFinancialDocumentsFunction> _log;

        public DownloadFinancialDocumentsFunction(
            IDynamicsRepository dynamicsRepository,
            ISharepointRepository sharepointRepository,
            IFinancialLinkEntityFactory financialLinkEntityFactory,
            ILogger<DownloadFinancialDocumentsFunction> log)
        {
            _log = log;
            _dynamicsRepository = dynamicsRepository;
            _sharepointRepository = sharepointRepository;
            _financialLinkEntityFactory = financialLinkEntityFactory;
        }

        [Function("DownloadSharepointFinancialDocuments")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestData req)
        {
            _log.LogInformation("DownloadSharepointFinancialDocuments Function was triggered by HttpRequest");
            
            InputModel input;
            try
            {
                input = await req.ReadFromJsonAsync<InputModel>();
                if (input == null)
                {
                    _log.LogCritical("Request must include a body");
                    return req.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception e)
            {
                _log.LogCritical("Exception thrown parsing request body: {Message}, {Trace}", e.Message, e.StackTrace);
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
            if (input.StartYear <= 0)
            {
                _log.LogCritical("Valid StartYear not provided in the input body");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
            if(input.EndYear == 0) input.EndYear = DateTime.Now.Year;

            if (!Enum.TryParse<SharepointDocumentType>(input.DocumentType, true, out var documentType))
            {
                _log.LogCritical("Valid DocumentType not provided in the input body");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            try
            {
                var sharePointRecords = new List<SharepointRecord>();

                foreach (var year in Enumerable.Range(input.StartYear, input.EndYear - input.StartYear))
                {
                    var sharePointLists = await _sharepointRepository.GetListsAsync(documentType, year);
                    if (!sharePointLists.Any()) break;

                    foreach(var list in sharePointLists)
                    {
                        var records = await _sharepointRepository.GetRecordsAsync(list);
                        sharePointRecords.AddRange(records);
                    }
                }
                
                foreach (var record in sharePointRecords)
                {
                    var establishmentReference = await _dynamicsRepository.GetEstablishmentEntityReferenceAsync(record.CompaniesHouseNumber);
                    if (establishmentReference is null) break;
                    
                    var financialLinkExists = await _dynamicsRepository.CheckFinancialLinkExistsAsync(establishmentReference, record.DynamicsYear, record.DynamicsFileUrl);
                    if (financialLinkExists) break;
                
                    var entity = _financialLinkEntityFactory.Create(record, establishmentReference);
                    if (entity is null) break; 
                    
                    _log.LogInformation("Enqueuing Company House Number: '{Number}', Doc Type: '{Type}', Year: '{Year}', URL: '{Url}'", 
                        record.CompaniesHouseNumber, 
                        record.DynamicsDocType, 
                        record.DynamicsYear, 
                        record.DynamicsFileUrl);
                    
                    _dynamicsRepository.EnqueueEntity(entity);
                }

                await _dynamicsRepository.ExecuteMultipleRequestsAsync();
            }
            catch (Exception e)
            {
                _log.LogError("Exception thrown during execution: {Message}, trace: {Exception} ", e.Message, e.StackTrace);
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}