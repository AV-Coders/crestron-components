using AVCoders.Crestron.SmartGraphics;
using Crestron.SimplSharpPro.DeviceSupport;

namespace AVCoders.Crestron.TouchPanel;

public class Confirmation
{
    private readonly List<BasicTriListWithSmartObject> _panels;
    private readonly List<SmartObject> _smartObjects;
    private readonly SubpageReferenceListHelper _srlHelper;
    private readonly uint _questionJoin;
    private readonly uint _pageJoin;
    private readonly uint[] _relatedPages;
    private readonly uint _cancelJoin;
    private readonly string _name;
    private bool _enableLogs;
    private List<KeyValuePair<string, Action?>> _options;

    private const uint SelectJoin = 1;
        

    public Confirmation(string name, List<BasicTriListWithSmartObject> panels, uint smartObjectId, uint questionJoin,
        uint pageJoin, uint[] relatedPages, uint cancelJoin)
    {
        _name = name;
        _panels = panels;
        _questionJoin = questionJoin;
        _pageJoin = pageJoin;
        _relatedPages = relatedPages;
        _cancelJoin = cancelJoin;
        _srlHelper = new SubpageReferenceListHelper(10, 10, 10);
        _options = new List<KeyValuePair<string, Action?>>();
        
        _smartObjects = new List<SmartObject>();
        _panels.ForEach(panel =>
        {
            var smartObject = panel.SmartObjects![smartObjectId]!;
            smartObject.SigChange += ConfirmationButtonPressed;
            _smartObjects.Add(smartObject);
            panel.SigChange += HandlePanelButtonPress;
        });
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
        var info = _srlHelper.GetBooleanSigInfo(args.Sig.Number);
        if (info.Join != SelectJoin)
            return;
        Log($"Option {info.Index} selected");
        
        CrestronPanel.Interlock(_panels, 0, _relatedPages);
        Thread.Sleep(100);
        
        _options[info.Index].Value?.Invoke();
        _options = new List<KeyValuePair<string, Action?>>();
    }

    public void Prompt(string question, List<KeyValuePair<string, Action?>> options)
    {
        _options = options;
        _panels.ForEach(panel =>
        {
            panel.StringInput[_questionJoin].StringValue = question;
        });
        _smartObjects.ForEach(x =>
        {
            x.UShortInput["Set Number of Items"].UShortValue = (ushort)options.Count;
            for (int i = 0; i < options.Count; i++)
            {
                x.StringInput[_srlHelper.SerialJoinFor(i, SelectJoin)].StringValue = options[i].Key;
            }

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