using Microsoft.EntityFrameworkCore;
using RMS.Service.Abstractions.Database;

namespace CngBooster
{
    /// <summary>
    /// EF Core instance for Cng Device
    /// </summary>
    public class CngDbContext : DbContext, IDbContext
    {
        public CngDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}
