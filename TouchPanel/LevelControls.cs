using AVCoders.Core;
using AVCoders.Crestron.SmartGraphics;
using Serilog;
using Serilog.Context;

namespace AVCoders.Crestron.TouchPanel;

public abstract class LevelControls : SrlPage
{
    
    protected readonly SubpageReferenceListHelper SrlHelper;
    protected bool ButtonHeld;

    public const uint VolumeUpJoin = 1;
    public const uint VolumeDownJoin = 2;
    public const uint MuteJoin = 3;

    public const uint VolumeLevelJoin = 1;
    public const uint FaderTypeJoin = 2;

    public const uint NameJoin = 1;

    public const uint DefaultJoinIncrement = 10;
    protected readonly uint JoinIncrement;

    protected LevelControls(string name, ushort numberOfAudioBlocks, List<SmartObject> smartObjects, uint joinIncrement) : base(name, smartObjects, joinIncrement)
    {
        JoinIncrement = joinIncrement;
        SrlHelper = new SubpageReferenceListHelper(JoinIncrement, JoinIncrement, JoinIncrement);
        

        smartObjects.ForEach(smartObject =>
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
                Debug("Volume command sent");
                Thread.Sleep(250);
            }
        }).Start();
    }

    private void HandleVolumePress(GenericBase currentDevice, SmartObjectEventArgs args)
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
                        Debug($"Queueing Volume up for fader index {selectionInfo.Index}");
                        StartVolumeUp(selectionInfo.Index);
                        break;
                    case VolumeDownJoin:
                        Debug($"Queueing Volume down for fader index {selectionInfo.Index}");
                        StartVolumeDown(selectionInfo.Index);
                        break;
                    case MuteJoin:
                        
                        Debug($"Toggling Volume mute for fader index {selectionInfo.Index}");
                        ToggleAudioMute(selectionInfo.Index);
                        break;
                    default:
                        Debug($"Join {selectionInfo.Join} for index {selectionInfo.Index} is not handled by this module");
                        break;
                }
                return;
            }
            case eSigType.UShort when args.Sig.Number > 10:
                Debug($"Analog sig, Number: {args.Sig.Number}");
                SetNewLevel(args.Sig);
                return;
            default:
                Debug($"Ignoring Sig, Type: {args.Sig.Type.ToString()}, Number: {args.Sig.Number}");
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
    
}