using RMS.Service.Abstractions;
using RMS.Service.Abstractions.Data;
using RMS.Service.Abstractions.Models;
using RMS.Service.Abstractions.Parser;
using RMS.Service.Abstractions.Providers;
using RMS.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TVS
{
    public class TvsProvider : RmsProviderBase, IRmsDeviceProvider
    {
        private static TimeZoneInfo _indianZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

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

        public override Task<IParsedItems> ProcessDataAsync(DeviceDataModel deviceData)
        {
            var res = new ParsedList();
            foreach (var signal in deviceData.SignalList)
            {
                if (signal.Type == NodeType.Content
                    && TryGetSignal((JsonContent)signal, out var item))
                {
                    item.DeviceId = /*(int)*/deviceData.DeviceId.ToString();

                    item.TimeReceived = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _indianZone).ToString("yyyy-MM-dd HH:mm:ss");
                    //item.TimeReceived = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    res.Add(item);
                }
            }
            return Task.FromResult<IParsedItems>(res);
        }
    }
}
