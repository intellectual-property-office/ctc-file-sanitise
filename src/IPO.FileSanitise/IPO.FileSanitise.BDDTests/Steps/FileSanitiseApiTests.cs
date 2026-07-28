using System.Net;
using AwesomeAssertions;
using IPO.FileSanitise.BDDTests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Reqnroll;

namespace IPO.FileSanitise.BDDTests.Steps
{
    [Binding]
    public class FileSanitiseApiTests
    {
		private readonly ScenarioContext _scenarioContext;
		private readonly TestServer _server;
		private readonly HttpClient _client;

		public FileSanitiseApiTests(ScenarioContext scenarioContext)
		{
			_scenarioContext = scenarioContext;
			_server = TestStartup.GetTestServer();
			_client = _server.CreateClient();
		}
		[Given(@"A valid file exists")]
		public void GivenAValidFileExists()
		{
			var moqFile = DocumentBuilder.CreateDocument("text.pdf", 1024);
			_scenarioContext.Add("UploadedFile", moqFile);
		}

		[When(@"apiURL SanitiseFile requested")]
		public async Task WhenApiURLSanitiseFileRequested()
		{
			var uploadedFile = _scenarioContext.Get<IFormFile>("UploadedFile");

			var form = new MultipartFormDataContent();
			form.Add(new StreamContent(uploadedFile.OpenReadStream()), "file", uploadedFile.FileName);

			var response = await _client.PostAsync("/", form);
			response.EnsureSuccessStatusCode();

			_scenarioContext.Add("ResponseContent", response.Content);
			_scenarioContext.Add("StatusCode", response.StatusCode);
		}

		[Then("The sanitised file is returned")]
		public void ThenTheSanitisedFileIsReturned()
		{
			var uploadedFile = _scenarioContext.Get<IFormFile>("UploadedFile");
			var response = _scenarioContext.Get<StreamContent>("ResponseContent");
			var statusCode = _scenarioContext.Get<HttpStatusCode>("StatusCode");

			response.Should().NotBeNull();
			response.Headers.ContentDisposition!.FileName.Should().Be(uploadedFile.FileName);
			response.Headers.ContentLength.Should().Be(1024);
			statusCode.Should().Be(HttpStatusCode.OK);
		}
	}
}
