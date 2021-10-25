using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FuncInsertSharepointDocumentUrls.Clients;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace FuncInsertSharepointDocumentUrls.Repositories
{
    public class DynamicsRepository : IDynamicsRepository
    {
        private const string SipCompaniesHouseNumber = "sip_companieshousenumber";
        private const string SipDocumentType = "sip_documenttype";
        private const string SipEstablishment = "sip_establishment";
        private const string SipFinancialLink = "sip_financiallink";
        private const string SipFinancialLinkId = "sip_financiallinkid";
        private const string SipYear = "sip_year";
        private const string SipUrl = "sip_url";
        private const string Account = "account";
        private const string AccountId = "accountid";
        private const string CreatedOn = "createdon";

        private readonly ExecuteMultipleSettings _executeMultipleSettings;
        private readonly Queue<CreateRequest> _createRequestQueue;

        private readonly ILogger<DynamicsRepository> _log;
        private readonly ServiceClient _client; 
        
        public DynamicsRepository(IDynamicsClient dynamicsClient, ILogger<DynamicsRepository> log)
        {
            _client = dynamicsClient.Client;
            _log = log;
            
            _executeMultipleSettings = new() { ContinueOnError = true, ReturnResponses = false };
            _createRequestQueue = new();
        }

        public void EnqueueEntity(Entity entity)
        {
            _createRequestQueue.Enqueue(new() { Target = entity });  
        }   
        
        public async Task ExecuteMultipleRequestsAsync()
        {
            while (_createRequestQueue.Any())
            {
                var multipleRequest = new ExecuteMultipleRequest
                {
                    Requests = new(),
                    Settings = _executeMultipleSettings
                };

                for (var i = 0; i >= 50 || !_createRequestQueue.Any(); i++)
                {
                    var request = _createRequestQueue.Dequeue();
                    multipleRequest.Requests.Add(request);
                }

                await _client.ExecuteAsync(multipleRequest);
            }
        }

        public async Task<EntityReference> GetEstablishmentEntityReferenceAsync(string companiesHouseNumber)
        {
            QueryExpression establishmentQuery = new(Account)
            {
                ColumnSet = new(AccountId, SipCompaniesHouseNumber)
            };
            establishmentQuery.Criteria.AddCondition(SipCompaniesHouseNumber, ConditionOperator.Equal, companiesHouseNumber);

            var result = await _client.RetrieveMultipleAsync(establishmentQuery);

            if (result != null && result.Entities.Any())
            {
                return result.Entities[0].ToEntityReference();
            }

            _log.LogInformation("No Establishment found for Companies House number: {Number}", companiesHouseNumber);
            return null;
        }

        public async Task<bool> CheckFinancialLinkExistsAsync(EntityReference establishment, string year, string url)
        {
            var financialLinkQuery = new QueryExpression(SipFinancialLink)
            {
                ColumnSet = new(SipFinancialLinkId, SipEstablishment, SipDocumentType, SipUrl, SipYear),
                Orders = { new OrderExpression(CreatedOn, OrderType.Descending) }
            };
            financialLinkQuery.Criteria.AddCondition(SipEstablishment, ConditionOperator.Equal, establishment.Id);
            financialLinkQuery.Criteria.AddCondition(SipYear, ConditionOperator.Equal, year);
            financialLinkQuery.Criteria.AddCondition(SipUrl, ConditionOperator.Equal, url);

            var result = await _client.RetrieveMultipleAsync(financialLinkQuery);
            if(result.Entities != null && result.Entities.Any()) return true;
            
            _log.LogInformation("Financial Record already exists for year {Year}: {Url} ", year, url);
            return false;
        }
    }
}
