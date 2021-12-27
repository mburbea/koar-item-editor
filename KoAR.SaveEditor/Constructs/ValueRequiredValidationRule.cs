using System.Globalization;
using System.Windows.Controls;

namespace KoAR.SaveEditor.Constructs;

public sealed class ValueRequiredValidationRule : ValidationRule
{
    private static readonly ValidationResult _invalidResult = new(false, "Value is required.");

    public override ValidationResult Validate(object value, CultureInfo cultureInfo) => value == null || value.Equals(string.Empty) 
        ? ValueRequiredValidationRule._invalidResult 
        : ValidationResult.ValidResult;
}
