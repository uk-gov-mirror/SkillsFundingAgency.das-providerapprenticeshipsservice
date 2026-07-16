using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using SFA.DAS.DfESignIn.Auth.Api.Client;
using SFA.DAS.DfESignIn.Auth.Api.Helpers;
using SFA.DAS.DfESignIn.Auth.Configuration;
using SFA.DAS.DfESignIn.Auth.Interfaces;
using SFA.DAS.Http;
using SFA.DAS.Http.Configuration;
using SFA.DAS.Http.TokenGenerators;
using SFA.DAS.PAS.Jobs.Configuration;
using SFA.DAS.PAS.Jobs.Services;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces.Configurations;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces.Data;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces.Services;
using SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Configuration;
using SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Data;
using SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Services;

namespace SFA.DAS.PAS.Jobs.Extensions;

[ExcludeFromCodeCoverage]
public static class AddApplicationRegistrationsExtension
{
    public static IServiceCollection AddServiceRegistrations(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PasJobsConfiguration>(configuration);
        services.AddSingleton<IDatabaseConfiguration>(provider => provider.GetService<IOptions<PasJobsConfiguration>>()!.Value);

        services.Configure<CommitmentsApiClientV2Configuration>(c => configuration.GetSection(ConfigurationKeys.CommitmentsApiClientV2).Bind(c));
        services.AddSingleton(provider => provider.GetService<IOptions<CommitmentsApiClientV2Configuration>>()!.Value);
        services.AddHttpClient<ICommitmentsV2ApiClient, CommitmentsV2ApiClient>();

        services.AddSingleton<TokenCredential>(new ChainedTokenCredential(
            new ManagedIdentityCredential(new ManagedIdentityCredentialOptions()),
            new AzureCliCredential()));

        services.AddTransient<IImportProviderService, ImportProviderService>();
        services.AddTransient<IProviderRepository, ProviderRepository>();

        services.Configure<DfEOidcConfiguration>(configuration.GetSection("DfEOidcConfiguration"));
        services.Configure<DfEOidcConfiguration>(configuration.GetSection("DfEOidcConfiguration_ProviderRoATP"));
        services.AddSingleton(provider => provider.GetService<IOptions<DfEOidcConfiguration>>()!.Value);
        services.AddSingleton(provider => provider.GetService<IOptions<PasJobsConfiguration>>()!.Value.CommitmentNotification);

        services.AddHttpClient<IApiHelper, DfeSignInApiHelper>(options => options.Timeout = TimeSpan.FromMinutes(30))
            .SetHandlerLifetime(TimeSpan.FromMinutes(10))
            .AddPolicyHandler(HttpClientRetryPolicy());

        services.AddTransient<ITokenDataSerializer, TokenDataSerializer>();
        services.AddTransient<ITokenBuilder, TokenBuilder>();
        services.AddTransient<IHttpClientWrapper>(provider =>
        {
            var config = provider.GetService<ProviderNotificationConfiguration>();
            return new HttpClientWrapper(GetHttpClient(config));
        });

        services.AddTransient<IIdamsSyncService, IdamsSyncService>();
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IIdamsEmailServiceWrapper, IdamsEmailServiceWrapper>();

        return services;
    }

    private static HttpClient GetHttpClient(IJwtClientConfiguration config)
    {
        return new HttpClientBuilder().WithBearerAuthorisationHeader(new JwtBearerTokenGenerator(config)).Build();
    }

    private static IAsyncPolicy<System.Net.Http.HttpResponseMessage> HttpClientRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
