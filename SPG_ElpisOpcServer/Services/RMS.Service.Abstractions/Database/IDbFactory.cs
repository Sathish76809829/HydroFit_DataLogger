using Microsoft.EntityFrameworkCore;
using System;

namespace RMS.Service.Abstractions.Database
{
    /// <summary>
    /// Database factory interface for creating differenct database
    /// </summary>
    public interface IDbFactory
    {
        /// <summary>
        /// SqlServer implementation
        /// </summary>
        void SqlServer<TDbContext>(string connectionString, DbContextOptionsBuilder options) where TDbContext: DbContext;
    }
}
