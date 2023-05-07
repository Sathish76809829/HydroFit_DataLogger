using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace RMS.Service.Abstractions.Parser
{
    /// <summary>
    /// Json content either number or string
    /// </summary>
    public class JsonContent : JsonNode, IEnumerable<JsonValue>
    {
        public JsonContent() : base(NodeType.Content)
        {
        }

        public JsonValue FirstValue
        {
            get
            {
                if (content is JsonValue)
                    return ((JsonValue)content).next;
                return JsonValue.Empty;
            }
        }

        public JsonValue LastValue
        {
            get
            {
                if (content is JsonValue)
                    return (JsonValue)content;
                return JsonValue.Empty;
            }
        }

        public bool NextOf(object value, out JsonValue res)
        {
            var n = (JsonValue)content;
            for (; ; )
            {
                n = n.next;
                if (n is null)
                    break;
                if (ReferenceEquals(n, content))
                {
                    break;
                }
                if (n.Equals(value))
                {
                    res = n.next;
                    return true;
                }
            }
            res = JsonValue.Empty;
            return false;
        }

        public bool NextOf(object value, JsonValue start, out JsonValue res)
        {
            var n = start;
            for (; ; )
            {
                n = n.next;
                if (n is null)
                    break;
                if (ReferenceEquals(n, content))
                {
                    break;
                }
                if (n.Equals(value))
                {
                    res = n.next;
                    return true;
                }
            }
            res = JsonValue.Empty;
            return false;
        }

        public JsonValue SkipTo(int count)
        {
            var n = (JsonValue)content;
            for (int i = 0; i < count; i++)
            {
                n = n.next;
                if (n is null)
                    return JsonValue.Empty;
                if (ReferenceEquals(n, content))
                    return JsonValue.Empty;
            }
            return n;
        }

        public IList<JsonValue> Split(params char[] separator)
        {
            var e = GetEnumerator();
            int index = 0;
            int size = separator.Length;
            var values = new List<JsonValue>();
            if (size == 0)
            {
                values.Add(FirstValue);
                return values;
            }
            e.MoveNext();
            JsonValue res = new JsonValue();
            for (; ; )
            {
                var format = separator[index];
                do
                {
                    if (e.Current.Equals(format))
                    {
                        e.MoveNext();
                        values.Add(res.next);
                        res = new JsonValue();
                        break;
                    }
                    res = res.Append(e.Current);
                } while (e.MoveNext());
                index++;
                if (index == size)
                {
                    do
                    {
                        res = res.Append(e.Current);
                    } while (e.MoveNext());
                    values.Add(res.next);
                    break;
                }
            }
            return values;
        }

        public override JsonNode LastNode => this;

        protected internal override void AppendText(StringBuilder sb)
        {
            if (content is JsonNode)
            {
                ((JsonNode)content).AppendText(sb);
                return;
            }
            if (content is JsonValue)
            {
                var n = (JsonValue)content;
                for (; ; )
                {
                    n = n.next;
                    if (n is null)
                        return;
                    if (ReferenceEquals(n, content))
                    {
                        n.AppendText(sb);
                        return;
                    }
                    n.AppendText(sb);
                }
            }
            sb.Append(content);
        }

        public IValueIterator Iterator()
        {
            if (content is JsonValue)
                return new ValueEnumerable((JsonValue)content);
            return default(ValueEnumerable);
        }

        public IEnumerator<JsonValue> GetEnumerator()
        {
            if (content is JsonValue)
                return new ValueEnumerable((JsonValue)content);
            return System.Linq.Enumerable.Empty<JsonValue>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public struct ValueEnumerable : IEnumerator<JsonValue>, IValueIterator
        {
            public readonly JsonValue start;

            private JsonValue current;

            public ValueEnumerable(JsonValue last)
            {
                start = last.next;
                current = null;
            }

            public JsonValue Current => current;

            object IEnumerator.Current => current;

            public void Dispose()
            {
                current = null;
            }

            public bool MoveNext()
            {
                if (current is null)
                {
                    current = start;
                    return true;
                }
                current = current.next;
                return !ReferenceEquals(current, start);
            }

            public void Reset()
            {
                current = null;
            }

            public JsonValue Next
            {
                get
                {
                    if (ReferenceEquals(current.next, start))
                        return JsonValue.Empty;
                    return current.next;
                }
            }

            public bool MoveNext(int count)
            {
                for (int i = 0; i < count; i++)
                {
                    current = current.next;
                }
                if (ReferenceEquals(current, start))
                    return false;
                return true;
            }
        }

        public interface IValueIterator : IEnumerator<JsonValue>
        {
            JsonValue Next { get; }

            bool MoveNext(int count);
        }
    }
}
