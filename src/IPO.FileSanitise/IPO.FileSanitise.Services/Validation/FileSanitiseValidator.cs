using Aspose.Words;
using Aspose.Words.Loading;
using IPO.Common.Infrastructure;
using IPO.FileSanitise.Interfaces;
using IPO.FileSanitise.Models.Configuration;
using IPO.FileSanitise.Models.FileSanitise;
using Microsoft.Extensions.Logging;
using Spire.Pdf;

namespace IPO.FileSanitise.Services.Validation
{
    public class FileSanitiseValidator : IFileSanitiseValidator
    {
        private readonly Settings _settings;
        private readonly ILogger<FileSanitiseValidator> _logger;

        public const string FileEncryptedErrorMessage = "Encrypted files are not supported.";
        public const string InvalidPDFVersionErrorMessage = "File must be a pdf with version 1.4 or higher.";
        public const string FileCannotBeLoadedErrorMessage = "The file cannot be loaded.";
        public const string NotClaimedFormatErrorMessage = "The file is not of the format claimed.";

        public FileSanitiseValidator(Settings settings, ILogger<FileSanitiseValidator> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public string GetErrorMessage(ErrorType errorType)
        {
            return errorType switch
            {
                ErrorType.FileEncrypted => FileEncryptedErrorMessage,
                ErrorType.InvalidPDFVersion => InvalidPDFVersionErrorMessage,
                ErrorType.FileCannotBeLoaded => FileCannotBeLoadedErrorMessage,
                ErrorType.NotClaimedFormat => NotClaimedFormatErrorMessage,
                _ => throw new NotImplementedException()
            };
        }

        public static StatusCodeException GetStatusCodeException<T>(int code, string errorMessage, string errorCode)
        {
            var error = Error.Create<T>(errorCode);
            error.Description += $" {errorMessage}";
            return new StatusCodeException(error, errorMessage, null, code);
        }

        public FileSanitiseValidationResult Validate(Stream file, string fileName, string contentType)
        {
            var maximumFileSize = _settings.ValidationSettings!.SizeLimit;

            if (file.Length > maximumFileSize)
            {
                return FileSanitiseValidationResult.CreatePayloadTooLargeValidationResult(maximumFileSize);
            }

            if (!_settings.ValidationSettings.AcceptedFileExtensions!.Contains(Path.GetExtension(fileName).ToUpperInvariant()))
            {
                return FileSanitiseValidationResult.CreateUnsupportedFileTypesValidationResult(_settings.ValidationSettings.AcceptedFileExtensions!);
            }

            if (!_settings.ValidationSettings.AcceptedFileMimeTypes!.Contains(contentType.ToUpperInvariant()))
            {
                return FileSanitiseValidationResult.CreateUnsupportedMediaTypesValidationResult(_settings.ValidationSettings.AcceptedFileMimeTypes!);
            }

            return FileSanitiseValidationResult.CreateSuccessValidationResult();
        }

        public ErrorType? ValidateFormat(Stream file, string fileName)
        {
            return GetDocumentType(fileName) switch
            {
                FileSanitiserFileType.Docx => ValidateWordDocument(file, LoadFormat.Docx),
                FileSanitiserFileType.Odt => ValidateWordDocument(file, LoadFormat.Odt),
                FileSanitiserFileType.Pdf => ValidatePdfDocument(file),
                FileSanitiserFileType.NotSupported => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };
        }

        public static FileSanitiserFileType GetDocumentType(string fileName)
        {
            Enum.TryParse(Path.GetExtension(fileName).Replace(".", ""), true, out FileSanitiserFileType type);

            return type;
        }

        private static bool HasValidVersion(PdfDocument document)
        {
            return document.FileInfo.Version switch
            {
                PdfVersion.Version1_0 or PdfVersion.Version1_1 or PdfVersion.Version1_2 or PdfVersion.Version1_3 => false,
                _ => true,
            };
        }

        private ErrorType? ValidatePdfDocument(Stream data)
        {
            try
            {
                var document = new PdfDocument();
                document.LoadFromStream(data);

                return (!HasValidVersion(document) ? ErrorType.InvalidPDFVersion : null);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("can not open an encrypted document. The password is invalid.", StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogError(ex, "The PDF document is encrypted.");
                    return ErrorType.FileEncrypted;
                }

                _logger.LogError(ex, "The PDF document cannot be loaded.");
                return ErrorType.FileCannotBeLoaded;
            }
        }

        private ErrorType? ValidateWordDocument(Stream data, LoadFormat loadFormat)
        {
            ErrorType? errorType = null;

            try
            {
                FileFormatInfo format = FileFormatUtil.DetectFileFormat(data);

                if (!loadFormat.Equals(format.LoadFormat))
                {
                    errorType = ErrorType.NotClaimedFormat;
                }

                _ = new Document(data, loadOptions: new LoadOptions() { LoadFormat = loadFormat });
            }
            catch (IncorrectPasswordException ex)
            {
                _logger.LogError(ex, "The Word document is encrypted.");
                return ErrorType.FileEncrypted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The Word document cannot be loaded.");
                return errorType == ErrorType.NotClaimedFormat ? errorType : ErrorType.FileCannotBeLoaded;
            }

            return errorType;
        }
    }
}