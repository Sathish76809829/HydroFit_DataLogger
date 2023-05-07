using Microsoft.Extensions.Logging;
using System;
using System.Text;

namespace RMS.Broker.Logger
{
    /// <summary>
    /// Generic file logger that works in a similar way to standard ConsoleLogger.
    /// </summary>
    public class FileLogger : ILogger
	{
		private readonly string logName;
		private readonly FileLoggerProvider LoggerProvider;

		public FileLogger(string logName, FileLoggerProvider loggerPrv)
		{
			this.logName = logName;
			LoggerProvider = loggerPrv;
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			return null;
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return logLevel >= LoggerProvider.MinLevel;
		}

		string GetShortLogLevel(LogLevel logLevel)
		{
			switch (logLevel)
			{
				case LogLevel.Trace:
					return "TRCE";
				case LogLevel.Debug:
					return "DBUG";
				case LogLevel.Information:
					return "INFO";
				case LogLevel.Warning:
					return "WARN";
				case LogLevel.Error:
					return "FAIL";
				case LogLevel.Critical:
					return "CRIT";
			}
			return logLevel.ToString().ToUpper();
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
			Exception exception, Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel))
			{
				return;
			}

			if (formatter == null)
			{
				throw new ArgumentNullException(nameof(formatter));
			}
			string message = null;
			if (null != formatter)
			{
				message = formatter(state, exception);
			}

			// default formatting logic
			var logBuilder = new StringBuilder();
			if (!string.IsNullOrEmpty(message))
			{
				logBuilder.Append(DateTime.Now.ToString("o"));
				logBuilder.Append('\t');
				logBuilder.Append(GetShortLogLevel(logLevel));
				logBuilder.Append("\t[");
				logBuilder.Append(logName);
				logBuilder.Append("]");
				logBuilder.Append("\t[");
				logBuilder.Append(eventId);
				logBuilder.Append("]\t");
				logBuilder.Append(message);
			}

			if (exception != null)
			{
				// exception message
				logBuilder.AppendLine(exception.ToString());
			}
			LoggerProvider.WriteEntry(logBuilder.ToString());
		}
	}
}