using System.Globalization;
using System.Windows.Controls;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public sealed class DurabilityValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return value is string text
                ? DurabilityValidationRule.Validate(text.Trim().TrimEnd('.'), cultureInfo)
                : ValidationResult.ValidResult;
        }

        private static ValidationResult Validate(string text, CultureInfo culture)
        {
            if (text.Length == 0)
            {
                return new(false, "Value is required.");
            }
            if (!float.TryParse(text, NumberStyles.Float, culture, out float durability))
            {
                return new(false, "Durability must be a numeric value.");
            }
            if (!Item.IsValidDurability(durability))
            {
                return new(false, $"Durability must be between {Item.DurabilityLowerBound} and {Item.DurabilityUpperBound}.");
            }
            return ValidationResult.ValidResult;
        }
    }
}
