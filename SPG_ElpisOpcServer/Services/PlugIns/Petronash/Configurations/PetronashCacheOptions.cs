using RMS.Service.Abstractions.Configurations;
using System;

namespace Petronash.Configurations
{
    /// <summary>
    /// Extended class for <see cref="CacheOptions"/> to include Input cache interval
    /// </summary>
    public class PetronashCacheOptions : CacheOptions
    {
        public TimeSpan InputCacheInterval { get; set; }
    }
}
