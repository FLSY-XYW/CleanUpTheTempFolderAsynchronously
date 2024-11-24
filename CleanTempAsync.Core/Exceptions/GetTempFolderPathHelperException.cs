namespace CleanTempAsync.Core.Exceptions;

public class GetTempFolderPathHelperException : Exception
{
    public GetTempFolderPathHelperException() : base("Get Temp Folder Path fail")
    {
    }

    public GetTempFolderPathHelperException(string? message) : base(message)
    {
    }

    public GetTempFolderPathHelperException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}