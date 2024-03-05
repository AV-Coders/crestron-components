﻿using AVCoders.Core;
using AVCoders.Crestron.SmartGraphics;
using AVCoders.Dsp;

namespace AVCoders.Crestron.TouchPanel;

public record QscAudioBlockInfo(string Name, string LevelInstanceTag, string MuteInstanceTag);

public class QscLevelControls : IVolumePage
{
    private readonly QscAudioBlockInfo[] _audioBlocks;
    private readonly QsysEcp _dsp;
    private readonly List<SmartObject> _smartObjects;
    private readonly SubpageReferenceListHelper _srlHelper;
    private readonly string _name;
    private bool _buttonHeld;
    private bool _enableLogs;

    public const uint VolumeUpJoin = 1;
    public const uint VolumeDownJoin = 2;
    public const uint MuteJoin = 3;

    public const uint VolumeLevelJoin = 1;

    public const uint NameJoin = 1;

    public static readonly uint JoinIncrement = 10;

    public QscLevelControls(string name, QscAudioBlockInfo[] audioBlocks, QsysEcp dsp, List<SmartObject> smartObjects)
    {
        _audioBlocks = audioBlocks;
        _dsp = dsp;
        _srlHelper = new SubpageReferenceListHelper(JoinIncrement, JoinIncrement, JoinIncrement);
        _smartObjects = smartObjects;
        _name = name;

        _smartObjects.ForEach(smartObject =>
        {
            smartObject.SigChange += HandleVolumePress;
            smartObject.UShortInput["Set Number of Items"].ShortValue = (short)_audioBlocks.Length;
        });

        ConfigureSmartObject();
    }

    private void HandleVolumePress(GenericBase currentDevice, SmartObjectEventArgs args)
    {
        var selectionInfo = _srlHelper.GetSigInfo(args.Sig);
        if (args.Sig.Type == eSigType.Bool)
        {
            if (args.Sig.BoolValue == false)
            {
                _buttonHeld = false;
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

    private void VolumeControl(Action action)
    {
        _buttonHeld = true;
        new Thread(_ =>
        {
            while (_buttonHeld)
            {
                action.Invoke();
                Log("Volume command sent");
                Thread.Sleep(250);
            }
        }).Start();
    }

    private void ConfigureSmartObject()
    {
        Log("Configuring smart object");


        for (int i = 0; i < _audioBlocks.Length; i++)
        {
            Log($"Setting up fader {i}");
            var faderIndex = i;
            _dsp.AddControl(volumeLevel => HandleVolumeLevel(volumeLevel, faderIndex),
                _audioBlocks[i].LevelInstanceTag);
            _dsp.AddControl(muteState => HandleMuteState(muteState, faderIndex), _audioBlocks[i].MuteInstanceTag);
            _smartObjects.ForEach(smartObject =>
                smartObject.StringInput[_srlHelper.SerialJoinFor(i, NameJoin)].StringValue = _audioBlocks[i].Name);
        }
    }

    private void HandleMuteState(MuteState state, int faderIndex) => _smartObjects.ForEach(x =>
        x.BooleanInput[_srlHelper.BooleanJoinFor(faderIndex, MuteJoin)].BoolValue = state == MuteState.On);

    private void HandleVolumeLevel(int volumeLevel, int faderIndex) => _smartObjects.ForEach(x =>
        x.UShortInput[_srlHelper.AnalogJoinFor(faderIndex, VolumeLevelJoin)].ShortValue = (short)volumeLevel);

    public void EnableLogs(bool enable) => _enableLogs = enable;

    private void Log(string message)
    {
        if (_enableLogs)
            CrestronConsole.PrintLine($"{DateTime.Now} - {_name} - QscLevelControls - {message}");
    }
}