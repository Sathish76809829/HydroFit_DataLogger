using RMS.Service.Models;
using System.Collections.Generic;

namespace RMS.Service.Abstractions
{
    /// <summary>
    /// HashSet for parsed signal Data
    /// </summary>
    public class ParsedSet : SignalSet<DataSendModel>, IParsedItems
    {
        public static readonly IEqualityComparer<DataSendModel> DefaultComparer = default(DataSendComparer);

        public ParsedSet() : base(DefaultComparer)
        {
        }

        public ParsedSet(int capacity) : base(capacity, DefaultComparer)
        {
        }

        public ParsedSet(int capacity, IEqualityComparer<DataSendModel> comparer) : base(capacity, comparer)
        {
        }
    }
}
