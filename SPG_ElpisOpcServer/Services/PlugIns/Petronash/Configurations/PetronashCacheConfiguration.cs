using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RMS.Service.Abstractions.Configurations;
using System;

namespace Petronash.Configurations
{
    /// <summary>
    /// Petronash cache configuration to set cache intervals
    /// </summary>
    public class PetronashCacheConfiguration : IConfigureOptions<CacheOptions>, IConfigureOptions<PetronashCacheOptions>
    {
        private readonly IConfiguration configuration;

        public PetronashCacheConfiguration(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        
        public void Configure(PetronashCacheOptions options)
        {
            var cacheOptions = configuration.GetSection(nameof(CacheOptions));
            options.FormulaCacheInterval = TimeSpan.Parse(cacheOptions[nameof(options.FormulaCacheInterval)]);
            options.InputCacheInterval = TimeSpan.Parse(cacheOptions[nameof(options.InputCacheInterval)]);
        }

        public void Configure(CacheOptions options)
        {
            var cacheOptions = configuration.GetSection(nameof(CacheOptions));
            options.FormulaCacheInterval = TimeSpan.Parse(cacheOptions[nameof(options.FormulaCacheInterval)]);

        }
    }
}
