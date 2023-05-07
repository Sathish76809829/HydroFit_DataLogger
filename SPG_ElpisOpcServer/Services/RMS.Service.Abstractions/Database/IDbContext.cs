using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace RMS.Service.Abstractions.Database
{
    /// <summary>
    /// DbContext interface used in RMS
    /// </summary>
    public interface IDbContext
    {
        /// <summary>
        /// Relation Database instance
        /// </summary>
        DatabaseFacade Database { get; }

        /// <summary>
        /// returns DbSet for Entity <typeparamref name="TQuery"/>
        /// </summary>
        DbSet<TQuery> Set<TQuery>() where TQuery : class;

        /// <summary>
        /// Save the db changes
        /// </summary>
        int SaveChanges();

        /// <summary>
        /// Save the db changes asynchronously
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for save task</param>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
