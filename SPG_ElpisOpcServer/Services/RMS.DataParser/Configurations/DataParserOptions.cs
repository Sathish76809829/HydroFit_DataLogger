namespace RMS.DataParser.Configurations
{
    /// <summary>
    /// Configuration for RMS Data Parser
    /// </summary>
    public class DataParserOptions
    {
        /// <summary>
        /// Specifies the topics for device types
        /// </summary>
        public string ProviderConfigFile { get; set; }

        /// <summary>
        /// Output topic in which kafka will publish
        /// </summary>
        public string OutgoingTopic { get; set; }
    }
}
