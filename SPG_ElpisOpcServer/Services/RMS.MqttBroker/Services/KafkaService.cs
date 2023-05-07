using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using RMS.Broker.Models;
using RMS.Broker.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RMS.Broker.Services
{
    /// <summary>
    /// A kafka consumer subscribed to DataParser topic (i.e: rmsparsed topic)
    /// </summary>
    public class KafkaService : BackgroundService
    {
        private readonly Mqtt.MqttServerService _mqttServer;

        private readonly ILogger<KafkaService> _logger;

        private readonly Configuration.KafkaConsumerSettings _settings;

        private readonly ClientService _clientService;

        private readonly IConsumer</*int*/string, byte[]> consumer;

        public KafkaService(Mqtt.MqttServerService mqttServer,
            ILogger<KafkaService> logger,
            ClientService clientService,
            Configuration.KafkaConsumerSettings settings)
        {
            _mqttServer = mqttServer;
            _logger = logger;
            _settings = settings;
            _clientService = clientService;
            var consumeConfig = new ConsumerConfig()
            {
                BrokerAddressFamily = BrokerAddressFamily.V4,
                BootstrapServers = settings.Bootstrap,
                GroupId = settings.GroupId,
            };

            consumer = new ConsumerBuilder</*int*/string, byte[]>(consumeConfig)
                .SetKeyDeserializer(Deserializers.Utf8)
                .SetValueDeserializer(Deserializers.ByteArray).Build();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"kafka details are, boostrap: {_settings.Bootstrap} ,topic: {string.Join(",", _settings.Topics)} ");
            Task.Factory.StartNew(() => Consume(stoppingToken), creationOptions: TaskCreationOptions.LongRunning);
            return Task.CompletedTask;
        }

        private async Task Consume(CancellationToken stoppingToken)
        {
            try
            {
                consumer.Subscribe(_settings.Topics);
                for (; ; )
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;
                    var consumeResult = consumer.Consume(stoppingToken);
                    _logger.LogInformation($"{consumeResult.Topic} topic message: {consumeResult.Message.Value}");
                    await Task.Factory.StartNew(Publish, consumeResult, stoppingToken).ConfigureAwait(false);
                }
            }
            catch (KafkaException ex)
            {
                _logger.LogError(ex, "Kafka error " + ex.Message);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Kafka operations cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error " + ex.Message);
            }
        }

        async void Publish(object state)
        {
            var data = (ConsumeResult<string, byte[]>)state;
            Message<string, byte[]> dataMessage = data.Message;
            var deviceId = dataMessage.Key;
            var deviceUsers = _mqttServer.ForDeviceKey(dataMessage.Key);
            if (deviceUsers == null)
            {
                return;
            }

            UserSignalModel[] signals = JsonSerializer.Deserialize<UserSignalModel[]>(dataMessage.Value);

            ////fetching inputs m and c for adc computation for hydac parameters (only input current type parameters as of this release)
            //var adcInputs = await _clientService.GetAdcInputsAsync(deviceId);

            Dictionary<Mqtt.MqttTopicSubscription, List<UserSignalModel>> publishMessages = new Dictionary<Mqtt.MqttTopicSubscription, List<UserSignalModel>>();
            foreach (var item in signals)
            {
                if (deviceUsers.TryGetValue(item.SignalId, out var users))
                {
                    foreach (var user in users)
                    {
                        if (publishMessages.TryGetValue(user, out var messages) == false)
                        {
                            messages = new List<UserSignalModel>();
                            publishMessages[user] = messages;
                        }

                        ////Doing the actual adc computation(formula is y= mx+ c)
                        //DoAdcComputation(adcInputs, item);

                        var abc = RoundOff(item.DataValue.GetRawText(), 1);
                        item.DataValue = System.Text.Json.JsonDocument.Parse(abc).RootElement;

                        //_logger.LogInformation($"value is: {item.DataValue}");
                        messages.Add(item);
                    }
                }
            }
            List<MqttApplicationMessage> applicationMessages = new List<MqttApplicationMessage>(publishMessages.Count);
            var buffer = new System.Buffers.ArrayBufferWriter<byte>();

            using (var writer = new Utf8JsonWriter(buffer))
            {
                foreach (var message in publishMessages)
                {
                    if (message.Value == null)
                        continue;
                    if (message.Value.Count == signals.Length)
                    {
                        // if all messages are being published. no serialize required
                        applicationMessages.Add(new MqttApplicationMessage { Topic = message.Key.Value, Payload = dataMessage.Value });
                    }
                    else
                    {
                        writer.Reset();
                        buffer.Clear();
                        writer.WriteStartArray();
                        WriteMessage(writer, message.Value);
                        writer.WriteEndArray();
                        await writer.FlushAsync();
                        applicationMessages.Add(new MqttApplicationMessage { Topic = message.Key.Value, Payload = buffer.WrittenMemory.ToArray() });
                    }
                }
            }
            await _mqttServer.PublishAsync(applicationMessages);
        }

        static void WriteMessage(Utf8JsonWriter writter, List<UserSignalModel> messages)
        {
            foreach (var item in messages)
            {
                writter.WriteStartObject();
                //writter.WriteNumber(JsonPropertyKeys.DeviceId, item.DeviceId);
                //writter.WriteNumber(JsonPropertyKeys.SignalId, item.SignalId);
                writter.WriteString(JsonPropertyKeys.DeviceId, item.DeviceId);
                writter.WriteString(JsonPropertyKeys.SignalId, item.SignalId);
                writter.WritePropertyName(JsonPropertyKeys.DataValue);
                item.DataValue.WriteTo(writter);
                writter.WriteString(JsonPropertyKeys.TimeReceived, item.TimeRecieved);
                writter.WriteEndObject();
            }
        }

        /// <summary>
        /// converting value of each user dashboard parameter nearest to 1 decimal place
        /// </summary>
        /// <param name="input">parameter value</param>
        /// <param name="nearestTo">number up to which the value should be rounded off. For this case its 1</param>
        /// <returns>parameter value nearest to 1 decimal place</returns>
        private string RoundOff(string input, int nearestTo)
        {
            //return Math.Round(input, nearestTo).ToString();
            try
            {
                //checking if the value is number or word type
                if (double.TryParse(input, out double result))
                {
                    return Math.Round(result, nearestTo).ToString();
                }
                return input;
            }
            catch
            {
                return input;
            }
        }

        ///// <summary>
        ///// Doing adc computation, formula being y = mx + c
        ///// </summary>
        ///// <param name="adcInputs">list containing m and c values against each signalId</param>
        ///// <param name="signal">UserSignalModel object</param>
        //private void DoAdcComputation(List<ADCInputModel> adcInputs, UserSignalModel signal = null)
        //{
        //    if (double.TryParse(signal.DataValue.GetRawText(), out double rawValue))
        //    {
        //        if (adcInputs.Count > 0)
        //        {
        //            var adcInput = adcInputs.FirstOrDefault(item => item.SignalId == signal.SignalId);
        //            if (adcInput != null)
        //                rawValue = (adcInput.MValue * rawValue) + adcInput.CValue;
        //        }
        //        var value = RoundOff(rawValue, 1);
        //        signal.DataValue = System.Text.Json.JsonDocument.Parse(value).RootElement;
        //    }
        //}
    }
}
