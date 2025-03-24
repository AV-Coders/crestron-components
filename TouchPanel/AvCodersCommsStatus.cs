using AVCoders.Core;
using AVCoders.Crestron.SmartGraphics;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Filters;

namespace AVCoders.Crestron.TouchPanel;

public class AvCodersCommsStatus
{
    private readonly List<IpComms> _communicationClients;
    private readonly List<List<string>> _logMessages;
    private readonly List<SmartObject> _smartObjects;
    private readonly SubpageReferenceListHelper _srlHelper;
    private readonly TouchpanelLoggerSink _sink;
    private readonly string _logKey = Guid.NewGuid().ToString().Substring(0,10);
    private readonly string _logVlaue = Guid.NewGuid().ToString().Substring(0,10);

    public const uint TxIndicator = 1;
    public const uint RxIndicator = 2;

    public const uint NameJoin = 1;
    public const uint ClassJoin = 2;
    public const uint IpJoin = 3;
    public const uint PortJoin = 4;
    public const uint ConnectionStatusJoin = 5;

    public static readonly uint[] LogJoins = { 11, 12, 13, 14, 15 };
    
    public static readonly uint JoinIncrement = 30;

    public AvCodersCommsStatus(List<IpComms> communicationClients, List<SmartObject> smartObjects)
    {
        _communicationClients = communicationClients;
        _srlHelper = new SubpageReferenceListHelper(JoinIncrement, JoinIncrement, JoinIncrement);
        
        _smartObjects = smartObjects;
        _smartObjects.ForEach(x => x.UShortInput["Set Number of Items"].ShortValue = (short)_communicationClients.Count);
        _sink = new TouchpanelLoggerSink();
        _sink.SerilogEventHandlers += HandleCommsClientLogEvent;

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Sink((ILogEventSink)Log.Logger)
            .WriteTo.Logger(l => l.Filter.ByIncludingOnly(Matching.WithProperty(_logKey, _logVlaue)).WriteTo.Sink(_sink))
            .CreateLogger();

        _logMessages = new List<List<string>>{};
        
        for (int i = 0; i < _communicationClients.Count; i++)
        {
            var deviceIndex = i;
            _communicationClients[deviceIndex].ConnectionStateHandlers += _ => FeedbackForDevice(deviceIndex);
            _communicationClients[deviceIndex].AddLogProperty("AvCodersCommsStatusIndex", deviceIndex.ToString());
            _communicationClients[deviceIndex].AddLogProperty(_logKey, _logVlaue);
            _logMessages.Add(new List<string>{ "Module started" });
            FeedbackForDevice(deviceIndex);
        }
    }

    private void HandleCommsClientLogEvent(LogEvent logEvent)
    {
        if (!logEvent.Properties.TryGetValue("AvCodersCommsStatusIndex", out var deviceIndexRaw))
        {
            Log.Error("A log event has been handed over without a device index.");
            return;
        }

        int deviceIndex = Convert.ToInt32(deviceIndexRaw.ToString().Replace("\"", null));
        if(deviceIndex > _communicationClients.Count - 1)
        {
            Log.Error($"The device Index {deviceIndex} out of range, max {_communicationClients.Count - 1}.");
            return;
        }
        _logMessages[deviceIndex].Add($"{DateTime.Now} - {logEvent.Level.ToString()} - {logEvent.RenderMessage()}");

        while (_logMessages[deviceIndex].Count > 5)
        {
            _logMessages[deviceIndex].RemoveRange(0, 1);
        }
        _smartObjects.ForEach(smartObject =>
        {
            smartObject.StringInput[_srlHelper.SerialJoinFor(deviceIndex, LogJoins[4])].StringValue = _logMessages[deviceIndex].Count > 0 ? _logMessages[deviceIndex][0] : String.Empty;
            smartObject.StringInput[_srlHelper.SerialJoinFor(deviceIndex, LogJoins[3])].StringValue = _logMessages[deviceIndex].Count > 1 ? _logMessages[deviceIndex][1] : String.Empty;
            smartObject.StringInput[_srlHelper.SerialJoinFor(deviceIndex, LogJoins[2])].StringValue = _logMessages[deviceIndex].Count > 2 ? _logMessages[deviceIndex][2] : String.Empty;
            smartObject.StringInput[_srlHelper.SerialJoinFor(deviceIndex, LogJoins[1])].StringValue = _logMessages[deviceIndex].Count > 3 ? _logMessages[deviceIndex][3] : String.Empty;
            smartObject.StringInput[_srlHelper.SerialJoinFor(deviceIndex, LogJoins[0])].StringValue = _logMessages[deviceIndex].Count > 4 ? _logMessages[deviceIndex][4] : String.Empty;
        });
    }

    private void FeedbackForDevice(int deviceIndex)
    {
        _smartObjects.ForEach(smartObject =>
        {
            smartObject.StringInput[_srlHelper.SerialJoinFor(deviceIndex, NameJoin)].StringValue = _communicationClients[deviceIndex].Name;
            smartObject.StringInput[_srlHelper.SerialJoinFor(deviceIndex, ClassJoin)].StringValue = _communicationClients[deviceIndex].GetType().ToString();
            smartObject.StringInput[_srlHelper.SerialJoinFor(deviceIndex, IpJoin)].StringValue = _communicationClients[deviceIndex].GetHost();
            smartObject.StringInput[_srlHelper.SerialJoinFor(deviceIndex, PortJoin)].StringValue = _communicationClients[deviceIndex].GetPort().ToString();
            smartObject.StringInput[_srlHelper.SerialJoinFor(deviceIndex, ConnectionStatusJoin)].StringValue = _communicationClients[deviceIndex].GetConnectionState().ToString();
        });
        
    }
}