#if DEBUG
using System.Diagnostics.CodeAnalysis;
using IPO.FileSanitise.Models;
#endif
using IPO.FileSanitise.Interfaces;
using IPO.FileSanitise.Models.API;
using Microsoft.AspNetCore.Http;

namespace IPO.FileSanitise.BDDTests.FileSanitise
{
    public class MockedFileSanitiseManagementService : IFileSanitiseManagementService
    {
        public SanitisedFile Sanitise(IFormFile file)
        {
			//Note: hard coded Contentype to pdf as not teting with a real pdf file so this gets dropped by middleware
            return new SanitisedFile(file.FileName, "application/pdf", file.OpenReadStream());
		}

#if DEBUG

        [ExcludeFromCodeCoverage]
        public IFormFile SetMetaData(IFormFile file, DocumentProperties value)
        {
            return file;
        }

        [ExcludeFromCodeCoverage]
        public TestMetaData? GetMetaData(IFormFile file)
        {
            return new TestMetaData();
        }
#endif
    }
}