using Microsoft.Extensions.DependencyInjection;
using RMS.Service.Abstractions.PlugIns;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace RMS.DataParser.PlugIns
{
    /// <summary
    /// >Initially loads the plugIns from plugin directory.
    /// Includes PlugIn Information for RMS Data Parser such as plugIn dll directory, Guid and Type Id
    /// </summary>
    public class PlugInContainer
    {
        private static readonly Type PlugInHostType = typeof(IPlugInHost);

        private readonly Dictionary<Guid, PlugInStart> plugIns;

        private readonly List<PlugInInfo> plugInInfos;

        public PlugInContainer()
        {
            plugIns = new Dictionary<Guid, PlugInStart>();
            plugInInfos = new List<PlugInInfo>();
        }

        public ICollection<PlugInStart> PlugIns
        {
            get => plugIns.Values;
        }

        public ICollection<PlugInInfo> PlugInInfos
        {
            get
            {
                return plugInInfos;
            }
        }

        public PlugInStart GetPlugInById(Guid id)
        {
            if (plugIns.TryGetValue(id, out var plugIn))
                return plugIn;
            return null;
        }

        public void AddPlugIn(PlugInStart plugIn)
        {
            if (plugIn == null)
                throw new ArgumentNullException(nameof(plugIn));
            Guid id = plugIn.Info.Id;
            if (plugIns.ContainsKey(id))
                throw new SystemException("Plug dupplicate id for plugin " + plugIn.Info.Name);
            plugIns.Add(id, plugIn);
            plugInInfos.Add(plugIn.Info);
        }

        public static Assembly LoadPlugin(FileInfo plugIn)
        {
            Console.WriteLine($"Loading plugins from: {plugIn}");
            //return Assembly.LoadFile(plugIn.FullName);
            PluginLoadContext loadContext = new PluginLoadContext(plugIn.FullName);
            return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(plugIn.Name)));
        }

        public void AddPlugIn(FileInfo plugIn, IServiceCollection services)
        {
            var assembly = LoadPlugin(plugIn);
            int count = 0;
            Type[] types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                if (PlugInHostType.IsAssignableFrom(type))
                {
                    var result = (IPlugInHost)Activator.CreateInstance(type);
                    if (result != null)
                    {
                        count++;
                        PlugInInfo info;
                        var infoAttr = type.GetCustomAttribute<HostInfoAttribute>();
                        if (infoAttr == null)
                        {
                            info = new PlugInInfo(plugIn.DirectoryName, Guid.NewGuid());
                        }
                        else
                        {
                            info = new PlugInInfo(infoAttr.Name, infoAttr.Id);
                        }
                        info[PlugIn.PlugInDirKey] = plugIn.DirectoryName;
                        info[PlugIn.PlugInFileKey] = plugIn.FullName;
                        info[PlugIn.PlugInTypeIdKey] = result.TypeId;
                        result.Configure(services);
                        AddPlugIn(new PlugInStart(result, info));
                    }
                }
            }

            if (count == 0)
            {
                string availableTypes = string.Join(",\r\n", Array.ConvertAll(types, t => t.FullName));
                throw new ApplicationException(
                    $"Can't find any type which implements IPlugInFactory in {assembly} from {assembly.Location}.\n" +
                    $"Available types: {availableTypes}");
            }
        }
    }
}
