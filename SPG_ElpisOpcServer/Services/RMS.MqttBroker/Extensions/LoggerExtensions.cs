using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RMS.Broker.Logger;
using System;

namespace RMS.Broker.Extensions
{
    /// <summary>
    /// File logger extensions
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Adds a file logger.
        /// </summary>
        public static ILoggingBuilder AddFile(this ILoggingBuilder builder, IConfiguration configuration, Action<FileLoggerOptions> configure = null)
        {
            var logProvider = CreateFromConfiguration(configuration, configure);
            if (logProvider != null)
            {
                builder.Services.Add(ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>(
                    (services) =>
                    {
                        return logProvider;
                    }
                ));
            }
            return builder;
        }

        private static FileLoggerProvider CreateFromConfiguration(IConfiguration configuration, Action<FileLoggerOptions> configure)
        {
            var fileSection = configuration.GetSection("File");
            if (fileSection == null)
                return null;  // file logger is not configured
            var fileName = fileSection["Path"];
            if (string.IsNullOrWhiteSpace(fileName))
                return null; // file logger is not configured

            var fileLoggerOptions = new FileLoggerOptions();
            var appendVal = fileSection["Append"];
            if (!string.IsNullOrEmpty(appendVal) && bool.TryParse(appendVal, out var append))
                fileLoggerOptions.Append = append;

            var fileLimitVal = fileSection["FileSizeLimitBytes"];
            if (!string.IsNullOrEmpty(fileLimitVal) && long.TryParse(fileLimitVal, out var fileLimit))
                fileLoggerOptions.FileSizeLimitBytes = fileLimit;

            var maxFilesVal = fileSection["MaxRollingFiles"];
            if (!string.IsNullOrEmpty(maxFilesVal) && int.TryParse(maxFilesVal, out var maxFiles))
                fileLoggerOptions.MaxRollingFiles = maxFiles;

            if (configure != null)
                configure(fileLoggerOptions);

            return new FileLoggerProvider(fileName, fileLoggerOptions);
        }
    }
}
