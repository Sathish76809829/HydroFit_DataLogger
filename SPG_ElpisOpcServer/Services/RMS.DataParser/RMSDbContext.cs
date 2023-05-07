using Microsoft.EntityFrameworkCore;
using RMS.Service.Abstractions.Database;
using System.Collections.Generic;

namespace RMS.DataParser
{
    /// <summary>
    /// RMS EF Context for RMS
    /// </summary>
    public class RMSDbContext : DbContext, IDbContext
    {
        private readonly IEnumerable<Entity> entities;

        public RMSDbContext(DbContextOptions options, IEnumerable<Entity> entities) : base(options)
        {
            this.entities = entities;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            foreach (var entity in entities)
            {
                modelBuilder.Entity(entity.Type, entity.BuildAction);
            }

        }
    }
}
