namespace Hopaut.SharedKernel;

/// <summary>Kind of expected failure; BuildingBlocks.Api maps it to an HTTP status (clean-architecture-ddd skill §5).</summary>
public enum ErrorType
{
    Failure = 0,
    Validation,
    Unauthorized,
    Forbidden,
    NotFound,
    Conflict,
    Rule,
}

/// <summary>
/// An expected failure. <see cref="Code"/> is stable, lower-case and dotted (<c>&lt;module&gt;.&lt;entity&gt;.&lt;reason&gt;</c>);
/// BingoMobile keys messages off it, so renaming a code is a contract change.
/// </summary>
public record Error(string Code, string Description, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public static Error Failure(string code, string description) => new(code, description, ErrorType.Failure);

    public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);

    public static Error Unauthorized(string code, string description) => new(code, description, ErrorType.Unauthorized);

    public static Error Forbidden(string code, string description) => new(code, description, ErrorType.Forbidden);

    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);

    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);

    public static Error Rule(string code, string description) => new(code, description, ErrorType.Rule);
}

/// <summary>Shape validation failure with messages per field path (camelCase on the wire).</summary>
public sealed record ValidationError(IReadOnlyDictionary<string, string[]> Errors)
    : Error(DefaultCode, DefaultDescription, ErrorType.Validation)
{
    public const string DefaultCode = "general.validation";
    public const string DefaultDescription = "One or more validation errors occurred.";
}

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new ArgumentException("A successful result cannot carry an error.", nameof(error));
        }

        if (!isSuccess && error == Error.None)
        {
            throw new ArgumentException("A failed result needs an error.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => Result<TValue>.Success(value);

    public static Result<TValue> Failure<TValue>(Error error) => Result<TValue>.Failure(error);

    public static implicit operator Result(Error error) => Failure(error);
}

public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    private Result(bool isSuccess, TValue? value, Error error)
        : base(isSuccess, error) => _value = value;

    /// <summary>The value of a successful result. Accessing it on a failure is a programming error and throws.</summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"The value of a failed result cannot be accessed (error '{Error.Code}').");

    public static Result<TValue> Success(TValue value) => new(true, value, Error.None);

    public static new Result<TValue> Failure(Error error) => new(false, default, error);

    public static implicit operator Result<TValue>(TValue value) => Success(value);

    public static implicit operator Result<TValue>(Error error) => Failure(error);
}
