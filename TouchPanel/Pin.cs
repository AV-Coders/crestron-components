﻿using System.Text;
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
    public const uint BackspaceJoin = 101;
    public const uint CancelJoin = 102;
    public const uint EnterJoin = 103;

    
    public const uint UnmaskedInputStringJoin = 11;
    public const uint MaskedInputStringJoin = 12;

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
        if (!args.Sig.BoolValue)
            return;
        if (args.Sig.Number == _cancelJoin)
        {
            CrestronPanel.Interlock(_panels, 0, _relatedPages);
            ClearText();
        }
    }

    private void PinButtonPressed(GenericBase currentDevice, SmartObjectEventArgs args)
    {
        if (args.Sig.Type != eSigType.Bool)
            return;
        if (!args.Sig.BoolValue)
            return;
        Log($"Button {args.Sig.Number} pressed");
        uint buttonNumber = args.Sig.Number - 4010;
        switch (buttonNumber)
        {
            case BackspaceJoin:
                if (_input.Length > 0)
                    _input = _input[..^1];
                break;
            case CancelJoin:
                ClearText();
                CrestronPanel.Interlock(_panels, 0, _relatedPages);
                break;
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

        _smartObjects.ForEach(x =>
        {
            x.StringInput[UnmaskedInputStringJoin + 10].StringValue = _input;
            x.StringInput[MaskedInputStringJoin + 10].StringValue = MaskInput();
            
        });

        if (_input == _pin)
        {
            _action?.Invoke();
            ClearText();
            CrestronPanel.Interlock(_panels, 0, _relatedPages);
            _pin = String.Empty;
        }
    }
    
    private string MaskInput()
    {
        StringBuilder sb = new StringBuilder(_input);
        for (int i = 0; i < _input.Length; i++)
        {
            sb[i] = '*';
        }
        return sb.ToString();
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
        _smartObjects.ForEach(x => x.StringInput[UnmaskedInputStringJoin + 10].StringValue = string.Empty);
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