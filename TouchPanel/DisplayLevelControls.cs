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

    protected override void StartVolumeUp(int index)
    {
        void VolumeUp() => _displays[index].Display.SetVolume(_displays[index].Display.GetCurrentVolume() + 1);
        VolumeControl(VolumeUp);
    }

    protected override void StartVolumeDown(int index)
    {
        void VolumeDown() => _displays[index].Display.SetVolume(_displays[index].Display.GetCurrentVolume() - 1);
        VolumeControl(VolumeDown);
    }
    
    protected override void ToggleAudioMute(int index) => _displays[index].Display.ToggleAudioMute();

    protected override void SetNewLevel(Sig sig)
    {
        var selectionInfo = SrlHelper.GetSigInfo(sig);
        _displays[selectionInfo.Index].Display.SetVolume(Math.PercentageToRange(sig.UShortValue, _displays[selectionInfo.Index].MaxVolume));
    }

    private new void HandleVolumeLevel(int volumeLevel, int displayIndex)
    {
        Log($"Handling volume level {volumeLevel} for display {displayIndex}.");
        SmartObjects.ForEach(x => x.UShortInput[SrlHelper.AnalogJoinFor(displayIndex, VolumeLevelJoin)].UShortValue = Math.PercentageFromRange(volumeLevel, _displays[displayIndex].MaxVolume));
    }
}