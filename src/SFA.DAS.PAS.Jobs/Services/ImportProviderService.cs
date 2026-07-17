using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces.Data;

namespace SFA.DAS.PAS.Jobs.Services;

public class ImportProviderService(IRoatpApiClient roatpApiClient, IProviderRepository providerRepository, ILogger<ImportProviderService> logger) : IImportProviderService
{
    private readonly IRoatpApiClient _roatpApiClient = roatpApiClient;
    private readonly IProviderRepository _providerRepository = providerRepository;
    private readonly ILogger<ImportProviderService> _logger = logger;

    public async Task Import()
    {
        _logger.LogInformation("Import Provider - Started");

        var providersResponse = await _roatpApiClient.GetProviders();
        var providers = providersResponse.Organisations;
        var batches = providers.Chunk(1000);

        foreach (var batch in batches)
        {
            List<Provider> providersList = new(batch.Length);
            foreach (var provider in batch)
            {
                providersList.Add(new Provider
                {
                    Ukprn = provider.Ukprn,
                    Name = provider.LegalName,
                });
            }
            await ImportProviders([.. providersList]);
        }

        _logger.LogInformation("ImportProvidersJob - Finished");
    }

    private Task ImportProviders(Provider[] providers)
    {
        return _providerRepository.ImportProviders(providers);
    }
}
