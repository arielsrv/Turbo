namespace Turbo.API.Exceptions;

/// <summary>
///     Base exception for domain-specific errors that can be mapped to Problem Details.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }

    protected DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public abstract string Type { get; }
    public abstract string Title { get; }
    public abstract int StatusCode { get; }
}

/// <summary>
///     Thrown when a requested resource is not found.
/// </summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} with identifier '{key}' was not found.")
    {
    }

    public override string Type => "https://tools.ietf.org/html/rfc7231#section-6.5.4";
    public override string Title => "Resource Not Found";
    public override int StatusCode => StatusCodes.Status404NotFound;
}

/// <summary>
///     Thrown when a validation error occurs.
/// </summary>
public class ValidationException : DomainException
{
    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string message, IDictionary<string, string[]> errors) : base(message)
    {
        Errors = errors;
    }

    public override string Type => "https://tools.ietf.org/html/rfc7231#section-6.5.1";
    public override string Title => "Validation Error";
    public override int StatusCode => StatusCodes.Status400BadRequest;

    public IDictionary<string, string[]> Errors { get; }
}

/// <summary>
///     Thrown when there's a conflict with the current state of the resource.
/// </summary>
public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message)
    {
    }

    public override string Type => "https://tools.ietf.org/html/rfc7231#section-6.5.8";
    public override string Title => "Conflict";
    public override int StatusCode => StatusCodes.Status409Conflict;
}

/// <summary>
///     Thrown when a business rule is violated.
/// </summary>
public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message) : base(message)
    {
    }

    public override string Type => "https://tools.ietf.org/html/rfc7231#section-6.5.1";
    public override string Title => "Business Rule Violation";
    public override int StatusCode => StatusCodes.Status422UnprocessableEntity;
}