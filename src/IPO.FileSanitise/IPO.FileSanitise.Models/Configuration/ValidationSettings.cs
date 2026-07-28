using System.ComponentModel.DataAnnotations;

namespace IPO.FileSanitise.Models.Configuration
{
	public class ValidationSettings
	{
		[Required]
		public IEnumerable<string>? AcceptedFileExtensions { get; set; }
		[Required]
		public IEnumerable<string>? AcceptedFileMimeTypes { get; set; }
		[Required, Range(1, long.MaxValue)]
		public long SizeLimit { get; set; }
	}
}
