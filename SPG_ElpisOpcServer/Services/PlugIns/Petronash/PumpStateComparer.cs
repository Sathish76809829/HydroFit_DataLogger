using RMS.Service.Abstractions;
using RMS.Service.Abstractions.Models;
using System.Diagnostics.CodeAnalysis;

namespace Petronash
{
    /// <summary>
    /// Pump state comparer for <see cref="SignalSet{T}.TryGetValue{U}(U, ISignalComparer{U}, out T)"/>
    /// </summary>
    public readonly struct PumpStateComparer : ISignalComparer</*int*/string>
    {
        public bool Equals(ISignalModel x, /*int*/string y)
        {
            return x.SignalId.Equals(y);
        }

        public int GetHashCode([DisallowNull] /*int*/string signalId)
        {
            return signalId.GetHashCode();
        }
    }
}
