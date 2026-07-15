using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SFA.DAS.PAS.Jobs.Extensions;

namespace SFA.DAS.PAS.Jobs;

[ExcludeFromCodeCoverage]
public class Program
{
    public static async Task Main()
    {
        using var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureAppConfiguration(builder => builder.AddConfiguration())
            .ConfigureDasLogging()
            .ConfigurePasServices()
            .Build();

        var logger = host.Services.GetService<ILogger<Program>>();

        logger.LogInformation("SFA.DAS.PAS.Jobs is starting up ...");
        //var configuration = host.Services.GetService<IConfiguration>();

        //var databaseConnectionString = configuration?.GetConfigValue("DatabaseConnectionString")
        //                              ?? configuration?.GetConfigValue("SFA.DAS.ProviderApprenticeshipsService:DatabaseConnectionString");
        //var commitmentsApiBaseUrl = configuration?.GetConfigValue("CommitmentsApiClientV2:ApiBaseUrl")
        //                            ?? configuration?.GetConfigValue("SFA.DAS.ProviderApprenticeshipsService:CommitmentsApiClientV2:ApiBaseUrl");
        //logger.LogInformation(
        //    "Startup config check: DatabaseConnectionString configured = {IsConfigured}",
        //    !string.IsNullOrWhiteSpace(databaseConnectionString));
        //logger.LogInformation(
        //    "Startup config check: CommitmentsApiClientV2 ApiBaseUrl configured = {IsConfigured}",
        //    !string.IsNullOrWhiteSpace(commitmentsApiBaseUrl));

        await host.RunAsync();
    }
}
