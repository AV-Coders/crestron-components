using AVCoders.Core;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.UC;

namespace AVCoders.Crestron.DSP;

public class UcEngineMtrSpeakerVolume : VolumeControl
{
    private readonly UcEngineAudioRsvdSigs _audioBlock;

    public UcEngineMtrSpeakerVolume(string name, UcEngineAudioRsvdSigs audioBlock) : base(name, VolumeType.Speaker)
    {
        _audioBlock = audioBlock;
        _audioBlock.DeviceExtenderSigChange += HandleTeamsLevelChange;
    }

    public override void LevelUp(int amount)
    {
        _audioBlock.ConferenceSpeakerVolume.UShortValue = (ushort) (_audioBlock.ConferenceSpeakerVolumeFeedback.UShortValue + (amount * 655.35));
    }

    public override void LevelDown(int amount)
    {
        _audioBlock.ConferenceSpeakerVolume.UShortValue = (ushort) (_audioBlock.ConferenceSpeakerVolumeFeedback.UShortValue - (amount * 655.35));
    }

    public override void SetLevel(int percentage)
    {
        _audioBlock.ConferenceSpeakerVolume.UShortValue = (ushort)(percentage * 655.35);
    }

    public override void ToggleAudioMute() { }

    public override void SetAudioMute(MuteState state) { }

    private void HandleTeamsLevelChange(DeviceExtender currentDeviceExtender, SigEventArgs args)
    {
        if (args.Sig.Number != _audioBlock.ConferenceSpeakerVolumeFeedback.Number)
            return;
        VolumeLevelHandlers?.Invoke((int)(args.Sig.UShortValue/655.35));
    }
}