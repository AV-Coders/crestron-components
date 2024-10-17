using AVCoders.MediaPlayer;

namespace AVCoders.Crestron.TouchPanel;

public class TvChannelControls
{
    private readonly ISetTopBox _stb;
    private readonly string _name;
    
    private readonly Dictionary<uint, Action> _actions = new ();

    public TvChannelControls(List<SmartObject> smartObjects, ISetTopBox stb, string name)
    {
        _stb = stb;
        _name = name;
        
        smartObjects.ForEach(x => x.SigChange += ExterityButtonPressed);
        
        _actions.Add(1001, _stb.ChannelUp);
        _actions.Add(1002, _stb.ChannelDown);
        _actions.Add(1003, () => _stb.SendIRCode(RemoteButton.VolumeUp));
        _actions.Add(1004, () => _stb.SendIRCode(RemoteButton.VolumeDown));
        
        _actions.Add(1011, () => _stb.SendIRCode(RemoteButton.Up));
        _actions.Add(1012, () => _stb.SendIRCode(RemoteButton.Down));
        _actions.Add(1013, () => _stb.SendIRCode(RemoteButton.Left));
        _actions.Add(1014, () => _stb.SendIRCode(RemoteButton.Right));
        _actions.Add(1015, () => _stb.SendIRCode(RemoteButton.Enter));
        _actions.Add(1016, () => _stb.SendIRCode(RemoteButton.Back));
        
        _actions.Add(1021, () => _stb.SendIRCode(RemoteButton.Power));
        _actions.Add(1022, _stb.ToggleSubtitles);
    }

    private void ExterityButtonPressed(GenericBase currentDevice, SmartObjectEventArgs args)
    {
        if (args.Sig.Number < 11)
            return;
        if (args.Sig.Type != eSigType.Bool)
            return;
        if (args.Sig.BoolValue != true)
            return;
        
        uint button = args.Sig.Number - 4010;
        Log($"Button {button} Pressed");
        
        if(_actions.TryGetValue(button, out var action))
            action.Invoke();
        else
        {
            _stb.SetChannel((int)button);
            Log($"Channel {button}");
        }
    }

    private void Log(string message)
    {
        CrestronConsole.PrintLine($"{_name} - TvChannelControls - {message}");
    }
    
}