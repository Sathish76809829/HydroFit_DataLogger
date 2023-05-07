using System;
using System.Threading.Tasks;

namespace RMS.Service.Abstractions.PlugIns
{
    /// <summary>
    /// PlugIn instance for <see cref="IPlugInHost"/>
    /// </summary>
    public class PlugIn : IPlugIn
    {
        public static readonly string BasePathKey = "plugin.basePath";
        public static readonly string PlugInDirKey = "plugin.dir";
        public static readonly string PlugInFileKey = "plugin.file";
        public static readonly string PlugInNameKey = "plugin.name";
        public static readonly string PlugInIdKey = "plugin.id";
        public static readonly string PlugInProvidersKey = "plugin.providers";
        public static readonly string PlugInTypeIdKey = "plugin.typeId";

        private readonly IServiceProvider services;

        private readonly PlugInInfo info;

        public PlugIn(PlugInContext context, IServiceProvider services)
        {
            Context = context;
            info = new PlugInInfo(context.Properties);
            this.services = services;
        }

        public PlugInContext Context { get; }

        public IServiceProvider Services => services;

        public PlugInInfo Info => info;

        public static PlugInBuilder CreateBuilder(PlugInInfo info)
        {
            return new PlugInBuilder(info);
        }

        public void Dispose()
        {
            if (services is IDisposable)
            {
                ((IDisposable)services).Dispose();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (services is IAsyncDisposable)
            {
                await ((IAsyncDisposable)services).DisposeAsync();
            }
        }
    }
}
