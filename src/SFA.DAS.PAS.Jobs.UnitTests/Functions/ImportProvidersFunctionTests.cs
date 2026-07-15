using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.PAS.Jobs.Functions;
using SFA.DAS.PAS.Jobs.Services;

namespace SFA.DAS.PAS.Jobs.UnitTests.Functions;

[TestFixture]
public class ImportProvidersFunctionTests
{
    private ImportProvidersFunction _sut;
    private Mock<IImportProviderService> _importProvidersService;

    [SetUp]
    public void Before_Each_Test()
    {
        _importProvidersService = new Mock<IImportProviderService>();
        _sut = new ImportProvidersFunction(_importProvidersService.Object, Mock.Of<ILogger<ImportProvidersFunction>>());
    }

    [Test]
    public async Task Run_CallsImportProviderServiceOnce()
    {
        await _sut.Run(null);

        _importProvidersService.Verify(x => x.Import(), Times.Once);
    }

    [Test]
    public async Task Run_WhenImportThrowsException_RethrowsError()
    {
        _importProvidersService.Setup(x => x.Import())
            .ThrowsAsync(new ApplicationException("Inner exception"));

        var act = async () => await _sut.Run(null);

        await act.Should().ThrowAsync<ApplicationException>().WithMessage("Inner exception");
    }

    [Test]
    public async Task Run_WhenImportThrowsAggregateException_DoesNotRethrowError()
    {
        _importProvidersService.Setup(x => x.Import())
            .ThrowsAsync(new AggregateException("Inner Aggregate Exception"));

        await _sut.Run(null);

        _importProvidersService.Verify(x => x.Import(), Times.Once);
    }
}
