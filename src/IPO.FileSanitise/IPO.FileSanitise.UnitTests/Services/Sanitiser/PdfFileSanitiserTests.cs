using System.Diagnostics.CodeAnalysis;
using AwesomeAssertions;
using IPO.FileSanitise.Models.FileSanitise;
using IPO.FileSanitise.Services.Sanitise;
using IPO.FileSanitise.UnitTests.Properties;
using Microsoft.AspNetCore.Http;
using Spire.Pdf;

namespace IPO.FileSanitise.UnitTests.Services.Sanitiser
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class PdfFileSanitiserTests
    {
        #region Test Setup

        private readonly PdfFileSanitiser _uut;
        private const string DocName = "test.pdf";

        public PdfFileSanitiserTests()
        {
            _uut = new PdfFileSanitiser();
        }

        #endregion

        [TestMethod]
        public void Sanitiser_ClearsPropertiesInUpdatedFile()
        {
            // Arrange
            var file = DocumentBuilder.CreateDocument(DocName, Resources.TestFile1);

            // Act
            var result = _uut.Sanitise(file);

            // Assert
            result.Updated!.Length.Should().NotBe(file.Length);

            using (var inputStream = file.OpenReadStream())
            using (var outputStream = result!.Updated!.OpenReadStream())
            {
                var input = new PdfDocument(inputStream);
                var output = new PdfDocument(outputStream);

                input.DocumentInformation.Author.Should().Be("Test Author");
                output.DocumentInformation.Author.Should().Be(string.Empty);

#pragma warning disable CS0618
                input.DocumentInformation.CreationDate.Should().Be(DateTime.MinValue);
                output.DocumentInformation.CreationDate.Should().Be(DateTime.MinValue);
#pragma warning restore CS0618

                input.DocumentInformation.Creator.Should().Be("Test Creator");
                output.DocumentInformation.Creator.Should().Be(string.Empty);

                input.DocumentInformation.Keywords.Should().Be("Test Keyword");
                output.DocumentInformation.Keywords.Should().Be(string.Empty);

                input.DocumentInformation.Producer.Should().Be("Test Producer");
                output.DocumentInformation.Producer.Should().Be(string.Empty);

                input.DocumentInformation.Subject.Should().Be("Test Subject");
                output.DocumentInformation.Subject.Should().Be(string.Empty);

                input.DocumentInformation.Title.Should().Be("Test Title");
                output.DocumentInformation.Title.Should().Be(string.Empty);

                void ValidateCustomProperties(Dictionary<string, string> custom, string expectedValue)
                {
                    custom.Count.Should().Be(1);
                    custom.ElementAt(0).Key.Should().Be("Custom");
                    custom.ElementAt(0).Value.Should().Be(expectedValue);
                }

                ValidateCustomProperties(input.DocumentInformation.GetAllCustomProperties(), "Test Custom Property");
                ValidateCustomProperties(output.DocumentInformation.GetAllCustomProperties(), string.Empty);
            }
        }

        [TestMethod]
        public void Sanitiser_ReturnsCorrectResultModelForInvalidFile()
        {
            // Arrange
            var file = DocumentBuilder.CreateDocument(DocName, new byte[] { 0x01, 0x02, 0x03 });

            // Act
            var result = _uut.Sanitise(file);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<FileSanitiseResult>();

            result.Code.Should().Be(422);
            result.ErrorCode.Should().Be(string.Empty);
            result.ErrorMessage.Should().Be("The submitted File could not be loaded");
        }

        [TestMethod]
        public void Sanitiser_ReturnsCorrectResultModelForValidFile()
        {
            // Arrange
            var file = DocumentBuilder.CreateDocument(DocName, Resources.TestFile1);

            // Act
            var result = _uut.Sanitise(file);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<FileSanitiseResult>();

            result.Code.Should().Be(200);
            result.ErrorCode.Should().Be(string.Empty);
            result.ErrorMessage.Should().Be(string.Empty);
            result.Updated.Should().NotBeNull();
            result.Updated.Should().BeOfType<FormFile>();
            result.Updated.Should().BeAssignableTo<IFormFile>();
            result.Updated.FileName.Should().Be(DocName);
            result.Updated.ContentType.Should().Be("application/pdf");
        }
    }
}