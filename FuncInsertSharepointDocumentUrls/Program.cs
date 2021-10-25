using System.IO;
using System.Threading.Tasks;
using FuncInsertSharepointDocumentUrls.Clients;
using FuncInsertSharepointDocumentUrls.Factories;
using FuncInsertSharepointDocumentUrls.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FuncInsertSharepointDocumentUrls
{
    public static class Program
    {
        public static async Task Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureAppConfiguration(config =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("local.settings.json");
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton<IDynamicsClient, DynamicsClient>();
                    services.AddSingleton<ISharepointClient, SharepointClient>();
                    services.AddSingleton<IFinancialLinkEntityFactory, FinancialLinkEntityFactory>();
                    services.AddSingleton<ISharepointRecordFactory, SharepointRecordFactory>();
                    services.AddTransient<IDynamicsRepository, DynamicsRepository>();
                    services.AddTransient<ISharepointRepository, SharepointRepository>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}