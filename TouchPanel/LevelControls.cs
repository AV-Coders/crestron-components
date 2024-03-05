using AVCoders.Core;
using AVCoders.Crestron.SmartGraphics;

namespace AVCoders.Crestron.TouchPanel;

public abstract class LevelControls
{
    protected readonly List<SmartObject> SmartObjects;
    protected readonly SubpageReferenceListHelper SrlHelper;
    protected bool ButtonHeld;
    
    private readonly string _name;
    private bool _enableLogs;

    public const uint VolumeUpJoin = 1;
    public const uint VolumeDownJoin = 2;
    public const uint MuteJoin = 3;

    public const uint VolumeLevelJoin = 1;

    public const uint NameJoin = 1;

    public const uint JoinIncrement = 10;

    protected LevelControls(string name, ushort numberOfAudioBlocks, List<SmartObject> smartObjects)
    {
        SmartObjects = smartObjects;
        SrlHelper = new SubpageReferenceListHelper(JoinIncrement, JoinIncrement, JoinIncrement);
        _name = name;
        

        SmartObjects.ForEach(smartObject =>
        {
            smartObject.SigChange += HandleVolumePress;
            smartObject.UShortInput["Set Number of Items"].UShortValue = numberOfAudioBlocks;
        });
    }
    
    protected void VolumeControl(Action action)
    {
        ButtonHeld = true;
        new Thread(_ =>
        {
            while (ButtonHeld)
            {
                action.Invoke();
                Log("Volume command sent");
                Thread.Sleep(250);
            }
        }).Start();
    }

    protected abstract void HandleVolumePress(GenericBase currentDevice, SmartObjectEventArgs args);

    protected void HandleMuteState(MuteState state, int faderIndex) => SmartObjects.ForEach(x =>
        x.BooleanInput[SrlHelper.BooleanJoinFor(faderIndex, MuteJoin)].BoolValue = state == MuteState.On);

    protected void HandleVolumeLevel(int volumeLevel, int faderIndex) => SmartObjects.ForEach(x =>
        x.UShortInput[SrlHelper.AnalogJoinFor(faderIndex, VolumeLevelJoin)].ShortValue = (short)volumeLevel);
    
    public void EnableLogs(bool enable) => _enableLogs = enable;

    protected void Log(string message)
    {
        if (_enableLogs)
            CrestronConsole.PrintLine($"{DateTime.Now} - {_name} - QscLevelControls - {message}");
    }
}