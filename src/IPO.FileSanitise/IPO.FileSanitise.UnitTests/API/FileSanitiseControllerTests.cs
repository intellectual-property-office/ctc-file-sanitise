using AwesomeAssertions;
using IPO.FileSanitise.API.Controllers;
using IPO.FileSanitise.Interfaces;
using IPO.FileSanitise.Models.API;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace IPO.FileSanitise.UnitTests.API
{
    [TestClass]
    public class FileSanitiseControllerTests
    {
        private readonly Mock<IFileSanitiseManagementService> _mockFileSanitiseManagementService;

        private FileSanitiseController? _uut;
        private Mock<IFileSanitiseManagementService>? _fileSanitiseManagementService;
        private byte[] _streamData;

        public FileSanitiseControllerTests()
        {
            _mockFileSanitiseManagementService = new Mock<IFileSanitiseManagementService>();
            _streamData = Array.Empty<byte>();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _streamData = new byte[] { 0x01, 0x02, 0x03 };
            _fileSanitiseManagementService = new Mock<IFileSanitiseManagementService>();
            _uut = new FileSanitiseController(_fileSanitiseManagementService.Object);
        }

        [TestMethod]
		public void PostSanitiseFileReturnsCorrectResults()
		{
			var fileSanitiseApi = new FileSanitiseController(_mockFileSanitiseManagementService.Object);
			var fileName = "text.pdf";
			var fileSize = 1024;
			var file = DocumentBuilder.CreateDocument(fileName, fileSize);
			var sanitisedFile = new SanitisedFile(file.FileName, file.ContentType, file.OpenReadStream());

			_mockFileSanitiseManagementService
				.Setup(s => s.Sanitise(It.IsAny<IFormFile>()))
				.Returns(sanitisedFile)
				.Verifiable();

			// Act 
			var fileSanitiseRequest = fileSanitiseApi.SanitiseFile(new SanitiseFileRequest()
				{
					file = file
				});

			// Assert
			var fileStreamResult = fileSanitiseRequest as FileStreamResult;
			fileStreamResult.Should().NotBeNull();
			fileStreamResult.ContentType.Should().Be(sanitisedFile.ContentType);
			fileStreamResult.FileDownloadName.Should().Be(sanitisedFile.FileName);
			fileStreamResult.FileStream.Length.Should().Be(sanitisedFile.Data.Length);
			_mockFileSanitiseManagementService.Verify();
		}

        [TestMethod]
        public void SanitiseFile_ReturnsSanitisedFile()
        {
            // Arrange
            var fileName = "test.txt";
            var contentType = "document/text";

            var returnedFile = new SanitisedFile(fileName, contentType, new MemoryStream(_streamData));

            _fileSanitiseManagementService!.Setup(e => e.Sanitise(It.IsAny<IFormFile>())).Returns(returnedFile);

            var request = new SanitiseFileRequest();

            // Act
            var result = _uut!.SanitiseFile(request);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<FileStreamResult>();

            var fileResult = result as FileStreamResult;
            fileResult.Should().NotBeNull();
            fileResult.ContentType.Should().Be(contentType);
            fileResult.FileDownloadName.Should().Be(fileName);

            fileResult.FileStream.Length.Should().Be(3);
            fileResult.FileStream.Should().BeAssignableTo<MemoryStream>();
            var stream = fileResult.FileStream as MemoryStream;
            stream.Should().NotBeNull();
            stream.ToArray().Should().BeEquivalentTo(_streamData);

            _fileSanitiseManagementService.Verify(e => e.Sanitise(It.IsAny<IFormFile>()), Times.Once);
        }
    }
}