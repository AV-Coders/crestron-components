using AVCoders.Core;
using AVCoders.Crestron.SmartGraphics;
using AVCoders.Power;

namespace AVCoders.Crestron.TouchPanel;

public class PduControls : SrlPage
{
    private readonly List<Outlet> _outlets;
    private readonly Confirmation _confirmation;

    public const uint PowerOnJoin = 1;
    public const uint PowerOffJoin = 2;
    public const uint RebootJoin = 3;
    
    public const uint NameJoin = 1;

    public PduControls(string name, List<Outlet> outlets, List<SmartObject> smartObjects, Confirmation confirmation) : base(name, smartObjects)
    {
        _outlets = outlets;
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
        SmartObjects.ForEach(x =>
        {
            x.UShortInput["Set Number of Items"].ShortValue = (short)_outlets.Count;
            x.SigChange += HandleOutletPress;
        });
        
        for (int i = 0; i < _outlets.Count; i++)
        {
            var deviceIndex = i;
            _outlets[deviceIndex].PowerStateHandlers += state => HandleOutletPowerState(deviceIndex, state);

            SmartObjects.ForEach(x =>
            {
                x.StringInput[_srlHelper.SerialJoinFor(deviceIndex, NameJoin)].StringValue = _outlets[deviceIndex].Name;
            });
        }
    }
    
    private void HandleOutletPress(GenericBase currentDevice, SmartObjectEventArgs args)
    {
        var selectionInfo = _srlHelper.GetSigInfo(args.Sig);
        Debug($"Display Join, id {args.Sig.Number}. Type: {args.Sig.Type.ToString()} Index {selectionInfo.Index}, Join: {selectionInfo.Join}");
        
        switch (args.Sig.Type)
        {
            case eSigType.Bool when args.Sig.BoolValue:
                switch (selectionInfo.Join)
                {
                    case PowerOnJoin:
                        _outlets[selectionInfo.Index].PowerOn();
                        Debug($"Turning on outlet {_outlets[selectionInfo.Index].Name}");
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
                        Debug($"Outlet power off requested for {_outlets[selectionInfo.Index].Name}");
                        break;
                    case RebootJoin:
                        _confirmation.Prompt(
                            $"Are you sure you want to REBOOT the {_outlets[selectionInfo.Index].Name} outlet?",
                            new List<KeyValuePair<string, Action?>>
                            {
                                new ("Yes", _outlets[selectionInfo.Index].Reboot),
                                new ("No", null)
                            }
                            );
                        Debug($"Outlet reboot requested for {_outlets[selectionInfo.Index].Name}");
                        break;
                    
                }
                break;
        }
    }
    
    private void HandleOutletPowerState(int deviceIndex, PowerState state)
    {
        SmartObjects.ForEach(smartObject =>
        {
            smartObject.BooleanInput[_srlHelper.BooleanJoinFor(deviceIndex, PowerOnJoin)].BoolValue = state == PowerState.On;
            smartObject.BooleanInput[_srlHelper.BooleanJoinFor(deviceIndex, PowerOffJoin)].BoolValue = state == PowerState.Off;
            smartObject.BooleanInput[_srlHelper.BooleanJoinFor(deviceIndex, RebootJoin)].BoolValue = state == PowerState.Rebooting;
        });
        
    }

    public override void PowerOn() { }

    public override void PowerOff() { }
}