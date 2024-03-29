using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pollr.Server.Common;
using Pollr.Server.Hubs;
using Pollr.Server.Services;

namespace Pollr.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public string HostUrl { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSignalR();

            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSingleton<CountManager>();
            services.AddSingleton<PollManager>();

            services.AddTransient(svc =>
            {
                var baseUrl = svc.GetService<IHttpContextAccessor>().HttpContext.Request.Host;

                var connection = new HubConnectionBuilder()
                    .WithUrl($"https://{baseUrl}/counthub")
                    .WithAutomaticReconnect()
                    .Build();

                return new CountHubProxy(connection);
            });

            services.AddTransient(svc =>
            {
                var baseUrl = svc.GetService<IHttpContextAccessor>().HttpContext.Request.Host;

                var connection = new HubConnectionBuilder()
                    .WithUrl($"https://{baseUrl}/pollhub")
                    .WithAutomaticReconnect()
                    .Build();

                return new PollHubProxy(connection);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapHub<CountHub>("/counthub");
                endpoints.MapHub<PollHub>("/pollhub");
            });
        }
    }
}
