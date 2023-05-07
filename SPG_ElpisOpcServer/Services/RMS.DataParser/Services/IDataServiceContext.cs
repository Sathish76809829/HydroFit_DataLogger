using RMS.Service.Abstractions.PlugIns;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RMS.DataParser.Services
{
    public interface IDataServiceContext
    {
        ValueTask DisposeAsync();
        IList<PlugInInfo> GetPlugIns();
        void Start();
        void StartProcess(CancellationToken cancellationToken);
        Task<bool> TryAddSync(IPlugIn plugIn);
        ValueTask<bool> TryRemoveAsync(Guid id);
    }
}