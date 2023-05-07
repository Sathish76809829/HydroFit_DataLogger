using System.Text;

namespace RMS.Service.Abstractions.Parser
{
    /// <summary>
    /// Parsed Json property property instance
    /// </summary>
    public class JsonProperty : JsonNode
    {
        public readonly string Name;

        public JsonProperty(string name) : base(NodeType.Property)
        {
            Name = name;
        }

        public object Value
        {
            get => content;
            internal set
            {
                if (value is JsonNode)
                {
                    var n = (JsonNode)value;
                    n.next = n;
                    content = n;
                    return;
                }
                content = value;
            }
        }

        public JsonNode Content
        {
            get
            {
                if (content is JsonNode)
                    return (JsonNode)content;
                JsonContent t = new JsonContent()
                {
                    content = content,
                    parent = this
                };
                t.next = t;
                System.Threading.Interlocked.CompareExchange<object>(ref content, t, content);
                return t;
            }
        }

        public bool IsContent
        {
            get => content is JsonContent;
        }

        protected internal override void AppendText(StringBuilder sb)
        {
            sb.Append(Tokens.DoubleQuote)
                .Append(Name)
                .Append("\":");
            if (content is JsonNode)
            {
                var n = (JsonNode)content;
                do
                {
                    n = n.next;
                    if (n is null)
                        break;
                    n.AppendText(sb);
                } while (n.parent == this && n != content);
            }
            else if (content is JsonValue)
            {
                var n = (JsonValue)content;
                do
                {
                    n = n.next;
                    if (n is null)
                        break;
                    n.AppendText(sb);
                } while (!ReferenceEquals(n, content));
            }
        }
    }
}
