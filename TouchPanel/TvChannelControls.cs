using AVCoders.Core;
using AVCoders.MediaPlayer;

namespace AVCoders.Crestron.TouchPanel;

public record TVControlActionDetails(Action Action, bool Repeat);

public class TvChannelControls : LogBase
{
    private readonly ISetTopBox _stb;
    private CancellationTokenSource _pressAndHoldToken = new ();
    
    private readonly Dictionary<uint, TVControlActionDetails> _actions = new ();

    public TvChannelControls(List<SmartObject> smartObjects, ISetTopBox stb, string name) : base(name)
    {
        _stb = stb;
        
        smartObjects.ForEach(x => x.SigChange += SetTopBoxButtonPressed);
        
        _actions.Add(1001, new TVControlActionDetails(_stb.ChannelUp, true));
        _actions.Add(1002, new TVControlActionDetails(_stb.ChannelDown, true));
        _actions.Add(1003, new TVControlActionDetails(() => _stb.SendIRCode(RemoteButton.VolumeUp), true));
        _actions.Add(1004, new TVControlActionDetails(() => _stb.SendIRCode(RemoteButton.VolumeDown), true));
        
        _actions.Add(1011, new TVControlActionDetails(() => _stb.SendIRCode(RemoteButton.Up), true));
        _actions.Add(1012, new TVControlActionDetails(() => _stb.SendIRCode(RemoteButton.Down), true));
        _actions.Add(1013, new TVControlActionDetails(() => _stb.SendIRCode(RemoteButton.Left), true));
        _actions.Add(1014, new TVControlActionDetails(() => _stb.SendIRCode(RemoteButton.Right), true));
        _actions.Add(1015, new TVControlActionDetails(() => _stb.SendIRCode(RemoteButton.Enter), true));
        _actions.Add(1016, new TVControlActionDetails(() => _stb.SendIRCode(RemoteButton.Back), true));
        
        _actions.Add(1021, new TVControlActionDetails(() => _stb.SendIRCode(RemoteButton.Power), false));
        _actions.Add(1022, new TVControlActionDetails(_stb.ToggleSubtitles, false));
    }

    private void SetTopBoxButtonPressed(GenericBase currentDevice, SmartObjectEventArgs args)
    {
        if (!CrestronPanel.EventIsAButtonPress(args))
        {
            _pressAndHoldToken.Cancel();
            return;
        }
        if (args.Sig.Number < 11)
            return;
        
        uint button = args.Sig.Number - 4010;
        
        if(_actions.TryGetValue(button, out var action))
        {
            if (action.Repeat)
            {
                _pressAndHoldToken = new CancellationTokenSource();
                new Task(() => SimulatePressAndHold(action.Action)).Start();
            }
            else
                action.Action.Invoke();
        }
        else
        {
            _stb.SetChannel((int)button);
        }
    }
    
    private void SimulatePressAndHold(Action action)
    {
        try
        {
            action.Invoke();
            Task.Delay(400, _pressAndHoldToken.Token).Wait();
            while (!_pressAndHoldToken.IsCancellationRequested)
            {
                action.Invoke();
                Task.Delay(150, _pressAndHoldToken.Token).Wait();
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
    
}