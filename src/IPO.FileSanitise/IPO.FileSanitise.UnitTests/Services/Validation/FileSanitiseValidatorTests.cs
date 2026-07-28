using System.Diagnostics.CodeAnalysis;
using Aspose.Words;
using Aspose.Words.Saving;
using AwesomeAssertions;
using IPO.Common.Infrastructure;
using IPO.FileSanitise.Models.Configuration;
using IPO.FileSanitise.Models.FileSanitise;
using IPO.FileSanitise.Services.Validation;
using Microsoft.Extensions.Logging;
using Moq;
using Spire.Pdf;
using static IPO.FileSanitise.Services.Validation.FileSanitiseValidator;

namespace IPO.FileSanitise.UnitTests.Services.Validation
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FileSanitiseValidatorTests
    {
        private readonly FileSanitiseValidator _validator;
        private readonly Settings _settings;
        private readonly Mock<ILogger<FileSanitiseValidator>> _mockLogger;

        public FileSanitiseValidatorTests()
        {
            _settings = FileSanitiseSettingsBuilder.Build();
            _mockLogger = new Mock<ILogger<FileSanitiseValidator>>();
            _validator = new FileSanitiseValidator(_settings, _mockLogger.Object);
        }

        [TestMethod]
        public void GetErrorMessageTest()
        {
            _validator.GetErrorMessage(ErrorType.FileEncrypted).Should().Be(FileEncryptedErrorMessage);
            _validator.GetErrorMessage(ErrorType.InvalidPDFVersion).Should().Be(InvalidPDFVersionErrorMessage);
            _validator.GetErrorMessage(ErrorType.FileCannotBeLoaded).Should().Be(FileCannotBeLoadedErrorMessage);
            _validator.GetErrorMessage(ErrorType.NotClaimedFormat).Should().Be(NotClaimedFormatErrorMessage);

            ErrorType undefined = (ErrorType)999999;
            Action act = () => _validator.GetErrorMessage(undefined);
            act.Should().Throw<NotImplementedException>();
        }

        [TestMethod]
        public void GetStatusCodeExceptionTest()
        {
            const string ErrorMessage = "Something went wrong";
            const int StatusCode = 4;
            const string ErrorCode = "002";

            var actual = GetStatusCodeException<FileSanitiseValidator>(StatusCode, ErrorMessage, ErrorCode);

            actual.Should().NotBeNull();
            actual.Should().BeOfType<StatusCodeException>();
            actual.Message.Should().Be(ErrorMessage);
            actual.StatusCode.Should().Be(StatusCode);

            actual.Error.Should().NotBeNull();
            actual.Error.Should().BeOfType<Error>();
            actual.Error.Code.Should().Be(ErrorCode);
            actual.Error.Description.Should().Be("The FileSanitiseValidator encountered an error. Something went wrong");
        }

        [TestMethod]
        public void ValidateWhenFileLengthGreaterThanMaximumFileSizeReturnsPayloadTooLargeValidationResult()
        {
            // Arrange
            var file = DocumentBuilder.CreateDocument("test.docx", (int)(_settings.ValidationSettings!.SizeLimit + 1));

            // Act
            var result = _validator.Validate(file.OpenReadStream(), file.FileName, file.ContentType);

            // Assert
            var validationResult = result.As<FileSanitiseValidationResult>();
            validationResult.Code.Should().Be(413);
            validationResult.ErrorCode.Should().Be("");
            validationResult.ErrorMessage.Should().Be($"File size larger than {(_settings.ValidationSettings.SizeLimit / 1024f / 1024f).ToString("#0")} MB.");
        }

        [TestMethod]
        public void ValidateWhenFileExtensionIsInvalidReturnsUnsupportedFileTypesValidationResult()
        {
            // Arrange
            var file = DocumentBuilder.CreateDocument("test.png", (int)(_settings.ValidationSettings!.SizeLimit));

            // Act
            var result = _validator.Validate(file.OpenReadStream(), file.FileName, file.ContentType);

            // Assert
            var validationResult = result.As<FileSanitiseValidationResult>();
            validationResult.Code.Should().Be(415);
            validationResult.ErrorCode.Should().Be("");
            validationResult.ErrorMessage.Should().Be($"Unsupported file type, supported media types: {string.Join(", ", _settings.ValidationSettings.AcceptedFileExtensions!)}.");
        }

        [TestMethod]
        public void ValidateWhenFileMimeTypeIsInvalidReturnsUnsupportedMediaTypesValidationResult()
        {
            // Arrange
            var file = DocumentBuilder.CreateDocument("test.docx", (int)(_settings.ValidationSettings!.SizeLimit));

            // Act
            var result = _validator.Validate(file.OpenReadStream(), file.FileName, "audio/mpeg3");

            // Assert
            var validationResult = result.As<FileSanitiseValidationResult>();
            validationResult.Code.Should().Be(415);
            validationResult.ErrorCode.Should().Be("");
            validationResult.ErrorMessage.Should().Be($"Unsupported media type, supported media types: {string.Join(", ", _settings.ValidationSettings.AcceptedFileMimeTypes!)}.");
        }

        [TestMethod]
        public void ValidateReturnsSuccessValidationResult()
        {
            // Arrange
            var file = DocumentBuilder.CreateDocument("test.docx", (int)(_settings.ValidationSettings!.SizeLimit));

            // Act
            var result = _validator.Validate(file.OpenReadStream(), file.FileName, file.ContentType);

            // Assert
            var validationResult = result.As<FileSanitiseValidationResult>();
            validationResult.Code.Should().Be(200);
            validationResult.ErrorCode.Should().Be("");
        }

        [TestMethod]
        public void GetDocumentType_Test()
        {
            FileSanitiseValidator.GetDocumentType("test.docx").Should().Be(FileSanitiserFileType.Docx);
            FileSanitiseValidator.GetDocumentType("test.odt").Should().Be(FileSanitiserFileType.Odt);
            FileSanitiseValidator.GetDocumentType("test.pdf").Should().Be(FileSanitiserFileType.Pdf);
            FileSanitiseValidator.GetDocumentType("test.txt").Should().Be(FileSanitiserFileType.NotSupported);
        }

        [TestMethod]
        public void ValidateFormat_DocX_File_Test()
        {
            using (var inStream = new MemoryStream(Properties.Resources.TestFileDocX, true))
            {
                var actual = _validator.ValidateFormat(inStream, "test.docx");
                actual.Should().BeNull();

                actual = _validator.ValidateFormat(inStream, "test.odt");
                actual.Should().Be(ErrorType.NotClaimedFormat);
            }
        }

        [TestMethod]
        public void ValidateFormat_DocX_EncryptedFile_Test()
        {
            using (var fileStream = new MemoryStream())
            {
                Document doc = new Document();

                DocSaveOptions saveOptions = new DocSaveOptions 
                {
                    Password = "password" 
                };

                doc.Save(fileStream, saveOptions);
                var actual = _validator.ValidateFormat(fileStream, "test.docx");
                actual.Should().Be(ErrorType.FileEncrypted);
            }
        }

        [TestMethod]
        public void ValidateFormat_Odt_File_Test()
        {
            using (var inStream = new MemoryStream(Properties.Resources.TestFileOdt, true))
            {
                var actual = _validator.ValidateFormat(inStream, "test.odt");
                actual.Should().BeNull();

                actual = _validator.ValidateFormat(inStream, "test.docx");
                actual.Should().Be(ErrorType.NotClaimedFormat);
            }
        }

        [TestMethod]
        public void ValidateFormat_Odt_EncryptedFile_Test()
        {
            using (var fileStream = new MemoryStream())
            {
                Document doc = new Document();

                DocSaveOptions saveOptions = new DocSaveOptions 
                { 
                    Password = "password"
                };

                doc.Save(fileStream, saveOptions);
                var actual = _validator.ValidateFormat(fileStream, "test.odt");
                actual.Should().Be(ErrorType.FileEncrypted);
            }
        }

        [TestMethod]
        public void ValidateFormat_Pdf_File_Test()
        {
            using (var inStream = new MemoryStream(Properties.Resources.TestFile1, true))
            {
                var actual = _validator.ValidateFormat(inStream, "test.pdf");
                actual.Should().BeNull();

                actual = _validator.ValidateFormat(inStream, "test.odt");
                actual.Should().Be(ErrorType.NotClaimedFormat);
            }
        }

        [TestMethod]
        public void ValidateFormat_Pdf_EncryptedFile_Test()
        {
            using (var pdfDocument = new PdfDocument())
            {
                var securityPolicy = new PdfPasswordSecurityPolicy("password1", "password2")
                {
                    EncryptionAlgorithm = PdfEncryptionAlgorithm.AES_256
                };

                pdfDocument.Encrypt(securityPolicy);

                using (var pdfStream = new MemoryStream())
                {
                    pdfDocument.SaveToStream(pdfStream);

                    var actual = _validator.ValidateFormat(pdfStream, "test.pdf");

                    actual.Should().Be(ErrorType.FileEncrypted);
                }
            }
        }

        [TestMethod]
        public void ValidateFormat_Pdf_InvalidFile_Test()
        {
            using (var pdfStream = new MemoryStream(Array.Empty<byte>()))
            {
                var actual = _validator.ValidateFormat(pdfStream, "test.pdf");
                actual.Should().Be(ErrorType.FileCannotBeLoaded);
            }
        }

        [TestMethod]
        public void ValidateFormat_Text_File_Test()
        {
            using (var inStream = new MemoryStream(Properties.Resources.TestFile1, true))
            {
                _validator
                    .Invoking(y => y.ValidateFormat(inStream, "test.txt"))
                    .Should()
                    .Throw<NotImplementedException>();
            }
        }
    }
}