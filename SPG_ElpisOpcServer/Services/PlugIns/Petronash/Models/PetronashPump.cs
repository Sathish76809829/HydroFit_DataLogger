namespace Petronash.Models
{
    /// <summary>
    /// Petronash pump info model which includes start and stop signal
    /// </summary>
    public class PetronashPump
    {
        public /*int*/string DeviceId { get; set; }
        public /*int*/string StartSignal { get; set; }
        public /*int*/string StopSignal { get; set; }
    }
}
