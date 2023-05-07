using RMS.Service.Abstractions.Utils;
using RMS.Service.Models;
using System;
using System.Collections.Generic;

namespace RMS.Service.Abstractions.Extensions
{
    /// <summary>
    /// Script extensions
    /// </summary>
    public static class ScriptExtensions
    {
        /// <summary>
        /// Calculate scripts for RMS
        /// </summary>
        /// <param name="self">Script service instance</param>
        /// <param name="deviceId">device id to calculate</param>
        /// <param name="receivedTime">received time for Signal</param>
        /// <param name="formulas">list of signals</param>
        /// <param name="values">Value of signals</param>
        public static void Calculate(this Services.IScriptService self, /*int*/string deviceId, string receivedTime, IReadOnlyList<Models.SignalFormulas> formulas, ParsedSet values)
        {
            for (int i = 0, count = formulas.Count; i < count; i++)
            {
                try
                {
                    var formula = formulas[i];
                    object res = self.Invoke(formula.Script);
                    if (formula.DataType != 0)
                    {
                        res = ValueConverters.Convert(res, formula.DataType);
                    }
                    values.AddOrUpdate(new DataSendModel
                    {
                        DeviceId = deviceId,
                        DataValue = res,
                        SignalId = formula.SignalId,
                        TimeReceived = receivedTime
                    });
                }
                catch (Exception )
                {

                    throw;
                }
            }
        }
        /// <summary>
        /// Calculate scripts with callback for RMS
        /// </summary>
        /// <param name="self">Script service instance</param>
        /// <param name="formulas">list of signals</param>
        /// <param name="callback">Script calculated callback</param>
        public static void Calculate(this Services.IScriptService self, IReadOnlyList<Models.SignalFormulas> formulas, Action<object> callback)
        {
            for (int i = 0, count = formulas.Count; i < count; i++)
            {
                var formula = formulas[i];
                object res = self.Invoke(formula.Script);
                if (formula.DataType != 0)
                {
                    res = ValueConverters.Convert(res, formula.DataType);
                }
                callback(res);
            }
        }
    }
}
