using CngBooster.Models;
using Microsoft.Extensions.Logging;
using RMS.Service.Abstractions;
using RMS.Service.Abstractions.Data;
using RMS.Service.Abstractions.Parser;
using RMS.Service.Abstractions.Providers;
using RMS.Service.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace CngBooster
{
    /// <summary>
    /// Cng Device Signal process <see cref="ProcessDataAsync(DeviceDataModel)"/>
    /// </summary>
    public class CngDeviceProvider : RmsProviderBase, IRmsDeviceProvider
    {
        private static readonly object BitType = 1;
        private readonly SignalBitRepository _signalBits;
        private readonly SignalMemicRepository _signalMemic;
        private readonly ADCCalculationRepository _adcCalc;
        private readonly ILogger<CngDeviceProvider> _logger;
        //private static TimeZoneInfo _indianZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        //private static TimeZoneInfo _arabianZone = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
        private static List<string> hydacDeviceIds = new List<string> { "EV1", "EV2" };
        //private static readonly List<string> istNonRtcDeviceIds = new List<string> { "cng_202", "cng_203", "cng_excon"};   //non-rtc device id list which are present inside Indian Standard Time Zone
        //private static readonly List<string> astNonRtcDeviceIds = new List<string> { "PD_00001", "PD_00002" }; //non-rtc device id list which are present inside Arabian Standard Time Zone
        //private static readonly List<string> hydacAdcSignalIds = new List<string> { "EV1s1", "EV1s2", "EV1s3", "EV1s4" };
        //private static readonly List<string> acilDeviceIds = new List<string> { "A002" };  //add all acil device ids here in this list whose parameter values is to be divided by 10
        //private static readonly List<string> acilSignalIds = new List<string> { "A002s275", "A002s276", "A002s277" };  //signalIds that shouldn't be divided by 10

        public CngDeviceProvider(ILogger<CngDeviceProvider> logger, SignalBitRepository signalBits, SignalMemicRepository signalMemic, ADCCalculationRepository adcCalc)
        {
            _signalBits = signalBits;
            _signalMemic = signalMemic;
            _adcCalc = adcCalc;
            _logger = logger;
        }

        public bool TryGetSignal(JsonContent content, out DataSendModel res, out float multiplyingFactor)
        {
            multiplyingFactor = 1.0f;
            res = new DataSendModel();
            var e = content.Iterator();
            if (e.MoveNext() && (e.Current.Type == JsonValueType.Number || e.Current.Type == JsonValueType.String)
                && e.Next.Equals('>'))
            {
                res.SignalId = /*(int)*/e.Current.Content.ToString();
                if (e.MoveNext(2) && e.Next.Equals('<'))
                {
                    res.DataValue = e.Current.Content;
                    if (e.MoveNext(2) && (e.Current.Type == JsonValueType.Number || e.Current.Type == JsonValueType.String))
                    {
                        if (int.TryParse(e.Current.Content.ToString(), out int dataType))
                        {
                            res.DataType = dataType;
                        }
                        else
                        {
                            string[] valArray = e.Current.Content.ToString().Split(";");
                            res.DataType = int.Parse(valArray[0]);
                            multiplyingFactor = float.Parse(valArray[1]);
                        }
                        //res.DataType = (int)e.Current.Content;
                        return e.Next.Type == JsonValueType.End;
                    }
                }
            }
            return false;
        }

        public override async Task<IParsedItems> ProcessDataAsync(DeviceDataModel deviceData)
        {
            string deviceId = Convert.ToString(deviceData.DeviceId);
            string timeReceived = null;
            //if (istNonRtcDeviceIds.Any(device => device == deviceId))
            //    timeReceived = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _indianZone).ToString("yyyy-MM-dd HH:mm:ss");
            //else if (astNonRtcDeviceIds.Any(device => device == deviceId))
            //    timeReceived = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _arabianZone).ToString("yyyy-MM-dd HH:mm:ss");


            // Take the date parameter for this data
            deviceData.Value.NextOf('>', out var receivedTime);
            var res = new ParsedList();
            foreach (var signal in deviceData.SignalList)
            {
                if (signal.Type == NodeType.Content
                    && TryGetSignal((JsonContent)signal, out var item, out float multiplyingFactor))
                {
                    // Retrieve bit info if the data type is Boolean
                    if (item.DataType.Equals(BitType))
                    {
                        await AddBitAsync(res, /*(int)*/deviceData.DeviceId.ToString(), item, receivedTime, timeReceived);
                        continue;
                    }
                    item.DeviceId = /*(int)*/deviceId;

                    if (item.DeviceId == "A002" && item.DataType == 2 && double.TryParse(item.DataValue.ToString(), out double tempValue))
                    {
                        item.DataValue = Math.Round((tempValue / 10), 1);
                    }
                    else
                    {
                        string parameterDataValue = Convert.ToString(item.DataValue);
                        if (multiplyingFactor != 1.0f)
                        {
                            item.DataValue = float.Parse(parameterDataValue) * multiplyingFactor;
                        }

                        //temporary computation for ADC parameters of HYDAC
                        else if (multiplyingFactor == 1.0f && hydacDeviceIds.Any(deviceId => deviceId == item.DeviceId) && (item.DataType == 3 || item.DataType == 4) && parameterDataValue != "0" && parameterDataValue != "2")
                        {
                            double? computedValue = await CalculateADCValue(item.SignalId, parameterDataValue);
                            if (computedValue.HasValue)
                                item.DataValue = computedValue.Value;
                            //item.DataValue = (0.004883 * (Convert.ToDouble(item.DataValue))) + (-56.25);
                        }
                    }

                    //adding proper timeReceived against analog parameters for non-rtc devices which doesnt have rtc
                    if (timeReceived != null)
                        item.TimeReceived = timeReceived;
                    else
                        item.TimeReceived = receivedTime.ToString();

                    res.Add(item);
                }
            }

            if (hydacDeviceIds.Any(device => device == res[0].DeviceId) && res.Count > 0)
                await SignalMemicConfig(res);

            return res;
        }

        async Task AddBitAsync(IList<DataSendModel> result, /*int*/string deviceId, DataSendModel data, JsonValue dateRecieved, string timeReceived)
        {
            try
            {
                IList<SignalBitInfo> bitInfo = await _signalBits.GetSignalBitsAsync(data.SignalId);
                if (bitInfo == null)
                {
                    return;
                }
                string binary = Convert.ToString(Convert.ToInt32(data.DataValue), 2).PadLeft(bitInfo.Count, '0');
                for (int i = 0; i < bitInfo.Count; i++)
                {
                    SignalBitInfo bit = bitInfo[i];
                    result.Add(new DataSendModel
                    {
                        //TimeReceived = istNonRtcDeviceIds.Any(device => device == deviceId)
                        //? TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _indianZone).ToString("yyyy-MM-dd HH:mm:ss")
                        //: astNonRtcDeviceIds.Any(device => device == deviceId)
                        //? TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _arabianZone).ToString("yyyy-MM-dd HH:mm:ss")
                        //: dateRecieved.ToString(),
                        TimeReceived = timeReceived == null ? dateRecieved.ToString() : timeReceived,
                        DeviceId = deviceId,
                        SignalId = bit.SignalId,
                        DataValue = binary[bit.BitId] - 48
                    });
                }
            }
            catch (DbException ex)
            {
                _logger.LogError("Db Error Cng SignalBit : {0}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Cng SignalBit : {0}", ex.Message);
            }
        }

        async Task SignalMemicConfig(ParsedList res = null)
        {
            try
            {

                string deviceId = res.First().DeviceId;
                IList<SignalMemicInfo> signalMemicInfo = await _signalMemic.GetSignalMemicAsync(deviceId);
                if (signalMemicInfo == null)
                {
                    return;
                }
                foreach (var item in signalMemicInfo)
                {
                    if (item.Type == "input")
                    {
                        item.DataValue = res.FirstOrDefault(signal => signal.SignalId == item.SignalId).DataValue;
                    }
                    else if (item.Type == "output")
                    {
                        item.DataValue = 0;
                    }
                }

                if (!res.Any(signal => signal.SignalId == signalMemicInfo.FirstOrDefault(info => info.Type == "output").SignalId))
                    return;

                var outputSignal = res.First(s => s.SignalId == signalMemicInfo.FirstOrDefault(info => info.Type == "output").SignalId);

                int i = 0; bool? result = null;

                while (i < (signalMemicInfo.Count - 1))
                {
                    if (!result.HasValue)
                    {
                        bool value1 = Convert.ToInt32(signalMemicInfo[i].DataValue) != 0;
                        bool value2 = Convert.ToInt32(signalMemicInfo[i + 1].DataValue) != 0;
                        if (signalMemicInfo[i].Comparator == "&")
                            result = value1 & value2;
                        else if (signalMemicInfo[i].Comparator == "|")
                            result = value1 | value2;
                    }
                    else
                    {
                        bool value2 = Convert.ToInt32(signalMemicInfo[i + 1].DataValue) != 0;
                        if (signalMemicInfo[i].Comparator == "&")
                            result = result.Value & value2;
                        else if (signalMemicInfo[i].Comparator == "|")
                            result = result.Value | value2;
                    }

                    i += 1;
                }

                i = Convert.ToInt32(result);

                outputSignal = new DataSendModel
                {
                    DataType = outputSignal.DataType,
                    DataValue = i,
                    DeviceId = outputSignal.DeviceId,
                    SignalId = outputSignal.SignalId,
                    TimeReceived = outputSignal.TimeReceived
                };

                int index = res.FindIndex(item => item.SignalId == outputSignal.SignalId);
                res[index] = outputSignal;
            }
            catch (DbException ex)
            {
                _logger.LogError("Db Error Cng SignalMemic : {0}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Cng SignalMemic : {0}", ex.Message);
            }
        }

        async Task<double?> CalculateADCValue(string signalId, string value)
        {
            double? result = null;
            try
            {
                IList<SignalADCInputInfo> adcInputInfo = await _adcCalc.GetSignalADCInputAsync(signalId);
                if (adcInputInfo == null)
                    return result;

                var adcInput = adcInputInfo.FirstOrDefault(item => item.SignalId == signalId);

                if (adcInput != null && double.TryParse(value, out double adcValue))
                    result = (adcInput.MValue * adcValue) + adcInput.CValue;

                return result;
            }
            catch (DbException ex)
            {
                _logger.LogError("Db Error Cng SignalMemic : {0}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Cng SignalMemic : {0}", ex.Message);
                return null;
            }
        }
    }
}
