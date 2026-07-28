namespace IPO.FileSanitise.Models.FileSanitise
{
	public class FileSanitiseValidationResult
	{
		public int Code { get; set; }
		public string? ErrorMessage { get; set; }
		public string? ErrorCode { get; set; }

		public static FileSanitiseValidationResult CreateSuccessValidationResult()
		{
			return new FileSanitiseValidationResult()
			{
				Code = 200,
				ErrorMessage = string.Empty,
				ErrorCode = string.Empty
			};
		}

		public static FileSanitiseValidationResult CreatePayloadTooLargeValidationResult(long sizeLimit)
		{
			return new FileSanitiseValidationResult()
			{
				Code = 413,
				ErrorMessage = $"File size larger than {(sizeLimit / 1024f / 1024f).ToString("#0")} MB.",
				ErrorCode = string.Empty
			};
		}

		public static FileSanitiseValidationResult CreateUnsupportedFileTypesValidationResult(IEnumerable<string> acceptedFileExtensions)
		{
			return new FileSanitiseValidationResult()
			{
				Code = 415,
				ErrorMessage = $"Unsupported file type, supported media types: {string.Join(", ", acceptedFileExtensions)}.",
				ErrorCode = string.Empty
			};
		}

		public static FileSanitiseValidationResult CreateUnsupportedMediaTypesValidationResult(IEnumerable<string> acceptedMediaExtensions)
		{
			return new FileSanitiseValidationResult()
			{
				Code = 415,
				ErrorMessage = $"Unsupported media type, supported media types: {string.Join(", ", acceptedMediaExtensions)}.",
				ErrorCode = string.Empty
			};
		}
	}
}
