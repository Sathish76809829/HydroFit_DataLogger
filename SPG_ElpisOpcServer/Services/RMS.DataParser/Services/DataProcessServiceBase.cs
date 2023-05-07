using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RMS.DataParser.Configurations;
using RMS.EventBusKafka;
using RMS.Service.Abstractions.Parser;
using RMS.Service.Models;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RMS.DataParser.Services
{
    public abstract class DataProcessServiceBase<TProvider> : IDataProcessService
    {
        protected readonly ILogger<DataProcessService> Logger;

        public readonly LiveDataRepository LiveData;

        protected readonly IDictionary<int, TProvider> Providers;

        public readonly string ProducerTopic;

        protected readonly IConsumer<Ignore, JsonSource> Consumer;
        protected readonly IProducer</*int*/string, byte[]> Producer;

        public KafkaOptions KafkaOptions { get; }


        public DataProcessServiceBase(IServiceProvider services,
            IDictionary<int, TProvider> providers)
        {
            var kafkaOptions = services.GetService<IOptions<KafkaOptions>>();
            var topicOptions = services.GetService<IOptions<DataParserOptions>>();
            this.Logger = services.GetRequiredService<ILogger<DataProcessService>>();
            Providers = providers;
            LiveData = services.GetService<LiveDataRepository>();
            KafkaOptions = kafkaOptions.Value;
            ProducerTopic = topicOptions.Value.OutgoingTopic;
            Consumer = new ConsumerBuilder<Ignore, JsonSource>(KafkaOptions.Consumer)
                .SetKeyDeserializer(Deserializers.Ignore)
                .SetPartitionsAssignedHandler(OnPartitionAssigned)
                .SetValueDeserializer(new Utils.JsonSerializer()).Build();
            Producer = new ProducerBuilder</*int*/string, byte[]>(KafkaOptions.Producer)
                .SetKeySerializer(Serializers.Utf8)
                .SetValueSerializer(Serializers.ByteArray).Build();
        }

        public void Dispose()
        {
            Consumer.Dispose();
            Producer.Dispose();
        }

        /// <summary>
        /// Create consumer for <paramref name="topic"/>
        /// </summary>
        /// <param name="topic">Topic in which customer is sending</param>
        /// <param name="stoppingToken">CancellationToken for consumer</param>
        /// <returns></returns>
        public async Task DoWork(string topic, CancellationToken stoppingToken)
        {
            try
            {
                Logger.LogInformation($"kafka details are, boostrap: {KafkaOptions.Consumer.BootstrapServers} ,topic: {topic} ");
                Consumer.Subscribe(topic);
                await ProcessData(stoppingToken);
            }
            catch (KafkaException ex)
            {
                Logger.LogError($"kafka error: {ex.Message} ,topic: {topic} ");
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("Operation Cancelled");
            }
            catch (ObjectDisposedException)
            {
                // Handle disposed exception 
                // writing log after application close
            }
            catch (StackOverflowException ex)
            {
                Logger.LogError($"error: {ex.Message}, trace: {ex.StackTrace} ,topic: {topic} ");
            }
        }

        protected abstract Task ProcessData(CancellationToken stoppingToken);

        protected async Task StoreAndProcess(/*int*/string deviceId, Service.Abstractions.IParsedItems res)
        {
            ArrayBufferWriter<byte> buffer = new ArrayBufferWriter<byte>();
            using (Utf8JsonWriter wriiter = new Utf8JsonWriter(buffer))
            {
                wriiter.WriteStartArray();
                foreach (DataSendModel item in res)
                {
                    item.WriteTo(wriiter);
                }
                wriiter.WriteEndArray();
                await wriiter.FlushAsync();
            }
            byte[] json = buffer.WrittenMemory.ToArray();
            await Producer.ProduceAsync(ProducerTopic, new Message</*int*/string, byte[]>
            {
                Key = /*(int)*/deviceId,
                Value = json
            });
            await LiveData.InsertAsync(json).ConfigureAwait(continueOnCapturedContext: false);
        }

        IEnumerable<TopicPartitionOffset> OnPartitionAssigned(IConsumer<Ignore, JsonSource> consumer, List<TopicPartition> topics)
        {
            return topics.ConvertAll(t => new TopicPartitionOffset(t, Offset.End));
        }
    }
}