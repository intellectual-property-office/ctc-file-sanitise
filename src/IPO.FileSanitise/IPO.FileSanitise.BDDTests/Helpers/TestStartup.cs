using IPO.FileSanitise.API;
using IPO.FileSanitise.BDDTests.FileSanitise;
using IPO.FileSanitise.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IPO.FileSanitise.BDDTests.Helpers
{
    public class TestStartup : Startup
    {
        public TestStartup(IConfiguration configuration) : base(configuration)
        { 
        }

		protected override void AddFileSanitiseValidators(IServiceCollection services)
		{
			services.AddScoped<IFileSanitiseValidator, MockedFileSanitiseValidator>();
		}

		protected override void AddFileSanitiseService(IServiceCollection services)
        {
            services.AddScoped<IFileSanitiseManagementService, MockedFileSanitiseManagementService>();
        }

        public static TestServer GetTestServer()
        {
            var hostBuilder = new HostBuilder()
               .ConfigureWebHost(webHost =>
               {
                   webHost
                      .UseTestServer()
                      .UseStartup<TestStartup>();
               });
            var host = hostBuilder.Start();
            return host.GetTestServer();
        }
    }
}