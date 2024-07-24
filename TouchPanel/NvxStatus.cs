using AVCoders.Crestron.SmartGraphics;
using Crestron.SimplSharpPro.DM.Streaming;

namespace AVCoders.Crestron.TouchPanel;

public class NvxStatus
{
    private readonly List<DmNvxBaseClass> _nvxDevices;
    private SmartObject _smartObject;
    private readonly string _name;
    private readonly SubpageReferenceListHelper _srlHelper;

    private const uint OnlineJoin = 1;
    
    private const uint NameJoin = 1;
    private const uint IpIdJoin = 2;
    private const uint ModeJoin = 3;
    

    public NvxStatus(string name, List<DmNvxBaseClass> nvxDevices, SmartObject smartObject)
    {
        _name = name;
        _nvxDevices = nvxDevices;
        _srlHelper = new SubpageReferenceListHelper(10, 10, 10);
        _smartObject = smartObject;
        ConfigureSmartObject();

        for (int i = 0; i < _nvxDevices.Count; i++)
        {
            var deviceIndex = i;
            _nvxDevices[i].OnlineStatusChange += (_, args) => HandleDeviceOnlineStatusChange(args, deviceIndex);
        }
    }

    private void HandleDeviceOnlineStatusChange(OnlineOfflineEventArgs args, int deviceIndex)
    {
        _smartObject.BooleanInput[_srlHelper.BooleanJoinFor(deviceIndex, OnlineJoin)].BoolValue = args.DeviceOnLine;
        
        if(args.DeviceOnLine)
            DeviceFeedback(deviceIndex);
    }

    private void ConfigureSmartObject()
    {
        Log("Configuring modal buttons");
        _smartObject.UShortInput["Set Number of Items"].ShortValue = (short)_nvxDevices.Count;

        for (int i = 0; i < _nvxDevices.Count; i++)
        {
            DeviceFeedback(i);
        }
    }
    
    private void DeviceFeedback(int deviceIndex)
    {
        _smartObject.StringInput[_srlHelper.SerialJoinFor(deviceIndex, NameJoin)].StringValue = _nvxDevices[deviceIndex].Description;
        _smartObject.StringInput[_srlHelper.SerialJoinFor(deviceIndex, IpIdJoin)].StringValue = $"IP ID: {_nvxDevices[deviceIndex].ID:x2}";
        _smartObject.BooleanInput[_srlHelper.BooleanJoinFor(deviceIndex, OnlineJoin)].BoolValue = _nvxDevices[deviceIndex].IsOnline;
        _smartObject.StringInput[_srlHelper.SerialJoinFor(deviceIndex, ModeJoin)].StringValue = $"Mode: {_nvxDevices[deviceIndex].Control.DeviceMode.ToString()}";
    }

    private void Log(string message)
    {
        CrestronConsole.PrintLine($"{_name} - NVX Status - {message}");
    }
}