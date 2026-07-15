using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.PAS.Jobs.Services;

namespace SFA.DAS.PAS.Jobs.Functions;

public class ImportProvidersFunction(IImportProviderService importProviderService, ILogger<ImportProvidersFunction> logger)
{
    private readonly IImportProviderService _importProviderService = importProviderService;
    private readonly ILogger<ImportProvidersFunction> _logger = logger;

    [Function(nameof(ImportProvidersFunction))]
    public async Task Run([TimerTrigger("%ImportProvidersJobSchedule%", RunOnStartup = true)] TimerInfo timerInfo)
    {
        try
        {
            _logger.LogInformation("ImportProvider job started");
            var timer = Stopwatch.StartNew();

            await _importProviderService.Import();

            timer.Stop();
            _logger.LogInformation("ImportProvider job done, Took: {ElapsedMilliseconds} milliseconds", timer.ElapsedMilliseconds);
        }
        catch (AggregateException exc)
        {
            _logger.LogError(exc, "Error running ImportProvider function");
            exc.Handle(ex =>
            {
                _logger.LogError(ex, "Inner exception running ImportProvider function");
                return false;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running ImportProvider function");
            throw;
        }
    }
}
