using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.PAS.Jobs.Services;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces.Data;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces.Services;

namespace SFA.DAS.PAS.Jobs.UnitTests.Services;

[TestFixture]
public class ImportProviderServiceTests
{
    [Test]
    public async Task WhenImport_AndProvidersReturnedInBatches_ThenCallsRepositoryForEachBatch()
    {
        var fixture = new ImportProviderServiceTestFixture();

        await fixture._sut.Import();

        fixture._providerRepository.Verify(x => x.ImportProviders(It.IsAny<Provider[]>()), Times.Exactly(2));
    }

    private class ImportProviderServiceTestFixture
    {
        public ImportProviderService _sut { get; }
        public Mock<IProviderRepository> _providerRepository { get; }

        public ImportProviderServiceTestFixture()
        {
            var autoFixture = new Fixture();
            var response = new GetAllProvidersResponse
            {
                Providers = autoFixture.CreateMany<Provider>(1600).ToList()
            };

            var commitmentsV2ApiClient = new Mock<ICommitmentsV2ApiClient>();
            commitmentsV2ApiClient.Setup(x => x.GetProviders()).ReturnsAsync(response);

            _providerRepository = new Mock<IProviderRepository>();
            _providerRepository.Setup(x => x.ImportProviders(It.IsAny<Provider[]>()));

            _sut = new ImportProviderService(
                commitmentsV2ApiClient.Object,
                _providerRepository.Object,
                Mock.Of<ILogger<ImportProviderService>>());
        }
    }
}
