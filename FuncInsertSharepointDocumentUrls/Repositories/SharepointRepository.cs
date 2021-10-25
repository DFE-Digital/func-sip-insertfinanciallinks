using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FuncInsertSharepointDocumentUrls.Clients;
using FuncInsertSharepointDocumentUrls.Enums;
using FuncInsertSharepointDocumentUrls.Factories;
using FuncInsertSharepointDocumentUrls.Models;
using Microsoft.Extensions.Logging;
using Microsoft.SharePoint.Client;

namespace FuncInsertSharepointDocumentUrls.Repositories
{
    public class SharepointRepository : ISharepointRepository
    {
        private readonly ISharepointClient _client;
        private readonly ISharepointRecordFactory _sharepointRecordFactory;
        private readonly ILogger<SharepointRepository> _log;

        private readonly string _sharepointDomain;

        public SharepointRepository(ISharepointClient client, ISharepointRecordFactory factory, ILogger<SharepointRepository> log)
        {
            _client = client;
            _sharepointRecordFactory = factory;
            _log = log;
            
            _sharepointDomain = Environment.GetEnvironmentVariable("SPDomain");

        }

        public async Task<List<SharepointList>> GetListsAsync(SharepointDocumentType documentType, int year)
        {
            List<SharepointList> listsToQuery = new(); 

            using var context = _client.GetContext();
            if (context is null) return null;

            var oWebsite = context.Web;
            context.Load(oWebsite, w => w.ServerRelativeUrl);
            await context.ExecuteQueryAsync();

            var documentNameWithWhitespace = $"{documentType.ToString().ToUpper()} {year}";
            var documentNameWithoutWhitespace = $"{documentType.ToString().ToUpper()} {year}";
            
            if (context.Web.ListExists(documentNameWithWhitespace))
            {
                SharepointList list = new()
                {
                    Type = documentType,
                    Name = documentNameWithWhitespace,
                    Year = year
                };
                listsToQuery.Add(list);
            }
            
            if (context.Web.ListExists(documentNameWithoutWhitespace))
            {
                SharepointList list = new()
                {
                    Type = documentType,
                    Name = documentNameWithoutWhitespace,
                    Year = year
                };
                listsToQuery.Add(list);
            }

            return listsToQuery;
        }

        public async Task<IEnumerable<SharepointRecord>> GetRecordsAsync(SharepointList sharepointList)
        {
            using var context = _client.GetContext();
            if (context is null) return null;
           
            var financialDocumentList = context.Web.Lists.GetByTitle(sharepointList.Name);

            var query = CamlQuery.CreateAllItemsQuery();
            query.ViewXml = @"<View Scope='Recursive'><Query></Query></View>";
            var items = financialDocumentList.GetItems(query);

            context.Load(items, retrievals => retrievals.IncludeWithDefaultProperties(i => i.File));
            await context.ExecuteQueryAsync();
                
            _log.LogInformation("Retrieved: {Name}, Year {Year}, Records {Count}", sharepointList.Name, sharepointList.Year, items.Count);

            return items.Any() 
                ? null 
                : items.Select(item => _sharepointRecordFactory.Create(_sharepointDomain, item.File.ServerRelativeUrl, sharepointList.Year, sharepointList.Type));
        }
    }
}
