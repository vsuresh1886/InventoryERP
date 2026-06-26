using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Infrastructure.Document.Helpers
{
    public static class AmountToWordsConverter
    {
        private static readonly string[] Ones =
        {
            "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
            "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen",
            "Seventeen", "Eighteen", "Nineteen"
        };

        private static readonly string[] Tens =
        {
            "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"
        };

        /// <summary>
        /// Converts an amount to words, e.g. 122720.00 -> "INR One Lakh
        /// Twenty-two Thousand Seven Hundred Twenty Only". Paise (decimal
        /// part) are appended as "... and Fifty Paise" when present.
        /// </summary>
        /// <param name="amount">The amount to convert. Negative values are converted as their absolute value with a leading "Minus".</param>
        /// <param name="currencyPrefix">Prefix shown before the words, e.g. "INR". Pass null/empty to omit.</param>
        public static string Convert(decimal amount, string currencyPrefix = "INR")
        {
            if (amount < 0)
                return $"Minus {Convert(Math.Abs(amount), currencyPrefix)}";

            // Split rupees and paise cleanly, avoiding floating point drift.
            long rupees = (long)Math.Truncate(amount);
            int paise = (int)Math.Round((amount - rupees) * 100, MidpointRounding.AwayFromZero);

            // Handle the case where rounding paise up to 100 should carry into rupees.
            if (paise == 100)
            {
                rupees += 1;
                paise = 0;
            }

            var rupeeWords = NumberToIndianWords(rupees);

            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(currencyPrefix))
                sb.Append(currencyPrefix).Append(' ');

            sb.Append(rupeeWords);

            if (paise > 0)
            {
                sb.Append(" and ").Append(NumberToIndianWords(paise)).Append(" Paise");
            }

            sb.Append(" Only");
            return sb.ToString();
        }

        /// <summary>
        /// Converts a non-negative whole number into Indian-system words
        /// (no currency prefix/suffix) — e.g. 122720 -> "One Lakh
        /// Twenty-two Thousand Seven Hundred Twenty". Exposed publicly in
        /// case you need raw number-to-words elsewhere (e.g. quantities).
        /// </summary>
        public static string NumberToIndianWords(long number)
        {
            if (number == 0)
                return "Zero";

            if (number < 0)
                throw new ArgumentOutOfRangeException(nameof(number), "Use Convert(decimal) for negative amounts.");

            long crore = number / 10_000_000;
            number %= 10_000_000;
            long lakh = number / 100_000;
            number %= 100_000;
            long thousand = number / 1_000;
            number %= 1_000;
            long hundred = number; // remaining 0-999

            var parts = new System.Collections.Generic.List<string>();

            if (crore > 0)
                parts.Add(ThreeDigitWords((int)crore) + " Crore");

            if (lakh > 0)
                parts.Add(ThreeDigitWords((int)lakh) + " Lakh");

            if (thousand > 0)
                parts.Add(ThreeDigitWords((int)thousand) + " Thousand");

            if (hundred > 0)
                parts.Add(ThreeDigitWords((int)hundred));

            return string.Join(" ", parts);
        }

        private static string ThreeDigitWords(int n)
        {
            if (n == 0) return "";
            if (n < 20) return Ones[n];
            if (n < 100) return TwoDigitWords(n);

            int hundredsDigit = n / 100;
            int remainder = n % 100;

            var result = Ones[hundredsDigit] + " Hundred";
            if (remainder > 0)
                result += " " + TwoDigitWords(remainder);

            return result;
        }

        private static string TwoDigitWords(int n)
        {
            if (n < 20) return Ones[n];

            int tensDigit = n / 10;
            int onesDigit = n % 10;

            // Use a hyphen between tens and ones, matching your sample PDF
            // style ("Twenty-two") rather than a plain space.
            return onesDigit == 0 ? Tens[tensDigit] : $"{Tens[tensDigit]}-{Ones[onesDigit]}";
        }
    }
}
