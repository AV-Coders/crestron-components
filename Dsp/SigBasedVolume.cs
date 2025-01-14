using AVCoders.Core;
using AVCoders.Dsp;
using Crestron.SimplSharpPro;

namespace AVCoders.Crestron.DSP;

public class SigBasedVolume : Fader
{
    private readonly UShortInputSig _volumeControl;
    private readonly UShortOutputSig _volumeFeedback;
    private readonly ThreadWorker _pollWorker;

    public SigBasedVolume(UShortInputSig volumeControl, UShortOutputSig volumeFeedback, double minGain, double maxGain, VolumeLevelHandler volumeLevelHandler) :
        base(volumeLevelHandler, false)
    {
        _volumeControl = volumeControl;
        _volumeFeedback = volumeFeedback;
        _pollWorker = new ThreadWorker(CheckVolumeLevel, TimeSpan.FromMilliseconds(400));
        _pollWorker.Restart();
        SetMinGain(minGain);
        SetMaxGain(maxGain);
    }

    public void SetLevel(int percentage)
    {
        _volumeControl.ShortValue = (short) PercentageToDb(percentage);
    }

    private Task CheckVolumeLevel(CancellationToken token)
    {
        int oldVolume = Volume;
        SetVolumeFromDb(_volumeFeedback.ShortValue);
        if(oldVolume != Volume)
            Report();
        return Task.CompletedTask;
    }

    public void LevelUp(short amount = 1)
    {
        _volumeControl.ShortValue = (short)(_volumeFeedback.ShortValue + amount);
    }

    public void LevelDown(int amount = 1)
    {
        _volumeControl.ShortValue = (short)(_volumeFeedback.ShortValue - amount);
    }
}

public class SigBasedMute : Mute
{
    private readonly Action _muteOn;
    private readonly Action _muteOff;
    private readonly BoolOutputSig _muteFeedback;
    private readonly ThreadWorker _pollWorker;

    public SigBasedMute(Action muteOn, Action muteOff, BoolOutputSig muteFeedback, MuteStateHandler muteStateHandler) :
        base(muteStateHandler)
    {
        _muteOn = muteOn;
        _muteOff = muteOff;
        _muteFeedback = muteFeedback;
        _pollWorker = new ThreadWorker(CheckMuteState, TimeSpan.FromMilliseconds(3000));
        _pollWorker.Restart();
    }

    public void SetAudioMute(MuteState state)
    {
        switch (state)
        {
            case MuteState.On:
                _muteOn();
                break;
            case MuteState.Off:
                _muteOff();
                break;
        }
    }

    public void ToggleAudioMute()
    {
        MuteState desiredMute = MuteState switch
        {
            MuteState.On => MuteState.Off,
            MuteState.Off => MuteState.On,
            _ => MuteState.On
        };
        SetAudioMute(desiredMute);
    }

    private Task CheckMuteState(CancellationToken token)
    {
        var oldMuteState = MuteState;
        MuteState = _muteFeedback.BoolValue switch
        {
            true => MuteState.On,
            false => MuteState.Off
        };
        
        if(oldMuteState != MuteState)
            Report();
        return Task.CompletedTask;
    }
}