using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using SFA.DAS.Encoding;
using SFA.DAS.ProviderApprenticeshipsService.Web.Authorization;
using SFA.DAS.ProviderApprenticeshipsService.Web.Routing;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.ProviderApprenticeshipsService.Web.UnitTests.Authorization;


public class AuthorizationContextProviderTestsNotFluent
{

    [Test, MoqAutoData]
    public void Then_The_Request_And_Feature_Is_Authorised_For_Non_DfeSign_In_Email(
        string email,
        long ukprn,
        string legalEntity,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        [Frozen] Mock<IActionContextAccessorWrapper> actionContextWrapper,
        AuthorizationContextProvider provider)
    {
        var httpContextBase = new Mock<HttpContext>();
        var httpRequest = new Mock<HttpRequest>();
        httpRequest.Setup(x => x.Query[RouteDataKeys.EmployerAccountLegalEntityPublicHashedId]).Returns("");
        var claim = new Claim(DasClaimTypes.Email, email);
        var claimUkprn = new Claim(DasClaimTypes.Ukprn, ukprn.ToString());
        var claimsPrinciple = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { claim, claimUkprn }) });
        httpContextBase.Setup(x => x.User).Returns(claimsPrinciple);
        httpContextBase.Setup(x => x.Request).Returns(httpRequest.Object);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContextBase.Object);

        var actual = provider.GetAuthorizationContext();

        actual.Should().NotBeNull();
        actual.Get<long>("ukprn").Should().Be(ukprn);
        actual.Get<string>("UserEmail").Should().Be(email);
    }

    [Test, MoqAutoData]
    public void Then_The_Request_And_Feature_Is_Authorised_For_DfeSign_In_Email(
        string email,
        long ukprn,
        string legalEntity,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        [Frozen] Mock<IActionContextAccessorWrapper> actionContextWrapper,
        AuthorizationContextProvider provider)
    {
        var httpContextBase = new Mock<HttpContext>();
        var httpRequest = new Mock<HttpRequest>();
        httpRequest.Setup(x => x.Query[RouteDataKeys.EmployerAccountLegalEntityPublicHashedId]).Returns("");
        var claim = new Claim(DasClaimTypes.DfEEmail, email);
        var claimUkprn = new Claim(DasClaimTypes.Ukprn, ukprn.ToString());
        var claimsPrinciple = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { claim, claimUkprn }) });
        httpContextBase.Setup(x => x.User).Returns(claimsPrinciple);
        httpContextBase.Setup(x => x.Request).Returns(httpRequest.Object);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContextBase.Object);

        var actual = provider.GetAuthorizationContext();

        actual.Should().NotBeNull();
        actual.Get<long>("ukprn").Should().Be(ukprn);
        actual.Get<string>("UserEmail").Should().Be(email);
    }
}

[TestFixture]
[Parallelizable]
public class AuthorizationContextProviderTests
{
    [Test]
    public void WhenGettingAuthorizationContextAndAllRequiredRouteValuesAreAvailable_ThenShouldReturnAuthorizationContextWithAllValuesSet()
    {
        // Arrange
        var fixture = new AuthorizationContextProviderTestsFixture();
        fixture.SetValidAccountLegalEntityPublicHashedId();
        fixture.SetValidProviderId();

        // Act
        var result = fixture.GetAuthorizationContext();

        // Assert
        result.Should().NotBeNull();
        result.Get<long?>(AuthorizationContextProviderTestsFixture.ContextKeys.AccountLegalEntityId)
            .Should().Be(fixture.AccountLegalEntityId);
        result.Get<long?>(AuthorizationContextProviderTestsFixture.ContextKeys.Ukprn)
            .Should().Be(fixture.ProviderId);
    }

    [Test]
    public void WhenGettingAuthorizationContextAndRequiredRouteValuesAreAvailableButProviderIdIsNotValid_ThenShouldThrowException()
    {
        // Arrange
        var fixture = new AuthorizationContextProviderTestsFixture();
        fixture.SetValidAccountLegalEntityPublicHashedId();
        fixture.SetInvalidProviderId();

        // Act
        Action act = () => fixture.GetAuthorizationContext();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("AuthorizationContextProvider error - Unable to extract ProviderId");
    }

    [Test]
    public void WhenGettingAuthorizationContextAndRequiredRouteValuesAreAvailableButProviderIdIsNotAvailable_ThenShouldThrowException()
    {
        // Arrange
        var fixture = new AuthorizationContextProviderTestsFixture();
        fixture.SetValidAccountLegalEntityPublicHashedId();

        // Act
        Action act = () => fixture.GetAuthorizationContext();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("AuthorizationContextProvider error - Unable to extract ProviderId");
    }
}

public class AuthorizationContextProviderTestsFixture
{
    public static class ContextKeys
    {
        public const string AccountLegalEntityId = "AccountLegalEntityId";
        public const string Ukprn = "Ukprn";
    }

    private readonly IAuthorizationContextProvider _authorizationContextProvider;
    private readonly Mock<IHttpContextAccessor> _httpContext;
    private readonly Mock<IEncodingService> _encodingService;
    private readonly RouteData _routeData;

    private IQueryCollection _queryParams;
    private string _providerIdRouteValue;
    private string _accountLegalEntityPublicHashedIdRouteValue;

    public long ProviderId { get; set; }
    public long AccountLegalEntityId { get; set; }


    public AuthorizationContextProviderTestsFixture()
    {
        _routeData = new RouteData();

        var actionContextAccessorWrapper = new Mock<IActionContextAccessorWrapper>();
        actionContextAccessorWrapper.Setup(c => c.GetRouteData()).Returns(_routeData);

        _httpContext = new Mock<IHttpContextAccessor>();
        _httpContext.Setup(c => c.HttpContext.User).Returns(new GenericPrincipal(new Mock<IIdentity>().Object, Array.Empty<string>()));

        _encodingService = new Mock<IEncodingService>();

        _authorizationContextProvider = new AuthorizationContextProvider(_httpContext.Object,
            _encodingService.Object,
            Mock.Of<ILogger<AuthorizationContextProvider>>(),
            actionContextAccessorWrapper.Object);
    }

    public IAuthorizationContext GetAuthorizationContext()
    {
        return _authorizationContextProvider.GetAuthorizationContext();
    }

    public AuthorizationContextProviderTestsFixture SetValidAccountLegalEntityPublicHashedId()
    {
        _accountLegalEntityPublicHashedIdRouteValue = "ABC123";
        AccountLegalEntityId = 123;

        var accountLegalEntityPublicHashedId = new Dictionary<string, StringValues>()
        {
            { RouteDataKeys.EmployerAccountLegalEntityPublicHashedId, new StringValues(_accountLegalEntityPublicHashedIdRouteValue) }
        };
        _queryParams = new QueryCollection(accountLegalEntityPublicHashedId);

        _httpContext.Setup(c => c.HttpContext.Request.Query).Returns(_queryParams);

        _encodingService.Setup(h => h.Decode(_accountLegalEntityPublicHashedIdRouteValue, EncodingType.PublicAccountLegalEntityId)).Returns(AccountLegalEntityId);

        return this;
    }

    public void SetValidProviderId()
    {
        _providerIdRouteValue = "123";
        ProviderId = 123;

        _routeData.Values[RouteDataKeys.ProviderId] = _providerIdRouteValue;
    }

    public void SetInvalidProviderId()
    {
        _providerIdRouteValue = "Skunk";

        _routeData.Values[RouteDataKeys.ProviderId] = _providerIdRouteValue;
    }
}