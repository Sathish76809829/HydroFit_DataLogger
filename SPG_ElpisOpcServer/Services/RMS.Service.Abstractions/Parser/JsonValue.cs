using System.Collections.Generic;
using System.Text;

namespace RMS.Service.Abstractions.Parser
{
    public class JsonValue
    {
        public static readonly JsonValue Empty = CreateEmpty(JsonValueType.End);

        internal object content;

        internal JsonValueType type;

        public object Content => content;

        public JsonValueType Type => type;

        internal JsonValue next;

        public JsonValue()
        {

        }

        public JsonValue(JsonValue value)
        {
            content = value.content;
            type = value.type;
        }

        public JsonValue Next => next;

        public bool HasValues => next != this;

        public JsonValue Append(object value, JsonValueType type)
        {
            var n = new JsonValue
            {
                content = value,
                type = type
            };
            if (next is null)
            {
                n.next = n;
                return n;
            }
            n.next = next;
            next = n;
            return n;
        }

        public JsonValue Append(JsonValue value)
        {
            var n = new JsonValue(value);
            if (next is null)
            {
                n.next = n;
                return n;
            }
            n.next = next;
            next = n;
            return n;
        }

        public void AppendText(StringBuilder sb)
        {
            sb.Append(content);
        }

        JsonValue SetContent(JsonSource s)
        {
            JsonValue res = this;
            int dot = 0;
            int exp = 0;
            while (s.CanAdvance)
            {
                char c = s.PeekChar();
                switch (c)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        s.Store(c)
                             .AdvanceChar();
                        continue;
                    case '.':
                        //skip .
                        s.AdvanceChar();
                        if (char.IsDigit(s.PeekChar()))
                        {
                            dot++;
                            //add .
                            s.Store('.')
                             .Store(s.ReadChar());
                            // skip digit
                            if (dot > 1)
                            {
                                res = res.Append(ReadStringValue(s), JsonValueType.String);
                            }
                            continue;
                        }
                        res = res.Append(ReadStringValue(s.Store('.')), JsonValueType.String);
                        continue;
                    case 'e':
                    case 'E':
                        s.Store(c)
                         .AdvanceChar();
                        exp++;
                        if (exp > 1)
                        {
                            break;
                        }
                        var next = s.PeekChar();
                        if (next == '+' || next == '-')
                        {
                            s.Store(next)
                             .AdvanceChar();
                        }
                        continue;
                    case '+':
                    case '-':
                        next = s.AdvanceChar().PeekChar();
                        if (s.StoreSize > 0 || char.IsDigit(next) == false )
                        {
                            res = res.Append(ReadStringValue(s.Store(c)), JsonValueType.String);
                            continue;
                        }
                        s.Store(c);
                        continue;
                    case '@':
                    case '<':
                    case '>':
                        if (s.StoreSize > 0)
                        {
                            res = res.Append(GetNumericValue(s, dot), JsonValueType.Number);
                            dot = 0;
                            exp = 0;
                        }
                        res = res.Append(s.ReadChar(), JsonValueType.String);
                        break;
                    case Tokens.SingleQuote:
                        res = res.Append(ReadQuotedStringValue(s.AdvanceChar()), JsonValueType.String);
                        break;
                    case Tokens.DoubleQuote:
                        s.AdvanceChar();
                        // Ends quoted string
                        if (s.StoreSize > 0)
                            return res.Append(GetNumericValue(s, dot), JsonValueType.Number);
                        return res;
                    case ' ':
                        res = res.ReadSpacedValue(s, dot);
                        break;
                    default:
                        res = res.Append(ReadStringValue(s), JsonValueType.String);
                        break;
                }
            }
            return res;
        }

        JsonValue ReadSpacedValue(JsonSource s, int dot)
        {
            while (s.CanAdvance)
            {
                char next = s.PeekChar();
                switch (next)
                {
                    case ' ':
                        s.Store(next)
                            .AdvanceChar();
                        continue;
                    case '@':
                    case '>':
                    case '<':
                        return Append(GetSpacedNumeric(s, dot), JsonValueType.Number)
                            .Append(s.ReadChar(), JsonValueType.String);
                    case Tokens.DoubleQuote:
                    case Tokens.CloseBrace:
                    case Tokens.CloseBracket:
                        return Append(GetSpacedNumeric(s, dot), JsonValueType.Number);
                    default:
                        return Append(ReadStringValue(s), JsonValueType.String);
                }
            }
            return Append(GetSpacedNumeric(s, dot), JsonValueType.Number);
        }

