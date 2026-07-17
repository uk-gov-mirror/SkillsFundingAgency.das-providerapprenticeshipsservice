using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
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
using SFA.DAS.PAS.Jobs.ApiClients;
using SFA.DAS.PAS.Jobs.Configuration;
using SFA.DAS.PAS.Jobs.Services;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces.Configurations;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces.Data;
using SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Data;

namespace SFA.DAS.PAS.Jobs.Extensions;

[ExcludeFromCodeCoverage]
public static class AddApplicationRegistrationsExtension
{
    public static IServiceCollection AddApplicationRegistrations(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PasJobsConfiguration>(configuration);
        services.AddSingleton<IDatabaseConfiguration>(provider => provider.GetRequiredService<IOptions<PasJobsConfiguration>>().Value);
        services.AddSingleton(provider => provider.GetRequiredService<IOptions<PasJobsConfiguration>>().Value.RoatpApiClient);

        services.AddSingleton<TokenCredential>(_ => new ChainedTokenCredential(
            new ManagedIdentityCredential(new ManagedIdentityCredentialOptions()),
            new AzureCliCredential()));

        services.AddTransient<RoatpBearerTokenHandler>();
        services.AddHttpClient<IRoatpApiClient, RoatpApiClient>()
            .AddHttpMessageHandler<RoatpBearerTokenHandler>()
            .AddPolicyHandler(HttpClientRetryPolicy());

        services.AddTransient<IImportProviderService, ImportProviderService>();
        services.AddTransient<IProviderRepository, ProviderRepository>();

        services.Configure<DfEOidcConfiguration>(configuration.GetSection("DfEOidcConfiguration"));
        services.Configure<DfEOidcConfiguration>(configuration.GetSection("DfEOidcConfiguration_ProviderRoATP"));
        services.AddSingleton(provider => provider.GetRequiredService<IOptions<DfEOidcConfiguration>>().Value);

        services.AddHttpClient<IApiHelper, DfeSignInApiHelper>(options => options.Timeout = TimeSpan.FromMinutes(30))
            .SetHandlerLifetime(TimeSpan.FromMinutes(10))
            .AddPolicyHandler(HttpClientRetryPolicy());

        services.AddTransient<ITokenDataSerializer, TokenDataSerializer>();
        services.AddTransient<ITokenBuilder, TokenBuilder>();

        services.AddTransient<IUserSyncService, UserSyncService>();
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

[ExcludeFromCodeCoverage]
internal sealed class RoatpBearerTokenHandler(TokenCredential tokenCredential, RoatpConfiguration config) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(config.IdentifierUri))
        {
            var token = await tokenCredential.GetTokenAsync(
                new TokenRequestContext([config.IdentifierUri.TrimEnd('/') + "/.default"]),
                cancellationToken);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
