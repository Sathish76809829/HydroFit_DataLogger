namespace RMS.SocketServer.Net.Messages
{
    /// <summary>
    /// Message response interface
    /// </summary>
    public interface IMessage
    {
        IUserMessage GetMessage();
    }
}
