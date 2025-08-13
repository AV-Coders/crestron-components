using AVCoders.Crestron.SmartGraphics;
using Crestron.SimplSharpPro.DM.Streaming;
using Serilog;

namespace AVCoders.Crestron.TouchPanel;

public class NvxStatus : SrlPage
{
    private readonly List<DmNvxBaseClass> _nvxDevices;

    private const uint OnlineJoin = 1;
    
    private const uint NameJoin = 1;
    private const uint IpIdJoin = 2;
    private const uint ModeJoin = 3;
    

    public NvxStatus(string name, List<DmNvxBaseClass> nvxDevices, List<SmartObject> smartObjects) : base(name, smartObjects)
    {
        _nvxDevices = nvxDevices;
        ConfigureSmartObject();

        for (int i = 0; i < _nvxDevices.Count; i++)
        {
            var deviceIndex = i;
            _nvxDevices[i].OnlineStatusChange += (_, args) => HandleDeviceOnlineStatusChange(args, deviceIndex);
        }
    }

    private void HandleDeviceOnlineStatusChange(OnlineOfflineEventArgs args, int deviceIndex)
    {
        SmartObjects.ForEach(x => x.BooleanInput[_srlHelper.BooleanJoinFor(deviceIndex, OnlineJoin)].BoolValue = args.DeviceOnLine);
        
        if(args.DeviceOnLine)
            DeviceFeedback(deviceIndex);
    }

    private void ConfigureSmartObject()
    {
        Log.Debug("Configuring modal buttons");
        SmartObjects.ForEach(x => x.UShortInput["Set Number of Items"].ShortValue = (short)_nvxDevices.Count);

        for (int i = 0; i < _nvxDevices.Count; i++)
        {
            DeviceFeedback(i);
        }
    }
    
    private void DeviceFeedback(int deviceIndex)
    {
        SmartObjects.ForEach(x => x.StringInput[_srlHelper.SerialJoinFor(deviceIndex, NameJoin)].StringValue = _nvxDevices[deviceIndex].Description);
        SmartObjects.ForEach(x => x.StringInput[_srlHelper.SerialJoinFor(deviceIndex, IpIdJoin)].StringValue = $"IP ID: {_nvxDevices[deviceIndex].ID:x2}");
        SmartObjects.ForEach(x => x.BooleanInput[_srlHelper.BooleanJoinFor(deviceIndex, OnlineJoin)].BoolValue = _nvxDevices[deviceIndex].IsOnline);
        SmartObjects.ForEach(x => x.StringInput[_srlHelper.SerialJoinFor(deviceIndex, ModeJoin)].StringValue = $"Mode: {_nvxDevices[deviceIndex].Control.DeviceMode.ToString()}");
    }

    public override void PowerOn() { }

    public override void PowerOff() { }
}