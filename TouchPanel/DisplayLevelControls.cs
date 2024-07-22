using AVCoders.Core;
using AVCoders.Crestron.SmartGraphics;

namespace AVCoders.Crestron.TouchPanel;

public class DisplayLevelControls : LevelControls
{
    private readonly List<DisplayInfo> _displays;

    public DisplayLevelControls(string name, List<DisplayInfo> displays, List<SmartObject> smartObjects, uint joinIncrement = DefaultJoinIncrement) :
        base(name, (ushort)displays.Count, smartObjects, joinIncrement)
    {
        _displays = displays;
        
        for (int i = 0; i < _displays.Count; i++)
        {
            Log($"Setting up fader {i}");
            var display = i;
            _displays[i].Display.VolumeLevelHandlers += level => HandleVolumeLevel(level, display);
            _displays[i].Display.MuteStateHandlers += muteState => HandleMuteState(muteState, display);
            SmartObjects.ForEach(smartObject => smartObject.StringInput[SrlHelper.SerialJoinFor(i, NameJoin)].StringValue = _displays[display].Name);
        }
    }

    protected override void HandleVolumePress(GenericBase currentDevice, SmartObjectEventArgs args)
    {
        var selectionInfo = SrlHelper.GetSigInfo(args.Sig);
        switch (args.Sig.Type)
        {
            case eSigType.Bool when args.Sig.BoolValue == false:
                ButtonHeld = false;
                return;
            case eSigType.Bool:
                Log($"Volume Button pressed, id {args.Sig.Number}");
                if (args.Sig.Number < 4000) // Some touch panels send a sig 1 event as well as the button press event.
                    return;

                if (selectionInfo.Join > 3)
                {
                    Log($"Ignoring button press {args.Sig.Number}");
                    return;
                }
            
                switch (selectionInfo.Join)
                {
                    case VolumeUpJoin:
                        void VolumeUp() => _displays[selectionInfo.Index].Display.SetVolume(_displays[selectionInfo.Index].Display.GetCurrentVolume() + 1);
                        VolumeControl(VolumeUp);
                        Log($"Queued volume up on display {selectionInfo.Index}");
                        break;
                    case VolumeDownJoin:
                        void VolumeDown() => _displays[selectionInfo.Index].Display.SetVolume(_displays[selectionInfo.Index].Display.GetCurrentVolume() - 1);
                        VolumeControl(VolumeDown);
                        Log($"Queued volume down on display {selectionInfo.Index}");
                        break;
                    case MuteJoin:
                        _displays[selectionInfo.Index].Display.ToggleAudioMute();
                        Log($"Toggled mute for display {selectionInfo.Index}");
                        break;
                }
                break;
            case eSigType.UShort when args.Sig.Number <= 10:
                return;
            case eSigType.UShort:
                _displays[selectionInfo.Index].Display.SetVolume(args.Sig.UShortValue);
                break;
        }
    }

    private new void HandleVolumeLevel(int volumeLevel, int displayIndex)
    {
        Log($"Handling volume level {volumeLevel} for display {displayIndex}.");
        SmartObjects.ForEach(x => x.UShortInput[SrlHelper.AnalogJoinFor(displayIndex, VolumeLevelJoin)].UShortValue = ScaleVolumeLevel(volumeLevel, _displays[displayIndex].MaxVolume));
    }

    private ushort ScaleVolumeLevel(int currentLevel, int maxLevel)
    {
        return (ushort)(currentLevel * 100 / maxLevel);
    }
}