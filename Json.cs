using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniJson
{
    /* Json is a minimal JSON implementation
     * It offers 2 static functions: Stringify & Parse
     * Parse converts a valid Json string to an object
     *      On failure, it will throw a FormatException
     *      Validation is not absolute; it'll parse "null1234" as null
     * Stringify converts an object to Json
     *      On failure, it will throw an ArgumentException
     * Objects are of type double, string, List<object>, Dictionary<string, object>, bool, or null
     * */
    public static class Json
    {
        private static int skipWhitespace(string str, int idx)
        {
            while (char.IsWhiteSpace(str[idx]))
            {
                idx += 1;
            }
            return idx;
        }

        private static object ParseValue(string str, ref int idx)
        {
            idx = skipWhitespace(str, idx);
            var ch = str[idx];
            if (ch == '{') {
                idx += 1;
                return ParseObject(str, ref idx);
            }
            else if (ch == '[')
            {
                idx += 1;
                return ParseArray(str, ref idx);
            }
            else if (ch == '"')
            {
                idx += 1;
                return ParseString(str, ref idx);
            }
            else if ((ch >= '0' && ch <= '9') || ch == '-')
            {
                return ParseNumber(str, ref idx);
            }
            else if (str.Substring(idx, 4) == "null")
            {
                idx += 4;
                return null;
            }
            else if (str.Substring(idx, 4) == "true")
            {
                idx += 4;
                return true;
            }
            else if (str.Substring(idx, 5) == "false")
            {
                idx += 5;
                return false;
            }
            else
            {
                throw new FormatException("Unknown value @ " + idx);
            }
        }

        private static Dictionary<string, object> ParseObject(string str, ref int idx)
        {
            var ret = new Dictionary<string, object>();
            idx = skipWhitespace(str, idx);
            if (str[idx] == '}')
            {
                idx += 1;
                return ret;
            }
            while (true)
            {
                if (str[idx] != '"')
                {
                    throw new FormatException("Object expected string @ " + idx);
                }
                idx += 1;
                var key = ParseString(str, ref idx);
                idx = skipWhitespace(str, idx);
                if (str[idx] != ':')
                {
                    throw new FormatException("Object expected ':' @ " + idx);
                }
                idx = skipWhitespace(str, idx+1);
                var val = ParseValue(str, ref idx);
                ret[key] = val;
                idx = skipWhitespace(str, idx);
                if (str[idx] == ',')
                {
                    idx = skipWhitespace(str, idx + 1);
                }
                else if (str[idx] != '}')
                {
                    throw new FormatException("Object expected ',' or '}' @ " + idx);
                }
                else
                {
                    idx += 1;
                    return ret;
                }
            }
        }

        private static List<object> ParseArray(string str, ref int idx)
        {
            var ret = new List<object>();
            idx = skipWhitespace(str, idx);
            if (str[idx] == ']')
            {
                idx += 1;
                return ret;
            }
            while (true)
            {
                ret.Add(ParseValue(str, ref idx));
                idx = skipWhitespace(str, idx);
                if (str[idx] == ',')
                {
                    idx = skipWhitespace(str, idx+1);
                }
                else if (str[idx] != ']')
                {
                    throw new FormatException("Array expected ',' or ']'");
                }
                else
                {
                    idx += 1;
                    return ret;
                }
            }
        }

        private static int chr2hexval(char ch)
        {
            if (ch >= '0' && ch <= '9')
            {
                return ch - '0';
            }
            else if (ch >= 'a' && ch <= 'f')
            {
                return (ch - 'a') + 10;
            }
            else if (ch >= 'A' && ch <= 'F')
            {
                return (ch - 'A') + 10;
            }
            else
            {
                return -1;
            }
        }

        private static string ParseString(string str, ref int idx)
        {
            var sb = new StringBuilder();
            while (str[idx] != '"')
            {
                var ch = str[idx];
                if (ch == '\\')
                {
                    var c2 = str[idx+1];
                    if (c2 == '"' || c2 == '/' || c2 == '\\') sb.Append(c2);
                    else if (c2 == 'n') sb.Append('\n');
                    else if (c2 == 't') sb.Append('\t');
                    else if (c2 == 'r') sb.Append('\r');
                    else if (c2 == 'f') sb.Append('\f');
                    else if (c2 == 'b') sb.Append('\b');
                    else
                    {
                        var hex4 = str.Substring(idx + 1, 4);
                        if (hex4.Length != 4){
                            throw new FormatException("Unicode escape not length 4");
                        }
                        var code = chr2hexval(str[idx+2])<<12 | chr2hexval(str[idx+3])<<8 | chr2hexval(str[idx+4])<<4 | chr2hexval(str[idx+5]);
                        if (code < 0 || code > 0xffff)
                        {
                            throw new FormatException("Invalid hexadecimal character");
                        }
                        sb.Append((char)code);
                        idx += 6;
                        continue;
                    }
                    idx += 2;
                }
                else
                {
                    sb.Append(ch);
                    idx += 1;
                }
            }
            idx += 1;
            return sb.ToString();
        }

        private static double ParseNumber(string str, ref int idx)
        {
            var ch = str[idx];
            bool neg;
            double result = 0;
            if (ch == '-')
            {
                neg = true;
                idx += 1;
                ch = str[idx];
            }
            else
            {
                neg = false;
            }
            if (ch == '0')
            {
                idx += 1;
                if (idx == str.Length || str[idx] != '.')
                {
                    return 0;
                }
                ch = '.';
            }
            else if (ch >= '1' && ch <= '9')
            {
                do {
                    result = result * 10 + (ch - '0');
                    idx += 1;
                    if (idx == str.Length) return neg ? -result : result;
                    ch = str[idx];
                } while (ch >= '0' && ch <= '9');
            } else {
                throw new FormatException("Expected digit @ " + idx);
            }
            if (ch == '.')
            {
                idx += 1;
                ch = str[idx];
                double nth = 0.0;
                while (ch >= '0' && ch <= '9')
                {
                    nth++;
                    result += (ch - '0') / Math.Pow(10, nth);
                    idx += 1;
                    if (idx == str.Length) return neg ? -result : result;
                    ch = str[idx];
                }
                if (nth == 0.0)
                {
                    throw new FormatException("Decimal followed by no digits @ idx");
                }
            }
            if (ch == 'e' || ch == 'E')
            {
                idx += 1;
                ch = str[idx];
                if ((ch < '0' || ch > '9') && ch != '+' && ch != '-') {
                    throw new FormatException("Exponential not followed by digits or + or -");
                }
                var expneg = ch == '-';
                if (ch == '-' || ch == '+') {
                    idx += 1;
                    ch = str[idx];
                    if (ch < '0' || ch > '9')
                    {
                        throw new FormatException("Exponential not followed by digits");
                    }
                }
                int exp = 0;
                while (ch >= '0' && ch <= '9')
                {
                    exp = exp * 10 + (ch - '0');
                    idx += 1;
                    if (idx == str.Length) break;
                    ch = str[idx];
                }
                result *= Math.Pow(10, (expneg ? -exp : exp));
            }
            return neg ? -result : result;
        }

        private static string StringifyString(string str)
        {
            var sb = new StringBuilder("\"", str.Length + 2);
            foreach (var c in str)
            {
                var substr = c == '"' ? "\\\"" :
                    c == '\\' ? "\\\\" :
                    c == '\n' ? "\\n" :
                    c == '\b' ? "\\b" :
                    c == '\f' ? "\\f" :
                    c == '\r' ? "\\r" :
                    c == '\t' ? "\\t" :
                    c < ' ' ? "\\u" + ((int)c).ToString("X4") : null;
                if (substr != null) {
                    sb.Append(substr);
                }
                else {
                    sb.Append(c);
                }
            }
            return sb.Append('"').ToString();
        }

        public static object Parse(string json)
        {
            try
            {
                var idx = 0;
                return ParseValue(json, ref idx);
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new FormatException("Unexpected end of input", ex);
            }
        }

        public static string Stringify(object obj)
        {
            if (obj == null) return "null";
            var oty = obj.GetType();
            if (oty == typeof(string))
            {
                return StringifyString((string)obj);
            }
            if (oty == typeof(double))
            {
                var val = (double)obj;
                if (!double.IsNaN(val) && !double.IsInfinity(val)) {
                    return val.ToString();
                }
            }
            if (oty == typeof(bool))
            {
                return (bool)obj ? "true" : "false";
            }
            if (oty == typeof(List<object>))
            {
                return "[" + string.Join(",", ((List<object>)obj).Select(Stringify)) + "]";
            }
            if (oty == typeof(Dictionary<string, object>))
            {
                return "{" + string.Join(",", ((Dictionary<string, object>)obj).Select(kv => {
                    return StringifyString(kv.Key) + ":" + Stringify(kv.Value);
                })) + "}";
            }
            throw new ArgumentException("Object not JSON convertible");
        }
    }
}
