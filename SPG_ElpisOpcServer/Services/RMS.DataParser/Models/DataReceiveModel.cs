using System.Collections.Generic;

namespace RMS.Service.Models
{
    /// <summary>
    /// Receive
    /// </summary>
    public class DataReceiveModel
    {
        public string Device { get; set; }
        public IList<string> SignalDataList { get; set; }
    }
}
