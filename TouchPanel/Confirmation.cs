using AVCoders.Core;
using AVCoders.Crestron.SmartGraphics;
using Crestron.SimplSharpPro.DeviceSupport;
using Serilog;

namespace AVCoders.Crestron.TouchPanel;

public class Confirmation : SrlPage
{
    private readonly List<BasicTriListWithSmartObject> _panels;
    private readonly uint _questionJoin;
    private readonly uint _pageJoin;
    private readonly uint[] _relatedPages;
    private readonly uint _cancelJoin;
    private List<KeyValuePair<string, Action?>> _options;

    private const uint SelectJoin = 1;
        

    public Confirmation(string name, List<BasicTriListWithSmartObject> panels, uint smartObjectId, uint questionJoin,
        uint pageJoin, uint[] relatedPages, uint cancelJoin) : base(name, [])
    {
        _panels = panels;
        _questionJoin = questionJoin;
        _pageJoin = pageJoin;
        _relatedPages = relatedPages;
        _cancelJoin = cancelJoin;
        _options = new List<KeyValuePair<string, Action?>>();
        
        _panels.ForEach(panel =>
        {
            var smartObject = panel.SmartObjects![smartObjectId]!;
            smartObject.SigChange += ConfirmationButtonPressed;
            SmartObjects.Add(smartObject);
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
        var info = SrlHelper.GetBooleanSigInfo(args.Sig.Number);
        if (info.Join != SelectJoin)
            return;
        Log.Verbose($"Option {info.Index} selected");
        
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
        SmartObjects.ForEach(x =>
        {
            x.UShortInput["Set Number of Items"].UShortValue = (ushort)options.Count;
            for (int i = 0; i < options.Count; i++)
            {
                x.StringInput[SrlHelper.SerialJoinFor(i, SelectJoin)].StringValue = options[i].Key;
            }

        });
        
        CrestronPanel.Interlock(_panels, _pageJoin, _relatedPages);
    }

    public override void PowerOn() { }

    public override void PowerOff() { }
}