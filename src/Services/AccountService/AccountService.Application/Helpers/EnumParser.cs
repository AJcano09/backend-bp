namespace AccountService.Application.Helpers;

/// <summary>
/// Parses API string values into domain enums, keeping controllers and use
/// cases thin. A null, empty, or invalid value throws
/// <see cref="ArgumentException"/> (mapped to 400 by the API pipeline).
/// </summary>
public static class EnumParser
{
    /// <summary>
    /// Parses <paramref name="value"/> into <typeparamref name="TEnum"/>,
    /// ignoring case. Throws when the value is null, empty, or is not a
    /// valid member of the enum.
    /// </summary>
    /// <param name="value">Raw string from the API payload.</param>
    /// <param name="enumName">Parameter name used in the exception.</param>
    public static TEnum Parse<TEnum>(string? value, string enumName)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value) || !Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed))
        {
            throw new ArgumentException(
                $"'{value}' is not a valid {typeof(TEnum).Name}. Allowed values: {string.Join(", ", Enum.GetNames<TEnum>())}.",
                enumName);
        }

        return parsed;
    }

    /// <summary>
    /// Overload that uses the enum type name as the parameter name, e.g.
    /// <c>EnumParser.Parse&lt;TipoCuenta&gt;(request.Tipo)</c>.
    /// </summary>
    public static TEnum Parse<TEnum>(string? value)
        where TEnum : struct, Enum
        => Parse<TEnum>(value, typeof(TEnum).Name);
}