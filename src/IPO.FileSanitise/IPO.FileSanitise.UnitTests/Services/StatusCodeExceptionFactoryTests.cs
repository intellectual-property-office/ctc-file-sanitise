using System.Diagnostics.CodeAnalysis;
using AwesomeAssertions;
using IPO.Common.Infrastructure;
using IPO.FileSanitise.Services;

namespace IPO.FileSanitise.UnitTests.Services
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class StatusCodeExceptionFactoryTests
    {
        [TestMethod]
        public void GetStatusCodeException_Test()
        {
            // Arrange
            const int code = 201;
            const string errorMessage = "Something went wrong";
            const string errorCode = "E00001";

            // Act
            var actual = StatusCodeExceptionFactory.GetStatusCodeException<StatusCodeExceptionFactoryTests>(code, errorMessage, errorCode);

            // Assert
            actual.Should().NotBeNull();
            actual.Should().BeOfType<StatusCodeException>();
            actual.StatusCode.Should().Be(code);
            actual.Message.Should().Be(errorMessage);

            actual.Error.Should().NotBeNull();
            actual.Error.Should().BeOfType<Error>();

            actual.Error.Code.Should().Be(errorCode);
            actual.Error.Description.Should().Be($"The {nameof(StatusCodeExceptionFactoryTests)} encountered an error. Something went wrong");
        }
    }
}