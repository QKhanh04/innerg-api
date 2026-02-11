using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InnerG.Api.Exceptions
{
    public class ValidationException : AppException
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException(IDictionary<string, string[]> errors)
            : base("Validation failed", StatusCodes.Status400BadRequest)
        {
            Errors = errors;
        }
    }
    public class BadRequestException : AppException
    {
        public BadRequestException(string message)
            : base(message, StatusCodes.Status400BadRequest) { }
    }

    public class NotFoundException : AppException
    {
        public NotFoundException(string message)
            : base(message, StatusCodes.Status404NotFound) { }
    }

    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message = "Unauthorized")
            : base(message, StatusCodes.Status401Unauthorized) { }
    }

    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message)
            : base(message, StatusCodes.Status403Forbidden) { }
    }

    public class ConflictException : AppException
    {
        public ConflictException(string message)
            : base(message, StatusCodes.Status409Conflict) { }
    }

    public class ExternalServiceException : AppException
    {
        public ExternalServiceException(string message)
            : base(message, StatusCodes.Status503ServiceUnavailable) { }
    }

    public class ConfigurationException : AppException
    {
        public ConfigurationException(string key)
            : base($"Missing or invalid configuration: {key}",
                   StatusCodes.Status500InternalServerError)
        { }
    }

}