#if DEBUG
using System.Diagnostics.CodeAnalysis;
using IPO.FileSanitise.Models;
#endif
using Aspose.Words;
using IPO.FileSanitise.Interfaces;
using IPO.FileSanitise.Models.FileSanitise;
using Microsoft.AspNetCore.Http;

namespace IPO.FileSanitise.Services.Sanitise
{
    public class DocXFileSanitiser : IFileSanitiser
    {
        public FileSanitiseResult Sanitise(IFormFile file)
        {
            using (var fileStream = file.OpenReadStream())
            {
                if (!fileStream.TryLoadWordFile(LoadFormat.Docx, out var document) || document == null)
                {
                    return FileSanitiseResult.CreateFailedToLoadResult();
                }

                var updatedFile = document.ClearAllAndUpdate(file, SaveFormat.Docx);

                return FileSanitiseResult.CreateSuccessResult(updatedFile);
            }
        }

#if DEBUG

        [ExcludeFromCodeCoverage]
        public IFormFile UpdateMetaData(IFormFile file, DocumentProperties value)
        {
            using (var fileStream = file.OpenReadStream())
            {
                if (!fileStream.TryLoadWordFile(LoadFormat.Docx, out var document) || document == null)
                {
                    return file;
                }

                document.BuiltInDocumentProperties.SetDocumentPropertiesFrom(value);
                document.SetDocumentCustomPropertiesFrom(value);

                return document.GetUpdatedDocument(file, SaveFormat.Docx);
            }
        }

        [ExcludeFromCodeCoverage]
        public TestMetaData? ReadMetaData(IFormFile file)
        {
            var result = new TestMetaDataWord();

            var fileStream = file.OpenReadStream();

            if (!fileStream.TryLoadWordFile(LoadFormat.Docx, out var document) || document == null)
            {
                return result;
            }

            result.GetDocumentPropertiesFrom(document.BuiltInDocumentProperties);
            result.GetDocumentCustomPropertiesFrom(document);

            return result;
        }

#endif
    }
}