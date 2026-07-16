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
public class UpdateUsersFromDfeSignInFunctionTests
{
    private UpdateUsersFromDfeSignInFunction _sut;
    private Mock<IIdamsSyncService> _updateUsersService;

    [SetUp]
    public void Before_Each_Test()
    {
        _updateUsersService = new Mock<IIdamsSyncService>();
        _sut = new UpdateUsersFromDfeSignInFunction(_updateUsersService.Object, Mock.Of<ILogger<UpdateUsersFromDfeSignInFunction>>());
    }

    [Test]
    public async Task WhenRun_AndSyncSucceeds_ThenCallsIdamsSyncServiceOnce()
    {
        await _sut.Run(null);

        _updateUsersService.Verify(x => x.SyncUsers(), Times.Once);
    }

    [Test]
    public async Task WhenRun_AndSyncUsersThrowsException_ThenRethrowsError()
    {
        _updateUsersService.Setup(x => x.SyncUsers())
            .ThrowsAsync(new ApplicationException("Inner exception"));

        var act = async () => await _sut.Run(null);

        await act.Should().ThrowAsync<ApplicationException>().WithMessage("Inner exception");
    }

    [Test]
    public async Task WhenRun_AndSyncUsersThrowsAggregateException_ThenDoesNotRethrowError()
    {
        _updateUsersService.Setup(x => x.SyncUsers())
            .ThrowsAsync(new AggregateException("Inner Aggregate Exception"));

        await _sut.Run(null);

        _updateUsersService.Verify(x => x.SyncUsers(), Times.Once);
    }
}
