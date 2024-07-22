using AVCoders.Dsp;

namespace AVCoders.Crestron.TouchPanel;

public record BiampAudioBlockInfo(string Name, string InstanceTag, int BlockIndex);

public class BiampLevelControls : LevelControls
{
    private readonly List<BiampAudioBlockInfo> _audioBlocks;
    private readonly BiampTtp _dsp;

    public BiampLevelControls(string name, List<BiampAudioBlockInfo> audioBlocks, BiampTtp dsp, List<SmartObject> smartObjects, uint joinIncrement = DefaultJoinIncrement) :
        base(name, (ushort)audioBlocks.Count, smartObjects, joinIncrement)
    {
        _audioBlocks = audioBlocks;
        _dsp = dsp;

        for (int i = 0; i < _audioBlocks.Count; i++)
        {
            Log($"Setting up fader {i}");
            var faderIndex = i;
            _dsp.AddControl(volumeLevel => HandleVolumeLevel(volumeLevel, faderIndex), _audioBlocks[i].InstanceTag,
                _audioBlocks[i].BlockIndex);
            _dsp.AddControl(muteState => HandleMuteState(muteState, faderIndex), _audioBlocks[i].InstanceTag,
                _audioBlocks[i].BlockIndex);
            SmartObjects.ForEach(x =>
                x.StringInput[SrlHelper.SerialJoinFor(i, NameJoin)].StringValue = _audioBlocks[i].Name);
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

            Log(
                $"Volume Button pressed, id {args.Sig.Number}.  Index {selectionInfo.Index}, Join: {selectionInfo.Join}");
            if (selectionInfo.Join == VolumeUpJoin)
            {
                string instanceTag = _audioBlocks[selectionInfo.Index].InstanceTag;
                void Action() => _dsp.LevelUp(instanceTag, _audioBlocks[selectionInfo.Index].BlockIndex, 2);
                VolumeControl(Action);
                Log($"Queued volume up on instance tag {instanceTag}");
            }
            else if (selectionInfo.Join == VolumeDownJoin)
            {
                string instanceTag = _audioBlocks[selectionInfo.Index].InstanceTag;
                void Action() => _dsp.LevelDown(instanceTag, _audioBlocks[selectionInfo.Index].BlockIndex, 2);
                VolumeControl(Action);
                Log($"Queued volume down on instance tag {instanceTag}");
            }
            else if (selectionInfo.Join == MuteJoin)
            {
                string instanceTag = _audioBlocks[selectionInfo.Index].InstanceTag;
                _dsp.ToggleAudioMute(instanceTag, _audioBlocks[selectionInfo.Index].BlockIndex);
                Log($"Toggled mute for instance tag {instanceTag}");
            }
        }
        else if (args.Sig.Type == eSigType.UShort)
        {
            _dsp.SetLevel(_audioBlocks[selectionInfo.Index].InstanceTag, _audioBlocks[selectionInfo.Index].BlockIndex,
                args.Sig.ShortValue);
        }
    }
}