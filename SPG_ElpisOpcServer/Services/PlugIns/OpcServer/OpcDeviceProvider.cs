using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpcServer.Models;
using RMS.Service.Abstractions;
using RMS.Service.Abstractions.Data;
using RMS.Service.Abstractions.Database;
using RMS.Service.Abstractions.Extensions;
using RMS.Service.Abstractions.Models;
using RMS.Service.Abstractions.Providers;
using RMS.Service.Abstractions.Services;
using RMS.Service.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcServer
{
    internal class OpcDeviceProvider : OpcProviderBase, IOpcDeviceProvider
    {
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        private readonly IScriptService scriptService;

        private readonly ILogger<OpcDeviceProvider> logger;

        private readonly OpcSignalRepository signalRepo;

        public OpcDeviceProvider(IServiceProvider services)
        {
            scriptService = services.GetRequiredService<IScriptService>();
            signalRepo = services.GetRequiredService<OpcSignalRepository>();
            logger = services.GetRequiredService<ILogger<OpcDeviceProvider>>();
        }




        public async Task<DataSendModel> TryGetSignalData(OpcDataModel opcData)
        {
            DataSendModel res = new DataSendModel();
            try
            {

                string[] para = opcData.SignalData.Split(',');
                List<string> splitList = para[0].Split('.').ToList();
                splitList.RemoveAt(0);

                //Re-create the string
                string outputString = string.Join(".", splitList);
                string id = await signalRepo.GetSignalId(outputString, opcData.DeviceId.ToString());
                if (id != null)
                {
                    res.SignalId = id;
                    res.DataValue = para[1];
                    res.TimeReceived = para[3];
                    res.DeviceId = /*(int)*/opcData.DeviceId.ToString();
                }
              
                return res;

            }
            catch/* (Exception ex)*/
            {
                return res;
            }
        }

        public override async Task<IParsedItems> ProcessDataAsync(OpcDataModel opcDataModel)
        {
            var res = new ParsedList();
            await getSignaData(res, opcDataModel);

            return res;
        }


        async Task getSignaData(IList<DataSendModel> result, OpcDataModel opcDataModel)
        {
            try
            {
                var val = await TryGetSignalData(opcDataModel);
                if (val.SignalId == null)
                    return;
                result.Add(val);
            }
            catch (DbException ex)
            {
                logger.LogError("Db Error opc Signalid : {0}", ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError("Error opc signalid : {0}", ex.Message);
            }
        }

    }
}
