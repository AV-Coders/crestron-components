using AVCoders.Crestron.SmartGraphics;

namespace AVCoders.Crestron.TouchPanel;

public class CrestronStatus
{
    private readonly GenericDevice[] _crestronDevices;
    private readonly SmartObject _smartObject;
    private readonly SubpageReferenceListHelper _srlHelper;
    private readonly string _name;

    private const uint OnlineJoin = 1;

    private const uint NameJoin = 1;
    private const uint IpIdJoin = 2;

    public CrestronStatus(string name, GenericDevice[] crestronDevices, SmartObject smartObject)
    {
        _name = name;
        _crestronDevices = crestronDevices;
        _smartObject = smartObject;
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
        _smartObject.BooleanInput[_srlHelper.BooleanJoinFor(deviceIndex, OnlineJoin)].BoolValue = args.DeviceOnLine;
        
        if(args.DeviceOnLine)
            FeedbackForDevice(deviceIndex);
    }

    private void ConfigureSmartObject()
    {
        Log("Configuring modal buttons");
        _smartObject.UShortInput["Set Number of Items"].ShortValue = (short)_crestronDevices.Length;

        for (int i = 0; i < _crestronDevices.Length; i++)
        {
            FeedbackForDevice(i);
        }
    }
    
    private void FeedbackForDevice(int deviceIndex)
    {
        _smartObject.StringInput[_srlHelper.SerialJoinFor(deviceIndex, NameJoin)].StringValue = _crestronDevices[deviceIndex].Name;
        _smartObject.StringInput[_srlHelper.SerialJoinFor(deviceIndex, IpIdJoin)].StringValue = $"IP ID: {_crestronDevices[deviceIndex].ID:x2}";
        _smartObject.BooleanInput[_srlHelper.BooleanJoinFor(deviceIndex, OnlineJoin)].BoolValue = _crestronDevices[deviceIndex].IsOnline;
    }

    private void Log(string message)
    {
        CrestronConsole.PrintLine($"{DateTime.Now} - {_name} - Crestron Status - {message}");
    }
}