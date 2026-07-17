using System;
using System.Threading.Tasks;
using AutoFixture.NUnit4;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.PAS.Jobs.Functions;
using SFA.DAS.PAS.Jobs.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.PAS.Jobs.UnitTests.Functions;

public class ImportProvidersFunctionTests
{
    [Test, MoqAutoData]
    public async Task WhenRun_AndImportSucceeds_ThenCallsImportProviderServiceOnce(
        [Frozen] Mock<IImportProviderService> importProviderService,
        [Greedy] ImportProvidersFunction sut)
    {
        await sut.Run(null);

        importProviderService.Verify(x => x.ImportProviders(), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenRun_AndImportThrowsException_ThenRethrowsError(
        [Frozen] Mock<IImportProviderService> importProviderService,
        [Greedy] ImportProvidersFunction sut)
    {
        importProviderService.Setup(x => x.ImportProviders())
            .ThrowsAsync(new ApplicationException("Inner exception"));

        var act = async () => await sut.Run(null);

        await act.Should().ThrowAsync<ApplicationException>().WithMessage("Inner exception");
    }

    [Test, MoqAutoData]
    public async Task WhenRun_AndImportThrowsAggregateException_ThenRethrowsError(
        [Frozen] Mock<IImportProviderService> importProviderService,
        [Greedy] ImportProvidersFunction sut)
    {
        importProviderService.Setup(x => x.ImportProviders())
            .ThrowsAsync(new AggregateException("Inner Aggregate Exception"));

        var act = async () => await sut.Run(null);

        await act.Should().ThrowAsync<AggregateException>();
    }
}
