using System.Collections.Generic;

namespace Petronash.Configurations
{
    /// <summary>
    /// Petronash pump info from config.json
    /// </summary>
    public class PetronashSettings
    {
        public List<Models.PetronashPump> Pumps { get; set; }
    }
}
