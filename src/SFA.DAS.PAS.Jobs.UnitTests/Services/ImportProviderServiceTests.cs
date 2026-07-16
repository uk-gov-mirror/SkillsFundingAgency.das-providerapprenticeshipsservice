using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit4;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.PAS.Jobs.Services;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces.Data;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.PAS.Jobs.UnitTests.Services;

public class ImportProviderServiceTests
{
    [Test, MoqAutoData]
    public async Task WhenImport_AndProvidersReturnedInBatches_ThenCallsRepositoryForEachBatch(
        [Frozen] Mock<ICommitmentsV2ApiClient> commitmentsV2ApiClient,
        [Frozen] Mock<IProviderRepository> providerRepository,
        [Greedy] ImportProviderService sut)
    {
        var providers = Enumerable.Range(1, 1600)
            .Select(i => new Provider { Ukprn = i, Name = $"Provider {i}" })
            .ToList();

        commitmentsV2ApiClient
            .Setup(x => x.GetProviders())
            .ReturnsAsync(new GetAllProvidersResponse { Providers = providers });

        await sut.Import();

        providerRepository.Verify(x => x.ImportProviders(It.IsAny<Provider[]>()), Times.Exactly(2));
    }
}
