using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.PAS.Jobs.Services;

namespace SFA.DAS.PAS.Jobs.Functions;

public class ImportProvidersFunction(IImportProviderService importProviderService, ILogger<ImportProvidersFunction> logger)
{
    [Function(nameof(ImportProvidersFunction))]
    public async Task Run([TimerTrigger("%ImportProvidersJobSchedule%", RunOnStartup = true)] TimerInfo timerInfo)
    {
        if (timerInfo?.IsPastDue == true)
        {
            logger.LogInformation("ImportProviders function is running later than scheduled");
        }
        logger.LogInformation("ImportProvidersFunction started");

        await importProviderService.ImportProviders();

        logger.LogInformation("ImportProvidersFunction completed");
    }
}
