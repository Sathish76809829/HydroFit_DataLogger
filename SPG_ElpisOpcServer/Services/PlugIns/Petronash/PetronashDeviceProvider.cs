using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RMS.Service.Abstractions;
using RMS.Service.Abstractions.Extensions;
using RMS.Service.Abstractions.Models;
using RMS.Service.Abstractions.Parser;
using RMS.Service.Abstractions.Providers;
using RMS.Service.Abstractions.Services;
using RMS.Service.Models;
using System;
using System.Threading.Tasks;

namespace Petronash
{
    /// <summary>
    /// Petronash data process <see cref="ProcessDataAsync(DeviceDataModel)"/>
    /// </summary>
    public class PetronashDeviceProvider : RmsProviderBase, IRmsDeviceProvider
    {
        private readonly IScriptService scriptService;

        private readonly PetronashSignalRepository inputRepo;

        private readonly IFormulaRepository formulaRepo;

        private readonly TestDetailsService testDetails;

        private readonly ILogger<PetronashDeviceProvider> logger;

        public PetronashDeviceProvider(IServiceProvider services)
        {
            scriptService = services.GetRequiredService<IScriptService>();
            inputRepo = services.GetRequiredService<PetronashSignalRepository>();
            formulaRepo = services.GetRequiredService<IFormulaRepository>();
            testDetails = services.GetRequiredService<TestDetailsService>();
            logger = services.GetRequiredService<ILogger<PetronashDeviceProvider>>();
        }

        public int Type => 5;

        public bool TryGetSignal(JsonContent content, out DataSendModel res)
        {
            res = new DataSendModel();
            var e = content.Iterator();
            if (e.MoveNext() && e.Current.Type == JsonValueType.Number
                && e.Next.Equals('>'))
            {
                res.SignalId = /*(int)*/e.Current.Content.ToString();
                if (e.MoveNext(2) && e.Next.Equals('<'))
                {
                    res.DataValue = e.Current.Content;
                    if (e.MoveNext(2) && e.Current.Type == JsonValueType.Number)
                    {
                        res.DataType = (int)e.Current.Content;
                        return e.Next.Type == JsonValueType.End;
                    }
                }
            }
            return false;
        }

        public override async Task<IParsedItems> ProcessDataAsync(DeviceDataModel deviceData)
        {
            // Use SignalSet for faster access
            // Initalize signal set with minimum 10
            var res = new ParsedSet(10);
            /*int*/string deviceId = /*(int)*/deviceData.DeviceId.ToString();
            deviceData.Value.NextOf('>', out var receivedTime);
            foreach (var signal in deviceData.SignalList)
            {
                if (signal.Type == NodeType.Content
                    && TryGetSignal((JsonContent)signal, out var item))
                {
                    item.DeviceId = deviceId;
                    item.TimeReceived = receivedTime.ToString();
                    res.Add(item);
                }
            }
            var scope = scriptService.Begin(res);
            try
            {
                // fetch the formulas from database
                var formulas = await formulaRepo.GetSignalsFormulas(deviceId);
                if (formulas.Count == 0)
                    return res;
                // inputs for petronash
                var inputs = await inputRepo.GetInputSignals(deviceId);
                foreach (var input in inputs)
                {
                    scope.Variables.Add(input.Id, input.ConvertValue());
                }
                scriptService.Calculate(deviceId, receivedTime.ToString(), formulas, res);
                await testDetails.UpdateDetails(res);
            }
            catch (Exception ex)
            {
                logger.LogError("Error : {0}", ex.Message);
            }
            finally
            {
                scope.Dispose();
            }
            return res;
        }

     
    }
}
