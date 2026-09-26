namespace loukupm.Services
{
    /// <summary>
    /// Normalizes a phone number to the German prefix +49.
    /// </summary>
    public static class PhoneNumberFormatter
    {
        public const string GermanyPrefix = "+49";

        public static string GetGuidanceMessage()
        {
            var message = Langue.AppResource.PhonePrefixRequired;

            if (string.IsNullOrWhiteSpace(message))
            {
                message = "Die Vorwahl +49 ist bereits eingetragen. Bitte ergänzen Sie nur die restlichen Ziffern, zum Beispiel:";
            }

            return message;
        }

        public readonly record struct PhoneFieldEdit(string National, string Text, int Cursor);

        /// <summary>
        /// Reads the digits typed after a fixed +49, without treating the calling code itself as part of the number.
        /// </summary>
        public static string ReadNational(string? raw)
        {
            var text = raw ?? string.Empty;
            var prefixIndex = text.IndexOf(GermanyPrefix, StringComparison.Ordinal);

            var digits = prefixIndex >= 0
                ? Digits(text.Remove(prefixIndex, GermanyPrefix.Length))
                : Digits(text);

            return SanitizeNational(digits);
        }

        /// <summary>
        /// Applies a keystroke while keeping +49 fixed.
        /// Digits typed over the calling code are appended after it, so the code is not repeated.
        /// </summary>
        public static PhoneFieldEdit MergeFixedPrefix(string? currentNational, string? newText)
        {
            var national = SanitizeNational(Digits(currentNational));
            var oldText = GermanyPrefix + national;
            var text = newText ?? string.Empty;

            if (string.Equals(text, oldText, StringComparison.Ordinal))
                return new PhoneFieldEdit(national, oldText, GermanyPrefix.Length + national.Length);

            var start = 0;
            var max = Math.Min(oldText.Length, text.Length);
            while (start < max && oldText[start] == text[start])
                start++;

            var oldEnd = oldText.Length - 1;
            var newEnd = text.Length - 1;
            while (oldEnd >= start && newEnd >= start && oldText[oldEnd] == text[newEnd])
            {
                oldEnd--;
                newEnd--;
            }

            var inserted = newEnd >= start ? text.Substring(start, newEnd - start + 1) : string.Empty;
            var removed = oldEnd >= start ? oldText.Substring(start, oldEnd - start + 1) : string.Empty;
            var insertedDigits = Digits(inserted);

            var removedNational = 0;
            for (var i = 0; i < removed.Length; i++)
            {
                if (start + i >= GermanyPrefix.Length && char.IsDigit(removed[i]))
                    removedNational++;
            }

            string updated;
            int cursor;

            if (start < GermanyPrefix.Length && removedNational == 0)
            {
                updated = SanitizeNational(national + insertedDigits);
                cursor = GermanyPrefix.Length + updated.Length;
            }
            else
            {
                var nationalIndex = Math.Clamp(start - GermanyPrefix.Length, 0, national.Length);
                var removeCount = Math.Min(removedNational, national.Length - nationalIndex);
                updated = national.Remove(nationalIndex, removeCount);
                updated = updated.Insert(Math.Min(nationalIndex, updated.Length), insertedDigits);
                var cursorInNational = Math.Min(nationalIndex + insertedDigits.Length, updated.Length);
                var leadingZeros = 0;
                while (leadingZeros < updated.Length && updated[leadingZeros] == '0')
                    leadingZeros++;

                updated = SanitizeNational(updated);
                cursorInNational = Math.Max(0, cursorInNational - leadingZeros);
                cursor = GermanyPrefix.Length + Math.Min(cursorInNational, updated.Length);
            }

            return new PhoneFieldEdit(updated, GermanyPrefix + updated, cursor);
        }

        private static string Digits(string? raw) =>
            new((raw ?? string.Empty).Where(char.IsDigit).ToArray());

        private static string SanitizeNational(string digits)
        {
            digits = digits.TrimStart('0');

            if (digits.Length > 13)
                digits = digits[..13];

            return digits;
        }

        public static bool IsServerPrefixMessage(string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return false;

            return message.Contains("+40", StringComparison.OrdinalIgnoreCase)
                || message.Contains("0040", StringComparison.Ordinal)
                || message.Contains("prefix", StringComparison.OrdinalIgnoreCase)
                || message.Contains("vorwahl", StringComparison.OrdinalIgnoreCase)
                || message.Contains("country code", StringComparison.OrdinalIgnoreCase)
                || message.Contains("ländervorwahl", StringComparison.OrdinalIgnoreCase)
                || message.Contains("بادئة", StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns a German number starting with +49.
        /// A local number without a prefix receives +49.
        /// </summary>
        public static bool TryNormalize(string? raw, out string normalized)
        {
            normalized = string.Empty;

            if (string.IsNullOrWhiteSpace(raw))
                return false;

            var trimmed = raw.Trim();
            var hadPlus = trimmed.StartsWith('+');
            var digits = new string(trimmed.Where(char.IsDigit).ToArray());

            if (digits.Length == 0)
                return false;

            string national;

            if (digits.StartsWith("0049", StringComparison.Ordinal))
            {
                national = digits[4..];
            }
            else if (digits.StartsWith("49", StringComparison.Ordinal)
                     && (hadPlus || digits.Length >= 11))
            {
                national = digits[2..];
            }
            else if (hadPlus || digits.StartsWith("00", StringComparison.Ordinal))
            {
                return false;
            }
            else if (digits.StartsWith('0'))
            {
                national = digits[1..];
            }
            else
            {
                national = digits;
            }

            national = national.TrimStart('0');

            if (national.Length is < 6 or > 13 || national.Any(ch => !char.IsDigit(ch)))
                return false;

            normalized = GermanyPrefix + national;
            return true;
        }
    }
}
