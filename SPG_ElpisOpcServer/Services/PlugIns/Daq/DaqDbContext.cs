using Microsoft.EntityFrameworkCore;
using RMS.Service.Abstractions.Database;
using RMS.Service.Abstractions.Models;

namespace Daq
{
    /// <summary>
    /// EF Core instance for Daq Device 
    /// </summary>
    public class DaqDbContext : DbContext, IDbContext
    {
        public DaqDbContext(DbContextOptions options) : base(options)
        {
        }

        /// <summary>
        /// Register Signal Formula Model for Daq
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SignalFormulas>();
        }
    }
}
