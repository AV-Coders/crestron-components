using AVCoders.Core;
using AVCoders.Crestron.SmartGraphics;
using AVCoders.Display;
using Serilog;

namespace AVCoders.Crestron.TouchPanel;

public class DisplayMenu : SrlPage
{
    private readonly List<DisplayInfo> _displays;
    private readonly string _name;

    public const uint PowerOnJoin = 1;
    public const uint PowerOffJoin = 2;
    public const uint Input1Join = 3;
    public const uint Input2Join = 4;
    public const uint Input3Join = 5;
    public const uint Input4Join = 6;
    public const uint Input1ShowJoin = 7;
    public const uint Input2ShowJoin = 8;
    public const uint Input3ShowJoin = 9;
    public const uint Input4ShowJoin = 10;
    public static readonly uint[] InputShowJoins = { Input1ShowJoin, Input2ShowJoin, Input3ShowJoin, Input4ShowJoin};

    public const uint MuteJoin = 30;

    public const uint VolumeJoin = 1;
    public const uint DriverStatusRedJoin = 2;
    public const uint DriverStatusGreenJoin = 3;
    public const uint DriverStatusBlueJoin = 4;
    public const uint CommsStatusRedJoin = 5;
    public const uint CommsStatusGreenJoin = 6;
    public const uint CommsStatusBlueJoin = 7;


    public const uint NameJoin = 1;

    public const uint Input1NameJoin = 3;
    public const uint Input2NameJoin = 4;
    public const uint Input3NameJoin = 5;
    public const uint Input4NameJoin = 6;
    public static readonly uint[] InputNameJoins = { Input1NameJoin, Input2NameJoin, Input3NameJoin, Input4NameJoin};

    public const uint DriverStatusLabelJoin = 11;
    public const uint CommsStatusLabelJoin = 12;
    
    public new static readonly uint DefaultJoinIncrement = 30;

    public DisplayMenu(string name, List<DisplayInfo> displays, List<SmartObject> smartObjects) : base(name, smartObjects, DefaultJoinIncrement)
    {
        _name = name;
        _displays = displays;
        SmartObjects.ForEach(x =>
        {
            x.UShortInput["Set Number of Items"].ShortValue = (short)_displays.Count;
            x.SigChange += HandleDisplayPress;
        });

        for (int i = 0; i < _displays.Count; i++)
        {
            var deviceIndex = i;
            FeedbackForDevice(deviceIndex);
            DriverStateFeedback(deviceIndex, _displays[deviceIndex].Display.CommunicationState);
            CommsStateFeedback(deviceIndex, _displays[deviceIndex].Display.CommunicationClient.ConnectionState);
            _displays[deviceIndex].Display.InputHandlers += _ => FeedbackForDevice(deviceIndex);
            _displays[deviceIndex].Display.PowerStateHandlers += _ => FeedbackForDevice(deviceIndex);
            _displays[deviceIndex].Display.VolumeLevelHandlers += volume => VolumeFeedback(deviceIndex, volume);
            _displays[deviceIndex].Display.MuteStateHandlers += _ => FeedbackForDevice(deviceIndex);
            _displays[deviceIndex].Display.CommunicationStateHandlers += state => DriverStateFeedback(deviceIndex, state);
            _displays[deviceIndex].Display.CommunicationClient.ConnectionStateHandlers += state => CommsStateFeedback(deviceIndex, state);

            for (int inputIndex = 0; inputIndex < _displays[deviceIndex].Inputs.Length; inputIndex++)
            {
                SmartObjects.ForEach(x =>
                {
                    x.BooleanInput[SrlHelper.BooleanJoinFor(deviceIndex, InputShowJoins[inputIndex])].BoolValue = true;
                    x.StringInput[SrlHelper.SerialJoinFor(deviceIndex, InputNameJoins[inputIndex])].StringValue = _displays[deviceIndex].Inputs[inputIndex].Name;
                });
            }
        }
    }

