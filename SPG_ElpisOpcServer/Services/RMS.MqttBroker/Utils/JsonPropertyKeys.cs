using System;
using System.Text;

namespace RMS.Broker.Utils
{
    /// <summary>
    /// Json property keys from Kafka Data <see cref="Services.KafkaService.Consume(System.Threading.CancellationToken)"/>
    /// </summary>
    public static class JsonPropertyKeys
    {
        private static readonly byte[] _deviceId = Encoding.UTF8.GetBytes("deviceId");

        private static readonly byte[] _signalId = Encoding.UTF8.GetBytes("signalId");

        private static readonly byte[] _dataValue = Encoding.UTF8.GetBytes("dataValue");

        private static readonly byte[] _timeReceived = Encoding.UTF8.GetBytes("timeReceived");

        public static ReadOnlySpan<byte> DeviceId => _deviceId;

        public static ReadOnlySpan<byte> SignalId => _signalId;

        public static ReadOnlySpan<byte> DataValue => _dataValue;

        public static ReadOnlySpan<byte> TimeReceived => _timeReceived;
    }
}
