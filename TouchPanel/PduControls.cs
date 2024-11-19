using AVCoders.Core;
using AVCoders.Crestron.SmartGraphics;
using AVCoders.Power;

namespace AVCoders.Crestron.TouchPanel;

public class PduControls
{
    private readonly List<Outlet> _outlets;
    private readonly List<SmartObject> _smartObjects;
    private readonly Confirmation _confirmation;
    private readonly SubpageReferenceListHelper _srlHelper;
    private readonly string _name;
    private bool _enableLogs;

    public const uint PowerOnJoin = 1;
    public const uint PowerOffJoin = 2;
    public const uint PowerCycleJoin = 3;
    
    public const uint NameJoin = 1;

    public PduControls(string name, List<Outlet> outlets, List<SmartObject> smartObjects, Confirmation confirmation)
    {
        _name = name;
        _outlets = outlets;
        _srlHelper = new SubpageReferenceListHelper(10, 10, 10);
        _smartObjects = smartObjects;
        _confirmation = confirmation;
        UpdateOutletInfo();
    }

    public void HandleNewOutlets(List<Outlet> outlets)
    {
        _outlets.Clear();
        outlets.ForEach(x => _outlets.Add(x));
        UpdateOutletInfo();
        
    }

    private void UpdateOutletInfo()
    {
        _smartObjects.ForEach(x =>
        {
            x.UShortInput["Set Number of Items"].ShortValue = (short)_outlets.Count;
            x.SigChange += HandleOutletPress;
        });
        
        for (int i = 0; i < _outlets.Count; i++)
        {
            var deviceIndex = i;
            _outlets[deviceIndex].PowerStateHandlers += state => HandleOutletPowerState(deviceIndex, state);

            _smartObjects.ForEach(x =>
            {
                x.StringInput[_srlHelper.SerialJoinFor(deviceIndex, NameJoin)].StringValue = _outlets[deviceIndex].Name;
            });
        }
    }
    
    private void HandleOutletPress(GenericBase currentDevice, SmartObjectEventArgs args)
    {
        var selectionInfo = _srlHelper.GetSigInfo(args.Sig);
        Log($"Display Join, id {args.Sig.Number}. Type: {args.Sig.Type.ToString()} Index {selectionInfo.Index}, Join: {selectionInfo.Join}");
        
        switch (args.Sig.Type)
        {
            case eSigType.Bool when args.Sig.BoolValue:
                switch (selectionInfo.Join)
                {
                    case PowerOnJoin:
                        _outlets[selectionInfo.Index].PowerOn();
                        Log($"Turning on outlet {_outlets[selectionInfo.Index].Name}");
                        break;
                    case PowerOffJoin:
                        _confirmation.Prompt(
                            $"Are you sure you want to turn off the {_outlets[selectionInfo.Index].Name} outlet?",
                            new List<KeyValuePair<string, Action?>>
                            {
                                new ("Yes", _outlets[selectionInfo.Index].PowerOff),
                                new ("No", null)
                            }
                            );
                        Log($"Outlet power off requested for {_outlets[selectionInfo.Index].Name}");
                        break;
                    case PowerCycleJoin:
                        _confirmation.Prompt(
                            $"Are you sure you want to REBOOT the {_outlets[selectionInfo.Index].Name} outlet?",
                            new List<KeyValuePair<string, Action?>>
                            {
                                new ("Yes", _outlets[selectionInfo.Index].Reboot),
                                new ("No", null)
                            }
                            );
                        Log($"Outlet reboot requested for {_outlets[selectionInfo.Index].Name}");
                        break;
                    
                }
                break;
        }
    }
    
    private void HandleOutletPowerState(int deviceIndex, PowerState state)
    {
        _smartObjects.ForEach(smartObject =>
        {
            smartObject.BooleanInput[_srlHelper.BooleanJoinFor(deviceIndex, PowerOnJoin)].BoolValue = state == PowerState.On;
            smartObject.BooleanInput[_srlHelper.BooleanJoinFor(deviceIndex, PowerOffJoin)].BoolValue = state == PowerState.Off;
        });
        
    }

    public void EnableLogs(bool enable) => _enableLogs = enable;

    private void Log(string message)
    {
        if(_enableLogs)
            CrestronConsole.PrintLine($"{DateTime.Now} - {_name} - Display Menu - {message}");
    }
}