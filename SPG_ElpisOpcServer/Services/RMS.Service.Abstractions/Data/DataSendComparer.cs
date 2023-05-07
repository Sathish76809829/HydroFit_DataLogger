using RMS.Service.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RMS.Service.Abstractions
{
    /// <summary>
    /// Comparer used for Dictionary or HashSet 
    /// </summary>
    public readonly struct DataSendComparer : IEqualityComparer<DataSendModel>
    {
        public bool Equals([AllowNull] DataSendModel x, [AllowNull] DataSendModel y)
        {
            return x.SignalId == y.SignalId;
        }
        public int GetHashCode([DisallowNull] DataSendModel obj)
        {
            return obj.SignalId.GetHashCode();
        }
    }
}
