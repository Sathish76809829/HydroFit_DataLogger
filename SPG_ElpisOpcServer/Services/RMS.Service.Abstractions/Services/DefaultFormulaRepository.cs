using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;
using RMS.Service.Abstractions.Database;
using RMS.Service.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RMS.Service.Abstractions.Services
{
    /// <summary>
    /// Default Formula Repository implementation
    /// </summary>
    public class DefaultFormulaRepository : IFormulaRepository
    {
        private readonly DbSet<SignalFormulas> outputSignals;

        private MemoryCache signalsCache;

        private readonly MemoryCacheOptions memoryCacheOptions;

        private readonly Configurations.CacheOptions cacheOptions;

        public DefaultFormulaRepository(IDbContext dbContext, IOptions<Configurations.CacheOptions> cacheOptions)
        {
            outputSignals = dbContext.Set<SignalFormulas>();
            memoryCacheOptions = new MemoryCacheOptions
            {
                Clock = new SystemClock()
            };
            signalsCache = new MemoryCache(memoryCacheOptions);
            this.cacheOptions = cacheOptions.Value;
        }

        public void Clear()
        {
            signalsCache.Dispose();
            signalsCache = new MemoryCache(memoryCacheOptions);
        }

        public async ValueTask<IReadOnlyList<SignalFormulas>> GetSignalsFormulas(/*int*/string deviceId)
        {
            if (signalsCache.TryGetValue<List<SignalFormulas>>(deviceId, out var res))
            {
                return res;
            }
            res = await outputSignals
                .Where(item => Convert.ToString(item.DeviceId) == deviceId)
                .ToListAsync();
            signalsCache.Set(deviceId, res, cacheOptions.FormulaCacheInterval);
            return res;
        }

        public void Remove(/*int*/string deviceId)
        {
            signalsCache.Remove(deviceId);
        }
    }
}
