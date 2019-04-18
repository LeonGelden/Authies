using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ResourceApi1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddAuthentication(cfg =>
            {
                cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = "https://login.microsoftonline.com/d2e0d94e-edef-4a1c-a2fe-c60200920149/v2.0";
                options.Audience = "https://klowdsoft.onmicrosoft.com/ResourceApi1";
                //options.TokenValidationParameters = new TokenValidationParameters
                //{
                //    ValidateIssuer = false,
                //    IssuerValidator = CustomIssuerValidator,
                //    SignatureValidator = CustomSignatureValidator,
                //    IssuerSigningKeyResolver = CustomIssuerSigningKeyResolver,
                //    IssuerSigningKeyValidator = CustomIssuerSigningKeyValidator,

                //    ValidateAudience = true,
                //    ValidAudiences = new string[]
                //    {
                //        "ae31802a-4851-4897-8280-1be1f56b6d6e",
                //        "Authies.ResourceApi1.Read"
                //    },

                //    ValidateLifetime = true,
                //};

                options.IncludeErrorDetails = true;
            });
        }

        private bool CustomIssuerSigningKeyValidator(SecurityKey securityKey, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            // this should definitely not be implemented like this!
            return true;
        }

        private IEnumerable<SecurityKey> CustomIssuerSigningKeyResolver(string token, SecurityToken securityToken, string kid, TokenValidationParameters validationParameters)
        {
            return null;
        }

        private SecurityToken CustomSignatureValidator(string token, TokenValidationParameters validationParameters)
        {
            return new JwtSecurityTokenHandler().ReadToken(token);
        }

        private string CustomIssuerValidator(string issuer, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            // this is where we would define the valid issuers that are allowed
            if (true)
                return issuer;
            else
                // if issue is not valid
                throw new SecurityTokenInvalidIssuerException("Invalid issuer");
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
                app.UseHsts();
            }

            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
