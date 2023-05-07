using System;

namespace Petronash.Models
{
    /// <summary>
    /// Pump state change info on <see cref="IntegrationEvents.EventHandling.InputChangeEventHandler.Handle(RMS.Service.Abstractions.Events.InputChangeEvent)"/> event
    /// </summary>
    public struct PumpStatusChange
    {
        public /*int*/string DeviceId { get; set; }
        public /*int*/string SignalId { get; set; }
        public PumpStatus Value { get; set; }
        public DateTime Date { get; set; }
    }
}
