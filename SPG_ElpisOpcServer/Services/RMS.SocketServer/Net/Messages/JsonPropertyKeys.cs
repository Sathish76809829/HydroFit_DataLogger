using System;
using System.Text;

namespace RMS.SocketServer.Net.Messages
{
    /// <summary>
    /// Json property names used for <see cref="System.Text.Json.Utf8JsonWriter"/>
    /// </summary>
    public static class JsonPropertyKeys
    {
        private static readonly byte[] _type = Encoding.UTF8.GetBytes("type");

        private static readonly byte[] _message = Encoding.UTF8.GetBytes("message");

        private static readonly byte[] _retain = Encoding.UTF8.GetBytes("retain");

        private static readonly byte[] _quality = Encoding.UTF8.GetBytes("quality");

        private static readonly byte[] _id = Encoding.UTF8.GetBytes("id");

        public static ReadOnlySpan<byte> Type => _type;

        public static ReadOnlySpan<byte> Message => _message;

        public static ReadOnlySpan<byte> Retain => _retain;

        public static ReadOnlySpan<byte> Quality => _quality;

        public static ReadOnlySpan<byte> Id => _id;
    }

}
