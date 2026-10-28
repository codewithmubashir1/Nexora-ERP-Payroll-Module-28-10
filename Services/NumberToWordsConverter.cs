using System.Text;

namespace ERP.Identity.Services
{
    public static class NumberToWordsConverter
    {
        private static readonly string[] Ones =
        {
            "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten",
            "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"
        };

        private static readonly string[] Tens =
        {
            "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"
        };

        /// <summary>e.g. 310500.48 -> "Three Hundred and Ten Thousand Five Hundred and Forty Eight Paisa Only"</summary>
        public static string ToWords(decimal amount)
        {
            amount = Math.Abs(Math.Round(amount, 2));
            var whole = (long)Math.Floor(amount);
            var fraction = (int)Math.Round((amount - whole) * 100);

            var sb = new StringBuilder();
            sb.Append(whole == 0 ? "Zero" : ConvertWhole(whole));

            if (fraction > 0)
            {
                sb.Append(" and ").Append(ConvertBelowThousand(fraction)).Append(" Paisa");
            }

            sb.Append(" Only");
            return sb.ToString();
        }

        private static string ConvertWhole(long number)
        {
            if (number == 0) return "";

            var parts = new List<string>();

            var billions = number / 1_000_000_000;
            number %= 1_000_000_000;
            var millions = number / 1_000_000;
            number %= 1_000_000;
            var thousands = number / 1_000;
            number %= 1_000;
            var remainder = number;

            if (billions > 0) parts.Add(ConvertBelowThousand(billions) + " Billion");
            if (millions > 0) parts.Add(ConvertBelowThousand(millions) + " Million");
            if (thousands > 0) parts.Add(ConvertBelowThousand(thousands) + " Thousand");
            if (remainder > 0) parts.Add(ConvertBelowThousand(remainder));

            return string.Join(" ", parts);
        }

        private static string ConvertBelowThousand(long number)
        {
            if (number == 0) return "";

            if (number < 20) return Ones[number];

            if (number < 100)
            {
                var tens = Tens[number / 10];
                var ones = number % 10;
                return ones > 0 ? $"{tens} {Ones[ones]}" : tens;
            }

            var hundreds = number / 100;
            var rest = number % 100;
            var result = $"{Ones[hundreds]} Hundred";
            if (rest > 0) result += " and " + ConvertBelowThousand(rest);
            return result;
        }
    }
}
