namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Exception thrown when invalid bars are encountered.
/// </summary>
[Serializable]
public class InvalidBarsException : ArgumentOutOfRangeException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidBarsException"/> class.
    /// </summary>
    public InvalidBarsException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidBarsException"/> class with the name of the parameter that causes this exception.
    /// </summary>
    /// <param name="paramName">Name of the parameter that causes this exception.</param>
    public InvalidBarsException(string? paramName)
        : base(paramName) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidBarsException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">Error message that explains the reason for this exception.</param>
    /// <param name="innerException">Exception that is the cause of the current exception.</param>
    public InvalidBarsException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidBarsException"/> class with the name of the parameter that causes this exception and a specified error message.
    /// </summary>
    /// <param name="paramName">Name of the parameter that causes this exception.</param>
    /// <param name="message">Error message that explains the reason for this exception.</param>
    public InvalidBarsException(string? paramName, string? message)
        : base(paramName, message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidBarsException"/> class with the name of the parameter that causes this exception, the value of the argument, and a specified error message.
    /// </summary>
    /// <param name="paramName">Name of the parameter that causes this exception.</param>
    /// <param name="actualValue">Value of the argument that causes this exception.</param>
    /// <param name="message">Error message that explains the reason for this exception.</param>
    public InvalidBarsException(string? paramName, object? actualValue, string? message)
        : base(paramName, actualValue, message) { }
}
