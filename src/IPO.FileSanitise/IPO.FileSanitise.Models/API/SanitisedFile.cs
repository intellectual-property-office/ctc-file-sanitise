using System.Diagnostics.CodeAnalysis;

namespace IPO.FileSanitise.Models.API
{
	[ExcludeFromCodeCoverage]
	public class SanitisedFile(string fileName, string contentType, Stream data)
	{
		public string FileName { get; set; } = fileName;
		public string ContentType { get; set; } = contentType;
		public Stream Data { get; set; } = data;
	}
}
