using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Demo.CsomDotnetCore.Core;
using Demo.CsomDotnetCore.Core.Authentication;
using Demo.CsomDotnetCore.Services;
using Demo.CsomDotnetCore.Services.Contracts;

namespace Demo.CsomDotnetCore.WebApi
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
            services.AddControllers();

            // Services
            services.AddMemoryCache();
            services.AddScoped<ISharePointService, SharePointService>();
            services.AddScoped<IAccessTokenProvider, DefaultAzureCredentialAccessTokenProvider>(); // can be interchanged with UserCredentialAccessTokenProvider, which requires app settings: UserUpn, UserPassword
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Required for EnvironmentCredential (local dev)
                var clientId = Configuration.GetValue<string>(Globals.AppSettings.ClientId);
                var tenantId = Configuration.GetValue<string>(Globals.AppSettings.TenantId);
                var devCertPath = Configuration.GetValue<string>(Globals.AppSettings.DevCertificatePath);

                Environment.SetEnvironmentVariable(Globals.AuthSettings.AZURE_CLIENT_ID, clientId);
                Environment.SetEnvironmentVariable(Globals.AuthSettings.AZURE_TENANT_ID, tenantId);
                Environment.SetEnvironmentVariable(Globals.AuthSettings.AZURE_CLIENT_CERTIFICATE_PATH, devCertPath);
            }

            app.UseHttpsRedirection();
            
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
