using System.Text;

namespace RMS.Service.Abstractions.Parser
{
    /// <summary>
    /// Base class for <see cref="JsonObject"/>, <see cref="JsonProperty"/>, <see cref="JsonArray"/>, <see cref="JsonContent"/>
    /// </summary>
    public class JsonNode
    {
        public readonly NodeType Type;

        protected JsonNode(NodeType type)
        {
            Type = type;
        }
        internal object content;

        internal JsonNode parent;

        internal JsonNode next;

        /// <summary>
        /// Get the first child node of this node.
        /// </summary>
        public JsonNode FirstNode
        {
            get
            {
                return LastNode?.next;
            }
        }

        /// <summary>
        /// Gets the parent node for self
        /// </summary>
        public JsonNode Parent => parent;

        /// <summary>
        /// Gets the previous sibling node of this node.
        /// </summary>
        /// <remarks>
        /// If this property does not have a parent, or if there is no previous node,
        /// then this property returns null.
        /// </remarks>
        public JsonNode PreviousNode
        {
            get
            {
                if (parent == null)
                    return null;
                JsonNode p = null;
                if (parent.content is JsonNode c)
                {
                    var n = c.next;
                    while (n != this)
                    {
                        p = n;
                        n = n.next;
                    }
                }
                return p;
            }
        }

        public JsonNode NextNode
        {
            get
            {
                return parent == null || this == parent.content ? null : next;
            }
        }

        /// <summary>
        /// Get the last child node of this node.
        /// </summary>
        public virtual JsonNode LastNode
        {
            get
            {
                if (content is null)
                    return null;
                if (content is JsonNode)
                    return (JsonNode)content;
                if (content is JsonValue)
                {
                    JsonContent t = new JsonContent()
                    {
                        content = content,
                        parent = this
                    };
                    t.next = t;
                    System.Threading.Interlocked.CompareExchange<object>(ref content, t, content);
                }
                return (JsonNode)content;
            }
        }

        public void Append(object value)
        {
            if (value is JsonNode n)
            {
                n.parent = this;
                if (content == null)
                {
                    n.next = n;
                }
                else
                {
                    var x = (JsonNode)content;
                    n.next = x.next;
                    x.next = n;
                }
                content = n;
            }
            if (content == null)
            {
                content = value;
                return;
            }
            n = new JsonContent()
            {
                parent = this,
                content = value
            };
            if (content is JsonValue)
            {
                var x = new JsonContent
                {
                    parent = this,
                    content = content
                };
                n.next = x;
                x.next = n;
                content = n;
                return;
            }
            if (content is JsonContent)
            {
                var x = (JsonNode)content;
                n.next = x.next;
                x.next = n;
                content = n;
                return;
            }
        }

        public string Text
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                AppendText(sb);
                return sb.ToString();
            }
        }

        protected internal virtual void AppendText(StringBuilder sb)
        {
        }

        public static object ParseNode(JsonSource s)
        {
            var c = s.ReadChar();
            switch (c)
            {
                case Tokens.OpenBrace:
                    {
                        var res = new JsonObject();
                        res.ReadJsonObject(s);
                        return res;
                    }

                case Tokens.OpenBracket:
                    {
                        var res = new JsonArray();
                        res.ReadJsonArray(s);
                        return res;
                    }

                case Tokens.DoubleQuote:
                    return JsonValue.Parse(s);
                case char.MinValue:
                    return JsonValue.Empty;
                default:
                    // char may be numeric
                    return JsonValue.ParseNumeric(s.Store(c));
            }
        }

        public static object ParseNode(string text) => ParseNode(new JsonSource(text));

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            AppendText(sb);
            return sb.ToString();
        }
    }
}
