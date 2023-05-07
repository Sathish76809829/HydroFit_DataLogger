
using Microsoft.EntityFrameworkCore;
using RMS.Service.Abstractions.Database;

namespace OpcServer
{
    class OpcDbContext:DbContext, IDbContext
    {
        public OpcDbContext(DbContextOptions options) : base(options)
        {
        }

        /// <summary>
        /// Model creating
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