        static object GetSpacedNumeric(JsonSource s, int dot)
        {
            string val = s.ReturnStore().TrimEnd();
            if (dot > 0)
            {
                return double.Parse(val, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                if (val.Length < 9)
                    return int.Parse(val, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                else
                    return long.Parse(val, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        internal JsonValue SetNumbericContent(JsonSource s)
        {
            type = JsonValueType.Number;
            int dot = 0;
            int exp = 0;
            for (; ; )
            {
                char c = s.PeekChar();
                switch (c)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        s.Store(c)
                             .AdvanceChar();
                        continue;
                    case '.':
                        //skip .
                        s.AdvanceChar();
                        if (char.IsDigit(s.PeekChar()))
                        {
                            dot++;
                            //add .
                            s.Store('.')
                             .Store(s.ReadChar());
                            //skip digit
                            if (dot > 1)
                            {
                                throw new System.Exception("Invalid Json numberic Identifier " + c);
                            }
                            continue;
                        }
                        throw new System.Exception("Invalid Json numberic Identifier " + c);
                    case 'e':
                    case 'E':
                        s.Store(c)
                         .AdvanceChar();
                        exp++;
                        if (exp > 1)
                        {
                            break;
                        }
                        var next = s.PeekChar();
                        if (next == '+' || next == '-')
                        {
                            s.Store(next)
                             .AdvanceChar();
                        }
                        continue;
                    case '+':
                    case '-':
                        next = s.AdvanceChar().PeekChar();
                        if (s.StoreSize > 0 || char.IsDigit(next) == false)
                        {
                            throw new System.Exception("Invalid Json numberic Identifier " + next);
                        }
                        s.Store(c);
                        continue;
                    case Tokens.CloseBracket:
                    case Tokens.CloseBrace:
                    case ' ':
                    case ',':
                    case char.MinValue:
                        content = GetNumericValue(s, dot);
                        return this;
                    default:
                        throw new System.Exception("Invalid Json numberic Identifier " + c);
                }
            }
        }

        public static JsonValue Parse(JsonSource s)
        {
            var value = new JsonValue();
            return value.SetContent(s);
        }

        public static JsonValue ParseNumeric(JsonSource s)
        {
            var value = new JsonValue();
            value.next = value;
            return value.SetNumbericContent(s);
        }

        public static JsonValue CreateEmpty(JsonValueType type)
        {
            var val = new JsonValue
            {
                content = string.Empty,
                type = type
            };
            val.next = val;
            return val;
        }

        public static JsonValue Create(string value)
        {
            var val = new JsonValue
            {
                content = value,
                type = JsonValueType.String
            };
            val.next = val;
            return val;
        }

        public object GetNumericValue(JsonSource s, int dot)
        {
            object value;
            if (dot > 0)
            {
                value = double.Parse(s.ReturnStore(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                if (s.StoreSize < 9)
                    value = int.Parse(s.ReturnStore(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                else
                    value = long.Parse(s.ReturnStore(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
            }

            return value;
        }

        public static string ReadStringValue(JsonSource s)
        {
            for (; ; )
            {
                char c = s.PeekChar();
                switch (c)
                {
                    case Tokens.DoubleQuote:
                    case '@':
                    case '>':
                    case '<':
                    case char.MinValue:
                        return s.ReturnStore();
                    default:
                        s.Store(c).AdvanceChar();
                        break;
                }
            }
        }

        public static string ReadQuotedStringValue(JsonSource s)
        {
            for (; ; )
            {
                char c = s.PeekChar();
                switch (c)
                {
                    case Tokens.SingleQuote:
                        var next = s.AdvanceChar().PeekChar();
                        if (next == Tokens.SingleQuote)
                        {
                            s.Store(next).AdvanceChar();
                            continue;
                        }
                        return s.ReturnStore();
                    case Tokens.DoubleQuote:
                    case char.MinValue:
                        return s.ReturnStore();
                    default:
                        s.Store(c).ReadChar();
                        break;
                }
            }
        }

        public override string ToString()
        {
            return content.ToString();
        }

        public override bool Equals(object obj)
        {
            if (content != null)
            {
                if (obj != null)
                    return obj.Equals(content);
                return false;
            }
            return obj == null;
        }

        public override int GetHashCode()
        {
            return -1896430574 + EqualityComparer<object>.Default.GetHashCode(content);
        }
    }
}
