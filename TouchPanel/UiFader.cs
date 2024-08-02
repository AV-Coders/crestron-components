using AVCoders.Core;
using AVCoders.Dsp;
using AVCoders.Display;

namespace AVCoders.Crestron.TouchPanel;

public enum FaderType : ushort
{
    Speaker = 0,
    Microphone = 1,
    LineIn = 2
}

public abstract class UIFader
{
    public readonly string Name;
    public readonly FaderType Type;
    public VolumeLevelHandler? VolumeLevelHandlers;
    public MuteStateHandler? MuteStateHandlers;

    protected UIFader(string name, FaderType type)
    {
        Name = name;
        Type = type;
    }

    public abstract void LevelUp(int amount);

    public abstract void LevelDown(int amount);

    public abstract void SetLevel(int percentage);

    public abstract void ToggleAudioMute();

    public abstract void SetAudioMute(MuteState state);
}

public class QscFader : UIFader
{
    private readonly string _levelNamedControl;
    private readonly string _muteNamedControl;
    private readonly QsysEcp _dsp;

    public QscFader(QscAudioBlockInfo audioBlockInfo, FaderType type, QsysEcp dsp) : base(audioBlockInfo.Name, type)
    {
        _dsp = dsp;
        _levelNamedControl = audioBlockInfo.LevelInstanceTag;
        _muteNamedControl = audioBlockInfo.MuteInstanceTag;
        _dsp.AddControl(volumeLevel => VolumeLevelHandlers?.Invoke(volumeLevel), _levelNamedControl);
        _dsp.AddControl(muteState => MuteStateHandlers?.Invoke(muteState), _muteNamedControl);
    }
    
    public override void LevelUp(int amount) => _dsp.LevelUp(_levelNamedControl, amount);

    public override void LevelDown(int amount) => _dsp.LevelDown(_levelNamedControl, amount);

    public override void SetLevel(int percentage) => _dsp.SetLevel(_levelNamedControl, percentage);

    public override void ToggleAudioMute() => _dsp.ToggleAudioMute(_muteNamedControl);
    
    public override void SetAudioMute(MuteState state) => _dsp.SetAudioMute(_muteNamedControl, state);
}

public class BiampFader : UIFader
{
    private readonly string _instanceTag;
    private readonly int _index;
    private readonly BiampTtp _dsp;

    public BiampFader(BiampAudioBlockInfo audioBlockInfo, FaderType type, BiampTtp dsp) : base(audioBlockInfo.Name, type)
    {
        _dsp = dsp;
        _instanceTag = audioBlockInfo.InstanceTag;
        _index = audioBlockInfo.BlockIndex;
        
        _dsp.AddControl(volumeLevel => VolumeLevelHandlers?.Invoke(volumeLevel), _instanceTag, _index);
        _dsp.AddControl(muteState => MuteStateHandlers?.Invoke(muteState), _instanceTag, _index);
    }
    public override void LevelUp(int amount) => _dsp.LevelUp(_instanceTag, _index, amount);

    public override void LevelDown(int amount) => _dsp.LevelDown(_instanceTag, _index, amount);

    public override void SetLevel(int percentage) => _dsp.SetLevel(_instanceTag, _index, percentage);

    public override void ToggleAudioMute() => _dsp.ToggleAudioMute(_instanceTag, _index);
    
    public override void SetAudioMute(MuteState state) => _dsp.SetAudioMute(_instanceTag, state);
}

public class DisplayFader : UIFader
{
    private readonly Display.Display _display;

    public DisplayFader(DisplayInfo displayInfo, FaderType type = FaderType.Speaker) : base(displayInfo.Name, type)
    {
        _display = displayInfo.Display;
        _display.VolumeLevelHandlers += volumeLevel => VolumeLevelHandlers?.Invoke(volumeLevel);
        _display.MuteStateHandlers += muteState => MuteStateHandlers?.Invoke(muteState);
    }

    public override void LevelUp(int amount) => _display.SetVolume(_display.GetCurrentVolume() + amount);

    public override void LevelDown(int amount) => _display.SetVolume(_display.GetCurrentVolume() - amount);

    public override void SetLevel(int percentage) => _display.SetVolume(percentage);

    public override void ToggleAudioMute() => _display.ToggleAudioMute();
    
    public override void SetAudioMute(MuteState state) => _display.SetAudioMute(state);
}