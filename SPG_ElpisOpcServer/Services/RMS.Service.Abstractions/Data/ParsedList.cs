using RMS.Service.Models;
using System.Collections.Generic;

namespace RMS.Service.Abstractions.Data
{
    /// <summary>
    /// Parsed result list implementation which contains list of Parsed items from Device
    /// </summary>
    public class ParsedList : List<DataSendModel>, IParsedItems
    {
        public ParsedList()
        {
        }

        /// <inheritdoc/>
        public ParsedList(int capacity) : base(capacity)
        {
        }

        /// <inheritdoc/>
        public ParsedList(IEnumerable<DataSendModel> collection) : base(collection)
        {
        }
    }
}
