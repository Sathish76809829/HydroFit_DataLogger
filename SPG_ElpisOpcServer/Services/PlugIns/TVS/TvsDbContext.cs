using Microsoft.EntityFrameworkCore;
using RMS.Service.Abstractions.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace TVS
{
    /// <summary>
    /// EF Core instance for TVS Device
    /// </summary>
    public class TvsDbContext : DbContext, IDbContext
    {
        public TvsDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}
