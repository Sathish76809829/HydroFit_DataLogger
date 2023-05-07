using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace RMS.Service.Abstractions.Database
{
    /// <summary>
    /// Db Extenstions for RMS
    /// </summary>
    public static class DatabaseExtensions
    {
        /// <summary>
        /// Execute procedure from <see cref="IDbContext"/>
        /// </summary>
        /// <param name="context">IDbContext instance</param>
        /// <param name="procedureName">procedure name</param>
        /// <param name="parameters">procedure parameters</param>
        /// <returns>DataReader for procedure</returns>
        public static async Task<DbDataReader> ExecuteReaderAsync(this IDbContext context, string procedureName, params CommandParameter[] parameters)
        {
            var connection = context.Database.GetDbConnection();
            // Wait for to open the connection
            if (connection.State == ConnectionState.Connecting
                && await WaitForConnection(connection) == false)
            {
                return null;
            }
            // Open DbConnection
            await context.Database.OpenConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = procedureName;
            command.CommandType = CommandType.StoredProcedure;
            foreach (var parameter in parameters)
            {
                var dbParameter = command.CreateParameter();
                dbParameter.ParameterName = parameter.Name;
                dbParameter.Value = parameter.Value;
                command.Parameters.Add(dbParameter);
                if (parameter.Direction == ParameterDirection.Input)
                {
                    dbParameter.DbType = parameter.Type.Value;
                    dbParameter.Direction = ParameterDirection.Input;
                    continue;
                }
            }
            return await command.ExecuteReaderAsync();
        }

        /// <summary>
        /// Wait for the connection state to Open
        /// </summary>
        /// <returns>true if state becomes open, Else return false</returns>
        public static Task<bool> WaitForConnection(this DbConnection connection)
        {
            var tcs = new TaskCompletionSource<bool>();
            void StateChange(object sender, StateChangeEventArgs e)
            {
                if (e.CurrentState == ConnectionState.Open)
                {
                    connection.StateChange -= StateChange;
                    tcs.TrySetResult(true);
                    return;
                }
                if (e.CurrentState == ConnectionState.Closed)
                {
                    connection.StateChange -= StateChange;
                    tcs.TrySetResult(false);
                }
            }
            connection.StateChange += StateChange;
            return tcs.Task;
        }
    }
}
