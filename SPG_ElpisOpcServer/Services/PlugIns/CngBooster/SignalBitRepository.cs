using CngBooster.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using RMS.Service.Abstractions.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;

namespace CngBooster
{
    /// <summary>
    /// Signal bit repository for caching and retrieving signal bit info from database
    /// </summary>
    public class SignalBitRepository
    {
        private readonly MemoryCache bitCache;

        private readonly TimeSpan expireTime;

        private readonly IDbContext _context;

        public SignalBitRepository()
        {

        }
        public SignalBitRepository(IDbContext context)
        {
            _context = context;
            bitCache = new MemoryCache(new MemoryCacheOptions
            {
                Clock = new SystemClock()
            });
            expireTime = TimeSpan.FromMinutes(10.0);
        }

        /// <summary>
        /// Get the signal bit info for particular <paramref name="signalId"/>.
        /// If the signal id is not present in the cache, Retrieve the info from Database
        /// </summary>
        /// <param name="signalId">Signal id to get the bit info</param>
        /// <returns>BitInfo for <paramref name="signalId"/> </returns>
        public async ValueTask<IList<SignalBitInfo>> GetSignalBitsAsync(object signalId)
        {
            if (bitCache.TryGetValue<IList<SignalBitInfo>>(signalId, out var result))
            {
                return result;
            }
            var reader = await _context.ExecuteReaderAsync("spGetSignalBitsAsJsonBySignalId", new CommandParameter[] {
               CommandParameter.WithInput("@signalId", DbType.String/*Int32*/, signalId),
               CommandParameter.With("@OutParam", 0),
               CommandParameter.With("@ErrMessage", string.Empty)
            });
            if (reader == null)
                return new SignalBitInfo[0];

            if (reader.HasRows && await reader.ReadAsync())
            {
                result = JsonSerializer.Deserialize<SignalBitInfo[]>(reader.GetString(0));
                bitCache.Set(signalId, result, expireTime);
            }
            return result;
        }
    }
}