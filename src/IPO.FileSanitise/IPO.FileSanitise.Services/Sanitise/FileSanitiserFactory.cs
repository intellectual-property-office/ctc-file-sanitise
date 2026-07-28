using System.Diagnostics.CodeAnalysis;
using IPO.FileSanitise.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace IPO.FileSanitise.Services.Sanitise
{
    [ExcludeFromCodeCoverage(Justification = "GetKeyedService extension method is not Moq'able")]
    public class FileSanitiserFactory : IFileSanitiserFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public FileSanitiserFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IFileSanitiser? Build(string fileType)
        {
            switch (fileType.ToLower())
            {
                case ".odt":
                    return _serviceProvider.GetKeyedService<IFileSanitiser>(".odt");
                case ".docx":
                    return _serviceProvider.GetKeyedService<IFileSanitiser>(".docx");
                case ".pdf":
                    return _serviceProvider.GetKeyedService<IFileSanitiser>(".pdf");
                default:
                    return null;
            }
        }
    }
}