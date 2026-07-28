using System.Diagnostics.CodeAnalysis;
using Aspose.Words;
using AwesomeAssertions;
using IPO.FileSanitise.Services;

namespace IPO.FileSanitise.UnitTests.Services
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ExtensionsTests
    {
        #region Test Setup

        private readonly TestModel _uut;

        class TestModel
        {
            public string Text { get; set; } = string.Empty;
            public int Number { get; set; }
            public bool Flag { get; set; }
        }

        public ExtensionsTests()
        {
            _uut = new TestModel();
        }

        #endregion

        [TestMethod]
        public void TryLoadWordFile_Test()
        {
            using (var uut = new MemoryStream(new byte[0]))
            {
                var actual = uut.TryLoadWordFile(LoadFormat.Docx, out Document? doc);

                actual.Should().BeTrue();
                doc.Should().NotBeNull();
            }
        }

        [TestMethod]
        public void With_Action_Test()
        {
            // Arrange
            const int Number = 1;
            const bool Flag = true;
            const string Text = "Something";

            // Act
            var result = _uut.With((m) =>
            {
                m.Number = Number;
                m.Flag = Flag;
                m.Text = Text;
            });

            // Assert
            _uut.Number.Should().Be(Number);
            _uut.Flag.Should().Be(Flag);
            _uut.Text.Should().Be(Text);
        }

        [TestMethod]
        public void With_NoAction_Test()
        {
            // Act
            var result = _uut.With(null);

            // Assert
            _uut.Number.Should().Be(0);
            _uut.Flag.Should().Be(false);
            _uut.Text.Should().Be(string.Empty);
        }
    }
}