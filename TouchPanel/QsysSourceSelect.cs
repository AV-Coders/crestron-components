using AVCoders.Dsp;

namespace AVCoders.Crestron.TouchPanel;

public record QscSource(string Name, int InputNumber);

public class QsysSourceSelect : LevelControls
{
    // This is designed to work with Qsys Level control.  Digital joins 4-10 are used by this module
    private readonly List<QscSource> _sources;
    private readonly List<QscAudioBlockInfo> _audioBlocks;
    private readonly QsysEcp _dsp;
    
    public QsysSourceSelect(string name, List<QscAudioBlockInfo> audioBlocks, QsysEcp dsp,  List<SmartObject> smartObjects, List<QscSource> sources, uint joinIncrement = DefaultJoinIncrement) : 
        base(name, (ushort)audioBlocks.Count, smartObjects, joinIncrement)
    {
        _audioBlocks = audioBlocks;
        _dsp = dsp;
        _sources = sources;

        for (int i = 0; i < audioBlocks.Count; i++)
        {
            Log($"Setting up source select {i}");
            var faderIndex = i;
            if (_audioBlocks[faderIndex].SelectInstanceTag == string.Empty)
                throw new InvalidOperationException($"Audio block at index {faderIndex} does not have a select instance tag");
            
            _dsp.AddControl(selection => HandleSourceChange(selection, faderIndex),
                audioBlocks[faderIndex].SelectInstanceTag);
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

    // Created in the base class as the generic handler.  Actually used as selection in this module.
    protected override void HandleVolumePress(GenericBase currentDevice, SmartObjectEventArgs args)
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
        
        Log($"Source button pressed, id {args.Sig.Number}.  Index {sourceIndex}, Join: {joinInfo.Join}");
        string instanceTag = _audioBlocks[joinInfo.Index].SelectInstanceTag;
        string inputSelection = _sources[sourceIndex].InputNumber.ToString();
        Log($"Setting source for {instanceTag} to {inputSelection}");
        _dsp.SetValue(instanceTag, inputSelection);
    }
}