    private void HandleDisplayPress(GenericBase currentDevice, SmartObjectEventArgs args)
    {
        using (PushProperties("HandleDisplayPress"))
        {
            var selectionInfo = SrlHelper.GetSigInfo(args.Sig);
            Log.Debug("Display Join, id {SigNumber}. Type: {S} Index {SelectionInfoIndex}, Join: {SelectionInfoJoin}",
                args.Sig.Number, args.Sig.Type.ToString(), selectionInfo.Index, selectionInfo.Join);

            switch (args.Sig.Type)
            {
                case eSigType.Bool when args.Sig.BoolValue:
                    switch (selectionInfo.Join)
                    {
                        case PowerOnJoin:
                            _displays[selectionInfo.Index].Display.PowerOn();
                            Log.Debug("Turning on display {SelectionInfoIndex}", selectionInfo.Index);
                            break;
                        case PowerOffJoin:
                            _displays[selectionInfo.Index].Display.PowerOff();
                            Log.Debug("Turning off display {SelectionInfoIndex}", selectionInfo.Index);
                            break;
                        case MuteJoin:
                            _displays[selectionInfo.Index].Display.ToggleAudioMute();
                            break;
                        case Input1Join:
                        {
                            var input = _displays[selectionInfo.Index].Inputs[0].Input;
                            _displays[selectionInfo.Index].Display.SetInput(input);
                            Log.Debug("Turning setting display {SelectionInfoIndex} to {Input}", selectionInfo.Index,
                                input);
                            break;
                        }
                        case Input2Join:
                        {
                            var input = _displays[selectionInfo.Index].Inputs[1].Input;
                            _displays[selectionInfo.Index].Display.SetInput(input);
                            Log.Debug("Turning setting display {SelectionInfoIndex} to {Input}", selectionInfo.Index,
                                input);
                            break;
                        }
                        case Input3Join:
                        {
                            var input = _displays[selectionInfo.Index].Inputs[2].Input;
                            _displays[selectionInfo.Index].Display.SetInput(input);
                            Log.Debug("Turning setting display {SelectionInfoIndex} to {Input}", selectionInfo.Index,
                                input);
                            break;
                        }
                        case Input4Join:
                        {
                            var input = _displays[selectionInfo.Index].Inputs[3].Input;
                            _displays[selectionInfo.Index].Display.SetInput(input);
                            Log.Debug("Turning setting display {SelectionInfoIndex} to {Input}", selectionInfo.Index,
                                input);
                            break;
                        }
                    }

                    break;
                case eSigType.UShort when args.Sig.Number > 10:
                    _displays[selectionInfo.Index].Display.SetVolume(
                        Math.PercentageToRange(args.Sig.UShortValue, _displays[selectionInfo.Index].MaxVolume));
                    break;
            }
        }
    }

    private void VolumeFeedback(int deviceIndex, int volume)
    {
        SmartObjects.ForEach(smartObject =>
        {
            smartObject.UShortInput[SrlHelper.AnalogJoinFor(deviceIndex, VolumeJoin)].UShortValue =
                Math.PercentageFromRange(volume, _displays[deviceIndex].MaxVolume);
        });
    }

    private void DriverStateFeedback(int deviceIndex, CommunicationState state)
    {
        ushort redValue = 0;
        ushort greenValue = 0;
        ushort blueValue = 0;
        string driverText = String.Empty;

        switch (state)
        {
            case CommunicationState.Okay:
                greenValue = 199;
                blueValue = 129;
                driverText = "Driver: Okay";
                break;
            case CommunicationState.Error:
                redValue = 146;
                greenValue = 8;
                blueValue = 8;
                driverText = "Driver: Error";
                break;
            case CommunicationState.NotAttempted:
                redValue = 141;
                greenValue = 141;
                blueValue = 141;
                driverText = "Driver: Not attempted";
                break;
            case CommunicationState.Unknown:
                redValue = 0;
                greenValue = 0;
                blueValue = 0;
                driverText = "Driver: Unknown";
                break;
                
        }
        SmartObjects.ForEach(smartObject =>
        {
            smartObject.UShortInput[SrlHelper.AnalogJoinFor(deviceIndex, DriverStatusRedJoin)].UShortValue = redValue;
            smartObject.UShortInput[SrlHelper.AnalogJoinFor(deviceIndex, DriverStatusGreenJoin)].UShortValue = greenValue;
            smartObject.UShortInput[SrlHelper.AnalogJoinFor(deviceIndex, DriverStatusBlueJoin)].UShortValue = blueValue;
            smartObject.StringInput[SrlHelper.SerialJoinFor(deviceIndex, DriverStatusLabelJoin)].StringValue = driverText;
        });
    }

