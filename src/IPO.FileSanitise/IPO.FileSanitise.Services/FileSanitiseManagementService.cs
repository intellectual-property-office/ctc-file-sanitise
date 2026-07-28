#if DEBUG
using System.Diagnostics.CodeAnalysis;
using IPO.FileSanitise.Models;
#endif
using IPO.FileSanitise.Interfaces;
using IPO.FileSanitise.Models.API;
using IPO.FileSanitise.Services.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IPO.FileSanitise.Services
{
    public class FileSanitiseManagementService : IFileSanitiseManagementService
    {
        private readonly ILogger<FileSanitiseManagementService> _logger;
        private readonly IFileSanitiseValidator _fileSanitiseValidator;
        private readonly IFileSanitiserFactory _fileSanitiseFactory;

        public FileSanitiseManagementService(ILogger<FileSanitiseManagementService> logger,
                                             IFileSanitiseValidator fileSanitiseValidator,
                                             IFileSanitiserFactory fileSanitiseFactory)
        {
            _logger = logger;
            _fileSanitiseValidator = fileSanitiseValidator ?? throw new ArgumentNullException(nameof(fileSanitiseValidator));
            _fileSanitiseFactory = fileSanitiseFactory ?? throw new ArgumentNullException(nameof(fileSanitiseFactory));
        }

        public SanitisedFile Sanitise(IFormFile file)
        {
            var fileData = file.OpenReadStream();
            var validationResult = _fileSanitiseValidator.Validate(fileData,
                                                              file.FileName,
                                                              file.ContentType);
            if (validationResult.Code != 200)
            {
                throw StatusCodeExceptionFactory.GetStatusCodeException<FileSanitiseValidator>(validationResult.Code, validationResult.ErrorMessage!, "E002");
            }

            var formatValidationResult = _fileSanitiseValidator.ValidateFormat(fileData, file.FileName);

            if (formatValidationResult != null)
            {
                throw FileSanitiseValidator.GetStatusCodeException<FileSanitiseValidator>(415, _fileSanitiseValidator.GetErrorMessage(formatValidationResult.Value), "E002");
            }

            var sanitiser = _fileSanitiseFactory.Build(Path.GetExtension(file.FileName));

            if (sanitiser == null)
            {
                throw StatusCodeExceptionFactory.GetStatusCodeException<FileSanitiseManagementService>(422, $" A sanitiser for the file submitted ('{file.ContentType}') was not found", "E004");
            }

            var result = sanitiser.Sanitise(file);

            if (result.Code != 200)
            {
                throw StatusCodeExceptionFactory.GetStatusCodeException<FileSanitiseManagementService>(result.Code, result.ErrorMessage, "E003");
            }

            return new SanitisedFile(result.Updated!.FileName, file.ContentType, result.Updated.OpenReadStream());
        }

#if DEBUG

        [ExcludeFromCodeCoverage]
        public TestMetaData? GetMetaData(IFormFile file)
        {
            var fileData = file.OpenReadStream();
            var validationResult = _fileSanitiseValidator.Validate(fileData, file.FileName, file.ContentType);

            if (validationResult.Code != 200)
            {
                throw StatusCodeExceptionFactory.GetStatusCodeException<FileSanitiseValidator>(validationResult.Code, validationResult.ErrorMessage!, "E002");
            }

            var sanitiser = _fileSanitiseFactory.Build(Path.GetExtension(file.FileName));

            if (sanitiser == null)
            {
                throw StatusCodeExceptionFactory.GetStatusCodeException<FileSanitiseManagementService>(422, $" A sanitiser for the file submitted ('{file.ContentType}') was not found", "E004");
            }

            var metaData = sanitiser.ReadMetaData(file);

            return metaData;
        }

        [ExcludeFromCodeCoverage]
        public IFormFile? SetMetaData(IFormFile file, DocumentProperties value)
        {
            var fileData = file.OpenReadStream();
            var validationResult = _fileSanitiseValidator.Validate(fileData, file.FileName, file.ContentType);

            if (validationResult.Code != 200)
            {
                throw StatusCodeExceptionFactory.GetStatusCodeException<FileSanitiseValidator>(validationResult.Code, validationResult.ErrorMessage!, "E002");
            }

            var sanitiser = _fileSanitiseFactory.Build(Path.GetExtension(file.FileName));

            if (sanitiser == null)
            {
                throw StatusCodeExceptionFactory.GetStatusCodeException<FileSanitiseManagementService>(422, $" A sanitiser for the file submitted ('{file.ContentType}') was not found", "E004");
            }

            var result = sanitiser.UpdateMetaData(file, value);

            return result;
        }
#endif
    }
}