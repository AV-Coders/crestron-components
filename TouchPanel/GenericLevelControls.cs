using AVCoders.Core;
using Serilog;
using Serilog.Context;

namespace AVCoders.Crestron.TouchPanel;

public class GenericLevelControls : LevelControls
{
    private readonly List<VolumeControl> _faders;

    public GenericLevelControls(string name, List<VolumeControl> faders, List<SmartObject> smartObjects, uint joinIncrement = DefaultJoinIncrement) :
        base(name, (ushort)faders.Count, smartObjects, joinIncrement)
    {
        using (PushProperties("Constructor"))
        {
            _faders = faders;

            for (int i = 0; i < faders.Count; i++)
            {
                Log.Debug($"Setting up fader {i}");
                var faderIndex = i;
                _faders[i].VolumeLevelHandlers += volumeLevel => HandleVolumeLevel(volumeLevel, faderIndex);
                _faders[i].MuteStateHandlers += muteState => HandleMuteState(muteState, faderIndex);
                SmartObjects.ForEach(smartObject =>
                {
                    smartObject.StringInput[SrlHelper.SerialJoinFor(i, NameJoin)].StringValue = faders[i].Name;
                    smartObject.UShortInput[SrlHelper.AnalogJoinFor(i, FaderTypeJoin)].UShortValue =
                        (ushort)faders[i].Type;
                });
                HandleVolumeLevel(_faders[i].Volume, i);
            }
        }
    }

    protected override void StartVolumeUp(int index)
    {
        using (PushProperties("StartVolumeUp"))
        {
            void Action() => _faders[index].LevelUp(2);
            VolumeControl(Action);
            Log.Debug("Queued volume up for fader {S}", _faders[index].Name);
        }
    }

    protected override void StartVolumeDown(int index)
    {
        using (PushProperties("StartVolumeDown"))
        {
            void Action() => _faders[index].LevelDown(2);
            VolumeControl(Action);
            Log.Debug("Queued volume down for fader {S}", _faders[index].Name);
        }
    }

    protected override void ToggleAudioMute(int index)
    {
        using (PushProperties("ToggleAudioMute"))
        {
            _faders[index].ToggleAudioMute();
            Debug($"Toggled mute for fader {_faders[index].Name}");
        }
    }

    protected override void SetNewLevel(Sig sig)
    {
        var selectionInfo = SrlHelper.GetSigInfo(sig);
        _faders[selectionInfo.Index].SetLevel(sig.ShortValue);
    }

    public override void PowerOn() { }

    public override void PowerOff() { }
}