    private void CommsStateFeedback(int deviceIndex, ConnectionState state)
    {
        ushort redValue = 0;
        ushort greenValue = 0;
        ushort blueValue = 0;
        string commsText = String.Empty;

        switch (state)
        {
            case ConnectionState.Connected:
                greenValue = 199;
                blueValue = 129;
                commsText = "Comms: Connected";
                break;
            case ConnectionState.Error:
                redValue = 146;
                greenValue = 8;
                blueValue = 8;
                commsText = "Comms: Error";
                break;
            case ConnectionState.Disconnected:
                redValue = 166;
                greenValue = 127;
                blueValue = 0;
                commsText = "Comms: Disconnected";
                break;
            case ConnectionState.Connecting:
            case ConnectionState.Disconnecting:
                redValue = 5;
                greenValue = 112;
                blueValue = 192;
                commsText = "Comms: Busy";
                break;
            case ConnectionState.Idle:
                redValue = 141;
                greenValue = 141;
                blueValue = 141;
                commsText = "Comms: Idle";
                break;
                
        }
        SmartObjects.ForEach(smartObject =>
        {
            smartObject.UShortInput[SrlHelper.AnalogJoinFor(deviceIndex, CommsStatusRedJoin)].UShortValue = redValue;
            smartObject.UShortInput[SrlHelper.AnalogJoinFor(deviceIndex, CommsStatusGreenJoin)].UShortValue = greenValue;
            smartObject.UShortInput[SrlHelper.AnalogJoinFor(deviceIndex, CommsStatusBlueJoin)].UShortValue = blueValue;
            smartObject.StringInput[SrlHelper.SerialJoinFor(deviceIndex, CommsStatusLabelJoin)].StringValue = commsText;
        });
    }

    private void FeedbackForDevice(int deviceIndex)
    {
        SmartObjects.ForEach(smartObject =>
        {
            smartObject.StringInput[SrlHelper.SerialJoinFor(deviceIndex, NameJoin)].StringValue = _displays[deviceIndex].Display.Name;
            smartObject.BooleanInput[SrlHelper.BooleanJoinFor(deviceIndex, PowerOnJoin)].BoolValue =
                _displays[deviceIndex].Display.PowerState == PowerState.On;
            smartObject.BooleanInput[SrlHelper.BooleanJoinFor(deviceIndex, PowerOffJoin)].BoolValue =
                _displays[deviceIndex].Display.PowerState == PowerState.Off;
            smartObject.BooleanInput[SrlHelper.BooleanJoinFor(deviceIndex, MuteJoin)].BoolValue =
                _displays[deviceIndex].Display.AudioMute == MuteState.On;
            if (_displays[deviceIndex].Inputs.Length > 0)
                smartObject.BooleanInput[SrlHelper.BooleanJoinFor(deviceIndex, Input1Join)].BoolValue =
                    _displays[deviceIndex].Inputs[0].Input == _displays[deviceIndex].Display.Input;
            if (_displays[deviceIndex].Inputs.Length > 1)
                smartObject.BooleanInput[SrlHelper.BooleanJoinFor(deviceIndex, Input2Join)].BoolValue =
                    _displays[deviceIndex].Inputs[1].Input == _displays[deviceIndex].Display.Input;
            if (_displays[deviceIndex].Inputs.Length > 2)
                smartObject.BooleanInput[SrlHelper.BooleanJoinFor(deviceIndex, Input3Join)].BoolValue =
                    _displays[deviceIndex].Inputs[2].Input == _displays[deviceIndex].Display.Input;
            if (_displays[deviceIndex].Inputs.Length > 3)
                smartObject.BooleanInput[SrlHelper.BooleanJoinFor(deviceIndex, Input4Join)].BoolValue =
                    _displays[deviceIndex].Inputs[3].Input == _displays[deviceIndex].Display.Input;
        });
        
    }

    public override void PowerOn() { }

    public override void PowerOff() { }
}