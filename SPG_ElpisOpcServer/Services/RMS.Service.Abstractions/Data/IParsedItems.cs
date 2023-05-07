using RMS.Service.Models;
using System.Collections.Generic;

namespace RMS.Service.Abstractions
{
    /// <summary>
    /// Interface for parsed result list
    /// </summary>
    public interface IParsedItems : IReadOnlyCollection<DataSendModel>
    {

    }
}
