namespace RMS.SocketServer.Net.Messages
{
    /// <summary>
    /// Message command properties for sending message across clients
    /// </summary>
    public interface IMessageResponse 
    {
        /// <summary>
        /// Type of message that server is sending
        /// </summary>
        MessageType ResponseType { get; }

        /// <summary>
        /// Client socket Id
        /// </summary>
        string ClientId { get; }
        MessageType RequestType { get; }
        IUserMessage Message { get; }

        /// <summary>
        /// Make message as delivered
        /// </summary>
        /// <param name="status"></param>
        void SetResult(bool status);
    }
}
