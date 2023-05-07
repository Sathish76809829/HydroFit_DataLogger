using Microsoft.EntityFrameworkCore;
using RMS.Service.Abstractions.Database;

namespace RMS.DataParser.Services
{
    /// <summary>
    /// Db Factory implementation which will be used by different providers (plugin)
    /// </summary>
    public class DbFactory : IDbFactory
    {
        public void SqlServer<TDbContext>(string connectionString, DbContextOptionsBuilder options) where TDbContext : DbContext
        {
            options.UseSqlServer(connectionString, (builder) => { });
        }
    }
}
