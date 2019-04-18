using IdentityModel.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace MvcClientWeb
{
    public partial class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string aadInstance = EnsureTrailingSlash(ConfigurationManager.AppSettings["ida:AADInstance"]);
        private static string tenantId = ConfigurationManager.AppSettings["ida:TenantId"];
        private static string postLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];
        private static string authority = aadInstance + tenantId;
        private static string clientSecret = "tvzpwTHl+cZB7h0GjO+QlJ0Wy15z+quGoqLRWGMZjzM=";

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = clientId,
                    Authority = authority,
                    RedirectUri = "https://localhost:6002/signin-oidc",
                    PostLogoutRedirectUri = postLogoutRedirectUri,
                    ResponseType = OpenIdConnectResponseType.CodeIdToken,

                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        AuthorizationCodeReceived = OnAuthorizationCodeReceived
                    }
                });
        }

        private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedNotification arg)
        {
            var tokenClient = new HttpClient();
            var authCodeRequest = new AuthorizationCodeTokenRequest
            {
                Address = string.Concat(authority, "/oauth2/v2.0/token"),
                ClientId = clientId,
                ClientSecret = clientSecret,
                RedirectUri = "https://localhost:6002/signin-oidc",
                Code = arg.Code,
            };

            authCodeRequest.Parameters.Add("Scope", "https://klowdsoft.onmicrosoft.com/ResourceApi1/Authies.ResourceApi1.Read");

            var response = await tokenClient.RequestAuthorizationCodeTokenAsync(authCodeRequest);

            arg.AuthenticationTicket.Identity.AddClaim(new System.Security.Claims.Claim("access_token", response.AccessToken));
        }

        private static string EnsureTrailingSlash(string value)
        {
            if (value == null)
            {
                value = string.Empty;
            }

            if (!value.EndsWith("/", StringComparison.Ordinal))
            {
                return value + "/";
            }

            return value;
        }
    }
}
