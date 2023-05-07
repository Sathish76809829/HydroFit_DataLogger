namespace RMS.SocketServer.Net.Messages
{
    /// <summary>
    /// At Message bytes to avoid invalid bytes from Device socket
    /// </summary>
    public class AtReplyMessage
    {
        public static readonly int[] AtHeaderReplay = new int[] { 65, 84, 43 };

        public static bool ValidateHeader(byte[] header)
        {
            for (int i = 1; i < 3; i++)
            {
                if (AtHeaderReplay[i] != header[i])
                    return false;
            }
            return true;
        }
    }
}
