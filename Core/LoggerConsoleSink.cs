using System.Text;
using AVCoders.Core;
using Crestron.SimplSharp;
using Serilog.Core;
using Serilog.Events;

namespace AVCoders.Crestron.Core;

public abstract class SinkBase : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        StringBuilder sb = new StringBuilder("[");
        sb.Append(DateTime.Now);
        sb.Append(" | ");
        sb.Append(logEvent.Level.ToString());
        sb.Append("] ");
        if (logEvent.Properties.TryGetValue("Class", out var c))
        {
            sb.Append(c.ToString().Replace("\"", null));
            sb.Append(" - ");
        }
        if (logEvent.Properties.TryGetValue("InstanceName", out var i))
        {
            sb.Append(i.ToString().Replace("\"", null));
            sb.Append(" - ");
        }

        if (logEvent.Properties.TryGetValue(LogBase.MethodProperty, out var m))
        {
            sb.Append(m.ToString().Replace("\"", null));
            sb.Append(" - ");
        }
        
        sb.Append(logEvent.RenderMessage());
        DoEmit(sb.ToString(), logEvent.Level);
    }

    protected abstract void DoEmit(string message, LogEventLevel logEventLevel);
}

public class CrestronConsoleSink : SinkBase
{
    protected override void DoEmit(string message, LogEventLevel logEventLevel) => CrestronConsole.PrintLine(message);
}

public class CrestronErrorLogSink : SinkBase
{
    protected override void DoEmit(string message, LogEventLevel logEventLevel)
    {
        switch (logEventLevel)
        {
            case LogEventLevel.Error:
                ErrorLog.Error(message);
                break;
            case LogEventLevel.Fatal:
                ErrorLog.Error(message);
                break;
            case LogEventLevel.Warning:
                ErrorLog.Warn(message);
                break;
            case LogEventLevel.Information:
                ErrorLog.Info(message);
                break;
            default:
                ErrorLog.Notice(message);
                break;
        }
        
    }
}