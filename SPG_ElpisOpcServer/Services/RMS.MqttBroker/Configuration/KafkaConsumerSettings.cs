using System.Collections.Generic;

namespace RMS.Broker.Configuration
{
    /// <summary>
    /// Kafka consumer options 
    /// </summary>
    public class KafkaConsumerSettings
    {
        public string Bootstrap { get; set; }

        public string GroupId { get; set; }

        public IList<string> Topics { get; set; }
    }
}
