// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdService.Data;
using IdService.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Threading.Tasks;

namespace IdService
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

            var connectionString = Configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IISOptions>(options =>
            {
                options.AutomaticAuthentication = false;
                options.AuthenticationDisplayName = "Windows";
            });

            var identityServer = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
            .AddTestUsers(TestUsers.Users)
            // this adds the config data from DB (clients, resources, CORS)
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString);
            })
            // this adds the operational data from DB (codes, tokens, consents)
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString);

                // this enables automatic token cleanup. this is optional.
                options.EnableTokenCleanup = true;
            })
            .AddInMemoryPersistedGrants()
            .AddAspNetIdentity<ApplicationUser>();


            if (Environment.IsDevelopment())
            {
                identityServer.AddDeveloperSigningCredential();
            }
            else
            {
                throw new Exception("need to configure key material");
            }

            services
            .AddAuthentication()
            .AddGoogle(options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                // register your IdentityServer with Google at https://console.developers.google.com
                // enable the Google+ API
                // set the redirect URI to http://localhost:5000/signin-google
                options.ClientId = "905933452317-v0bbf22aibjhg7idpp6kgikssg5r1o7b.apps.googleusercontent.com";
                options.ClientSecret = "w1FLicMD3PIUJnqsccqaJ5WG";
            })
            .AddOpenIdConnect("oidc-aad", "Azure AD", aadOptions =>
            {
                aadOptions.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                aadOptions.Authority = "https://login.microsoftonline.com/574f98af-654b-466c-9f76-b07f7a64f1a9/";
                aadOptions.CallbackPath = new PathString("/signin-aad");
                aadOptions.ClientId = "504922f5-7fc9-4e37-a838-3899bba655f0";
                aadOptions.ClientSecret = "T!|Y#:=@N9%W_]>=G&l{XQvi(0D5#c]N;4-a";
                aadOptions.TokenValidationParameters = new TokenValidationParameters() { NameClaimType = "name" };

            })
            .AddOpenIdConnect("oidc-aadb2c", "Azure AD B2C", aadOptions =>
            {
                aadOptions.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                aadOptions.Authority = "https://login.microsoftonline.com/tfp/ac3b0247-b6bd-49d1-a02a-94bf02620874/B2C_1_SignUpIn/v2.0";
                aadOptions.CallbackPath = new PathString("/signin-aadb2c");
                aadOptions.ClientId = "5898583c-ef8d-4516-a20c-e9c196864e34";
                aadOptions.ClientSecret = "ro8|&2kasYN9Tx&{taM;8.K)";
                aadOptions.TokenValidationParameters = new TokenValidationParameters() { NameClaimType = "name" };
            })
            .AddOpenIdConnect("oidc-onelogin", "OneLogin", oneLoginoptions =>
            {
                oneLoginoptions.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                oneLoginoptions.Authority = "https://openid-connect.onelogin.com/oidc";
                oneLoginoptions.ClientId = "e80fab80-3d3a-0137-eded-022a22e482b8146383";
                oneLoginoptions.ClientSecret = "26cea401afda96785eabc32603f8bdeefacd2944dfc7282954cfc9712c398791";
                oneLoginoptions.CallbackPath = new PathString("/signin-onelogin");
                oneLoginoptions.TokenValidationParameters = new TokenValidationParameters() { NameClaimType = "name" };

                oneLoginoptions.Events = new OpenIdConnectEvents()
                {
                    OnRedirectToIdentityProviderForSignOut = context =>
                    {
                        context.ProtocolMessage.IssuerAddress = "https://onk2labs-dev.onelogin.com/logout";
                        context.ProtocolMessage.PostLogoutRedirectUri = "https://localhost:5000";

                        return Task.CompletedTask;
                    },
                };
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseIdentityServer();
            app.UseMvcWithDefaultRoute();
        }
    }
}