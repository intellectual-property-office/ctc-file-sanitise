#if DEBUG
using System.Diagnostics.CodeAnalysis;
using IPO.FileSanitise.Models;
using static IPO.FileSanitise.Models.TestMetaData;
#endif
using IPO.FileSanitise.Interfaces;
using IPO.FileSanitise.Models.FileSanitise;
using Microsoft.AspNetCore.Http;
using Spire.Pdf;

namespace IPO.FileSanitise.Services.Sanitise
{
    public class PdfFileSanitiser : IFileSanitiser
    {
        #region Fields and constructors

        /// <summary>Defines a collection of custom property names that should be ignored</summary>
        /// <seealso cref="PdfDocumentInformation.SetCustomProperty"/>
        private readonly string[] _ignoreCustomProperties;

        public PdfFileSanitiser()
        {
            _ignoreCustomProperties = new[]
                {
                    "Title",
                    "Author",
                    "Subject",
                    "Keywords",
                    "Creator",
                    "Producer",
                    "CreationDate",
                    "ModificationDate",
                    "Trap"
                };
        }

        #endregion

        public FileSanitiseResult Sanitise(IFormFile file)
        {
            var fileStream = file.OpenReadStream();

            if (!TryLoadFile(fileStream, out var document))
            {
                return FileSanitiseResult.CreateFailedToLoadResult();
            }

            document.DocumentInformation.With(di =>
            {
                di.Author = string.Empty;
                di.Creator = string.Empty;
#pragma warning disable CS0618
                di.CreationDate = DateTime.MinValue;
#pragma warning restore CS0618
                di.Keywords = string.Empty;
                di.Producer = string.Empty;
                di.Subject = string.Empty;
                di.Title = string.Empty;

                foreach (var prop in di.GetAllCustomProperties().Where(e => !_ignoreCustomProperties.Contains(e.Key)))
                {
                    // Annoyingly the custom property does not appear to actually be removed
                    // in the downloaded file.
                    //i.RemoveCustomProperty(prop.Key);
                    di.SetCustomProperty(prop.Key, string.Empty);
                }
            });

            var updatedFileStream = document.SaveToStream(FileFormat.PDF).First();

            var updatedFile = new FormFile(updatedFileStream, 0, updatedFileStream.Length, file.ContentDisposition, file.FileName)
            {
                Headers = file.Headers ?? new HeaderDictionary()
            };

            updatedFile.ContentType = file.ContentType;

            return FileSanitiseResult.CreateSuccessResult(updatedFile);
        }

#if DEBUG

        [ExcludeFromCodeCoverage]
        public IFormFile UpdateMetaData(IFormFile file, DocumentProperties value)
        {
            var fileStream = file.OpenReadStream();

            if (!TryLoadFile(fileStream, out var document))
            {
                return file;
            }

            document.DocumentInformation.With(di =>
            {
                di.Author = DocumentProperties.TryFind(nameof(di.Author), value, out string? auth) ? auth : (di.Author == null ? "" : di.Author);
                di.Creator = DocumentProperties.TryFind(nameof(di.Creator), value, out string? cre) ? cre : (di.Creator == null ? "" : di.Creator);
#pragma warning disable CS0618
                di.CreationDate = DocumentProperties.TryFind(nameof(di.CreationDate), value, out string? crd) ? DateTime.Parse(crd!) : di.CreationDate;
                di.ModificationDate = DocumentProperties.TryFind(nameof(di.ModificationDate), value, out string? mod) ? DateTime.Parse(mod!) : di.ModificationDate;
#pragma warning restore CS0618
                di.Keywords = DocumentProperties.TryFind(nameof(di.Keywords), value, out string? kw) ? kw : (di.Keywords == null ? "" : di.Keywords);
                di.Producer = DocumentProperties.TryFind(nameof(di.Producer), value, out string? prod) ? prod : (di.Producer == null ? "" : di.Producer);
                di.Subject = DocumentProperties.TryFind(nameof(di.Subject), value, out string? sub) ? sub : (di.Subject == null ? "" : di.Subject);
                di.Title = DocumentProperties.TryFind(nameof(di.Title), value, out string? title) ? title : (di.Title == null ? "" : di.Title);

                foreach (var prop in value.Custom.Where(e => !_ignoreCustomProperties.Contains(e.Name)))
                {
                    di.SetCustomProperty(prop.Name, prop.Value);
                }
            });

            var updatedFileStream = document.SaveToStream(FileFormat.PDF).First();
            var updatedFile = new FormFile(updatedFileStream, 0, updatedFileStream.Length, file.ContentDisposition, file.FileName);
            updatedFile.Headers = file.Headers ?? new HeaderDictionary();

            return updatedFile;
        }

        [ExcludeFromCodeCoverage]
        public TestMetaData? ReadMetaData(IFormFile file)
        {
            var fileStream = file.OpenReadStream();

            if (!TryLoadFile(fileStream, out var document))
            {
                // todo failed to load error
                return null;
            }

            var result = new TestMetaDataPdf();

            document.DocumentInformation.With(di =>
            {
                result.Author = di.Author;
                result.Creator = di.Creator;
#pragma warning disable CS0618
                result.CreationDate = di.CreationDate;
                result.ModificationDate = di.ModificationDate;
#pragma warning restore CS0618
                result.Keywords = di.Keywords;
                result.Producer = di.Producer;
                result.Subject = di.Subject;
                result.Title = di.Title;

                foreach (var prop in di.GetAllCustomProperties().Where(e => !_ignoreCustomProperties.Contains(e.Key)))
                {
                    result.CustomProperties.Add(new CustomProperty { Name = prop.Key, Value = prop.Value });
                }
            });

            return result;
        }
#endif

        #region Internal

        private static bool TryLoadFile(Stream data, out PdfDocument document)
        {
            try
            {
                document = new PdfDocument();
                document.LoadFromStream(data);
                return true;
            }
            catch (Exception ex)
            {
                document = null!;

                if (ex.Message.Equals("can not open an encrypted document. The password is invalid.", StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }

                return false;
            }
        }

        #endregion
    }
}