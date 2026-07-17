using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.PAS.Jobs.Extensions;
using SFA.DAS.PAS.Jobs.Services;

namespace SFA.DAS.PAS.Jobs.Functions;

public class ImportProvidersFunction(IImportProviderService importProviderService, ILogger<ImportProvidersFunction> logger)
{
    [Function(nameof(ImportProvidersFunction))]
    public Task Run([TimerTrigger("%ImportProvidersJobSchedule%", RunOnStartup = true)] TimerInfo timerInfo)
    {
        return TimedJobRunner.RunAsync(logger, "ImportProviders", importProviderService.Import);
    }
}
