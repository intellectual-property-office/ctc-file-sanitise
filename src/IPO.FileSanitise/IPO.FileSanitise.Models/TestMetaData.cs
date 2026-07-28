#if DEBUG

using System.Diagnostics.CodeAnalysis;

namespace IPO.FileSanitise.Models
{
    [ExcludeFromCodeCoverage]
    public class TestMetaData
    {
        public string? Author { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; }
        public string? Keywords { get; set; } = string.Empty;
        public DateTime ModificationDate { get; set; }
        public string? Subject { get; set; } = string.Empty;
        public string? Title { get; set; } = string.Empty;

        public List<CustomProperty> CustomProperties { get; set; } = new List<CustomProperty>();

        public class CustomProperty
        {
            public CustomProperty(string? name = null, object? value = null)
            {
                Name = name ?? string.Empty;
                Value = value == null ? string.Empty : value.ToString()!;
            }

            public string Name { get; set; } = string.Empty;

            public string Value { get; set; } = string.Empty;
        }
    }
}

#endif