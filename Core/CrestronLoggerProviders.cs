using System.Text;
using AVCoders.Core;
using Crestron.SimplSharp;
using Microsoft.Extensions.Logging;

namespace AVCoders.Crestron.Core;

public abstract class CrestronLoggerProviderBase : ILoggerProvider, ISupportExternalScope
{
    private IExternalScopeProvider _scopeProvider = new LoggerExternalScopeProvider();

    public ILogger CreateLogger(string categoryName) => new CrestronLogger(this);

    public void SetScopeProvider(IExternalScopeProvider scopeProvider) => _scopeProvider = scopeProvider;

    public void Dispose() { }

    protected abstract void DoEmit(string message, LogLevel logLevel);

    private sealed class CrestronLogger(CrestronLoggerProviderBase provider) : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull =>
            provider._scopeProvider.Push(state);

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var properties = new Dictionary<string, object?>();
            provider._scopeProvider.ForEachScope((scope, accumulator) =>
            {
                if (scope is not IEnumerable<KeyValuePair<string, object>> scopeProperties)
                    return;
                foreach (var property in scopeProperties)
                    accumulator[property.Key] = property.Value;
            }, properties);

            StringBuilder sb = new StringBuilder("[");
            sb.Append(DateTime.Now);
            sb.Append(" | ");
            if (properties.TryGetValue("service_name", out var serviceName))
            {
                sb.Append(serviceName);
                sb.Append(" | ");
            }
            sb.Append(logLevel.ToString());
            sb.Append("] ");
            if (properties.TryGetValue("Class", out var c))
            {
                sb.Append(c);
                sb.Append(" - ");
            }
            if (properties.TryGetValue("InstanceName", out var i))
            {
                sb.Append(i);
                sb.Append(" - ");
            }
            if (properties.TryGetValue(LogBase.MethodProperty, out var m))
            {
                sb.Append(m);
                sb.Append(" - ");
            }

            sb.Append(formatter(state, exception));
            if (exception != null)
            {
                sb.Append(Environment.NewLine);
                sb.Append(exception.GetType().Name);
                sb.Append(" - ");
                sb.Append(exception.Message);
                sb.Append(Environment.NewLine);
                sb.Append(exception.StackTrace ?? "No stack trace available");
                if (exception.InnerException != null)
                {
                    sb.Append(Environment.NewLine);
                    sb.Append("Inner Exception:");
                    sb.Append(exception.InnerException.Message);
                    sb.Append(Environment.NewLine);
                    sb.Append(exception.InnerException.StackTrace ?? "No inner stack trace available");
                }
            }
            provider.DoEmit(sb.ToString(), logLevel);
        }
    }
}

public class CrestronConsoleLoggerProvider : CrestronLoggerProviderBase
{
    protected override void DoEmit(string message, LogLevel logLevel) => CrestronConsole.PrintLine(message);
}

public class CrestronErrorLogLoggerProvider : CrestronLoggerProviderBase
{
    protected override void DoEmit(string message, LogLevel logLevel)
    {
        switch (logLevel)
        {
            case LogLevel.Critical:
            case LogLevel.Error:
                ErrorLog.Error(message);
                break;
            case LogLevel.Warning:
                ErrorLog.Warn(message);
                break;
            case LogLevel.Information:
                ErrorLog.Info(message);
                break;
            default:
                ErrorLog.Notice(message);
                break;
        }
    }
}
