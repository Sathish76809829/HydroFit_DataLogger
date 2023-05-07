using Petronash.Models;

namespace Petronash
{
    public enum PumpStatus : byte
    {
        None = 0,
        Started = 1,
        Stoped = 2,
    }

    public static class PumpStateBoxed
    {
        public static readonly object High = 1;
        public static readonly object Low = 0;
    }

    /// <summary>
    /// Pumpstate holder for petronash
    /// </summary>
    public class PumpState 
    {
        private PumpStatus status = PumpStatus.Stoped;
        private TestDetails details;
        private readonly PetronashPump pump;

        public TestDetails Details
        {
            get => details;
            internal set
            {
                if (value == null)
                    return;
                details = value;
                status = value.StopTime.HasValue ? PumpStatus.Stoped : PumpStatus.Started;
            }
        }

        public PumpState(PetronashPump pump)
        {
            this.pump = pump;
        }

        public /*int*/string DeviceId => pump.DeviceId;

        public /*int*/string StartSignal => pump.StartSignal;

        public /*int*/string StopSignal => pump.StopSignal;

        public PumpStatus Status
        {
            get => status;
            set => status = value;
        }
    }
}
