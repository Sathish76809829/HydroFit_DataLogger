using RMS.Service.Abstractions.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Petronash.Models
{
    /// <summary>
    /// Test details model from RMS Database
    /// </summary>
    public class TestDetails : ISignalModel
    {
        [Key]
        public long TestId { get; set; }

        public /*int*/string DeviceId { get; set; }

        public /*int*/string SignalId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? StopTime { get; set; }

        public long? RunTime { get; set; }

        internal void UpdateRunTime()
        {
            RunTime = (long)(StopTime - StartTime).Value.TotalMilliseconds;
        }
    }
}
