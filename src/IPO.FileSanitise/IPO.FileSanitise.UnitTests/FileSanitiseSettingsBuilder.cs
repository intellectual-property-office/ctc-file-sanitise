using IPO.FileSanitise.Models.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace IPO.FileSanitise.UnitTests
{
	public static class FileSanitiseSettingsBuilder
	{
		[ExcludeFromCodeCoverage]
		public static Settings Build(int sizeLimit = 2024)
		{
			return new Settings()
			{
				ValidationSettings = new ValidationSettings()
				{
					AcceptedFileExtensions = new string[] { ".ODT", ".DOCX", ".PDF" },
					AcceptedFileMimeTypes = new string[] { "application/vnd.oasis.opendocument.text".ToUpperInvariant(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document".ToUpperInvariant(), "application/pdf".ToUpperInvariant() },
					SizeLimit = sizeLimit
				}
			};
		}
	}
}
