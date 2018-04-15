using System;
using System.Globalization;
using System.Text;

namespace AutoServiss.Helpers
{
    public class Diacritics
    {
        public static string RemoveDiacritics(String s)
        {
            var normalizedString = s.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }
            return stringBuilder.ToString();
        }
    }
}