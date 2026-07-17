using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using SFA.DAS.DfESignIn.Auth.Api.Client;
using SFA.DAS.DfESignIn.Auth.Api.Helpers;
using SFA.DAS.DfESignIn.Auth.Configuration;
using SFA.DAS.DfESignIn.Auth.Interfaces;
using SFA.DAS.PAS.Jobs.Configuration;
using SFA.DAS.PAS.Jobs.Services;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces.Configurations;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces.Data;
using SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Data;

namespace SFA.DAS.PAS.Jobs.Extensions;

[ExcludeFromCodeCoverage]
public static class AddApplicationRegistrationsExtension
{
    public static IServiceCollection AddServiceRegistrations(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PasJobsConfiguration>(configuration);
        services.AddSingleton<IDatabaseConfiguration>(provider => provider.GetService<IOptions<PasJobsConfiguration>>()!.Value);

        services.AddSingleton(provider =>
        {
            var roatpConfig = provider.GetService<IOptions<PasJobsConfiguration>>()!.Value.RoatpApiClient;
            return new RoatpConfiguration
            {
                ApiBaseUrl = roatpConfig.ApiBaseUrl,
                IdentifierUri = roatpConfig.IdentifierUri
            };
        });
        services.AddHttpClient<IRoatpApiClient, RoatpApiClient>();

        services.AddSingleton<TokenCredential>(new ChainedTokenCredential(
            new ManagedIdentityCredential(new ManagedIdentityCredentialOptions()),
            new AzureCliCredential()));

        services.AddTransient<IImportProviderService, ImportProviderService>();
        services.AddTransient<IProviderRepository, ProviderRepository>();

        services.Configure<DfEOidcConfiguration>(configuration.GetSection("DfEOidcConfiguration"));
        services.Configure<DfEOidcConfiguration>(configuration.GetSection("DfEOidcConfiguration_ProviderRoATP"));
        services.AddSingleton(provider => provider.GetService<IOptions<DfEOidcConfiguration>>()!.Value);

        services.AddHttpClient<IApiHelper, DfeSignInApiHelper>(options => options.Timeout = TimeSpan.FromMinutes(30))
            .SetHandlerLifetime(TimeSpan.FromMinutes(10))
            .AddPolicyHandler(HttpClientRetryPolicy());

        services.AddTransient<ITokenDataSerializer, TokenDataSerializer>();
        services.AddTransient<ITokenBuilder, TokenBuilder>();

        services.AddTransient<IIdamsSyncService, IdamsSyncService>();
        services.AddTransient<IUserRepository, UserRepository>();

        return services;
    }

    private static AsyncRetryPolicy<HttpResponseMessage> HttpClientRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.InternalServerError)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
