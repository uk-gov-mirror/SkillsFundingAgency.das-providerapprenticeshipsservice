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

public class UpdateUsersFromDfeSignInFunctionTests
{
    [Test, MoqAutoData]
    public async Task WhenRun_AndSyncSucceeds_ThenCallsIdamsSyncServiceOnce(
        [Frozen] Mock<IIdamsSyncService> idamsSyncService,
        [Greedy] UpdateUsersFromDfeSignInFunction sut)
    {
        await sut.Run(null);

        idamsSyncService.Verify(x => x.SyncUsers(), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenRun_AndSyncUsersThrowsException_ThenRethrowsError(
        [Frozen] Mock<IIdamsSyncService> idamsSyncService,
        [Greedy] UpdateUsersFromDfeSignInFunction sut)
    {
        idamsSyncService.Setup(x => x.SyncUsers())
            .ThrowsAsync(new ApplicationException("Inner exception"));

        var act = async () => await sut.Run(null);

        await act.Should().ThrowAsync<ApplicationException>().WithMessage("Inner exception");
    }

    [Test, MoqAutoData]
    public async Task WhenRun_AndSyncUsersThrowsAggregateException_ThenDoesNotRethrowError(
        [Frozen] Mock<IIdamsSyncService> idamsSyncService,
        [Greedy] UpdateUsersFromDfeSignInFunction sut)
    {
        idamsSyncService.Setup(x => x.SyncUsers())
            .ThrowsAsync(new AggregateException("Inner Aggregate Exception"));

        await sut.Run(null);

        idamsSyncService.Verify(x => x.SyncUsers(), Times.Once);
    }
}
