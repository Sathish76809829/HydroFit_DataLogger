namespace RMS.Service.Abstractions.PlugIns
{
    /// <summary>
    /// PlugIn Start info used in RMS
    /// </summary>
    public class PlugInStart
    {
        public PlugInStart(IPlugInHost host, PlugInInfo info)
        {
            Host = host;
            Info = info;
        }

        public IPlugInHost Host { get; }

        public PlugInInfo Info { get; }
    }
}
