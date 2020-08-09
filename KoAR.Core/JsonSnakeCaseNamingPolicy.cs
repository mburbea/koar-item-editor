using System;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace KoAR.Core
{
    public sealed class JsonSnakeCaseNamingPolicy : JsonNamingPolicy
    {
        public static readonly JsonNamingPolicy Instance = new JsonSnakeCaseNamingPolicy();

        public override string ConvertName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            StringBuilder builder = new StringBuilder(name.Length + Math.Max(2, name.Length / 5));
            UnicodeCategory? previousCategory = null;
            for (int index = 0; index < name.Length; index++)
            {
                char current = name[index];
                if (current == '_')
                {
                    builder.Append('_');
                    previousCategory = null;
                    continue;
                }
                UnicodeCategory currentCategory = char.GetUnicodeCategory(current);
                switch (currentCategory)
                {
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.TitlecaseLetter:
                        if (previousCategory == UnicodeCategory.SpaceSeparator ||
                            previousCategory == UnicodeCategory.LowercaseLetter ||
                            previousCategory != UnicodeCategory.DecimalDigitNumber &&
                            index > 0 &&
                            index + 1 < name.Length &&
                            char.IsLower(name, index + 1))
                        {
                            builder.Append('_');
                        }
                        current = char.ToLowerInvariant(current);
                        break;
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.DecimalDigitNumber:
                        if (previousCategory == UnicodeCategory.SpaceSeparator)
                        {
                            builder.Append('_');
                        }
                        break;
                    case UnicodeCategory.Surrogate:
                        break;
                    default:
                        if (previousCategory != null)
                        {
                            previousCategory = UnicodeCategory.SpaceSeparator;
                        }
                        continue;
                }
                builder.Append(current);
                previousCategory = currentCategory;
            }
            return builder.ToString();
        }
    }
}
