#if DEBUG

using System.Diagnostics.CodeAnalysis;

namespace IPO.FileSanitise.Models
{
    [ExcludeFromCodeCoverage]
    public class TestMetaDataPdf : TestMetaData
    {
        public string? Creator { get; set; } = string.Empty;
        
        public string? Producer { get; set; } = string.Empty;
    }
}

#endif