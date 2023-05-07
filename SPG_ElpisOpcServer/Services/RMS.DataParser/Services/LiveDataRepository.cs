using System;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RMS.Service.Abstractions.Database;

namespace RMS.DataParser.Services
{
    /// <summary>
    /// Stores the parsed data into Database
    /// </summary>
    public class LiveDataRepository : IDisposable
    {
        private readonly IDbContext _context;

        private readonly ILogger<LiveDataRepository> _logger;

        private readonly CancellationTokenSource cts;

        public LiveDataRepository(IDbContext context, ILogger<LiveDataRepository> logger)
        {
            _context = context;
            _logger = logger;
            cts = new CancellationTokenSource();
        }

        public Task InsertAsync(byte[] raw)
        {
            return Task.Factory.StartNew(new Action<object>(InsertLiveData), raw, cts.Token, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
        }

        private void InsertLiveData(object state)
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                if (connection.State == ConnectionState.Connecting)
                    connection.WaitForConnection().Wait();
                if (connection.State == ConnectionState.Closed
                    || connection.State == ConnectionState.Broken)
                    _context.Database.OpenConnection();
                using SqlCommand command = (SqlCommand)connection.CreateCommand();
                command.CommandText = "spLiveInsertData";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@json", SqlDbType.NVarChar)
                {
                    Value = Encoding.UTF8.GetString((byte[])state)
                });
                command.Parameters.Add(new SqlParameter("@OutParam", (object)0)
                {
                    Direction = ParameterDirection.Output
                });
                command.Parameters.Add(new SqlParameter("@ErrMessage", string.Empty)
                {
                    Direction = ParameterDirection.Output
                });
                int noOfRowsAffected = command.ExecuteNonQuery();
                _logger.LogInformation($"no of rows added is = {noOfRowsAffected}");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public void Dispose()
        {
            cts.Cancel();
            cts.Dispose();
        }
    }
}
