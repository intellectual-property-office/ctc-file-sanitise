using System.ComponentModel.DataAnnotations;

namespace IPO.FileSanitise.Models.Configuration
{
    public class Settings
    {
		[Required]
		public ValidationSettings? ValidationSettings { get; set; }
	}
}