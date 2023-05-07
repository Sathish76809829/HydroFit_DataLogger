using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RMS.Service.Abstractions.Parser;
using RMS.Service.Abstractions.Providers;
using RMS.Service.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RMS.DataParser.Services
{
    /// <summary>
    /// Data Process service which will consume topic from the customer and based on the device type data will be processed
    /// </summary>
    public partial class DataProcessService : DataProcessServiceBase<IRmsDeviceProvider>, IDisposable
    {
        public DataProcessService(IServiceProvider services, IDictionary<int, IRmsDeviceProvider> providers) : base(services, providers)
        {
        }

        async Task DataProccessing(ConsumeResult<Ignore, JsonSource> data)
        {
            var source = data.Message.Value;
            var node = JsonNode.ParseNode(source);
            if (node is JsonArray array)
            {
                await Process(array.FirstNode as JsonObject);
                return;
            }
            if (node is JsonObject jObj)
            {
                await Process(jObj);
                return;
            }
        
        }

        async Task Process(JsonObject obj)
        {
            // validate the data
            if (obj == null
                || !DeviceDataModel.TryParse(obj, out var deviceData)
                || !Providers.TryGetValue(deviceData.Type, out var provider))
            {
                return;
            }
            var res = await provider.ProcessDataAsync(deviceData);
            if (res.Count == 0)
                return;
            await StoreAndProcess(/*(int)*/deviceData.DeviceId.ToString(), res);
        }

        protected async override Task ProcessData(CancellationToken stoppingToken)
        {
            for (; ; )
            {
                if (stoppingToken.IsCancellationRequested)
                    break;
                var consumeResult = Consumer.Consume(stoppingToken);
                Logger.LogDebug("processing {0} topic message : {1}", consumeResult.Topic, consumeResult.Message.Value);
                await DataProccessing(consumeResult);
            }
        }
    }
}
