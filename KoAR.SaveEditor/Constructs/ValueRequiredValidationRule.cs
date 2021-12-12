using System.Globalization;
using System.Windows.Controls;

namespace KoAR.SaveEditor.Constructs;

public sealed class ValueRequiredValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        return value == null || value.Equals(string.Empty) ? new(false, "Value is required.") : ValidationResult.ValidResult;
    }
}
