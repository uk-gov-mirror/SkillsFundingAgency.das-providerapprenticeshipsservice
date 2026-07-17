using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Authentication.Extensions.Legacy;
using SFA.DAS.PAS.Jobs.ApiModels;
using SFA.DAS.PAS.Jobs.Configuration;

namespace SFA.DAS.PAS.Jobs.Services;

public class RoatpApiClient(HttpClient httpClient, RoatpConfiguration config, ILogger<RoatpApiClient> logger) : ApiClientBase(httpClient), IRoatpApiClient
{
    private readonly RoatpConfiguration _config = config;
    private readonly ILogger<RoatpApiClient> _logger = logger;

    public async Task<GetAllProvidersResponse> GetProviders()
    {
        _logger.LogInformation("Getting Providers from RoATP API");
        var url = $"{BaseUrl()}Organisations";
        var response = JsonConvert.DeserializeObject<GetAllProvidersResponse>(await GetAsync(url));

        return response;
    }

    private string BaseUrl()
    {
        if (_config.ApiBaseUrl.EndsWith("/"))
        {
            return _config.ApiBaseUrl;
        }

        return _config.ApiBaseUrl + "/";
    }
}