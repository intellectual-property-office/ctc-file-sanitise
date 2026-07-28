#if DEBUG
using IPO.FileSanitise.Models;
#endif
using IPO.FileSanitise.Models.API;
using Microsoft.AspNetCore.Http;

namespace IPO.FileSanitise.Interfaces
{
    public interface IFileSanitiseManagementService
    {
        SanitisedFile Sanitise(IFormFile file);

#if DEBUG
        TestMetaData? GetMetaData(IFormFile file);

        IFormFile? SetMetaData(IFormFile file, DocumentProperties metaData);
#endif
    }
}