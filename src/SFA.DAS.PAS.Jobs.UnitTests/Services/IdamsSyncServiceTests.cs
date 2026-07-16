using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.DfESignIn.Auth.Configuration;
using SFA.DAS.DfESignIn.Auth.Interfaces;
using SFA.DAS.PAS.Jobs.Services;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Enums;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces.Data;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Models;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Models.DfESignInUser;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Models.IdamsUser;
using SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Services;

namespace SFA.DAS.PAS.Jobs.UnitTests.Services;

[TestFixture]
public class IdamsSyncServiceTests
{
    [Test]
    public async Task WhenSyncUsers_AndProviderExists_ThenGetsNextProviderToProcess()
    {
        var fixture = new IdamsSyncServiceTestFixture();

        await fixture._sut.SyncUsers();

        fixture._providerRepository.Verify(x => x.GetNextProviderForIdamsUpdate(), Times.Once);
    }

    [Test]
    public async Task WhenSyncUsers_AndProviderExists_ThenCallsIdamsServiceForProvider()
    {
        var fixture = new IdamsSyncServiceTestFixture();

        await fixture._sut.SyncUsers();

        fixture._apiHelper.Verify(
            x => x.Get<DfeUser>($"{fixture._configuration.APIServiceUrl}/organisations/{fixture._provider.Ukprn}/users"),
            Times.Once);
    }

    [Test]
    public async Task WhenSyncUsers_AndUsersReturned_ThenSyncsUsersWithLocalRepository()
    {
        var fixture = new IdamsSyncServiceTestFixture();

        await fixture._sut.SyncUsers();

        fixture._userRepository.Verify(
            x => x.SyncIdamsUsers(
                It.IsAny<long>(),
                It.Is<List<IdamsUser>>(p => p.Count(z => z.UserType == UserType.NormalUser) == fixture._users.Users.Count)),
            Times.Once);
    }

    [Test]
    public async Task WhenSyncUsers_AndProviderProcessed_ThenMarksProviderAsUpdated()
    {
        var fixture = new IdamsSyncServiceTestFixture();

        await fixture._sut.SyncUsers();

        fixture._providerRepository.Verify(x => x.MarkProviderIdamsUpdated(fixture._provider.Ukprn), Times.Once);
    }

    [Test]
    public void WhenSyncUsers_AndIdamsThrowsException_ThenRethrowsException()
    {
        var fixture = new IdamsSyncServiceTestFixture().SetupIdamsToThrowException();

        Assert.ThrowsAsync<ApplicationException>(() => fixture._sut.SyncUsers());
    }

    [Test]
    public void WhenSyncUsers_AndIdamsThrowsException_ThenMarksProviderAsUpdated()
    {
        var fixture = new IdamsSyncServiceTestFixture().SetupIdamsToThrowException();

        Assert.ThrowsAsync<ApplicationException>(() => fixture._sut.SyncUsers());

        fixture._providerRepository.Verify(x => x.MarkProviderIdamsUpdated(fixture._provider.Ukprn), Times.Once);
    }

    [Test]
    public async Task WhenSyncUsers_AndNoProvidersFound_ThenDoesNotCallIdamsService()
    {
        var fixture = new IdamsSyncServiceTestFixture().WithNoProviders();

        await fixture._sut.SyncUsers();

        fixture._apiHelper.Verify(x => x.Get<DfeUser>(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void WhenSyncUsers_AndIdamsThrowsHttp404Exception_ThenMarksProviderAsUpdatedButDoesNotThrow()
    {
        var fixture = new IdamsSyncServiceTestFixture().SetupIdamsToThrowHttpRequestException();

        Assert.DoesNotThrowAsync(() => fixture._sut.SyncUsers());

        fixture._providerRepository.Verify(x => x.MarkProviderIdamsUpdated(fixture._provider.Ukprn), Times.Once);
    }

    [Test]
    public void WhenSyncUsers_AndIdamsThrowsNon404HttpException_ThenRethrowsAndMarksProviderAsUpdated()
    {
        var fixture = new IdamsSyncServiceTestFixture().SetupIdamsToThrowHttpRequestException(HttpStatusCode.InternalServerError);

        Assert.ThrowsAsync<CustomHttpRequestException>(() => fixture._sut.SyncUsers());

        fixture._providerRepository.Verify(x => x.MarkProviderIdamsUpdated(fixture._provider.Ukprn), Times.Once);
    }

    [Test]
    public async Task WhenSyncUsers_AndIdamsReturnsNull_ThenSyncsEmptyUserList()
    {
        var fixture = new IdamsSyncServiceTestFixture().SetupIdamsToReturnNull();

        await fixture._sut.SyncUsers();

        fixture._userRepository.Verify(
            x => x.SyncIdamsUsers(fixture._provider.Ukprn, It.Is<List<IdamsUser>>(p => p.Count == 0)),
            Times.Once);
        fixture._providerRepository.Verify(x => x.MarkProviderIdamsUpdated(fixture._provider.Ukprn), Times.Once);
    }

    private class IdamsSyncServiceTestFixture
    {
        public IdamsSyncService _sut { get; }
        public Mock<IUserRepository> _userRepository { get; }
        public Mock<IProviderRepository> _providerRepository { get; }
        public Mock<IApiHelper> _apiHelper { get; }
        public Provider _provider { get; }
        public DfeUser _users { get; }
        public DfEOidcConfiguration _configuration { get; }

        public IdamsSyncServiceTestFixture()
        {
            var autoFixture = new Fixture();
            _provider = autoFixture.Create<Provider>();

            var users = autoFixture.Build<User>().With(c => c.UserStatus, 1).CreateMany().ToList();
            _users = autoFixture.Build<DfeUser>().With(c => c.Users, users).Create();

            _providerRepository = new Mock<IProviderRepository>();
            _providerRepository.Setup(x => x.GetNextProviderForIdamsUpdate()).ReturnsAsync(_provider);

            _userRepository = new Mock<IUserRepository>();

            _apiHelper = new Mock<IApiHelper>();
            _apiHelper.Setup(x => x.Get<DfeUser>(It.IsAny<string>())).ReturnsAsync(_users);

            _configuration = new DfEOidcConfiguration
            {
                APIServiceUrl = "https://some.test.url"
            };

            _sut = new IdamsSyncService(
                _userRepository.Object,
                _providerRepository.Object,
                Mock.Of<ILogger<IdamsSyncService>>(),
                _apiHelper.Object,
                _configuration);
        }

        public IdamsSyncServiceTestFixture SetupIdamsToThrowException()
        {
            _apiHelper
                .Setup(x => x.Get<DfeUser>(It.IsAny<string>()))
                .Throws<ApplicationException>();

            return this;
        }

        public IdamsSyncServiceTestFixture SetupIdamsToThrowHttpRequestException(HttpStatusCode statusCode = HttpStatusCode.NotFound)
        {
            _apiHelper
                .Setup(x => x.Get<DfeUser>(It.IsAny<string>()))
                .Throws(new CustomHttpRequestException(statusCode, null));

            return this;
        }

        public IdamsSyncServiceTestFixture SetupIdamsToReturnNull()
        {
            _apiHelper.Setup(x => x.Get<DfeUser>(It.IsAny<string>())).ReturnsAsync((DfeUser)null);
            return this;
        }

        public IdamsSyncServiceTestFixture WithNoProviders()
        {
            _providerRepository.Setup(x => x.GetNextProviderForIdamsUpdate()).ReturnsAsync((Provider)null);
            return this;
        }
    }
}
