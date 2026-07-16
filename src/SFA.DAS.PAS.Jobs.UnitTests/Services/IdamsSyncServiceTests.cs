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
    public async Task SyncUsers_GetsNextProviderToProcess()
    {
        var fixture = new IdamsSyncServiceTestFixture();

        await fixture._sut.SyncUsers();

        fixture.VerifyItGetsTheNextProvider();
    }

    [Test]
    public async Task SyncUsers_CallsIdamsServiceForProvider()
    {
        var fixture = new IdamsSyncServiceTestFixture();

        await fixture._sut.SyncUsers();

        fixture.VerifyWeCallIdamsServiceForThisProvider();
    }

    [Test]
    public async Task SyncUsers_WhenUsersReturned_SyncsUsersWithLocalRepository()
    {
        var fixture = new IdamsSyncServiceTestFixture();

        await fixture._sut.SyncUsers();

        fixture.VerifyIdamsUsersAreSyncedInUserRepository();
    }

    [Test]
    public async Task SyncUsers_WhenProviderProcessed_MarksProviderAsUpdated()
    {
        var fixture = new IdamsSyncServiceTestFixture();

        await fixture._sut.SyncUsers();

        fixture.VerifyItMarksProviderAsIdamsUpdated();
    }

    [Test]
    public void SyncUsers_WhenIdamsThrowsException_RethrowsException()
    {
        var fixture = new IdamsSyncServiceTestFixture().SetupIdamsToThrowException();

        Assert.ThrowsAsync<ApplicationException>(() => fixture._sut.SyncUsers());
    }

    [Test]
    public void SyncUsers_WhenIdamsThrowsException_MarksProviderAsUpdated()
    {
        var fixture = new IdamsSyncServiceTestFixture().SetupIdamsToThrowException();

        Assert.ThrowsAsync<ApplicationException>(() => fixture._sut.SyncUsers());

        fixture.VerifyItMarksProviderAsIdamsUpdated();
    }

    [Test]
    public async Task SyncUsers_WhenNoProvidersFound_DoesNotCallIdamsService()
    {
        var fixture = new IdamsSyncServiceTestFixture().WithNoProviders();

        await fixture._sut.SyncUsers();

        fixture.VerifyIdamsServiceIsNotCalled();
    }

    [Test]
    public void SyncUsers_WhenIdamsThrowsHttp404Exception_MarksProviderAsUpdatedButDoesNotThrow()
    {
        var fixture = new IdamsSyncServiceTestFixture().SetupIdamsToThrowHttpRequestException();

        Assert.DoesNotThrowAsync(() => fixture._sut.SyncUsers());

        fixture.VerifyItMarksProviderAsIdamsUpdated();
    }

    [Test]
    public void SyncUsers_WhenIdamsThrowsNon404HttpException_RethrowsAndMarksProviderAsUpdated()
    {
        var fixture = new IdamsSyncServiceTestFixture().SetupIdamsToThrowHttpRequestException(HttpStatusCode.InternalServerError);

        Assert.ThrowsAsync<CustomHttpRequestException>(() => fixture._sut.SyncUsers());

        fixture.VerifyItMarksProviderAsIdamsUpdated();
    }

    [Test]
    public async Task SyncUsers_WhenIdamsReturnsNull_SyncsEmptyUserList()
    {
        var fixture = new IdamsSyncServiceTestFixture().SetupIdamsToReturnNull();

        await fixture._sut.SyncUsers();

        fixture.VerifyIdamsUsersAreSyncedWithEmptyList();
        fixture.VerifyItMarksProviderAsIdamsUpdated();
    }

    private class IdamsSyncServiceTestFixture
    {
        private readonly Mock<IUserRepository> _userRepository;
        private readonly Mock<IProviderRepository> _providerRepository;
        private readonly Provider _providerResponse;
        private readonly DfeUser _normalUsers;
        private readonly Mock<IApiHelper> _apiHelper;
        private readonly DfEOidcConfiguration _configuration;
        public IdamsSyncService _sut { get; }

        public IdamsSyncServiceTestFixture()
        {
            var autoFixture = new Fixture();
            _providerResponse = autoFixture.Create<Provider>();

            var users = autoFixture.Build<User>().With(c => c.UserStatus, 1).CreateMany().ToList();

            _normalUsers = autoFixture.Build<DfeUser>().With(c => c.Users, users).Create();

            _providerRepository = new Mock<IProviderRepository>();
            _providerRepository.Setup(x => x.GetNextProviderForIdamsUpdate()).ReturnsAsync(_providerResponse);
            _userRepository = new Mock<IUserRepository>();

            _apiHelper = new Mock<IApiHelper>();
            _apiHelper.Setup(x => x.Get<DfeUser>(It.IsAny<string>())).ReturnsAsync(_normalUsers);

            _configuration = new DfEOidcConfiguration
            {
                APIServiceUrl = "https://some.test.url"
            };

            _sut = new IdamsSyncService(_userRepository.Object,
                _providerRepository.Object, Mock.Of<ILogger<IdamsSyncService>>(), _apiHelper.Object, _configuration);
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

        public void VerifyItGetsTheNextProvider()
        {
            _providerRepository.Verify(x => x.GetNextProviderForIdamsUpdate());
        }

        public void VerifyWeCallIdamsServiceForThisProvider()
        {
            _apiHelper.Verify(x => x.Get<DfeUser>($"{_configuration.APIServiceUrl}/organisations/{_providerResponse.Ukprn}/users"), Times.Once);
        }

        public void VerifyIdamsUsersAreSyncedInUserRepository()
        {
            _userRepository.Verify(x => x.SyncIdamsUsers(It.IsAny<long>(),
                It.Is<List<IdamsUser>>(p => p.Count(z => z.UserType == UserType.NormalUser) == _normalUsers.Users.Count)));
        }

        public void VerifyIdamsUsersAreSyncedWithEmptyList()
        {
            _userRepository.Verify(x => x.SyncIdamsUsers(_providerResponse.Ukprn,
                It.Is<List<IdamsUser>>(p => p.Count == 0)), Times.Once);
        }

        public void VerifyItMarksProviderAsIdamsUpdated()
        {
            _providerRepository.Verify(x => x.MarkProviderIdamsUpdated(_providerResponse.Ukprn));
        }

        public void VerifyIdamsServiceIsNotCalled()
        {
            _apiHelper.Verify(x => x.Get<DfeUser>(It.IsAny<string>()), Times.Never);
        }
    }
}
