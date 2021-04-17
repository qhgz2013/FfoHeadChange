using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FfoHeadChange
{
    public class TokenScanEOFException: Exception
    {

    }
    public class SimpleCsvParser
    {
        public static CsvTable ParseTable(string csvFile)
        {
            var table = new CsvTable();
            var sr = new StreamReader(csvFile, Encoding.UTF8);
            try
            {
                var headers = ParseLine(sr);
                foreach (var header in headers)
                {
                    table.Add(header, new List<string>());
                }
                List<string> line = ParseLine(sr);
                while (line.Count > 0)
                {
                    for (int i = 0; i < Math.Min(line.Count, headers.Count); i++)
                    {
                        table[headers[i]].Add(line[i]);
                    }
                    for (int i = line.Count; i <headers.Count; i++)
                    {
                        table[headers[i]].Add(null); // empty value
                    }
                    line = ParseLine(sr);
                }
            }
            finally
            {
                sr.Close();
            }
            return table;
        }

        private static List<string> ParseLine(StreamReader sr)
        {
            char chr = '\0';
            var tokens = new List<string>();
            try
            {
                do
                {
                    tokens.Add(ParseToken(sr, ref chr));
                } while (chr != '\n' && chr != '\uffff');
            }
            catch (TokenScanEOFException)
            {
            }
            return tokens;
        }

        private static string ParseToken(StreamReader sr, ref char chr)
        {
            chr = (char)sr.Read();
            if (chr == '\uffff')
                throw new TokenScanEOFException();
            if (chr == '"')
                return ParseQuotedToken(sr, ref chr);
            else
                return chr.ToString() + ParseTokenUntilEOT(sr, ",\n\uffff", ref chr);
        }

        private static string ParseQuotedToken(StreamReader sr, ref char chr)
        {
            var token = ParseTokenUntilEOT(sr, "\"\uffff", ref chr);
            if (ParseTokenUntilEOT(sr, ",\n\uffff", ref chr) != "")
                throw new FileFormatException("Unexpected data after '\"' token");
            return token;
        }
        private static string ParseTokenUntilEOT(StreamReader sr, string eot, ref char chr)
        {
            var sb = new StringBuilder();
            while (!eot.Contains(chr = (char)sr.Read()))
            {
                sb.Append(chr);
            }
            return sb.ToString();
        }
    }
    public class CsvTable : IDictionary<string, List<string>>
    {
        private Dictionary<string, List<string>> table = new Dictionary<string, List<string>>();

        public List<string> this[string key] { get => ((IDictionary<string, List<string>>)table)[key]; set => ((IDictionary<string, List<string>>)table)[key] = value; }

        public ICollection<string> Keys => ((IDictionary<string, List<string>>)table).Keys;

        public ICollection<List<string>> Values => ((IDictionary<string, List<string>>)table).Values;

        public int Count => ((IDictionary<string, List<string>>)table).Count;

        public bool IsReadOnly => ((IDictionary<string, List<string>>)table).IsReadOnly;

        public void Add(string key, List<string> value)
        {
            ((IDictionary<string, List<string>>)table).Add(key, value);
        }

        public void Add(KeyValuePair<string, List<string>> item)
        {
            ((IDictionary<string, List<string>>)table).Add(item);
        }

        public void Clear()
        {
            ((IDictionary<string, List<string>>)table).Clear();
        }

        public bool Contains(KeyValuePair<string, List<string>> item)
        {
            return ((IDictionary<string, List<string>>)table).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, List<string>>)table).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, List<string>>[] array, int arrayIndex)
        {
            ((IDictionary<string, List<string>>)table).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, List<string>>> GetEnumerator()
        {
            return ((IDictionary<string, List<string>>)table).GetEnumerator();
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, List<string>>)table).Remove(key);
        }

        public bool Remove(KeyValuePair<string, List<string>> item)
        {
            return ((IDictionary<string, List<string>>)table).Remove(item);
        }

        public bool TryGetValue(string key, out List<string> value)
        {
            return ((IDictionary<string, List<string>>)table).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<string, List<string>>)table).GetEnumerator();
        }
    }
}
