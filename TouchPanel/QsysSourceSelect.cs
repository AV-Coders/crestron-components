using AVCoders.Dsp;

namespace AVCoders.Crestron.TouchPanel;

public record QscSource(string Name, int InputNumber);

public class QsysSourceSelect : LevelControls
{
    // This is designed to work with QscLevelControl.  Digital joins 4-10 are used by this module
    private readonly List<QscSource> _sources;
    private readonly List<QscAudioBlockWithSelectInfo> _audioBlocks;
    private readonly QsysEcp _dsp;
    
    public QsysSourceSelect(string name, List<QscAudioBlockWithSelectInfo> audioBlocks, QsysEcp dsp,  List<SmartObject> smartObjects, List<QscSource> sources, uint joinIncrement = DefaultJoinIncrement) : 
        base(name, (ushort)audioBlocks.Count, smartObjects, joinIncrement)
    {
        _audioBlocks = audioBlocks;
        _dsp = dsp;
        _sources = sources;

        for (int i = 0; i < audioBlocks.Count; i++)
        {
            Debug($"Setting up source select {i}");
            var faderIndex = i;
            if (_audioBlocks[faderIndex].SelectInstanceTag == string.Empty)
                throw new InvalidOperationException($"Audio block at index {faderIndex} does not have a select instance tag");
            
            _dsp.AddControl(selection => HandleSourceChange(selection, faderIndex),
                audioBlocks[faderIndex].SelectInstanceTag);
            SmartObjects.ForEach(smartObject => smartObject.SigChange += HandleSourceSelect);
            for (uint sourceIndex = 0; sourceIndex < sources.Count; sourceIndex++)
            {
                smartObjects.ForEach(smartObject =>
                {
                    smartObject.StringInput[SrlHelper.SerialJoinFor(faderIndex, sourceIndex + 4)].StringValue =
                        sources[(int)sourceIndex].Name;
                });
            }
        }
    }

    private void HandleSourceChange(string selection, int faderIndex)
    {
        var intVersion = Int32.Parse(selection);
        var sourceIndex = _sources.FindIndex(x => x.InputNumber == intVersion);
        if (sourceIndex < 0)
            return;
        for (uint i = 4; i < JoinIncrement; i++)
        {
            SmartObjects.ForEach(x => x.BooleanInput[SrlHelper.BooleanJoinFor(faderIndex, i)].BoolValue = sourceIndex == (i-4));
        }
    }
    
    private void HandleSourceSelect(GenericBase currentDevice, SmartObjectEventArgs args)
    {
        if (args.Sig.Type != eSigType.Bool)
            return;
        if (!args.Sig.BoolValue)
            return;
        if (args.Sig.Number < 4000) // Some touch panels send a sig 1 event as well as the button press event.
            return;
        var joinInfo = SrlHelper.GetBooleanSigInfo(args.Sig.Number);
        int sourceIndex = (int) joinInfo.Join - 4;
        if (sourceIndex > _sources.Count)
            return;
        
        Debug($"Source button pressed, id {args.Sig.Number}.  Index {sourceIndex}, Join: {joinInfo.Join}");
        string instanceTag = _audioBlocks[joinInfo.Index].SelectInstanceTag;
        string inputSelection = _sources[sourceIndex].InputNumber.ToString();
        Debug($"Setting source for {instanceTag} to {inputSelection}");
        _dsp.SetValue(instanceTag, inputSelection);
    }

    protected override void StartVolumeUp(int index) {}

    protected override void StartVolumeDown(int index) {}

    protected override void ToggleAudioMute(int index) {}

    protected override void SetNewLevel(Sig sig){}
}