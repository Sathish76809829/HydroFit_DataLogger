using RMS.Service.Abstractions.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RMS.Service.Abstractions.Services
{
    /// <summary>
    /// Formula repository interface for RMS
    /// </summary>
    public interface IFormulaRepository
    {
        void Clear();
        /// <summary>
        /// Remove particular formula from cache
        /// </summary>
        void Remove(/*int*/string deviceId);
        ValueTask<IReadOnlyList<SignalFormulas>> GetSignalsFormulas(/*int*/string deviceId);
    }
}
