using AVCoders.Dsp;

namespace AVCoders.Crestron.TouchPanel;

public record QscAudioBlockInfo(string Name, string LevelInstanceTag, string MuteInstanceTag);

public class QscLevelControls : LevelControls
{
    private readonly QscAudioBlockInfo[] _audioBlocks;
    private readonly QsysEcp _dsp;

    public QscLevelControls(string name, QscAudioBlockInfo[] audioBlocks, QsysEcp dsp, List<SmartObject> smartObjects) :
        base(name, (ushort)audioBlocks.Length, smartObjects)
    {
        _audioBlocks = audioBlocks;
        _dsp = dsp;

        SmartObjects.ForEach(smartObject =>
        {
            smartObject.SigChange += HandleVolumePress;
            smartObject.UShortInput["Set Number of Items"].ShortValue = (short)_audioBlocks.Length;
        });

        for (int i = 0; i < _audioBlocks.Length; i++)
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
            if (!args.Sig.BoolValue)
            {
                ButtonHeld = false;
                return;
            }

            Log(
                $"Volume Button pressed, id {args.Sig.Number}.  Index {selectionInfo.Index}, Join: {selectionInfo.Join}");
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