using RMS.EventBus.Abstrations;
using RMS.Service.Abstractions;
using RMS.Service.Abstractions.Events;
using RMS.Service.Abstractions.Services;
using System.Threading.Tasks;

namespace Petronash.IntegrationEvents.EventHandling
{
    /// <summary>
    /// Even bus (Kafka) listener for RMS Input Change event
    /// </summary>
    public class InputChangeEventHandler : IIntegrationEventHandler<InputChangeEvent>
    {
        private readonly IEventMonitor monitor;
        public InputChangeEventHandler(IEventMonitor monitor)
        {
            this.monitor = monitor;
        }

        public Task Handle(InputChangeEvent e)
        {
            var signal = e.Signal;
            if (signal.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                Models.PumpStatusChange statusChange = default;
                statusChange.Date = e.CreationDate;
                var signalId = signal.GetProperty(JsonPropertyKeys.SignalId);
                statusChange.SignalId = signalId.GetString()/*GetInt32()*/;
                var deviceId = signal.GetProperty(JsonPropertyKeys.DeviceId);
                statusChange.DeviceId = deviceId.GetString()/*GetInt32()*/;
                var value = signal.GetProperty(JsonPropertyKeys.DataValue).GetRawText();
                statusChange.Value = value.Equals("0") ? PumpStatus.Stoped : value.Equals("1") ? PumpStatus.Started : PumpStatus.None;
                monitor.Notify(statusChange);
            }
            return Task.CompletedTask;
        }
    }
}
