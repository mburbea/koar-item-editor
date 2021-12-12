using System.Globalization;
using System.Windows.Controls;

namespace KoAR.SaveEditor.Constructs;

public sealed class RangeValidationRule : ValidationRule
{
    public long Max { get; set; } = long.MaxValue;

    public long Min { get; set; } = long.MinValue;

    public NumberStyles NumberStyle { get; set; } = NumberStyles.Any;

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        return value is string text
            && long.TryParse(text, this.NumberStyle, cultureInfo, out long number)
            && number >= this.Min && number <= this.Max
            ? ValidationResult.ValidResult
            : new(false, $"Value must be a number between {this.Min} and {this.Max}.");
    }
}
