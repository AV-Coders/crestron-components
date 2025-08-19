using AVCoders.Core;
using AVCoders.Crestron.SmartGraphics;
using Serilog;
using Serilog.Context;

namespace AVCoders.Crestron.TouchPanel;

public class CrestronStatus : SrlPage
{
    private readonly List<GenericDevice> _crestronDevices;

    private const uint OnlineJoin = 1;

    private const uint ModelJoin = 1;
    private const uint IpIdJoin = 2;
    private const uint NameJoin = 3;

    public CrestronStatus(string name, List<GenericDevice> crestronDevices, List<SmartObject> smartObjects) : base(name, smartObjects)
    {
        _crestronDevices = crestronDevices;
        ConfigureSmartObject();

        for (int i = 0; i < _crestronDevices.Count; i++)
        {
            var deviceIndex = i;
            _crestronDevices[i].OnlineStatusChange += (_, args) => HandleDeviceOnlineStatusChange(args, deviceIndex);
        }
    }

    private void HandleDeviceOnlineStatusChange(OnlineOfflineEventArgs args, int deviceIndex)
    {
        SmartObjects.ForEach(x => x.BooleanInput[SrlHelper.BooleanJoinFor(deviceIndex, OnlineJoin)].BoolValue = args.DeviceOnLine);
        
        if(args.DeviceOnLine)
            FeedbackForDevice(deviceIndex);
    }

    private void ConfigureSmartObject()
    {
        SmartObjects.ForEach(x => x.UShortInput["Set Number of Items"].ShortValue = (short)_crestronDevices.Count);

        for (int i = 0; i < _crestronDevices.Count; i++)
        {
            FeedbackForDevice(i);
        }
    }
    
    private void FeedbackForDevice(int deviceIndex)
    {
        SmartObjects.ForEach(x =>
        {
            x.StringInput[SrlHelper.SerialJoinFor(deviceIndex, NameJoin)].StringValue = _crestronDevices[deviceIndex].Description;
            x.StringInput[SrlHelper.SerialJoinFor(deviceIndex, IpIdJoin)].StringValue = $"IP ID: {_crestronDevices[deviceIndex].ID:x2}";
            x.StringInput[SrlHelper.SerialJoinFor(deviceIndex, ModelJoin)].StringValue = _crestronDevices[deviceIndex].Name;
            x.BooleanInput[SrlHelper.BooleanJoinFor(deviceIndex, OnlineJoin)].BoolValue = _crestronDevices[deviceIndex].IsOnline;
        });
    }

    public override void PowerOn() { }

    public override void PowerOff() { }
}