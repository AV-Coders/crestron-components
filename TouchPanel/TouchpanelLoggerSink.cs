using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace AVCoders.Crestron.TouchPanel;

public delegate void SerilogEventHandler(LogEvent logEvent);

public class TouchpanelLoggerSink : ILogEventSink
{
    public SerilogEventHandler? SerilogEventHandlers;
    private int _failedEmissions = 0;
    
    public void Emit(LogEvent logEvent)
    {
        try
        {
            SerilogEventHandlers?.Invoke(logEvent);
        }
        catch (Exception e)
        {
            _failedEmissions++;
            if (_failedEmissions > 10)
                return;
            Log.Fatal("Exception while emitting to a delegate.");
            Log.Fatal(e.Message);
            Log.Fatal(e.StackTrace ?? "No stack trace available.");
            Log.Fatal(e.InnerException?.Message ?? "No inner exception");
            Log.Fatal(e.InnerException?.StackTrace ?? "No inner stack trace available.");
        }

        _failedEmissions = 0;
    }
}