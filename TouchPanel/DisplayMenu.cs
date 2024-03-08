using AVCoders.Core;
using AVCoders.Crestron.SmartGraphics;
using AVCoders.Display;

namespace AVCoders.Crestron.TouchPanel;

public record InputInfo(string Name, Input Input);

public record DisplayInfo(Display.Display Display, string Name, InputInfo[] Inputs, int MaxVolume = 100);

public static class DisplayPageButtonJoins
{
    public static readonly uint[][] InputShowButtons =
    { 
        new uint[] { 4017, 4047, 4077, 4107, 4137, 4167, 4197, 4127, 4157 }, // Input 1
        new uint[] { 4018, 4048, 4078, 4108, 4138, 4168, 4198, 4128, 4158 }, 
        new uint[] { 4019, 4049, 4079, 4109, 4139, 4169, 4199, 4129, 4159 }, 
        new uint[] { 4020, 4050, 4080, 4110, 4140, 4170, 4200, 4230, 4260 }  // Input 4
    };

    public static readonly uint[][] InputLabels =
    {
        new uint[] { 13, 43, 73, 103, 133, 163, 193, 223, 253 }, // Input 1 
        new uint[] { 14, 44, 74, 104, 134, 164, 194, 224, 254 }, 
        new uint[] { 15, 45, 75, 105, 135, 165, 195, 225, 255 }, 
        new uint[] { 16, 46, 76, 106, 136, 166, 196, 226, 256 } // Input 4
    };
}

public class DisplayMenu
{
    private readonly List<DisplayInfo> _displays;
    private readonly List<SmartObject> _smartObjects;
    private readonly SubpageReferenceListHelper _srlHelper;
    private readonly string _name;
    private bool _enableLogs;

    public const uint PowerOnJoin = 1;
    public const uint PowerOffJoin = 2;
    public const uint Input1Join = 3;
    public const uint Input2Join = 4;
    public const uint Input3Join = 5;
    public const uint Input4Join = 6;

    public const uint NameJoin = 1;
    
    public static readonly uint JoinIncrement = 30;

    public DisplayMenu(string name, List<DisplayInfo> displays, List<SmartObject> smartObjects)
    {
        _name = name;
        _displays = displays;
        _srlHelper = new SubpageReferenceListHelper(JoinIncrement, JoinIncrement, JoinIncrement);
        _smartObjects = smartObjects;
        _smartObjects.ForEach(x => x.UShortInput["Set Number of Items"].ShortValue = (short)_displays.Count);
        _smartObjects.ForEach(x => x.SigChange += HandleDisplayPress);

        for (int i = 0; i < _displays.Count; i++)
        {
            var deviceIndex = i;
            FeedbackForDevice(deviceIndex);
            _displays[deviceIndex].Display.InputHandlers += _ => FeedbackForDevice(deviceIndex);
            _displays[deviceIndex].Display.PowerStateHandlers += _ => FeedbackForDevice(deviceIndex);
            _displays[deviceIndex].Display.VolumeLevelHandlers += _ => FeedbackForDevice(deviceIndex);

            for (int inputIndex = 0; inputIndex < _displays[deviceIndex].Inputs.Length; inputIndex++)
            {
                _smartObjects.ForEach(x =>
                {
                    x.BooleanInput[DisplayPageButtonJoins.InputShowButtons[inputIndex][deviceIndex]].BoolValue = true;
                    x.StringInput[DisplayPageButtonJoins.InputLabels[inputIndex][deviceIndex]].StringValue = _displays[deviceIndex].Inputs[inputIndex].Name;
                });
            }
        }
    }

