using System.Text;

namespace RMS.Service.Abstractions.Parser
{
    /// <summary>
    /// Parsed json object instance
    /// </summary>
    public class JsonObject : JsonNode
    {
        public JsonObject() : base(NodeType.Object)
        {
        }

        internal void SetContent(JsonSource s)
        {
            // parent for value
            var c = s.ReadChar();
            if (c == Tokens.OpenBrace)
            {
                ReadJsonObject(s);
            }
        }

        protected internal override void AppendText(StringBuilder sb)
        {
            sb.Append(Tokens.OpenBrace);
            if (content is JsonProperty)
            {
                var n = (JsonNode)content;
                do
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
                } while (n.parent == this);
            }
            sb.Append(Tokens.CloseBrace);
        }

        internal void ReadJsonObject(JsonSource s)
        {
            for (; ; )
            {
                string key = null;
                char c = s.PeekChar();
                switch (c)
                {
                    case '\r':
                    case ' ':
                    case ',':
                        s.AdvanceChar();
                        continue;
                    case Tokens.CloseBrace:
                    case char.MinValue:
                        // skip the } char
                        s.AdvanceChar();
                        return;
                    case Tokens.DoubleQuote:
                        // skip the quote
                        ReadJsonName(s.AdvanceChar(), out key);
                        s.AdvanceChar();
                        break;

                }
                JsonProperty p = new JsonProperty(key);
                c = s.ReadChar();
                if (c == ':')
                {
                    p.Value = ParseNode(s);
                }
                Append(p);
            }
        }

        void ReadJsonName(JsonSource s, out string key)
        {
            do
            {
                char c = s.PeekChar();
                if (!IsValidName(c))
                    break;
                s.Store(c)
                 .AdvanceChar();
            } while (true);
            key = s.ReturnStore();
        }

        static bool IsValidName(char c)
        {
            switch (c)
            {
                case Tokens.DoubleQuote:
                    return false;
                case ':':
                    return false;
                case '\r':
                    return false;
                case char.MinValue:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Parse json object from JsonSource <paramref name="s"/>
        /// </summary>
        public static JsonObject Parse(JsonSource s)
        {
            var res = new JsonObject();
            res.SetContent(s);
            return res;
        }

        /// <summary>
        /// Parse json object from <see cref="string"/> <paramref name="s"/>
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static JsonObject Parse(string s)
        {
            var res = new JsonObject();
            res.SetContent(new JsonSource(s));
            return res;
        }
    }
}
