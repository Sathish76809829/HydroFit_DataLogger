using Microsoft.VisualStudio.TestTools.UnitTesting;
using RMS.Service.Abstractions.Parser;
using System;
using System.Diagnostics;
using System.Text;

namespace RMS.Service.Tests
{
    [TestClass]
    public class ParsingTest
    {
        [TestMethod]
        public void CustomPerformance()
        {
            var expectedIds = new int[] { 51, 55, 56, 67,57 };
            var values = new object[] { 0, 25, 0.002540, 0.02450,0.01 };
            var expectedTypes = new int[] { 1, 2, 3, 3,3 };
            StringBuilder builder = new StringBuilder();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var deviceId = "cng_001";
            builder.Length = 0;
            builder.Append(@"{""device"":""").Append(deviceId).Append(@">2020-10-30 06:06:42<3"",""SignalDataList"":[""51>0<1""}");
            var obj =  JsonObject.Parse(builder.ToString());
            if (obj.FirstNode is JsonProperty first && first.Name == "device"
                && first.Content is JsonContent device
                && obj.LastNode is JsonProperty last
                && last.Content is JsonArray signalList)
            {
                Assert.AreEqual(deviceId, device.FirstValue.Content);
                Assert.AreEqual(3, device.LastValue.Content);
                int index = 0;
                foreach (JsonContent signal in signalList)
                {
                    Assert.AreEqual(expectedIds[index], signal.FirstValue.Content);
                    Assert.AreEqual(values[index], signal.SkipTo(3).Content);
                    Assert.AreEqual(expectedTypes[index], signal.LastValue.Content);
                    index++;
                }
            }
            stopwatch.Stop();
            Debug.WriteLine(stopwatch.Elapsed);
            Assert.IsTrue(stopwatch.Elapsed < TimeSpan.FromMilliseconds(10));
        }

        [TestMethod]
        public void JsonPerformance()
        {
            var expectedIds = new string[] { "51", "55", "56", "67", "57" };
            var values = new string[] { "0", "025", "0.002540", "0.02450", "0.01" };
            var expectedTypes = new string[] { "1", "2", "3", "3", "3" };
            StringBuilder builder = new StringBuilder();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var deviceId = "cng_001";
            builder.Length = 0;
            builder.Append(@"{""device"":""").Append(deviceId).Append(@">2020-10-30 06:06:42<3"",""SignalDataList"":[""51>0<1"",""55>025<2"",""56>0.002540<3"",""67>0.02450<3"",""57>0.01<3""]}");
            var value = System.Text.Json.JsonSerializer.Deserialize<Models.DataRecivedModel>(builder.ToString());
            string[] deviceValues = value.device.Split('>');
            Assert.AreEqual(deviceId, deviceValues[0]);
            Assert.AreEqual("3", deviceValues[1].Split('<')[1]);
            for (int index = 0; index < value.SignalDataList.Count; index++)
            {
                string signal = value.SignalDataList[index];
                string[] signalValues = signal.Split('>');
                Assert.AreEqual(expectedIds[index], signalValues[0]);
                Assert.AreEqual(values[index], signalValues[1].Split('<')[0]);
                Assert.AreEqual(expectedTypes[index], signalValues[1].Split('<')[1]);
            }
            stopwatch.Stop();
            Debug.WriteLine(stopwatch.Elapsed);
            Assert.IsTrue(stopwatch.Elapsed < TimeSpan.FromMilliseconds(10));
        }
    }
}
