#if DEBUG

using System.Diagnostics.CodeAnalysis;

namespace IPO.FileSanitise.Models
{
    [ExcludeFromCodeCoverage]
    public class TestMetaDataWord : TestMetaData
    {
        public string? LastSavedBy { get; set; }
        public string? Manager { get; set; }
        public DateTime LastPrinted { get; set; }
        public string? Comments { get; set; }
        public string? Template { get; set; }
        public string? ContentStatus { get; set; }
        public string? Category { get; set; }
        public string? HyperlinkBase { get; set; }
        public string? Company { get; set; }
    }
}

#endif