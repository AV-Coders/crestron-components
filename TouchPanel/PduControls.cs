using AVCoders.Core;
using AVCoders.Crestron.SmartGraphics;
using AVCoders.Power;

namespace AVCoders.Crestron.TouchPanel;

public class PduControls : SrlPage
{
    private readonly List<Outlet> _allOutlets;
    private readonly Confirmation _confirmation;
    private readonly Dictionary<string, List<Outlet>> _pduOutlets = new ();

    public const uint PowerOnJoin = 1;
    public const uint PowerOffJoin = 2;
    public const uint RebootJoin = 3;
    
    public const uint NameJoin = 1;

    public PduControls(string name, List<Outlet> allOutlets, List<SmartObject> smartObjects, Confirmation confirmation) : base(name, smartObjects)
    {
        _allOutlets = allOutlets;
        _confirmation = confirmation;
        UpdateOutletInfo();
    }

    public void HandleNewOutlets(List<Outlet> outlets) => CreateMasterOutletList(outlets);

    public void HandleNewOutlets(string pduName, List<Outlet> outlets)
    {
        _pduOutlets[pduName] = outlets;
        
        var allOutlets = _pduOutlets.Values.SelectMany(x => x).ToList();
        CreateMasterOutletList(allOutlets);
    }

    private void CreateMasterOutletList(List<Outlet> outlets)
    {
        _allOutlets.Clear();
        outlets.ForEach(x => _allOutlets.Add(x));
        UpdateOutletInfo();
    }

    private void UpdateOutletInfo()
    {
        SmartObjects.ForEach(x =>
        {
            x.UShortInput["Set Number of Items"].ShortValue = (short)_allOutlets.Count;
            x.SigChange += HandleOutletPress;
        });
        
        for (int i = 0; i < _allOutlets.Count; i++)
        {
            var deviceIndex = i;
            _allOutlets[deviceIndex].PowerStateHandlers += state => HandleOutletPowerState(deviceIndex, state);

            SmartObjects.ForEach(x =>
            {
                x.StringInput[SrlHelper.SerialJoinFor(deviceIndex, NameJoin)].StringValue = _allOutlets[deviceIndex].Name;
            });
        }
    }
    
    private void HandleOutletPress(GenericBase currentDevice, SmartObjectEventArgs args)
    {
        var selectionInfo = SrlHelper.GetSigInfo(args.Sig);
        Debug($"Display Join, id {args.Sig.Number}. Type: {args.Sig.Type.ToString()} Index {selectionInfo.Index}, Join: {selectionInfo.Join}");
        
        switch (args.Sig.Type)
        {
            case eSigType.Bool when args.Sig.BoolValue:
                switch (selectionInfo.Join)
                {
                    case PowerOnJoin:
                        _allOutlets[selectionInfo.Index].PowerOn();
                        Debug($"Turning on outlet {_allOutlets[selectionInfo.Index].Name}");
                        break;
                    case PowerOffJoin:
                        _confirmation.Prompt(
                            $"Are you sure you want to turn off the {_allOutlets[selectionInfo.Index].Name} outlet?",
                            new List<KeyValuePair<string, Action?>>
                            {
                                new ("Yes", _allOutlets[selectionInfo.Index].PowerOff),
                                new ("No", null)
                            }
                            );
                        Debug($"Outlet power off requested for {_allOutlets[selectionInfo.Index].Name}");
                        break;
                    case RebootJoin:
                        _confirmation.Prompt(
                            $"Are you sure you want to REBOOT the {_allOutlets[selectionInfo.Index].Name} outlet?",
                            new List<KeyValuePair<string, Action?>>
                            {
                                new ("Yes", _allOutlets[selectionInfo.Index].Reboot),
                                new ("No", null)
                            }
                            );
                        Debug($"Outlet reboot requested for {_allOutlets[selectionInfo.Index].Name}");
                        break;
                    
                }
                break;
        }
    }
    
    private void HandleOutletPowerState(int deviceIndex, PowerState state)
    {
        SmartObjects.ForEach(smartObject =>
        {
            smartObject.BooleanInput[SrlHelper.BooleanJoinFor(deviceIndex, PowerOnJoin)].BoolValue = state == PowerState.On;
            smartObject.BooleanInput[SrlHelper.BooleanJoinFor(deviceIndex, PowerOffJoin)].BoolValue = state == PowerState.Off;
            smartObject.BooleanInput[SrlHelper.BooleanJoinFor(deviceIndex, RebootJoin)].BoolValue = state == PowerState.Rebooting;
        });
        
    }

    public override void PowerOn() { }

    public override void PowerOff() { }
}