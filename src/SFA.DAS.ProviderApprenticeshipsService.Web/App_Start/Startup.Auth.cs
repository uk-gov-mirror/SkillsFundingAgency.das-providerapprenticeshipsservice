using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.WsFederation;
using Owin;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces;
using SFA.DAS.ProviderApprenticeshipsService.Web.App_Start;
using SFA.DAS.ProviderApprenticeshipsService.Web.Orchestrators;
using System.Security.Claims;
using IdentityServer3.Core.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security.OpenIdConnect;
using SFA.DAS.OidcMiddleware;
using SFA.DAS.ProviderRelationships.Types.Models;
using OpenIdConnectMessage = Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectMessage;

namespace SFA.DAS.ProviderApprenticeshipsService.Web
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {


            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                CookieManager = new SystemWebCookieManager()
            });

            //UseProviderIdams(app);

            UseFakeIdams(app);

        }

        private void UseFakeIdams(IAppBuilder app)
        {
            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                AuthenticationType = OpenIdConnectAuthenticationDefaults.AuthenticationType, //"oidc",
                SignInAsAuthenticationType = "Cookies",
                Authority = "https://localhost:44381/",
                ClientId = "openIdConnectClient",
                RedirectUri = "https://127.0.0.1:44347/signin-oidc",
                RequireHttpsMetadata = false,
                Scope = "openid profile idams",
                ResponseType = "id_token",
                UseTokenLifetime = false,
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    //SecurityTokenValidated = notification => Task.CompletedTask;
                }
            });
        }



        private void UseProviderIdams(IAppBuilder app)
        {
            var logger = StructuremapMvc.StructureMapDependencyScope.Container.GetInstance<IProviderCommitmentsLogger>();

            var authenticationOrchestrator = StructuremapMvc.StructureMapDependencyScope.Container.GetInstance<AuthenticationOrchestrator>();
            var accountOrchestrator = StructuremapMvc.StructureMapDependencyScope.Container.GetInstance<AccountOrchestrator>();


            var realm = CloudConfigurationManager.GetSetting("IdamsRealm");
            var adfsMetadata = CloudConfigurationManager.GetSetting("IdamsADFSMetadata");

            //todo: may need more options here
            var options = new WsFederationAuthenticationOptions
            {
                Wtrealm = realm,
                MetadataAddress = adfsMetadata,
                Notifications = new WsFederationAuthenticationNotifications
                {
                    SecurityTokenValidated = notification => SecurityTokenValidated(notification, logger, authenticationOrchestrator, accountOrchestrator)
                }
                //,Wreply = "?"
                //,SignOutWreply = "?"
            };

            app.UseWsFederationAuthentication(options);
        }


        
        private async Task SecurityTokenValidated(SecurityTokenValidatedNotification<WsFederationMessage, WsFederationAuthenticationOptions> notification, IProviderCommitmentsLogger logger, 
            AuthenticationOrchestrator orchestrator, AccountOrchestrator accountOrchestrator)
        {
            logger.Info("SecurityTokenValidated notification called");

            var identity = notification.AuthenticationTicket.Identity;

            var id = identity.Claims.FirstOrDefault(claim => claim.Type == (DasClaimTypes.Upn))?.Value;
            var displayName = identity.Claims.FirstOrDefault(claim => claim.Type == (DasClaimTypes.DisplayName))?.Value;
            var ukprn = identity.Claims.FirstOrDefault(claim => claim.Type == (DasClaimTypes.Ukprn))?.Value;
            var email = identity.Claims.FirstOrDefault(claim => claim.Type == (DasClaimTypes.Email))?.Value;

            long parsedUkprn;
            if (!long.TryParse(ukprn, out parsedUkprn))
            {
                logger.Info($"Unable to parse Ukprn \"{ukprn}\" from claims for user \"{id}\"");
                return;
            }

            var showReservations = await accountOrchestrator.ProviderHasPermission(parsedUkprn, Operation.CreateCohort);

            identity.AddClaim(new Claim(DasClaimTypes.ShowReservations, showReservations.ToString(), "bool"));

            await orchestrator.SaveIdentityAttributes(id, parsedUkprn, displayName, email);
        }
    }
}
