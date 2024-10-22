using AVCoders.Crestron.SmartGraphics;
using Crestron.SimplSharpPro.DeviceSupport;

namespace AVCoders.Crestron.TouchPanel;

public class Confirmation
{
    private readonly List<BasicTriListWithSmartObject> _panels;
    private readonly List<SmartObject> _smartObjects;
    private readonly SubpageReferenceListHelper _srlHelper;
    private readonly uint _smartObjectId;
    private readonly uint _pageJoin;
    private readonly uint[] _relatedPages;
    private readonly uint _cancelJoin;
    private Action? _action;
    private readonly string _name;
    private bool _enableLogs;
    
    private const uint ConfirmationJoin = 1;
    private const uint CancelJoin = 2;
        

    public Confirmation(string name, List<BasicTriListWithSmartObject> panels, uint smartObjectId, 
        uint pageJoin, uint[] relatedPages, uint cancelJoin)
    {
        _name = name;
        _panels = panels;
        _smartObjectId = smartObjectId;
        _pageJoin = pageJoin;
        _relatedPages = relatedPages;
        _cancelJoin = cancelJoin;
        _srlHelper = new SubpageReferenceListHelper(10, 10, 10);
        
        _smartObjects = new List<SmartObject>();
        _panels.ForEach(panel =>
        {
            _smartObjects.Add(panel.SmartObjects![_smartObjectId]);
            panel.SigChange += HandlePanelButtonPress;
        });
        _smartObjects.ForEach(smartObject => smartObject.SigChange += ConfirmationButtonPressed);
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
        }
    }
    
    private void ConfirmationButtonPressed(GenericBase currentDevice, SmartObjectEventArgs args)
    {
        if (args.Sig.Type != eSigType.Bool)
            return;
        if (!args.Sig.BoolValue)
            return;
        Log($"Button {args.Sig.Number} pressed");
        
        CrestronPanel.Interlock(_panels, 0, _relatedPages);
        Thread.Sleep(100);
        
        if(args.Sig.Number - 4010 == ConfirmationJoin)
            _action?.Invoke();
    }

    public void Prompt(Action confirmationAction, string question, string confirmationText, string cancelText)
    {
        _action = confirmationAction;
        _smartObjects.ForEach(x =>
        {
            x.StringInput[_srlHelper.SerialJoinFor(0, 1)].StringValue = question;
            x.StringInput[_srlHelper.SerialJoinFor(0, 2)].StringValue = confirmationText;
            x.StringInput[_srlHelper.SerialJoinFor(0, 3)].StringValue = cancelText;

        });
        
        CrestronPanel.Interlock(_panels, _pageJoin, _relatedPages);
    }

    public void EnableLogs(bool enable) => _enableLogs = enable;

    private void Log(string message)
    {
        if(_enableLogs)
            CrestronConsole.PrintLine($"{DateTime.Now} - {_name} - PIN - {message}");
    }
}