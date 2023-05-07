using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RMS.Service.Abstractions.Models;
using RMS.Service.Abstractions.Parser;
using RMS.Service.Abstractions.Providers;
using RMS.Service.Models;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RMS.DataParser.Services.Opc
{
    public class OpcDataProcessService : DataProcessServiceBase<IOpcDeviceProvider>
    {
        public OpcDataProcessService(IServiceProvider services, IDictionary<int, IOpcDeviceProvider> providers) : base(services, providers)
        {
        }

        protected async override Task ProcessData(CancellationToken stoppingToken)
        {
            for (; ; )
            {
                if (stoppingToken.IsCancellationRequested)
                    break;
                var consumeResult = Consumer.Consume(stoppingToken);
                Logger.LogDebug("processing {0} topic message : {1}", consumeResult.Topic, consumeResult.Message.Value);

                await OpcDataProccessing(consumeResult);
            }
        }


        async Task OpcDataProccessing(ConsumeResult<Ignore, JsonSource> data)
        {
            var source = data.Message.Value;
            var node = JsonNode.ParseNode(source);
            if (node is JsonArray array)
            {
                var val = array.FirstNode as JsonObject;
                await ProcessOpc(array.FirstNode as JsonObject);
                return;
            }
            if (node is JsonObject jObj)
            {
                await ProcessOpc(jObj);
                return;
            }

            return;
        }

        async Task ProcessOpc(JsonObject obj)
        {
            if (obj == null)
            {
                return;
            }
            string deviceid = obj.FirstNode.LastNode.ToString();
            string source = obj.LastNode.LastNode.ToString();


            if (!OpcDataModel.TryParse(source, deviceid, out var opcDataModel) || !Providers.TryGetValue(4, out var provider))
            {
                return;
            }

            var res = await provider.ProcessDataAsync(opcDataModel);
            if (res.Count == 0)
                return;
            await StoreAndProcess(/*(int)*/opcDataModel.DeviceId.ToString(), res).ConfigureAwait(false);

        }

    }
}
