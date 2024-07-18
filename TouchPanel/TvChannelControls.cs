using AVCoders.MediaPlayer;

namespace AVCoders.Crestron.TouchPanel;

public class TvChannelControls
{
    public const uint ChannelUpJoin = 1001;
    public const uint ChannelDownJoin = 1002;
    public const uint SubtitleJoin = 1003;
    
    private readonly ISetTopBox _stb;
    private readonly string _name;

    public TvChannelControls(List<SmartObject> smartObjects, ISetTopBox stb, string name)
    {
        _stb = stb;
        _name = name;
        
        smartObjects.ForEach(x => x.SigChange += ExterityButtonPressed);
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

        switch (button)
        {
            case ChannelUpJoin:
            {
                _stb.ChannelUp();
                Log("Channel Up");
                break;
            }
            case ChannelDownJoin:
            {
                _stb.ChannelDown();
                Log("Channel Down");
                break;
            }
            case SubtitleJoin:
            {
                _stb.SendIRCode(RemoteButton.Subtitle);
                Log("Subtitle");
                break;
            }
            default:
            {
                _stb.SetChannel((int)button);
                Log($"Channel {button}");
                break;
            }
        }
    }

    private void Log(string message)
    {
        CrestronConsole.PrintLine($"{_name} - TvChannelControls - {message}");
    }
    
}