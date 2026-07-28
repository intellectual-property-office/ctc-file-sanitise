using IPO.Common.Infrastructure;

namespace IPO.FileSanitise.Services
{
    public static class StatusCodeExceptionFactory
    {
        public static StatusCodeException GetStatusCodeException<T>(int code, string errorMessage, string errorCode)
        {
            var error = Error.Create<T>(errorCode);
            error.Description += $" {errorMessage}";
            return new StatusCodeException(error, errorMessage, null, code);
        }
    }
}