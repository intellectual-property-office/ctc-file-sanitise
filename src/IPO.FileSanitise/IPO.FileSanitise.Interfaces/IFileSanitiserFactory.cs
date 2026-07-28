namespace IPO.FileSanitise.Interfaces
{
    public interface IFileSanitiserFactory
	{
        IFileSanitiser? Build(string contentType);
    }
}
