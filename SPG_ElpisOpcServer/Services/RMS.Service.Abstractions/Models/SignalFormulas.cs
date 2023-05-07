using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RMS.Service.Abstractions.Models
{
    /// <summary>
    /// Signal formula model
    /// </summary>
    public class SignalFormulas : ISignalModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public /*int*/string DeviceId { get; set; }

        public /*int*/string SignalId { get; set; }

        public SignalDataType DataType { get; set; }

        public string Script { get; set; }
    }
}
