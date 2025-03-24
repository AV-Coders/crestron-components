using AVCoders.Core;
using AVCoders.Crestron.SmartGraphics;
using AVCoders.Matrix;

namespace AVCoders.Crestron.TouchPanel;

public class AVoIPMenu
{
    public static readonly uint JoinIncrement = 30;
    private readonly List<AVoIPEndpoint> _devices;
    private readonly List<SmartObject> _smartObjects;
    private readonly SubpageReferenceListHelper _srlHelper;
    private readonly string _name;
    private bool _enableLogs;

    private const uint OnlineJoin = 1;

    public const uint DriverStatusRedJoin = 2;
    public const uint DriverStatusGreenJoin = 3;
    public const uint DriverStatusBlueJoin = 4;
    public const uint CommsStatusRedJoin = 5;
    public const uint CommsStatusGreenJoin = 6;
    public const uint CommsStatusBlueJoin = 7;

    private const uint NameJoin = 1;
    private const uint TypeJoin = 2;
    private const uint StreamIdJoin = 3;
    private const uint SyncStatusJoin = 4;

    public const uint DriverStatusLabelJoin = 11;
    public const uint CommsStatusLabelJoin = 12;


    public AVoIPMenu(List<AVoIPEndpoint> devices, List<SmartObject> smartObjects, string name)
    {
        _devices = devices;
        _smartObjects = smartObjects;
        _srlHelper = new SubpageReferenceListHelper(JoinIncrement, JoinIncrement, JoinIncrement);
        _name = name;
        _smartObjects.ForEach(x =>
        {
            x.UShortInput["Set Number of Items"].UShortValue = (ushort)_devices.Count;
        });
        
        for (int i = 0; i < _devices.Count; i++)
        {
            var deviceIndex = i;
            _devices[deviceIndex].StreamChangeHandlers += _ => FeedbackForDevice(deviceIndex);
            _devices[deviceIndex].PreviewUrlChangeHandlers += _ => FeedbackForDevice(deviceIndex);
            _devices[deviceIndex].CommunicationClient.ConnectionStateHandlers += state => CommsStateFeedback(deviceIndex, state);
            switch (devices[deviceIndex].DeviceType)
            {
                case AVoIPDeviceType.Encoder:
                    devices[deviceIndex].InputStatusChangedHandlers += (status, resolution) => HandleDeviceSync(deviceIndex, status, resolution);
                    break;
                case AVoIPDeviceType.Decoder:
                    devices[deviceIndex].OutputStatusChangedHandlers += (status, resolution) => HandleDeviceSync(deviceIndex, status, resolution);
                    break;
            }
            CommsStateFeedback(deviceIndex, _devices[deviceIndex].CommunicationClient.GetConnectionState());
            FeedbackForDevice(deviceIndex);
        }
    }

    private void HandleDeviceSync(int deviceIndex, ConnectionStatus status, string resolution)
    {
        string syncStatus = status switch
        {
            ConnectionStatus.Disconnected => "Disconnected",
            ConnectionStatus.Connected => $"Connected at {resolution}",
            _ => "Unknown"
        };
        _smartObjects.ForEach(smartObject =>
        {
            smartObject.StringInput[_srlHelper.SerialJoinFor(deviceIndex, SyncStatusJoin)].StringValue = syncStatus;
        });
    }

    private void FeedbackForDevice(int deviceIndex)
    {
        _smartObjects.ForEach(smartObject =>
        {
            smartObject.StringInput[_srlHelper.SerialJoinFor(deviceIndex, NameJoin)].StringValue = _devices[deviceIndex].Name;
            smartObject.StringInput[_srlHelper.SerialJoinFor(deviceIndex, TypeJoin)].StringValue = _devices[deviceIndex].DeviceType.ToString();
            smartObject.StringInput[_srlHelper.SerialJoinFor(deviceIndex, StreamIdJoin)].StringValue = 
                _devices[deviceIndex].DeviceType == AVoIPDeviceType.Encoder? 
                    $"Streaming to {_devices[deviceIndex].StreamAddress}" :
                    $"Source: {_devices[deviceIndex].StreamAddress}";
        });
        
    }
    
    private void CommsStateFeedback(int deviceIndex, ConnectionState state)
    {
        ushort redValue = 0;
        ushort greenValue = 0;
        ushort blueValue = 0;
        string commsText = String.Empty;

        switch (state)
        {
            case ConnectionState.Connected:
                greenValue = 199;
                blueValue = 129;
                commsText = "Comms: Connected";
                break;
            case ConnectionState.Error:
                redValue = 146;
                greenValue = 8;
                blueValue = 8;
                commsText = "Comms: Error";
                break;
            case ConnectionState.Disconnected:
                redValue = 166;
                greenValue = 127;
                blueValue = 0;
                commsText = "Comms: Disconnected";
                break;
            case ConnectionState.Connecting:
            case ConnectionState.Disconnecting:
                redValue = 5;
                greenValue = 112;
                blueValue = 192;
                commsText = "Comms: Busy";
                break;
            case ConnectionState.Idle:
                redValue = 141;
                greenValue = 141;
                blueValue = 141;
                commsText = "Comms: Idle";
                break;
                
        }
        _smartObjects.ForEach(smartObject =>
        {
            smartObject.UShortInput[_srlHelper.AnalogJoinFor(deviceIndex, CommsStatusRedJoin)].UShortValue = redValue;
            smartObject.UShortInput[_srlHelper.AnalogJoinFor(deviceIndex, CommsStatusGreenJoin)].UShortValue = greenValue;
            smartObject.UShortInput[_srlHelper.AnalogJoinFor(deviceIndex, CommsStatusBlueJoin)].UShortValue = blueValue;
            smartObject.StringInput[_srlHelper.SerialJoinFor(deviceIndex, CommsStatusLabelJoin)].StringValue = commsText;
        });
    }
    
    private void DriverStateFeedback(int deviceIndex, CommunicationState state)
    {
        ushort redValue = 0;
        ushort greenValue = 0;
        ushort blueValue = 0;
        string driverText = String.Empty;

        switch (state)
        {
            case CommunicationState.Okay:
                greenValue = 199;
                blueValue = 129;
                driverText = "Driver: Okay";
                break;
            case CommunicationState.Error:
                redValue = 146;
                greenValue = 8;
                blueValue = 8;
                driverText = "Driver: Error";
                break;
            case CommunicationState.NotAttempted:
                redValue = 141;
                greenValue = 141;
                blueValue = 141;
                driverText = "Driver: Not attempted";
                break;
            case CommunicationState.Unknown:
                redValue = 0;
                greenValue = 0;
                blueValue = 0;
                driverText = "Driver: Unknown";
                break;
                
        }
        _smartObjects.ForEach(smartObject =>
        {
            smartObject.UShortInput[_srlHelper.AnalogJoinFor(deviceIndex, DriverStatusRedJoin)].UShortValue = redValue;
            smartObject.UShortInput[_srlHelper.AnalogJoinFor(deviceIndex, DriverStatusGreenJoin)].UShortValue = greenValue;
            smartObject.UShortInput[_srlHelper.AnalogJoinFor(deviceIndex, DriverStatusBlueJoin)].UShortValue = blueValue;
            smartObject.StringInput[_srlHelper.SerialJoinFor(deviceIndex, DriverStatusLabelJoin)].StringValue = driverText;
        });
    }

    public void EnableLogs(bool enable) => _enableLogs = enable;

    private void Log(string message)
    {
        if(_enableLogs)
            CrestronConsole.PrintLine($"{DateTime.Now} - {_name} - AVoIP Menu - {message}");
    }
}