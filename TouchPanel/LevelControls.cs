using AVCoders.Core;
using AVCoders.Crestron.SmartGraphics;

namespace AVCoders.Crestron.TouchPanel;

public abstract class LevelControls
{
    protected readonly List<SmartObject> SmartObjects;
    
    protected readonly SubpageReferenceListHelper SrlHelper;
    protected bool ButtonHeld;
    
    private readonly string _name;
    private bool _enableLogs;

    public const uint VolumeUpJoin = 1;
    public const uint VolumeDownJoin = 2;
    public const uint MuteJoin = 3;

    public const uint VolumeLevelJoin = 1;
    public const uint FaderTypeJoin = 2;

    public const uint NameJoin = 1;

    public const uint DefaultJoinIncrement = 10;
    protected readonly uint JoinIncrement;

    protected LevelControls(string name, ushort numberOfAudioBlocks, List<SmartObject> smartObjects, uint joinIncrement)
    {
        SmartObjects = smartObjects;
        JoinIncrement = joinIncrement;
        SrlHelper = new SubpageReferenceListHelper(JoinIncrement, JoinIncrement, JoinIncrement);
        _name = name;
        

        SmartObjects.ForEach(smartObject =>
        {
            smartObject.SigChange += HandleVolumePress;
            smartObject.UShortInput["Set Number of Items"].UShortValue = numberOfAudioBlocks;
        });
    }
    
    protected void VolumeControl(Action action)
    {
        ButtonHeld = true;
        new Thread(_ =>
        {
            while (ButtonHeld)
            {
                action.Invoke();
                Log("Volume command sent");
                Thread.Sleep(250);
            }
        }).Start();
    }

    protected void HandleVolumePress(GenericBase currentDevice, SmartObjectEventArgs args)
    {
        var selectionInfo = SrlHelper.GetSigInfo(args.Sig);
        switch (args.Sig.Type)
        {
            case eSigType.Bool when args.Sig.Number > 4000:
            {
                if (!args.Sig.BoolValue)
                {
                    ButtonHeld = false;
                    return;
                }

                switch (selectionInfo.Join)
                {
                    case VolumeUpJoin:
                        Log($"Queueing Volume up on fader index {selectionInfo.Index}");
                        StartVolumeUp(selectionInfo.Index);
                        break;
                    case VolumeDownJoin:
                        Log($"Queueing Volume down on fader index {selectionInfo.Index}");
                        StartVolumeDown(selectionInfo.Index);
                        break;
                    case MuteJoin:
                        
                        Log($"Toggling Volume mute for fader index {selectionInfo.Index}");
                        ToggleAudioMute(selectionInfo.Index);
                        break;
                    default:
                        Log($"Ignoring button press {args.Sig.Number}");
                        break;
                }
                return;
            }
            case eSigType.UShort when args.Sig.Number > 10:
                Log($"Analog sig, Number: {args.Sig.Number}");
                SetNewLevel(args.Sig);
                return;
            default:
                Log($"Ignoring Sig, Type: {args.Sig.Type.ToString()}, Number: {args.Sig.Number}");
                break;
        }
    }


    protected abstract void StartVolumeUp(int index);
    protected abstract void StartVolumeDown(int index);
    protected abstract void ToggleAudioMute(int index);
    protected abstract void SetNewLevel(Sig sig);

    protected void HandleMuteState(MuteState state, int faderIndex) => SmartObjects.ForEach(x =>
        x.BooleanInput[SrlHelper.BooleanJoinFor(faderIndex, MuteJoin)].BoolValue = state == MuteState.On);

    protected void HandleVolumeLevel(int volumeLevel, int faderIndex) => SmartObjects.ForEach(x =>
        x.UShortInput[SrlHelper.AnalogJoinFor(faderIndex, VolumeLevelJoin)].ShortValue = (short)volumeLevel);
    
    public void EnableLogs(bool enable) => _enableLogs = enable;

    protected void Log(string message)
    {
        if (_enableLogs)
            CrestronConsole.PrintLine($"{DateTime.Now} - {_name} - LevelControls - {message}");
    }
}