using System.Diagnostics.CodeAnalysis;
using AwesomeAssertions;
using IPO.Common.Infrastructure;
using IPO.FileSanitise.Interfaces;
using IPO.FileSanitise.Models.FileSanitise;
using IPO.FileSanitise.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace IPO.FileSanitise.UnitTests.Services
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FileSanitiseManagementServiceTests
    {
        private Mock<ILogger<FileSanitiseManagementService>> _mockLogger;
        private Mock<IFileSanitiseValidator> _mockValidator;
        private Mock<IFileSanitiserFactory> _mockFactory;
        private Mock<IFileSanitiser> _mockSanitiser;
        private FileSanitiseManagementService _service;

        public FileSanitiseManagementServiceTests()
        {
            _mockLogger = new Mock<ILogger<FileSanitiseManagementService>>();
            _mockValidator = new Mock<IFileSanitiseValidator>();
            _mockFactory = new Mock<IFileSanitiserFactory>();
            _mockSanitiser = new Mock<IFileSanitiser>();

            _service = new FileSanitiseManagementService(_mockLogger.Object, _mockValidator.Object, _mockFactory.Object);
        }

        [TestMethod]
        public void Ctor_ThrowsArgumentNullExceptionForDependencies()
        {
            // Act and Assert

            Assert.ThrowsExactly<ArgumentNullException>(() => _service = new FileSanitiseManagementService(_mockLogger.Object, null!, _mockFactory.Object));

            Assert.ThrowsExactly<ArgumentNullException>(() => _service = new FileSanitiseManagementService(_mockLogger.Object, _mockValidator.Object, null!));
        }

        [TestMethod]
        public void Sanitise_ValidFile_ReturnsSanitisedFile()
        {
            // Arrange
            var mockFile = CreateMockFile("test.pdf", "application/pdf", new byte[] { 1, 2, 3 });

            _mockValidator.Setup(v => v.Validate(It.IsAny<Stream>(), mockFile.Object.FileName, mockFile.Object.ContentType))
                          .Returns(new FileSanitiseValidationResult { Code = 200 });

            _mockFactory.Setup(v => v.Build(It.IsAny<string>())).Returns(_mockSanitiser.Object);
            _mockSanitiser.Setup(v => v.Sanitise(mockFile.Object))
                .Returns(new FileSanitiseResult { Code = 200, Updated = mockFile.Object });

            // Act
            var result = _service.Sanitise(mockFile.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mockFile.Object.FileName, result.FileName);
            Assert.AreEqual(mockFile.Object.ContentType, result.ContentType);
            _mockValidator.Verify(v => v.Validate(It.IsAny<Stream>(), mockFile.Object.FileName, mockFile.Object.ContentType), Times.Once);
        }

        [TestMethod]
        public void Sanitise_Non200_ReturnsStatusCodeException()
        {
            // Arrange
            var mockFile = CreateMockFile("test.pdf", "application/pdf", new byte[] { 1, 2, 3 });

            _mockValidator.Setup(v => v.Validate(It.IsAny<Stream>(), mockFile.Object.FileName, mockFile.Object.ContentType))
                          .Returns(new FileSanitiseValidationResult { Code = 200 });

            _mockFactory.Setup(v => v.Build(It.IsAny<string>())).Returns(_mockSanitiser.Object);

            _mockSanitiser.Setup(v => v.Sanitise(mockFile.Object))
                .Returns(new FileSanitiseResult { Code = 404, Updated = mockFile.Object });

            // Act & Assert
            var exception = Assert.ThrowsExactly<StatusCodeException>(() =>
            {
                _service.Sanitise(mockFile.Object);
            });

            exception.Should().NotBeNull();
            exception.Message.Should().Be(string.Empty);
        }

        [TestMethod]
        public void Sanitise_InvalidFile_ThrowsException()
        {
            // Arrange
            var mockFile = CreateMockFile("test.pdf", "application/pdf", new byte[] { 1, 2, 3 });

            _mockValidator.Setup(v => v.Validate(It.IsAny<Stream>(), mockFile.Object.FileName, mockFile.Object.ContentType))
                          .Returns(new FileSanitiseValidationResult { Code = 400, ErrorMessage = "Invalid file" });

            // Act & Assert
            var exception = Assert.ThrowsExactly<StatusCodeException>(() =>
            {
                _service.Sanitise(mockFile.Object);
            });
            exception.Should().NotBeNull();
            exception.Message.Should().Contain("Invalid file");
        }

        [TestMethod]
        public void Sanitise_NoFactory_ReturnsStatusCodeException()
        {
            // Arrange
            var mockFile = CreateMockFile("test.pdf", "application/pdf", new byte[] { 1, 2, 3 });

            _mockValidator.Setup(v => v.Validate(It.IsAny<Stream>(), mockFile.Object.FileName, mockFile.Object.ContentType))
                          .Returns(new FileSanitiseValidationResult { Code = 200 });

            _mockFactory.Setup(v => v.Build(It.IsAny<string>())).Returns(value: null);

            // Act & Assert
            var exception = Assert.ThrowsExactly<StatusCodeException>(() =>
            {
                _service.Sanitise(mockFile.Object);
            });

            exception.Should().NotBeNull();
            exception.Message.Should().Be(" A sanitiser for the file submitted ('application/pdf') was not found");
        }

        private static Mock<IFormFile> CreateMockFile(string fileName, string contentType, byte[] fileData)
        {
            var mockFile = new Mock<IFormFile>();
            var fileStream = new MemoryStream(fileData);

            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.ContentType).Returns(contentType);
            mockFile.Setup(f => f.OpenReadStream()).Returns(fileStream);

            return mockFile;
        }
    }
}