using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace ClientWebApp
{
    public class Startup
    {
        private readonly string ClientId = "clientwebapp";
        private readonly string ClientSecret = "@Cyclo31!";
        private readonly string Authority = "https://localhost:5001";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddAuthentication(options =>
            {
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            //.AddWsFederation(wsfedOptions =>
            //{

            //})
            .AddOpenIdConnect(oidcOptions =>
            {
                oidcOptions.AuthenticationMethod = OpenIdConnectRedirectBehavior.FormPost;
                oidcOptions.Authority = Authority;
                oidcOptions.ClientId = ClientId;
                oidcOptions.ClientSecret = ClientSecret;
                oidcOptions.CallbackPath = new PathString("/signin-oidc");
                oidcOptions.Scope.Add("resource.api1.read");
                oidcOptions.Scope.Add("offline_access");
                oidcOptions.ResponseType = OpenIdConnectResponseType.CodeIdToken;

                oidcOptions.TokenValidationParameters = new TokenValidationParameters()
                {
                    NameClaimType = "name",                    
                };

                oidcOptions.Events = new OpenIdConnectEvents()
                {
                    //OnAuthorizationCodeReceived = OnAuthorizationCodeReceived,
                };

                oidcOptions.ClaimActions.Clear();
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        private Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedContext arg)
        {
            var code = arg.ProtocolMessage.Code;

            return Task.CompletedTask;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
