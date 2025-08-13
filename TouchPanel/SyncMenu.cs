using AVCoders.Core;
using AVCoders.Crestron.SmartGraphics;
using AVCoders.Matrix;

namespace AVCoders.Crestron.TouchPanel;

public class SyncMenu : SrlPage
{
    public static readonly uint JoinIncrement = 30;
    private readonly List<SyncStatus> _devices = new ();

    private const uint NameJoin = 1;
    private const uint TypeJoin = 2;
    private const uint StreamIdJoin = 3;
    private const uint SyncStatusJoin = 4;

    public SyncMenu(List<SyncStatus> devices, List<SmartObject> smartObjects, string name) : base(name, smartObjects, JoinIncrement)
    {
        RegisterFeedback(_devices);
    }

    private void RegisterFeedback(List<SyncStatus> devices)
    {
        SmartObjects.ForEach(x =>
        {
            x.UShortInput["Set Number of Items"].UShortValue = (ushort)_devices.Count;
        });
        
        for (int i = 0; i < devices.Count; i++)
        {
            var deviceIndex = i;
            var device = devices[deviceIndex];
            device.StreamChangeHandlers += _ => FeedbackForDevice(deviceIndex);
            switch (device.DeviceType)
            {
                case AVEndpointType.Encoder:
                    device.InputStatusChangedHandlers += (status, resolution, hdcpStatus) 
                        => HandleDeviceSync(deviceIndex, status, resolution, hdcpStatus);
                    break;
                case AVEndpointType.Decoder:
                    device.OutputStatusChangedHandlers += (status, resolution, hdcpStatus) 
                        => HandleDeviceSync(deviceIndex, status, resolution, hdcpStatus);
                    break;
            }
            FeedbackForDevice(deviceIndex);
            _devices.Add(device);
        }
    }

    private void UnregisterFeedback()
    {
        for (int i = 0; i < _devices.Count; i++)
        {
            var deviceIndex = i;
            _devices[deviceIndex].StreamChangeHandlers -= _ => FeedbackForDevice(deviceIndex);
            _devices[deviceIndex].InputStatusChangedHandlers -= (status, resolution, hdcpStatus) 
                => HandleDeviceSync(deviceIndex, status, resolution, hdcpStatus);
            _devices[deviceIndex].OutputStatusChangedHandlers -= (status, resolution, hdcpStatus) 
                => HandleDeviceSync(deviceIndex, status, resolution, hdcpStatus);
        }
        _devices.Clear();
    }

    public void HandleDeviceListChange(List<SyncStatus> devices)
    {
        UnregisterFeedback();
        RegisterFeedback(devices);
    }

    private void HandleDeviceSync(int deviceIndex, ConnectionState status, string resolution, HdcpStatus hdcpStatus)
    {
        string syncStatus = status switch
        {
            ConnectionState.Disconnected => "Disconnected",
            ConnectionState.Connected => $"Connected at {resolution}",
            _ => "Unknown"
        };
        SmartObjects.ForEach(smartObject =>
        {
            smartObject.StringInput[_srlHelper.SerialJoinFor(deviceIndex, SyncStatusJoin)].StringValue = syncStatus;
        });
    }

    private void FeedbackForDevice(int deviceIndex)
    {
        SmartObjects.ForEach(smartObject =>
        {
            smartObject.StringInput[_srlHelper.SerialJoinFor(deviceIndex, NameJoin)].StringValue = _devices[deviceIndex].Name;
            smartObject.StringInput[_srlHelper.SerialJoinFor(deviceIndex, TypeJoin)].StringValue = _devices[deviceIndex].DeviceType.ToString();
            smartObject.StringInput[_srlHelper.SerialJoinFor(deviceIndex, StreamIdJoin)].StringValue = 
                _devices[deviceIndex].DeviceType == AVEndpointType.Encoder? 
                    $"Streaming to {_devices[deviceIndex].StreamAddress}" :
                    $"Source: {_devices[deviceIndex].StreamAddress}";
        });
        
    }

    public override void PowerOn() { }

    public override void PowerOff() { }
}