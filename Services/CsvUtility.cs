using System.Text;

namespace ERP.Identity.Services
{
    /// <summary>
    /// Minimal RFC-4180-ish CSV reader/writer. Deliberately dependency-free (no
    /// CsvHelper/ClosedXML) so the project restores with nothing beyond the
    /// packages already in the .csproj. Handles quoted fields, embedded commas,
    /// and embedded newlines/quotes inside quoted fields.
    /// </summary>
    public static class CsvUtility
    {
        public static List<Dictionary<string, string>> ParseWithHeader(Stream stream)
        {
            var rows = ParseRows(stream);
            var result = new List<Dictionary<string, string>>();
            if (rows.Count == 0) return result;

            var headers = rows[0].Select(h => h.Trim()).ToList();

            for (var r = 1; r < rows.Count; r++)
            {
                var row = rows[r];
                if (row.All(string.IsNullOrWhiteSpace)) continue; // skip blank trailing lines

                var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (var c = 0; c < headers.Count; c++)
                {
                    dict[headers[c]] = c < row.Count ? row[c] : string.Empty;
                }
                result.Add(dict);
            }

            return result;
        }

        private static List<List<string>> ParseRows(Stream stream)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var text = reader.ReadToEnd();

            var rows = new List<List<string>>();
            var row = new List<string>();
            var field = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < text.Length && text[i + 1] == '"')
                        {
                            field.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        field.Append(c);
                    }
                }
                else
                {
                    switch (c)
                    {
                        case '"':
                            inQuotes = true;
                            break;
                        case ',':
                            row.Add(field.ToString());
                            field.Clear();
                            break;
                        case '\r':
                            break;
                        case '\n':
                            row.Add(field.ToString());
                            field.Clear();
                            rows.Add(row);
                            row = new List<string>();
                            break;
                        default:
                            field.Append(c);
                            break;
                    }
                }
            }

            // last field/row (file may not end with a newline)
            if (field.Length > 0 || row.Count > 0)
            {
                row.Add(field.ToString());
                rows.Add(row);
            }

            return rows;
        }

        public static byte[] WriteWithHeader(IEnumerable<string> headers, IEnumerable<IEnumerable<string?>> rows)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", headers.Select(Escape)));

            foreach (var row in rows)
            {
                sb.AppendLine(string.Join(",", row.Select(Escape)));
            }

            // UTF-8 BOM so Excel opens it with correct encoding
            var preamble = Encoding.UTF8.GetPreamble();
            var body = Encoding.UTF8.GetBytes(sb.ToString());
            var result = new byte[preamble.Length + body.Length];
            Buffer.BlockCopy(preamble, 0, result, 0, preamble.Length);
            Buffer.BlockCopy(body, 0, result, preamble.Length, body.Length);
            return result;
        }

        private static string Escape(string? value)
        {
            value ??= string.Empty;
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }
            return value;
        }

        public static string? Get(this Dictionary<string, string> row, string key)
        {
            return row.TryGetValue(key, out var value) ? value.Trim() : null;
        }
    }
}
