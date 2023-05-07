using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Petronash.Configurations;
using Petronash.Models;
using RMS.Service.Abstractions.Database;
using RMS.Service.Abstractions.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Petronash
{
    /// <summary>
    /// Petronash input from database
    /// </summary>
    internal class PetronashSignalRepository : IDisposable
    {
        private readonly DbSet<DeviceInputs> signalInputs;

        private readonly MemoryCache inputCache;
        
        private readonly IDisposable eventSubscription;

        private readonly PetronashCacheOptions cacheOptions;

        public PetronashSignalRepository(IDbContext dbContext, IEventMonitor monitor, IOptions<PetronashCacheOptions> options)
        {
            inputCache = new MemoryCache(new MemoryCacheOptions());
            signalInputs = dbContext.Set<DeviceInputs>();
            eventSubscription = monitor.Subscribe<PumpStatusChange>(OnStateChange);
            cacheOptions = options.Value;
        }

        Task OnStateChange(PumpStatusChange state)
        {
            inputCache.Remove(state.DeviceId);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            eventSubscription.Dispose();
        }

        public async ValueTask<DeviceInputs[]> GetInputSignals(/*int*/string deviceId)
        {
            if (inputCache.TryGetValue<DeviceInputs[]>(deviceId, out var inputs) == false)
            {
                inputs = await InvalidateInputs(deviceId);
                inputCache.Set(deviceId, inputs, cacheOptions.InputCacheInterval);
            }
            return inputs;
        }

        async Task<DeviceInputs[]> InvalidateInputs(/*int*/string deviceId)
        {
            return await signalInputs
              .Where(item => item.DeviceId == deviceId && item.IsActive)
              .ToArrayAsync();
        }
    }
}
