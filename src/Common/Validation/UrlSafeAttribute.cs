using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Validates that a string is URL-safe.
/// <remarks>
/// Note that this will allow nullable strings which may cause other issues,
/// so nullability should be checked separately if necessary.
/// </remarks>
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed partial class UrlSafeAttribute : ValidationAttribute
{
    [GeneratedRegex(@"^[a-zA-Z0-9\-._]+$", RegexOptions.Compiled)]
    private static partial Regex GenerateUrlSafeRegex();

    private static readonly Regex UrlSafeRegex = GenerateUrlSafeRegex();

    /// <summary>
    /// Determines whether the specified value is URL-safe.
    /// </summary>
    /// <param name="value">Value to validate.</param>
    /// <returns><c>true</c> if the value is URL-safe; otherwise, <c>false</c>.</returns>
    public override bool IsValid(object? value)
        => value is null || (value is string str && (string.IsNullOrEmpty(str) || UrlSafeRegex.IsMatch(str)));

    /// <summary>
    /// Formats the error message to include the property name.
    /// </summary>
    /// <param name="name">Name of the property being validated.</param>
    /// <returns>Formatted error message.</returns>
    public override string FormatErrorMessage(string name)
        => $"The '{name}' field is not URL safe.";
}
