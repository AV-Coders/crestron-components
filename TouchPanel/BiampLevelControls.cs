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

    protected override void StartVolumeUp(int index)
    {
        string instanceTag = _audioBlocks[index].InstanceTag;
        void Action() => _dsp.LevelUp(instanceTag, _audioBlocks[index].BlockIndex, 2);
        VolumeControl(Action);
        Log($"Queued volume up on instance tag {instanceTag}");
    }

    protected override void StartVolumeDown(int index)
    {
        string instanceTag = _audioBlocks[index].InstanceTag;
        void Action() => _dsp.LevelDown(instanceTag, _audioBlocks[index].BlockIndex, 2);
        VolumeControl(Action);
        Log($"Queued volume down on instance tag {instanceTag}");
    }

    protected override void ToggleAudioMute(int index)
    {
        string instanceTag = _audioBlocks[index].InstanceTag;
        _dsp.ToggleAudioMute(instanceTag, _audioBlocks[index].BlockIndex);
        Log($"Toggled mute for instance tag {instanceTag}");
    }

    protected override void SetNewLevel(Sig sig)
    {
        var selectionInfo = SrlHelper.GetSigInfo(sig);
        _dsp.SetLevel(_audioBlocks[selectionInfo.Index].InstanceTag, _audioBlocks[selectionInfo.Index].BlockIndex,
            sig.ShortValue);
    }
}