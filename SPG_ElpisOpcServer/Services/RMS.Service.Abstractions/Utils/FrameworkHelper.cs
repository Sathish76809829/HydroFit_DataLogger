namespace RMS.Service.Abstractions.Utils
{
    /// <summary>
    /// Contains framework info
    /// </summary>
    public static class FrameworkHelper
    {
        public const string TargetFramework =
#if NET5_0
            "net5.0"
#elif NETCOREAPP3_1
            "netcoreapp3.1"
#else
             ""
#endif
            ;
    }
}
