using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace RMS.Service.Abstractions.Parser
{
    /// <summary>
    /// Json parsed array instance
    /// </summary>
    public class JsonArray : JsonNode, IEnumerable<JsonNode>
    {
        public JsonArray() : base(NodeType.Array)
        {
        }

        public static JsonArray Parse(JsonSource s)
        {
            var json = new JsonArray();
            json.SetContent(s);
            return json;
        }

        public static JsonArray Parse(string s)
        {
            var res = new JsonArray();
            res.SetContent(new JsonSource(s));
            return res;
        }

        internal void SetContent(JsonSource s)
        {
            char c = s.ReadChar();
            if (c == Tokens.OpenBracket)
            {
                ReadJsonArray(s);
            }
        }

        internal void ReadJsonArray(JsonSource s)
        {
            while (s.CanAdvance)
            {
                var n = s.PeekChar();
                if (n == ',')
                {
                    s.AdvanceChar();
                    continue;
                }

                if (n == Tokens.CloseBracket)
                {
                    // skip the ] char
                    s.AdvanceChar();
                    return;
                }
                // Avoid Empty array ex: []
                Append(ParseNode(s));

            }
        }

        protected internal override void AppendText(StringBuilder sb)
        {
            sb.Append(Tokens.OpenBracket);
            if (content is JsonNode)
            {
                JsonNode n = (JsonNode)content;
                for (; ; )
                {
                    n = n.next;
                    if (n == null)
                        break;
                    if (n == content)
                    {
                        n.AppendText(sb);
                        break;
                    }
                    n.AppendText(sb);
                    sb.Append(',');
                    continue;
                }
            }
            else if (content is JsonValue)
            {
                var n = (JsonValue)content;
                do
                {
                    n = n.next;
                    if (n == null)
                        break;
                    n.AppendText(sb);
                } while (n != content);
            }
            else
            {
                sb.Append(content);
            }

            sb.Append(Tokens.CloseBracket);
        }

        public IEnumerator<JsonNode> GetEnumerator()
        {
            if (content is JsonNode)
                return new JsonArrayEnumerable((JsonNode)content);
			if(content is JsonValue)
            {
                JsonContent t = new JsonContent()
                {
                    content = content,
                    parent = this
                };
                t.next = t;
                System.Threading.Interlocked.CompareExchange<object>(ref content, t, content);
                return new JsonArrayEnumerable((JsonNode)content);
            }
            return System.Linq.Enumerable.Empty<JsonNode>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public struct JsonArrayEnumerable : IEnumerator<JsonNode>
        {
            public readonly JsonNode start;

            private JsonNode current;

            public JsonArrayEnumerable(JsonNode last)
            {
                start = last.next;
                current = null;
            }

            public JsonNode Current => current;

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
        }
    }
}
