using Microsoft.AspNetCore.Http;

namespace IPO.FileSanitise.Models.FileSanitise
{
	public class FileSanitiseResult
	{
		public int Code { get; set; } = 200;

		public string ErrorMessage { get; set; } = string.Empty;

		public string? ErrorCode { get; set; } = string.Empty;

        public IFormFile? Updated { get; set; }

        public static FileSanitiseResult CreateSuccessResult(IFormFile updated)
		{
			return new FileSanitiseResult()
			{
				Code = 200,
				ErrorMessage = string.Empty,
				ErrorCode = string.Empty,
                Updated = updated
            };
		}

		public static FileSanitiseResult CreateFailedToLoadResult()
		{
			return new FileSanitiseResult()
			{
				Code = 422,
				ErrorMessage = $"The submitted File could not be loaded",
				ErrorCode = string.Empty,
				Updated = null
			};
		}
    }
}