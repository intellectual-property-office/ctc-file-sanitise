using System.Diagnostics.CodeAnalysis;
using IPO.FileSanitise.Models.FileSanitise;
using IPO.FileSanitise.Interfaces;

namespace IPO.FileSanitise.BDDTests.FileSanitise
{
	[ExcludeFromCodeCoverage]
	public class MockedFileSanitiseValidator : IFileSanitiseValidator
	{
        public string GetErrorMessage(ErrorType errorType)
        {
            throw new NotImplementedException();
        }

        public FileSanitiseValidationResult Validate(Stream file, string fileName, string contentType)
		{
            throw new NotImplementedException();
        }

        public ErrorType? ValidateFormat(Stream file, string fileName)
        {
            throw new NotImplementedException();
        }
    }
}