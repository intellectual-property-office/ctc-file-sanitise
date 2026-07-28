using IPO.FileSanitise.Models.FileSanitise;

namespace IPO.FileSanitise.Interfaces
{
	public interface IFileSanitiseValidator
	{
        string GetErrorMessage(ErrorType errorType);

        FileSanitiseValidationResult Validate(Stream file, string fileName, string contentType);

		ErrorType? ValidateFormat(Stream file, string fileName);
    }
}