    private void HandleDisplayPress(GenericBase currentDevice, SmartObjectEventArgs args)
    {
        if (args.Sig.Type != eSigType.Bool)
            return;
        if (!args.Sig.BoolValue)
            return;
        var selectionInfo = _srlHelper.GetBooleanSigInfo(args.Sig.Number);
        Log($"Volume Button pressed, id {args.Sig.Number}.  Index {selectionInfo.Index}, Join: {selectionInfo.Join}");

        if (selectionInfo.Join == PowerOnJoin)
        {
            _displays[selectionInfo.Index].Display.PowerOn();
            Log($"Turning on display {selectionInfo.Index}");
        }
        else if (selectionInfo.Join == PowerOffJoin)
        {
            _displays[selectionInfo.Index].Display.PowerOff();
            Log($"Turning off display {selectionInfo.Index}");
        }
        else if (selectionInfo.Join == Input1Join)
        {
            var input = _displays[selectionInfo.Index].Inputs[0].Input;
            _displays[selectionInfo.Index].Display.SetInput(input);
            Log($"Turning setting display {selectionInfo.Index} to {input}");
        }
        else if (selectionInfo.Join == Input2Join)
        {
            var input = _displays[selectionInfo.Index].Inputs[1].Input;
            _displays[selectionInfo.Index].Display.SetInput(input);
            Log($"Turning setting display {selectionInfo.Index} to {input}");
        }
        else if (selectionInfo.Join == Input3Join)
        {
            var input = _displays[selectionInfo.Index].Inputs[2].Input;
            _displays[selectionInfo.Index].Display.SetInput(input);
            Log($"Turning setting display {selectionInfo.Index} to {input}");
        }
        else if (selectionInfo.Join == Input4Join)
        {
            var input = _displays[selectionInfo.Index].Inputs[3].Input;
            _displays[selectionInfo.Index].Display.SetInput(input);
            Log($"Turning setting display {selectionInfo.Index} to {input}");
        }
    }

    private void FeedbackForDevice(int deviceIndex)
    {
        _smartObjects.ForEach(smartObject =>
        {
            smartObject.StringInput[_srlHelper.SerialJoinFor(deviceIndex, NameJoin)].StringValue = _displays[deviceIndex].Name;
            smartObject.BooleanInput[_srlHelper.BooleanJoinFor(deviceIndex, PowerOnJoin)].BoolValue =
                _displays[deviceIndex].Display.GetCurrentPowerState() == PowerState.On;
            smartObject.BooleanInput[_srlHelper.BooleanJoinFor(deviceIndex, PowerOffJoin)].BoolValue =
                _displays[deviceIndex].Display.GetCurrentPowerState() == PowerState.Off;
            if (_displays[deviceIndex].Inputs.Length > 0)
                smartObject.BooleanInput[_srlHelper.BooleanJoinFor(deviceIndex, Input1Join)].BoolValue =
                    _displays[deviceIndex].Inputs[0].Input == _displays[deviceIndex].Display.GetCurrentInput();
            if (_displays[deviceIndex].Inputs.Length > 1)
                smartObject.BooleanInput[_srlHelper.BooleanJoinFor(deviceIndex, Input2Join)].BoolValue =
                    _displays[deviceIndex].Inputs[1].Input == _displays[deviceIndex].Display.GetCurrentInput();
            if (_displays[deviceIndex].Inputs.Length > 2)
                smartObject.BooleanInput[_srlHelper.BooleanJoinFor(deviceIndex, Input3Join)].BoolValue =
                    _displays[deviceIndex].Inputs[2].Input == _displays[deviceIndex].Display.GetCurrentInput();
            if (_displays[deviceIndex].Inputs.Length > 3)
                smartObject.BooleanInput[_srlHelper.BooleanJoinFor(deviceIndex, Input4Join)].BoolValue =
                    _displays[deviceIndex].Inputs[3].Input == _displays[deviceIndex].Display.GetCurrentInput();
        });
        
    }

    public void EnableLogs(bool enable) => _enableLogs = enable;

    private void Log(string message)
    {
        if(_enableLogs)
            CrestronConsole.PrintLine($"{DateTime.Now} - {_name} - Display Menu - {message}");
    }
}