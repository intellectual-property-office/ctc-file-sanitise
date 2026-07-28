#if DEBUG
using System.Diagnostics.CodeAnalysis;
using IPO.FileSanitise.API.ModelBinders;
using IPO.FileSanitise.Models;
#endif
using IPO.Common.Infrastructure;
using IPO.FileSanitise.Interfaces;
using IPO.FileSanitise.Models.API;
using IPO.FileSanitise.Models.API.Validation;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IPO.FileSanitise.API.Controllers
{
    [Route("/")]
    [ApiController]
    public class FileSanitiseController : ControllerBase
    {
        private IFileSanitiseManagementService _fileSanitiseManagementService { get; }

        public FileSanitiseController(IFileSanitiseManagementService fileSanitiseManagementService)
        {
            _fileSanitiseManagementService = fileSanitiseManagementService;
        }

        [SwaggerOperation(
            Summary = "Accepts a file to sanitise and returns the sanitised file.",
            Description = "**Notes:** \n\n See the Integration guide for limitations of file size and formats\n\n" +
            "[See the API Limitations guide](https://dev.azure.com/Ukipo/CTC-Programme/_wiki/wikis/CTC-Programme.wiki/14851/API-Limitations?anchor=document-processing-limitations) for information on limitations in processing documents.")]
        [Produces("application/json")]
        [HttpPost]
        [Route("/")]
        [RequestSizeLimit(115343360)]
        [RequestFormLimits(MultipartBodyLengthLimit = 115343360)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status413PayloadTooLarge, Type = typeof(IPOErrorResponse))]
        [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType, Type = typeof(IPOErrorResponse))]
        public ActionResult SanitiseFile([FromForm, OnlyOneFormFileIsAllowed] SanitiseFileRequest model)
        {
            var file = _fileSanitiseManagementService.Sanitise(model.file!);

            return File(file.Data, file.ContentType, file.FileName);
        }

#if DEBUG

        [HttpPost]
        [Route("/setmetadata")]
        [ExcludeFromCodeCoverage]
        [SwaggerOperation(
            Summary = "Accepts a file and sets its metadata based on the values defined in the metadata model passed to this API",
            Description = "**Notes:** \n\n This is a debug build only API to assist generation of simple test data files",
            Tags = new[] { "Debug Endpoints" })]
        public async Task<ActionResult> SetMetaData(
            [ModelBinder(BinderType = typeof(JsonModelBinder))] DocumentProperties metaData,
            IFormFile file)
        {
            var resultFile = _fileSanitiseManagementService.SetMetaData(file, metaData);

            if (resultFile == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            var stream = new MemoryStream();
            await resultFile.CopyToAsync(stream);
            stream.Position = 0;
            return File(stream, resultFile.ContentType, resultFile.FileName);
        }

        [HttpPost]
        [Route("/getmetadata")]
        [ExcludeFromCodeCoverage]
        [SwaggerOperation(
            Summary = "Accepts a file and returns a model represent its current metadata values of the file",
            Description = "**Notes:** \n\n This is a debug build only API to assist the checking of simple test data files", 
            Tags = new[] { "Debug Endpoints" })]
        public IActionResult GetMetaData(IFormFile file)
        {
            var result = _fileSanitiseManagementService.GetMetaData(file);

            return Ok(result);
        }
#endif
    }
}