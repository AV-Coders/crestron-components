using AVCoders.MediaPlayer;
using Crestron.SimplSharpPro;
using Directory = Crestron.SimplSharp.CrestronIO.Directory;

namespace AVCoders.Crestron.MediaPlayer;

public class BluRayPlayerOverCrestronIr : AVCoders.MediaPlayer.MediaPlayer
{
    
    private readonly string _name;
    private readonly IROutputPort _port;
    private readonly ushort _pulseTimeInMs;
    
    private static readonly Dictionary<RemoteButton, string> RemoteButtonMap = new()
    {
        { RemoteButton.Play, "PLAY"},
        { RemoteButton.Pause, "PAUSE"},
        { RemoteButton.Stop, "STOP"},
        { RemoteButton.Previous, "PREVIOUS"},
        { RemoteButton.Next, "NEXT"},
        { RemoteButton.Rewind, "REWIND"},
        { RemoteButton.FastForward, "FASTFORWARD"},
        { RemoteButton.Eject, "EJECTS"},
        { RemoteButton.TopMenu, "TOPMENU"},
        { RemoteButton.PopupMenu, "POPUPMENU"},
        { RemoteButton.Subtitle, "SUBTITLE"},
        { RemoteButton.Display, "DISPLAY"},
        { RemoteButton.Back, "BACK"},
        { RemoteButton.Power, "POWER"}
    };
    public BluRayPlayerOverCrestronIr(string name, IROutputPort port, string irFileName, ushort pulseTimeInMs = 25) : base(name)
    {
        _port = port;
        _pulseTimeInMs = pulseTimeInMs;
        port.LoadIRDriver($"{Directory.GetApplicationDirectory()}/{irFileName}");
    }

    public override void PowerOn()
    {
        Pulse(RemoteButtonMap[RemoteButton.Power]);
    }

    public override void PowerOff()
    {
        Pulse(RemoteButtonMap[RemoteButton.Power]);
    }
    
    public void SendIRCode(RemoteButton button) => Pulse(RemoteButtonMap[button]);

    private void Pulse(string key)
    {
        Debug($"Pulsing {key}");
        _port.PressAndRelease(key, _pulseTimeInMs);
    }
}