using AVCoders.Dsp;

namespace AVCoders.Crestron.TouchPanel;

public record QscAudioBlockInfo(string Name, string LevelInstanceTag, string MuteInstanceTag, string SelectInstanceTag)
{
    public QscAudioBlockInfo(string Name, string LevelInstanceTag, string MuteInstanceTag) : 
        this(Name, LevelInstanceTag, MuteInstanceTag, string.Empty)
    {
    }
}

public class QscLevelControls : LevelControls
{
    // This is designed to work with QsysSourceSelect if required. 
    private readonly List<QscAudioBlockInfo> _audioBlocks;
    private readonly QsysEcp _dsp;

    public QscLevelControls(string name, List<QscAudioBlockInfo> audioBlocks, QsysEcp dsp, List<SmartObject> smartObjects, uint joinIncrement = DefaultJoinIncrement) :
        base(name, (ushort)audioBlocks.Count, smartObjects, joinIncrement)
    {
        _audioBlocks = audioBlocks;
        _dsp = dsp;

        for (int i = 0; i < _audioBlocks.Count; i++)
        {
            Log($"Setting up fader {i}");
            var faderIndex = i;
            _dsp.AddControl(volumeLevel => HandleVolumeLevel(volumeLevel, faderIndex),
                _audioBlocks[i].LevelInstanceTag);
            _dsp.AddControl(muteState => HandleMuteState(muteState, faderIndex), _audioBlocks[i].MuteInstanceTag);
            SmartObjects.ForEach(smartObject =>
                smartObject.StringInput[SrlHelper.SerialJoinFor(i, NameJoin)].StringValue = _audioBlocks[i].Name);
        }
    }

    protected override void HandleVolumePress(GenericBase currentDevice, SmartObjectEventArgs args)
    {
        var selectionInfo = SrlHelper.GetSigInfo(args.Sig);
        if (args.Sig.Type == eSigType.Bool)
        {
            if (args.Sig.Number < 4000) // Some touch panels send a sig 1 event as well as the button press event.
                return;
            
            if (!args.Sig.BoolValue)
            {
                ButtonHeld = false;
                return;
            }

            if (selectionInfo.Join > 3)
            {
                Log($"Ignoring button press {args.Sig.Number}");
                return;
            }

            Log($"Volume Button pressed, id {args.Sig.Number}.  Index {selectionInfo.Index}, Join: {selectionInfo.Join}");
            if (selectionInfo.Join == VolumeUpJoin)
            {
                string instanceTag = _audioBlocks[selectionInfo.Index].LevelInstanceTag;
                void Action() => _dsp.LevelUp(instanceTag, 2);
                VolumeControl(Action);
                Log($"Queued volume up on instance tag {instanceTag}");
            }
            else if (selectionInfo.Join == VolumeDownJoin)
            {
                string instanceTag = _audioBlocks[selectionInfo.Index].LevelInstanceTag;
                void Action() => _dsp.LevelDown(instanceTag, 2);
                VolumeControl(Action);
                Log($"Queued volume down on instance tag {instanceTag}");
            }
            else if (selectionInfo.Join == MuteJoin)
            {
                string instanceTag = _audioBlocks[selectionInfo.Index].MuteInstanceTag;
                _dsp.ToggleAudioMute(instanceTag);
                Log($"Toggled mute for instance tag {instanceTag}");
            }
        }
        else if (args.Sig.Type == eSigType.UShort)
        {
            _dsp.SetLevel(_audioBlocks[selectionInfo.Index].LevelInstanceTag, args.Sig.ShortValue);
        }
    }
}