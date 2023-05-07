using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.IO;

namespace RMS.DataParser.PlugIns
{
    /// <summary>
    /// PlugIn Extension which will load the plugIns from plugIn directory and store in <see cref="PlugInContainer"/> instance
    /// </summary>
    public static class PlugInExtensions
    {
        public static IServiceCollection AddPlugIns(this IServiceCollection self, string plugInFolder)
        {
            System.Diagnostics.Debug.WriteLine($"Looking for plugins in {plugInFolder}");
            var plugInDirectory = new DirectoryInfo(plugInFolder);
            var plugIns = new List<FileInfo>();
            if (plugInDirectory.Exists)
            {
                var subdirectories = plugInDirectory.GetDirectories();
                if (subdirectories.Length > 0)
                {
                    foreach (var directory in subdirectories)
                    {
                        var file = new FileInfo(Path.Combine(directory.FullName, directory.Name + ".dll"));
                        if (file.Exists)
                            plugIns.Add(file);
                    }
                }
            }
            var container = new PlugInContainer();
            foreach (var plugIn in plugIns)
            {
                container.AddPlugIn(plugIn, self);
            }
            self.AddSingleton(container);
            return self;
        }


    }
}
