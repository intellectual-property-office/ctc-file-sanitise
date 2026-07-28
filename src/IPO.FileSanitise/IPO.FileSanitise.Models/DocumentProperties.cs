#if DEBUG

using System.Diagnostics.CodeAnalysis;

namespace IPO.FileSanitise.Models
{
    [ExcludeFromCodeCoverage]
    public class DocumentProperties
    {
        public List<DocumentProperty> Values { get; set; } = new List<DocumentProperty>();

        public List<DocumentProperty> Custom { get; set; } = new List<DocumentProperty>();

        public static bool TryFind(string name, DocumentProperties value, out string? newValue)
        {
            var propertyToUpdate = value.Values.FirstOrDefault(e => e.Name == name);

            if (propertyToUpdate != null)
            {
                newValue = propertyToUpdate.Value;
                return true;
            }

            newValue = null;
            return false;
        }

        public class DocumentProperty
        {
            public string? Name { get; set; }
            public string? Value { get; set; }
        }
    }
}

#endif