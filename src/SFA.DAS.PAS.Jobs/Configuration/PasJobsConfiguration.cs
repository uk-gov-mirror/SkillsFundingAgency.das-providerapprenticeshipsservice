using System.Diagnostics.CodeAnalysis;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces.Configurations;
using SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Configuration;

namespace SFA.DAS.PAS.Jobs.Configuration;

[ExcludeFromCodeCoverage]
public class PasJobsConfiguration : IBaseConfiguration
{
    public string DatabaseConnectionString { get; set; }
    public string ServiceBusConnectionString { get; set; }
    public string NServiceBusLicense { get; set; }
    public CommitmentsApiClientV2Configuration CommitmentsApiClientV2 { get; set; }
    public ProviderNotificationConfiguration CommitmentNotification { get; set; }
}

