using Microsoft.EntityFrameworkCore;
using Petronash.Models;
using RMS.Service.Abstractions.Database;
using RMS.Service.Abstractions.Models;
using System.Diagnostics.CodeAnalysis;

namespace Petronash
{
    /// <summary>
    /// EF Core RMS DbContext
    /// </summary>
    public class PetronashDbContext : DbContext, IDbContext
    {
        public PetronashDbContext([NotNull] DbContextOptions options) : base(options)
        {
        }

        /// <summary>
        /// Register entity model for petronash
        /// </summary>
        /// <param name="modelBuilder">EF Core Model Builder</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SignalFormulas>();
            modelBuilder.Entity<DeviceInputs>();
            modelBuilder.Entity<SignalFormulas>();
            modelBuilder.Entity<TestDetails>();
        }
    }
}
