using System.Diagnostics.CodeAnalysis;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces.Configurations;
using SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Configuration;

namespace SFA.DAS.PAS.Jobs.Configuration;

[ExcludeFromCodeCoverage]
public class PasJobsConfiguration : IDatabaseConfiguration
{
    public string DatabaseConnectionString { get; set; }
    public CommitmentsApiClientV2Configuration CommitmentsApiClientV2 { get; set; }
    public ProviderNotificationConfiguration CommitmentNotification { get; set; }
}
