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
    public async Task Import_ImportsProvidersInBatches_CallsRepositoryForEachBatch()
    {
        var fixture = new ImportProviderServiceTestFixture();

        await fixture.Import();

        fixture.VerifyImportProviderRepositoryCalled();
    }

    private class ImportProviderServiceTestFixture
    {
        private readonly ImportProviderService _sut;
        private readonly Mock<IProviderRepository> _importProviderRepository;

        public ImportProviderServiceTestFixture()
        {
            var autoFixture = new Fixture();
            var response = new GetAllProvidersResponse
            {
                Providers = autoFixture.CreateMany<Provider>(1600).ToList()
            };

            var commitmentsV2ApiClient = new Mock<ICommitmentsV2ApiClient>();
            commitmentsV2ApiClient.Setup(x => x.GetProviders()).ReturnsAsync(response);

            _importProviderRepository = new Mock<IProviderRepository>();
            _importProviderRepository.Setup(x => x.ImportProviders(It.IsAny<Provider[]>()));

            _sut = new ImportProviderService(commitmentsV2ApiClient.Object, _importProviderRepository.Object, Mock.Of<ILogger<ImportProviderService>>());
        }

        public Task Import()
        {
            return _sut.Import();
        }

        public void VerifyImportProviderRepositoryCalled()
        {
            _importProviderRepository.Verify(x => x.ImportProviders(It.IsAny<Provider[]>()), Times.Exactly(2));
        }
    }
}
