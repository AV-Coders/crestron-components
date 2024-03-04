using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace AVCoders.Crestron.TouchPanel;
public class Pin
{
    private readonly List<BasicTriListWithSmartObject> _panels;
    private readonly List<SmartObject> _smartObjects;
    private readonly uint _smartObjectId;
    private readonly uint _pageJoin;
    private readonly uint[] _relatedPages;
    private readonly uint _cancelJoin;
    private Action? _action;
    private string _pin = string.Empty;
    private string _input = string.Empty;
    private readonly string _name;
    private bool _enableLogs;
    
    
    public const uint ZeroJoin = 10;
    public const uint ClearJoin = 100;
    
    public const uint UnmaskedInputStringJoin = 11;

    public Pin(string name, List<BasicTriListWithSmartObject> panels, uint smartObjectId, 
        uint pageJoin, uint[] relatedPages, uint cancelJoin)
    {
        _smartObjectId = smartObjectId;
        _pageJoin = pageJoin;
        _relatedPages = relatedPages;
        _cancelJoin = cancelJoin;
        _panels = panels;
        _name = name;
        _smartObjects = new List<SmartObject>();
        _panels.ForEach(panel =>
        {
            _smartObjects.Add(panel.SmartObjects![_smartObjectId]);
            panel.SigChange += HandlePanelButtonPress;
        });
        _smartObjects.ForEach(smartObject => smartObject.SigChange += PinButtonPressed);
    }

    private void HandlePanelButtonPress(BasicTriList currentDevice, SigEventArgs args)
    {
        if (args.Sig.Type != eSigType.Bool)
            return;
        if (args.Sig.BoolValue != true)
            return;
        if(args.Sig.Number == _cancelJoin)
            CrestronPanel.Interlock(_panels, 0, _relatedPages);
    }

    private void PinButtonPressed(GenericBase currentDevice, SmartObjectEventArgs args)
    {
        if (args.Sig.Type != eSigType.Bool)
            return;
        if (args.Sig.BoolValue != true)
            return;
        Log($"Button {args.Sig.Number} pressed");
        uint buttonNumber = args.Sig.Number - 4010;
        switch (buttonNumber)
        {
            case ClearJoin:
                ClearText();
                break;
            case ZeroJoin:
                _input += "0";
                break;
            default:
                _input += $"{buttonNumber}";
                break;
        }

        _smartObjects.ForEach(x => x.StringInput[UnmaskedInputStringJoin].StringValue = _input);

        if (_input == _pin)
        {
            _action?.Invoke();
            ClearText();
            CrestronPanel.Interlock(_panels, 0, _relatedPages);
            _pin = String.Empty;
        }
    }

    public void Authenticate(string? expectedPin, Action successAction)
    {
        if (expectedPin == null)
        {
            Log("Authentication not required");
            successAction.Invoke();
            return;
        }
        ClearText();
        Log("Authenticating...");
        CrestronPanel.Interlock(_panels, _pageJoin, _relatedPages);
        _action = successAction;
        _pin = expectedPin;
    }

    private void ClearText()
    {
        _smartObjects.ForEach(x => x.StringInput[UnmaskedInputStringJoin].StringValue = string.Empty);
        _input = string.Empty;
        Log("Cleared text");
    }

    public void EnableLogs(bool enable) => _enableLogs = enable;

    private void Log(string message)
    {
        if(_enableLogs)
            CrestronConsole.PrintLine($"{DateTime.Now} - {_name} - PIN - {message}");
    }
}