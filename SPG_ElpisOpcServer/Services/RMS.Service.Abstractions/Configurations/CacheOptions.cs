using Microsoft.Extensions.Options;
using System;

namespace RMS.Service.Abstractions.Configurations
{
    /// <summary>
    /// ASP.Net Caching Options used in RMS Formula Repository 
    /// <see cref="Services.IFormulaRepository"/>
    /// </summary>
    public class CacheOptions : IOptions<CacheOptions>
    {
        /// <summary>
        /// Number of interval cache retain
        /// </summary>
        public TimeSpan FormulaCacheInterval { get; set; } = TimeSpan.FromHours(20);

        CacheOptions IOptions<CacheOptions>.Value => this;
    }
}
