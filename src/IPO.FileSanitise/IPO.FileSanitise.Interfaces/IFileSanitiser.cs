#if DEBUG
using IPO.FileSanitise.Models;
#endif
using IPO.FileSanitise.Models.FileSanitise;
using Microsoft.AspNetCore.Http;

namespace IPO.FileSanitise.Interfaces
{
    public interface IFileSanitiser
    {
        FileSanitiseResult Sanitise(IFormFile file);

#if DEBUG
        IFormFile UpdateMetaData(IFormFile file, DocumentProperties values);

        TestMetaData? ReadMetaData(IFormFile file); 
#endif
    }
}