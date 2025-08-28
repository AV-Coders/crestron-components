using AVCoders.MediaPlayer;
using Crestron.SimplSharpPro;
using Serilog;
using Directory = Crestron.SimplSharp.CrestronIO.Directory;

namespace AVCoders.Crestron.MediaPlayer;

public class BluRayPlayerOverCrestronIr : AVCoders.MediaPlayer.MediaPlayer
{
    private readonly IROutputPort _port;
    private readonly ushort _pulseTimeInMs;
    
    private static string GetStringFromRemoteButtons(RemoteButton button) => button switch
    {
        RemoteButton.Play => "PLAY",
        RemoteButton.Pause => "PAUSE",
        RemoteButton.Stop => "STOP",
        RemoteButton.Previous => "PREVIOUS",
        RemoteButton.Next => "NEXT",
        RemoteButton.Rewind => "RSCAN",
        RemoteButton.FastForward => "FSCAN",
        RemoteButton.Eject => "EJECT",
        RemoteButton.TopMenu => "TOPMENU",
        RemoteButton.PopupMenu => "POPUPMENU",
        RemoteButton.Subtitle => "SUBTITLE",
        RemoteButton.Display => "DISPLAY",
        RemoteButton.Back => "RETURN",
        RemoteButton.Power => "POWER",
        RemoteButton.Up => "UP_ARROW",
        RemoteButton.Down => "DN_ARROW",
        RemoteButton.Left => "LEFT_ARROW",
        RemoteButton.Right => "RIGHT_ARROW",
        RemoteButton.Enter => "ENTER",
        RemoteButton.Red => "RED",
        RemoteButton.Green => "GREEN",
        RemoteButton.Yellow => "YELLOW",
        RemoteButton.Blue => "BLUE",
        _ => throw new ArgumentOutOfRangeException($"The remote button {button.ToString()} is not in the map")
    };
    public BluRayPlayerOverCrestronIr(string name, IROutputPort port, string irFileName, ushort pulseTimeInMs = 25) : base(name)
    {
        _port = port;
        _pulseTimeInMs = pulseTimeInMs;
        port.LoadIRDriver($"{Directory.GetApplicationDirectory()}/{irFileName}");
    }

    public override void PowerOn()
    {
        Pulse(GetStringFromRemoteButtons(RemoteButton.Power));
    }

    public override void PowerOff()
    {
        Pulse(GetStringFromRemoteButtons(RemoteButton.Power));
    }
    
    public void SendIRCode(RemoteButton button) => Pulse(GetStringFromRemoteButtons(button));

    private void Pulse(string key)
    {
        Log.Debug($"Pulsing {key}");
        _port.PressAndRelease(key, _pulseTimeInMs);
    }
}