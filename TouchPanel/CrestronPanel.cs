using Crestron.SimplSharpPro.DeviceSupport;

namespace AVCoders.Crestron.TouchPanel;

public static class CrestronPanel
{
    public static void Interlock(BasicTriList panel, uint active, uint[] all)
    {
        foreach (uint join in all)
        {
            panel.BooleanInput[join].BoolValue = join == active;
        }
    }

    public static void Interlock(SmartObject smartObject, uint active, uint[] all)
    {
        foreach (uint join in all.Where(x => smartObject.BooleanInput.Contains(x)))
        {
            smartObject.BooleanInput[join].BoolValue = join == active;
        }
    }

    public static void Interlock(List<BasicTriListWithSmartObject> panels, uint active, uint[] all)
    {
        panels.ForEach(panel => Interlock(panel, active, all));
    }

    public static void Interlock(List<SmartObject> smartObjects, uint active, uint[] all)
    {
        smartObjects.ForEach(smartObject => Interlock(smartObject, active, all));
    }

    public static void Interlock(List<BasicTriList> panels , uint active, uint[] all)
    {
        panels.ForEach(panel => Interlock(panel, active, all));
    }

    public static void TogglingInterlock(BasicTriList panel, uint active, uint[] all)
    {
        foreach (uint join in all)
        {
            if (join == active)
                panel.BooleanInput[join].BoolValue = !panel.BooleanInput[join].BoolValue;
            else
                panel.BooleanInput[join].BoolValue = join == active;
        }
    }

    public static void TogglingInterlock(SmartObject smartObject, uint active, uint[] all)
    {
        foreach (uint join in all.Where(x => smartObject.BooleanInput.Contains(x)))
        {
            if (join == active)
                smartObject.BooleanInput[join].BoolValue = !smartObject.BooleanInput[join].BoolValue;
            else
                smartObject.BooleanInput[join].BoolValue = join == active;
        }
    }

    public static void TogglingInterlock(List<BasicTriList> panels, uint active, uint[] all)
    {
        uint newActive = panels.First().BooleanInput[active].BoolValue? 0 : active;
        panels.ForEach(x => Interlock(x, newActive, all));
    }

    public static void TogglingInterlock(List<BasicTriListWithSmartObject> panels, uint active, uint[] all)
    {
        uint newActive = panels.First().BooleanInput[active].BoolValue? 0 : active;
        panels.ForEach(x => Interlock(x, newActive, all));
    }

    public static bool EventIsAButtonPress(SigEventArgs args)
    {
        return args.Sig is { Type: eSigType.Bool, BoolValue: true };
    }

    public static bool EventIsAStringChange(SigEventArgs args)
    {
        return args.Sig is { Type: eSigType.String };
    }

    public static bool EventIsAnAnalogChange(SigEventArgs args)
    {
        return args.Sig is { Type: eSigType.UShort };
    }

    public static bool SigIsAButtonPress(Sig sig)
    {
        return sig is { Type: eSigType.Bool, BoolValue: true };
    }
    
    public static bool SigIsAStringChange(Sig sig)
    {
        return sig is { Type: eSigType.String };
    }
    
    public static bool SigIsAnAnalogChange(Sig sig)
    {
        return sig is { Type: eSigType.UShort };
    }
}