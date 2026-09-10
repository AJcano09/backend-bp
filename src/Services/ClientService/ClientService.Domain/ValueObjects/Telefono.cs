using System.Text.RegularExpressions;

namespace ClientService.Domain.ValueObjects;

/// <summary>
/// Value object encapsulating a valid phone number.
/// Immutable: created only through <see cref="Create"/>.
/// </summary>
public sealed record Telefono
{
    private static readonly Regex ValidPattern = new(
        @"^\+?[0-9][0-9\s\-]{6,14}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Value { get; }

    private Telefono(string value) => Value = value;

    public static Telefono Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim();
        if (!ValidPattern.IsMatch(normalized))
            throw new ArgumentException(
                "Phone must contain between 7 and 15 digits; '+', spaces and dashes are allowed.",
                nameof(value));

        return new Telefono(normalized);
    }

    public override string ToString() => Value;
}