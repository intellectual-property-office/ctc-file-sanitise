using System.Diagnostics.CodeAnalysis;
using Aspose.Words;
using AwesomeAssertions;
using IPO.FileSanitise.Models.FileSanitise;
using IPO.FileSanitise.Services.Sanitise;
using IPO.FileSanitise.UnitTests.Properties;
using Microsoft.AspNetCore.Http;

namespace IPO.FileSanitise.UnitTests.Services.Sanitiser
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class OdtFileSanitiserTests
    {
        #region Test Setup

        private readonly OdtFileSanitiser _uut;
        private const string DocName = "test.odt";

        public OdtFileSanitiserTests()
        {
            _uut = new OdtFileSanitiser();
        }

        #endregion

        [TestMethod]
        public void Sanitiser_ClearsPropertiesInUpdatedFile()
        {
            // Arrange
            var file = DocumentBuilder.CreateDocument(DocName, Resources.TestFileOdt);

            // Act
            var result = _uut.Sanitise(file);

            // Assert
            result.Updated!.Length.Should().NotBe(file.Length);

            using (var outputStream = result!.Updated!.OpenReadStream())
            {
                var output = new Document(outputStream);

                output.BuiltInDocumentProperties.Manager.Should().Be(string.Empty);
                output.BuiltInDocumentProperties.Author.Should().Be(string.Empty);
                output.BuiltInDocumentProperties.LastSavedBy.Should().Be(string.Empty);
                output.BuiltInDocumentProperties.Title.Should().Be(string.Empty);
                output.BuiltInDocumentProperties.Comments.Should().Be(string.Empty);
                output.BuiltInDocumentProperties.Template.Should().Be(string.Empty);
                output.BuiltInDocumentProperties.ContentStatus.Should().Be(string.Empty);
                output.BuiltInDocumentProperties.Category.Should().Be(string.Empty);
                output.BuiltInDocumentProperties.Subject.Should().Be(string.Empty);
                output.BuiltInDocumentProperties.HyperlinkBase.Should().Be(string.Empty);
                output.BuiltInDocumentProperties.Company.Should().Be(string.Empty);
                output.BuiltInDocumentProperties.Keywords.Should().Be(string.Empty);
                output.CustomDocumentProperties.Count.Should().Be(0);
            }
        }

        [TestMethod]
        public void Sanitiser_ReturnsCorrectResultModelForValidFile()
        {
            // Arrange
            var file = DocumentBuilder.CreateDocument(DocName, Resources.TestFileOdt);

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
            result.Updated.ContentType.Should().Be("application/vnd.oasis.opendocument.text");
        }
    }
}