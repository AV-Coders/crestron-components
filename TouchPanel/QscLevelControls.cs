using AVCoders.Dsp;

namespace AVCoders.Crestron.TouchPanel;

public record QscAudioBlockWithSelectInfo(
    string Name,
    string LevelInstanceTag,
    string MuteInstanceTag,
    string SelectInstanceTag): QscAudioBlockInfo(Name, LevelInstanceTag, MuteInstanceTag);

public record QscAudioBlockInfo(string Name, string LevelInstanceTag, string MuteInstanceTag);

public class QscLevelControls : LevelControls
{
    // This is designed to work with QscSourceSelect if required. 
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

    protected override void StartVolumeUp(int index)
    {
        string instanceTag = _audioBlocks[index].LevelInstanceTag;
        void Action() => _dsp.LevelUp(instanceTag, 2);
        Log($"Queued volume up on instance tag {instanceTag}");
        VolumeControl(Action);

    }

    protected override void StartVolumeDown(int index)
    {
        string instanceTag = _audioBlocks[index].LevelInstanceTag;
        void Action() => _dsp.LevelDown(instanceTag, 2);
        VolumeControl(Action);
        Log($"Queued volume down on instance tag {instanceTag}");

    }

    protected override void ToggleAudioMute(int index)
    {
        string instanceTag = _audioBlocks[index].MuteInstanceTag;
        _dsp.ToggleAudioMute(instanceTag);
        Log($"Toggled mute for instance tag {instanceTag}");

    }

    protected override void SetNewLevel(Sig sig)
    {
        var selectionInfo = SrlHelper.GetSigInfo(sig);
        _dsp.SetLevel(_audioBlocks[selectionInfo.Index].LevelInstanceTag, sig.ShortValue);
    }
}