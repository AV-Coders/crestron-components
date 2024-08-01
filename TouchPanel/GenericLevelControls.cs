using AVCoders.Dsp;

namespace AVCoders.Crestron.TouchPanel;

public class GenericLevelControls : LevelControls
{
    private readonly List<UIFader> _faders;

    public GenericLevelControls(string name, List<UIFader> faders, List<SmartObject> smartObjects, uint joinIncrement = DefaultJoinIncrement) :
        base(name, (ushort)faders.Count, smartObjects, joinIncrement)
    {
        _faders = faders;

        for (int i = 0; i < faders.Count; i++)
        {
            Log($"Setting up fader {i}");
            var faderIndex = i;
            _faders[i].VolumeLevelHandlers += volumeLevel => HandleVolumeLevel(volumeLevel, faderIndex); 
            _faders[i].MuteStateHandlers += muteState => HandleMuteState(muteState, faderIndex);
            SmartObjects.ForEach(smartObject => smartObject.StringInput[SrlHelper.SerialJoinFor(i, NameJoin)].StringValue = faders[i].Name);
        }
    }

    protected override void HandleVolumePress(GenericBase currentDevice, SmartObjectEventArgs args)
    {
        var selectionInfo = SrlHelper.GetSigInfo(args.Sig);
        switch (args.Sig.Type)
        {
            // Some touch panels send a sig 1 event as well as the button press event.
            case eSigType.Bool when args.Sig.Number < 4000:
                return;
            case eSigType.Bool when !args.Sig.BoolValue:
                ButtonHeld = false;
                return;
            case eSigType.Bool when selectionInfo.Join > 3:
                Log($"Ignoring button press {args.Sig.Number}");
                return;
            case eSigType.Bool:
            {
                Log($"Volume Button pressed, id {args.Sig.Number}.  Index {selectionInfo.Index}, Join: {selectionInfo.Join}");
                switch (selectionInfo.Join)
                {
                    case VolumeUpJoin:
                    {
                        void Action() => _faders[selectionInfo.Index].LevelUp(2);
                        VolumeControl(Action);
                        Log($"Queued volume up for fader {_faders[selectionInfo.Index].Name}");
                        break;
                    }
                    case VolumeDownJoin:
                    {
                        void Action() => _faders[selectionInfo.Index].LevelDown(2);
                        VolumeControl(Action);
                        Log($"Queued volume down for fader {_faders[selectionInfo.Index].Name}");
                        break;
                    }
                    case MuteJoin:
                        _faders[selectionInfo.Index].ToggleAudioMute();
                        Log($"Toggled mute for fader {_faders[selectionInfo.Index].Name}");
                        break;
                }
                break;
            }
            case eSigType.UShort:
                _faders[selectionInfo.Index].SetLevel(args.Sig.ShortValue);
                break;
        }
    }
}