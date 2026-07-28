using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using IPO.Common.API;
using IPO.FileSanitise.Interfaces;
using IPO.FileSanitise.Models.Configuration;
using IPO.FileSanitise.Services;
using IPO.FileSanitise.Services.Sanitise;
using IPO.FileSanitise.Services.Validation;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.OpenApi.Models;

namespace IPO.FileSanitise.API
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Helper = new IPOStartupHelper("IPO.FileSanitise.API", "version");
        }

        public IConfiguration Configuration { get; }

        public IPOStartupHelper Helper { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Helper.AddIPOServicesConfiguration(services);
            services.AddSingleton(typeof(ILogger), typeof(Logger<Startup>));
            services.AddSwaggerGen(config =>
            {
                config.SchemaGeneratorOptions.CustomTypeMappings.Add(typeof(IFormFile)
                    , () => new OpenApiSchema()
                    {
                        Type = "file",
                        Format = "binary"
                    });
                config.EnableAnnotations();
            });

            AddFileSanitiseValidators(services);
            AddFileSanitiserServices(services);
            AddFileSanitiseService(services);

            services.Configure<Settings>(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRewriter(new RewriteOptions().Add(RewriteRules.RewriteAlwaysOn));

            Helper.UseIPOConfigurations(app, env);

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        protected virtual void AddFileSanitiseValidators(IServiceCollection services)
        {
            services.AddIPOErrorAwareScoped<IFileSanitiseValidator, FileSanitiseValidator>("E002");
        }

        private void AddFileSanitiserServices(IServiceCollection services)
        {
            services.AddIPOErrorAwareScoped<IFileSanitiserFactory, FileSanitiserFactory>("E004");

            services.AddIPOErrorAwareKeyedScoped<IFileSanitiser, DocXFileSanitiser>("E005", serviceKey: ".docx");
            services.AddIPOErrorAwareKeyedScoped<IFileSanitiser, OdtFileSanitiser>("E006", serviceKey: ".odt");
            services.AddIPOErrorAwareKeyedScoped<IFileSanitiser, PdfFileSanitiser>("E007", serviceKey: ".pdf");
            
            AddSpireLicense(Configuration.GetValue<string>("PdfLibraryLicenseKey")!);
            AddAsposeLicense(Configuration.GetValue<string>("WordLibraryLicenseKey")!);
        }

        protected virtual void AddFileSanitiseService(IServiceCollection services)
        {
            var validationSettings = new ValidationSettings()
            {
                AcceptedFileExtensions = Configuration["ValidationSettings:AcceptedFileExtensions"]!.ToUpperInvariant().Split(','),
                AcceptedFileMimeTypes = Configuration["ValidationSettings:AcceptedFileMimeTypes"]!.ToUpperInvariant().Split(','),
                SizeLimit = long.Parse(Configuration["ValidationSettings:SizeLimit"]!)
            };
            Validator.ValidateObject(validationSettings, new ValidationContext(validationSettings), validateAllProperties: true);

            var settings = new Settings()
            {
                ValidationSettings = validationSettings
            };
            Validator.ValidateObject(settings, new ValidationContext(settings), validateAllProperties: true);

            services.AddScoped<Settings>(x => settings);
            services.AddIPOErrorAwareScoped<IFileSanitiseManagementService, FileSanitiseManagementService>("E003");
        }

        protected virtual void AddSpireLicense(string licenseKey)
        {
            if (string.IsNullOrEmpty(licenseKey) || licenseKey.Equals("test", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            Spire.Pdf.License.LicenseProvider.SetLicense(new MemoryStream(Convert.FromBase64String(licenseKey)));
        }

        protected virtual void AddAsposeLicense(string licenseKey)
        {
            if (string.IsNullOrEmpty(licenseKey) || licenseKey.Equals("test", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            var license = new Aspose.Words.License();
            license.SetLicense(new MemoryStream(Convert.FromBase64String(licenseKey)));
        }
    }
}