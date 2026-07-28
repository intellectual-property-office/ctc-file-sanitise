using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace IPO.FileSanitise.Models.API
{
	public class SanitiseFileRequest
	{
		[Required(ErrorMessage = "The file is required.")]
		public IFormFile? file { get; set; }
	}
}
