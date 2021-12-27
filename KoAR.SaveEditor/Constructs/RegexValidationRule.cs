using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace KoAR.SaveEditor.Constructs;

public sealed class RegexValidationRule : ValidationRule
{
    private static readonly ValidationResult _invalidResult = new(false, "Text does not conform to the expected pattern.");

    public string? Pattern { get; set; }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        string? text = value as string;
        return string.IsNullOrEmpty(this.Pattern) || string.IsNullOrEmpty(text) || Regex.IsMatch(text, this.Pattern)
            ? ValidationResult.ValidResult
            : RegexValidationRule._invalidResult;
    }
}
