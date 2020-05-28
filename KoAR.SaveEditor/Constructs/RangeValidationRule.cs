using System.Globalization;
using System.Windows.Controls;

namespace KoAR.SaveEditor.Constructs
{
    public sealed class RangeValidationRule : ValidationRule
    {
        public long Min { get; set; } = long.MinValue;

        public long Max { get; set; } = long.MaxValue;

        public NumberStyles NumberStyle { get; set; } = NumberStyles.Any;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return value is string text
                && text.Length != 0
                && long.TryParse(text, NumberStyle, cultureInfo, out long number)
                && (number >= Min)
                && (number <= Max)
                ? ValidationResult.ValidResult
                : new ValidationResult(false, $"Value must be a number between {Min} and {Max}.");
        }
    }
}
