using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using RMS.Service.Abstractions.Database;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using Microsoft.Extensions.Internal;
using System.Threading.Tasks;
using OpcServer.Models;

namespace OpcServer
{
    class OpcSignalRepository
    {
        private readonly MemoryCache signalIdCache;
        private readonly TimeSpan expireTime;

        private readonly IDbContext _context;

        public OpcSignalRepository()
        {
        }
        public OpcSignalRepository(IDbContext context)
        {
            _context = context;
            signalIdCache = new MemoryCache(new MemoryCacheOptions
            {
                Clock = new SystemClock()
            });
            expireTime = TimeSpan.FromMinutes(10.0);
        }



        
        public async /*int*/Task<string> GetSignalId(string signalName, string deviceId)
        {
            try
            {
                if (signalIdCache.TryGetValue<IList<SignalInfo>>(signalName, out var result))
                {
                    return result[0].signalId;
                }

                string id = null;
                var connection = _context.Database.GetDbConnection();
                if (connection.State == ConnectionState.Connecting)
                    connection.WaitForConnection().Wait();
                if (connection.State == ConnectionState.Closed
                    || connection.State == ConnectionState.Broken)
                    _context.Database.OpenConnection();

                using SqlCommand command = (SqlCommand)connection.CreateCommand();
                command.CommandText = "spGetSignalIdBySignalName";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@deviceId", SqlDbType.NVarChar)
                {
                    Value = deviceId
                });
                command.Parameters.Add(new SqlParameter("@signalName", SqlDbType.NVarChar)
                {
                    Value = signalName
                });
                command.Parameters.Add(new SqlParameter("@OutParam", (object)0)
                {
                    Direction = ParameterDirection.Output
                });
                command.Parameters.Add(new SqlParameter("@ErrMessage", string.Empty)
                {
                    Direction = ParameterDirection.Output
                });
                var val = await command.ExecuteScalarAsync();
                if (val != null)
                {
                    id = Convert.ToString(val);
                    result = new List<SignalInfo>();
                    result.Add(new SignalInfo()
                    {
                        deviceId=deviceId,
                        signalId=id,
                        signalName=signalName
                    });
                    signalIdCache.Set(signalName, result, expireTime);
                }

                return id;
            }
            catch/* (Exception ex)*/
            {
                //_logger.LogError(ex.Message);
                return null;
            }
        }

        #region old commented bec performance issue
        //public async ValueTask<IList<SignalInfo>> GetSignalIdAsync(object signalName, object deviceId)
        //{
        //    if (bitCache.TryGetValue<IList<SignalInfo>>(signalName, out var result))
        //    {
        //        return result;
        //    }
        //    var reader = await _context.ExecuteReaderAsync("spGetSignalIdBySignalName", new CommandParameter[] {
        //       CommandParameter.WithInput("@deviceId", DbType.Int32, deviceId),
        //       CommandParameter.WithInput("@signalName", DbType.String, signalName),
        //       CommandParameter.With("@OutParam", 0),
        //       CommandParameter.With("@ErrMessage", string.Empty)
        //    });
        //    if (reader == null)
        //        return new SignalInfo[0];

        //    if (reader.HasRows && await reader.ReadAsync())
        //    {
        //        var val = reader.GetString(0);
        //        result = JsonSerializer.Deserialize<SignalInfo[]>(val);
        //        bitCache.Set(signalName, result, expireTime); ;
        //    }
        //    return result;
        //}
        #endregion
    }
}


