using CngBooster.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using RMS.Service.Abstractions.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CngBooster
{
    public class SignalMemicRepository
    {
        private readonly MemoryCache bitCache;

        private readonly TimeSpan expireTime;

        private readonly IDbContext _context;

        public SignalMemicRepository()
        {

        }
        public SignalMemicRepository(IDbContext context)
        {
            _context = context;
            bitCache = new MemoryCache(new MemoryCacheOptions
            {
                Clock = new SystemClock()
            });
            expireTime = TimeSpan.FromMinutes(10.0);
        }

        /// <summary>
        /// Get the signal memic info for particular <paramref name="deviceId"/>.
        /// If the device id is not present in the cache, Retrieve the info from Database
        /// </summary>
        /// <param name="deviceId">Device id to get the memic info</param>
        /// <returns>SignalMemicModel for <paramref name="deviceId"/> </returns>
        public async ValueTask<IList<SignalMemicInfo>> GetSignalMemicAsync(object deviceId)
        {
            if (bitCache.TryGetValue<IList<SignalMemicInfo>>(deviceId, out var result))
            {
                return result;
            }
            var reader = await _context.ExecuteReaderAsync("spGetSignalMemicByDeviceId", new CommandParameter[] {
               CommandParameter.WithInput("@deviceId", DbType.String/*Int32*/, deviceId),
               CommandParameter.With("@OutParam", 0),
               CommandParameter.With("@ErrMessage", string.Empty)
            });
            if (reader == null)
                return new SignalMemicInfo[0];

            if (reader.HasRows && await reader.ReadAsync())
            {
                result = JsonSerializer.Deserialize<SignalMemicInfo[]>(reader.GetString(0));
                bitCache.Set(deviceId, result, expireTime);
            }
            return result;
        }
    }
}
