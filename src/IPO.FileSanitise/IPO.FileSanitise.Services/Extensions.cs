using Aspose.Words;
using Aspose.Words.Loading;
using Aspose.Words.Properties;
using Microsoft.AspNetCore.Http;
#if DEBUG
using IPO.FileSanitise.Models;
using static IPO.FileSanitise.Models.TestMetaData;
#endif

namespace IPO.FileSanitise.Services
{
    internal static class Extensions
    {
        /// <summary>
        /// Clears a documents properties, custom document properties and returns a new 
        /// <see cref="FormFile"/> representing the changes.
        /// </summary>
        /// <param name="document">The <see cref="Document"/> being extended by this method.</param>
        /// <param name="original">The original <see cref="IFormFile"/> to be updated.</param>
        /// <param name="format">The word document format to save the updated document as.</param>
        /// <returns>The new <see cref="FormFile"/> with the updated applied.</returns>
        internal static FormFile ClearAllAndUpdate(this Document document, IFormFile original, SaveFormat format)
        {
            document.UnlinkFields();
            document.BuiltInDocumentProperties.ClearDocumentProperties();
            document.CustomDocumentProperties.Clear();
            return GetUpdatedDocument(document, original, format);
        }

        internal static void ClearDocumentProperties(this BuiltInDocumentProperties docProperties)
        {
            docProperties.With(dp =>
            {
                dp.Manager = string.Empty;
                dp.Author = string.Empty;
                dp.LastSavedBy = string.Empty;
                dp.LastSavedTime = DateTime.MinValue;
                dp.LastPrinted = DateTime.MinValue;
                dp.CreatedTime = DateTime.MinValue;
                dp.Title = string.Empty;
                dp.Comments = string.Empty;
                dp.Template = string.Empty;
                dp.ContentStatus = string.Empty;
                dp.Category = string.Empty;
                dp.Subject = string.Empty;
                dp.HyperlinkBase = string.Empty;
                dp.Company = string.Empty;
                dp.Keywords = string.Empty;
            });
        }

        internal static FormFile GetUpdatedDocument(this Document document, IFormFile file, SaveFormat format)
        {
            var updatedFileStream = new MemoryStream();
            document.Save(updatedFileStream, format);

            var updatedFile = new FormFile(updatedFileStream, 0, updatedFileStream.Length, file.ContentDisposition, file.FileName)
            {
                Headers = file.Headers ?? new HeaderDictionary(),
                ContentType = file.ContentType
            };

            return updatedFile;
        }

#if DEBUG

        internal static void GetDocumentPropertiesFrom(this TestMetaDataWord document, BuiltInDocumentProperties docProperties)
        {
            docProperties.With(dp =>
            {
                document.Manager = dp.Manager;
                document.Author = dp.Author;
                document.LastSavedBy = dp.LastSavedBy;
                document.ModificationDate = dp.LastSavedTime;
                document.LastPrinted = dp.LastPrinted;
                document.CreationDate = dp.CreatedTime;
                document.Title = dp.Title;
                document.Comments = dp.Comments;
                document.Template = dp.Template;
                document.ContentStatus = dp.ContentStatus;
                document.Category = dp.Category;
                document.Subject = dp.Subject;
                document.HyperlinkBase = dp.HyperlinkBase;
                document.Company = dp.Company;
                document.Keywords = dp.Keywords;
            });
        }

        internal static void GetDocumentCustomPropertiesFrom(this TestMetaDataWord result, Document document)
        {
            document.CustomDocumentProperties.With(di =>
            {
                foreach (var prop in di)
                {
                    result.CustomProperties.Add(new CustomProperty(prop.Name, prop.Value));
                }
            });
        }

        internal static void SetDocumentPropertiesFrom(this BuiltInDocumentProperties docProperties, DocumentProperties value)
        {
            docProperties.With(dp =>
            {
                dp.LastSavedBy = DocumentProperties.TryFind(nameof(dp.LastSavedBy), value, out string? lsb) ? lsb : dp.LastSavedBy;
                dp.Manager = DocumentProperties.TryFind(nameof(dp.Manager), value, out string? man) ? man : dp.Manager;
                dp.LastPrinted = DocumentProperties.TryFind(nameof(dp.LastPrinted), value, out string? lp) ? DateTime.Parse(lp!) : dp.LastPrinted;
                dp.Comments = DocumentProperties.TryFind(nameof(dp.Comments), value, out string? comm) ? comm : dp.Comments;
                dp.Template = DocumentProperties.TryFind(nameof(dp.Template), value, out string? temp) ? temp : dp.Template;
                dp.ContentStatus = DocumentProperties.TryFind(nameof(dp.ContentStatus), value, out string? cs) ? cs : dp.ContentStatus;
                dp.Category = DocumentProperties.TryFind(nameof(dp.Category), value, out string? cat) ? cat : dp.Category;
                dp.HyperlinkBase = DocumentProperties.TryFind(nameof(dp.HyperlinkBase), value, out string? hlb) ? hlb : dp.HyperlinkBase;
                dp.Company = DocumentProperties.TryFind(nameof(dp.Company), value, out string? comp) ? comp : dp.Company;
                dp.Author = DocumentProperties.TryFind(nameof(dp.Author), value, out string? auth) ? auth : dp.Author;
                dp.CreatedTime = DocumentProperties.TryFind(nameof(dp.CreatedTime), value, out string? cre) ? DateTime.Parse(cre!) : dp.CreatedTime;
                dp.Keywords = DocumentProperties.TryFind(nameof(dp.Keywords), value, out string? key) ? key : dp.Keywords;
                dp.LastSavedTime = DocumentProperties.TryFind(nameof(dp.LastSavedTime), value, out string? lst) ? DateTime.Parse(lst!) : dp.LastSavedTime;
                dp.Subject = DocumentProperties.TryFind(nameof(dp.Subject), value, out string? sub) ? sub : dp.Subject;
                dp.Title = DocumentProperties.TryFind(nameof(dp.Title), value, out string? title) ? title : dp.Title;
            });
        }

        internal static void SetDocumentCustomPropertiesFrom(this Document document, DocumentProperties value)
        {
            foreach (var customProperty in value.Custom)
            {
                if (document.CustomDocumentProperties.Contains(customProperty.Name))
                {
                    document.CustomDocumentProperties[customProperty.Name].Value = customProperty.Value;
                }
                else
                {
                    document.CustomDocumentProperties.Add(customProperty.Name, customProperty.Value);
                }
            }
        }

#endif

        internal static bool TryLoadWordFile(this Stream data, LoadFormat loadFormat, out Document? document)
        {
            document = null;

            try
            {
                _ = FileFormatUtil.DetectFileFormat(data);
                document = new Document(data, loadOptions: new LoadOptions() { LoadFormat = loadFormat });
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static T With<T>(this T obj, Action<T>? action)
        {
            if (obj == null || action == null)
            {
                return obj;
            }

            action(obj);
            return obj;
        }
    }
}