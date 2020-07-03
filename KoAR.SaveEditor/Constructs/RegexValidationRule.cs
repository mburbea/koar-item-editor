using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace KoAR.SaveEditor.Constructs
{
    public sealed class RegexValidationRule : ValidationRule
    {
        public string? ErrorText { get; set; }

        public string? Pattern { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string? text = value as string;
            return string.IsNullOrEmpty(this.Pattern) || string.IsNullOrEmpty(text) || Regex.IsMatch(text, this.Pattern)
                ? ValidationResult.ValidResult
                : new ValidationResult(false, this.ErrorText ?? "Text does not conform to the expected pattern.");
        }
    }
}
