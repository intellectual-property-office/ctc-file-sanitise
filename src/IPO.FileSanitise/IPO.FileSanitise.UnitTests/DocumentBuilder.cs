using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Moq;

namespace IPO.FileSanitise.UnitTests
{
	[ExcludeFromCodeCoverage]
	public static class DocumentBuilder
	{
		public static IFormFile CreateDocument(string name, int length)
		{
			var mockedFile = new Mock<IFormFile>();
			var rnd = new Random();
			char[] contentArray = new char[length];
			int currentCharIndex = 0;
			while (currentCharIndex < length)
			{
				char randomChar = (char)rnd.Next('a', 'z');
				contentArray[currentCharIndex] = randomChar;
				currentCharIndex++;
			}
			var ms = new MemoryStream();
			var writer = new StreamWriter(ms);
			writer.Write(contentArray);
			writer.Flush();
			ms.Position = 0;
			mockedFile.Setup(o => o.OpenReadStream()).Returns(ms);
			mockedFile.Setup(o => o.FileName).Returns(name);
			mockedFile.Setup(o => o.Length).Returns(ms.Length);
			mockedFile.Setup(o => o.ContentType).Returns(GetContentType(name));
			return mockedFile.Object;
		}

        public static IFormFile CreateDocument(string name, byte[] contentArray)
        {
            var mockedFile = new Mock<IFormFile>();

            var ms = new MemoryStream(contentArray);

            ms.Position = 0;
            mockedFile.Setup(o => o.OpenReadStream()).Returns(ms);
            mockedFile.Setup(o => o.FileName).Returns(name);
            mockedFile.Setup(o => o.Length).Returns(ms.Length);
            mockedFile.Setup(o => o.ContentType).Returns(GetContentType(name));
            return mockedFile.Object;
        }

        public static string GetContentType(string fileName)
		{
			if (Path.GetExtension(fileName) == ".odt")
			{
				return "application/vnd.oasis.opendocument.text";
			}

            if (Path.GetExtension(fileName) == ".pdf")
            {
                return "application/pdf";
            }

            if (!new FileExtensionContentTypeProvider().TryGetContentType(fileName, out string? contentType))
			{
				contentType = "application/octet-stream";
			}

			return contentType;
		}
	}
}