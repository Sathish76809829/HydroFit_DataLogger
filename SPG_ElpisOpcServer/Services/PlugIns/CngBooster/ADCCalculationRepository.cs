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
    public class ADCCalculationRepository
    {
        private readonly MemoryCache bitCache;

        private readonly TimeSpan expireTime;

        private readonly IDbContext _context;

        public ADCCalculationRepository()
        {

        }
        public ADCCalculationRepository(IDbContext context)
        {
            _context = context;
            bitCache = new MemoryCache(new MemoryCacheOptions
            {
                Clock = new SystemClock()
            });
            expireTime = TimeSpan.FromMinutes(10.0);
        }

        /// <summary>
        /// Get the signal adc input info for particular <paramref name="signalId"/>.
        /// If the signal id is not present in the cache, Retrieve the info from Database
        /// </summary>
        /// <param name="signalId">Signal id to get the adc input info</param>
        /// <returns>SignalADCInputInfo for <paramref name="signalId"/> </returns>
        public async ValueTask<IList<SignalADCInputInfo>> GetSignalADCInputAsync(object signalId)
        {
            if (bitCache.TryGetValue<IList<SignalADCInputInfo>>(signalId, out var result))
            {
                return result;
            }
            var reader = await _context.ExecuteReaderAsync("spGetAdcInputBySignalId", new CommandParameter[] {
               CommandParameter.WithInput("@signalId", DbType.String/*Int32*/, signalId),
               CommandParameter.With("@OutParam", 0),
               CommandParameter.With("@ErrMessage", string.Empty)
            });
            if (reader == null)
                return new SignalADCInputInfo[0];

            if (reader.HasRows && await reader.ReadAsync())
            {
                result = JsonSerializer.Deserialize<SignalADCInputInfo[]>(reader.GetString(0));
                bitCache.Set(signalId, result, expireTime);
            }
            return result;
        }
    }
}
