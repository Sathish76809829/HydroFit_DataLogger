using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RMS.Service.Abstractions;
using RMS.Service.Abstractions.Extensions;
using RMS.Service.Abstractions.Parser;
using RMS.Service.Abstractions.Providers;
using RMS.Service.Abstractions.Services;
using RMS.Service.Models;
using System;
using System.Threading.Tasks;

namespace Daq
{
    /// <summary>
    /// Daq Device data process <see cref="ProcessDataAsync(DeviceDataModel)"/>
    /// </summary>
    public class DaqDeviceProvider : RmsProviderBase, IRmsDeviceProvider
    {
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        private static TimeZoneInfo _indianZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

        private readonly IScriptService scriptService;

        private readonly IFormulaRepository formulaRepo;

        private readonly ILogger<DaqDeviceProvider> logger;

        public DaqDeviceProvider(IServiceProvider services)
        {
            scriptService = services.GetRequiredService<IScriptService>();
            formulaRepo = services.GetRequiredService<IFormulaRepository>();
            logger = services.GetRequiredService<ILogger<DaqDeviceProvider>>();
        }

        public bool TryGetSignal(JsonContent content, out DataSendModel res)
        {
            res = new DataSendModel();
            var e = content.Iterator();
            if (e.MoveNext() && (e.Current.Type == JsonValueType.Number || e.Current.Type == JsonValueType.String)
                && e.Next.Equals('>'))
            {
                res.SignalId = /*(int)*/e.Current.Content.ToString();
                if (e.MoveNext(2) && e.Next.Equals('@'))
                {
                    res.DataValue = e.Current.Content;
                    if (e.MoveNext(2) && e.Current.Type == JsonValueType.String)
                    {
                        res.TimeReceived = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _indianZone).ToString("yyyy-MM-dd HH:mm:ss");
                        //res.TimeReceived = e.Current.ToString();
                        return e.Next.Type == JsonValueType.End;
                    }
                }
            }
            return false;
        }

        public override async Task<IParsedItems> ProcessDataAsync(DeviceDataModel deviceData)
        {
            /*int*/string deviceId = /*(int)*/deviceData.DeviceId.ToString();
            var res = new ParsedSet(3);
            foreach (var signal in deviceData.SignalList)
            {
                if (signal.Type == NodeType.Content
                    && TryGetSignal((JsonContent)signal, out var item))
                {
                    item.DeviceId = deviceId;
                    res.Add(item);
                }
            }
            var scope = scriptService.Begin(res);
            try
            {
                // fetch formula for calculation
                var formulas = await formulaRepo.GetSignalsFormulas(deviceId);
                if (formulas.Count == 0)
                    return res;
                // datetime format yyyy-mm-dd HH:mm:ss
                scriptService.Calculate(deviceId, DateTime.Now.ToString(DateTimeFormat), formulas, res);
            }
            catch (Exception ex)
            {
                logger.LogError("Calculate error : {0}", ex.Message);
            }
            finally
            {
                scope.Dispose();
            }
            return res;
        }


    }
}
