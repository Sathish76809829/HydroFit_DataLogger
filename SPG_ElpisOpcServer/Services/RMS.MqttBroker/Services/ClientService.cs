using Microsoft.Extensions.Configuration;
using RMS.Broker.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace RMS.Broker.Services
{
    /// <summary>
    /// RMS Client Authentication Service used for <see cref="Mqtt.MqttServerService.ValidateConnectionAsync(MQTTnet.Server.MqttConnectionValidatorContext)"/>
    /// </summary>
    public class ClientService : IDisposable
    {
        private readonly HttpClient client;

        private readonly Configuration.RMSConfiguration rmsConfig;

        public ClientService(IConfiguration configuration)
        {
            var rmsConfig = new Configuration.RMSConfiguration();
            configuration.Bind("RMS", rmsConfig);
            client = new HttpClient()
            {
                BaseAddress = rmsConfig.BaseUrl
            };
            this.rmsConfig = rmsConfig;
        }

        public async Task<Models.User> GetPreferenceAsync(long userAccountId)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, string.Format(rmsConfig.PreferenceEndPoint, userAccountId)))
            {
                var res = await client.SendAsync(request);
                if (res.IsSuccessStatusCode)
                {
                    return await JsonSerializer.DeserializeAsync<Models.User>(await res.Content.ReadAsStreamAsync());
                }
                return null;
            }
        }

        public async Task<Models.UserDetails> AuthAsync(string email, string password)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, rmsConfig.AuthEndPoint))
            {
                using (System.IO.StringWriter sb = new System.IO.StringWriter())
                {
                    sb.Write("{\"email\": \"");
                    sb.Write(email);
                    sb.Write("\",\"password\":\"");
                    sb.Write(password);
                    sb.Write("\"}");
                    request.Content = new StringContent(sb.ToString(), sb.Encoding, "application/json");
                }
                var res = await client.SendAsync(request);
                if (res.IsSuccessStatusCode)
                {
                    return await JsonSerializer.DeserializeAsync<Models.UserDetails>(await res.Content.ReadAsStreamAsync());
                }
                return null;
            }
        }

        ///// <summary>
        ///// call the remos config api service url that fetches the adc inputs for all the signals of given deviceId
        ///// </summary>
        ///// <param name="deviceId">deviceId</param>
        ///// <returns>object</returns>
        //public async Task<List<ADCInputModel>> GetAdcInputsAsync(string deviceId)
        //{
        //    using (var request = new HttpRequestMessage(HttpMethod.Get, string.Format(rmsConfig.ADCComputationEndPoint, deviceId)))
        //    {
        //        var res = await client.SendAsync(request);
        //        if (res.IsSuccessStatusCode)
        //        {
        //            return await JsonSerializer.DeserializeAsync<List<ADCInputModel>>(await res.Content.ReadAsStreamAsync());
        //        }
        //        return null;
        //    }
        //}

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
