using AVCoders.Core;
using AVCoders.Crestron.SmartGraphics;
using Serilog;
using Serilog.Context;

namespace AVCoders.Crestron.TouchPanel;

public class CrestronStatus : LogBase
{
    private readonly GenericDevice[] _crestronDevices;
    private readonly List<SmartObject> _smartObjects;
    private readonly SubpageReferenceListHelper _srlHelper;

    private const uint OnlineJoin = 1;

    private const uint ModelJoin = 1;
    private const uint IpIdJoin = 2;
    private const uint NameJoin = 3;

    public CrestronStatus(string name, GenericDevice[] crestronDevices, List<SmartObject> smartObjects) : base(name)
    {
        _crestronDevices = crestronDevices;
        _smartObjects = smartObjects;
        _srlHelper = new SubpageReferenceListHelper(10, 10, 10);
        ConfigureSmartObject();

        for (int i = 0; i < _crestronDevices.Length; i++)
        {
            var deviceIndex = i;
            _crestronDevices[i].OnlineStatusChange += (_, args) => HandleDeviceOnlineStatusChange(args, deviceIndex);
        }
    }

    private void HandleDeviceOnlineStatusChange(OnlineOfflineEventArgs args, int deviceIndex)
    {
        _smartObjects.ForEach(x => x.BooleanInput[_srlHelper.BooleanJoinFor(deviceIndex, OnlineJoin)].BoolValue = args.DeviceOnLine);
        
        if(args.DeviceOnLine)
            FeedbackForDevice(deviceIndex);
    }

    private void ConfigureSmartObject()
    {
        Debug("Configuring modal buttons");
        _smartObjects.ForEach(x => x.UShortInput["Set Number of Items"].ShortValue = (short)_crestronDevices.Length);

        for (int i = 0; i < _crestronDevices.Length; i++)
        {
            FeedbackForDevice(i);
        }
    }
    
    private void FeedbackForDevice(int deviceIndex)
    {
        _smartObjects.ForEach(x =>
        {
            x.StringInput[_srlHelper.SerialJoinFor(deviceIndex, NameJoin)].StringValue = _crestronDevices[deviceIndex].Description;
            x.StringInput[_srlHelper.SerialJoinFor(deviceIndex, IpIdJoin)].StringValue = $"IP ID: {_crestronDevices[deviceIndex].ID:x2}";
            x.StringInput[_srlHelper.SerialJoinFor(deviceIndex, ModelJoin)].StringValue = _crestronDevices[deviceIndex].Name;
            x.BooleanInput[_srlHelper.BooleanJoinFor(deviceIndex, OnlineJoin)].BoolValue = _crestronDevices[deviceIndex].IsOnline;
        });
    }
}