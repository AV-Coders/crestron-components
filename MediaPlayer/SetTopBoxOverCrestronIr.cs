using AVCoders.MediaPlayer;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Directory = Crestron.SimplSharp.CrestronIO.Directory;

namespace AVCoders.Crestron.MediaPlayer;

public class SetTopBoxOverCrestronIr : ISetTopBox
{
    private static readonly Dictionary<RemoteButton, string> RemoteButtonMap = new()
    {
        { RemoteButton.Button0, "0"},
        { RemoteButton.Button1, "1"},
        { RemoteButton.Button2, "2"},
        { RemoteButton.Button3, "3"},
        { RemoteButton.Button4, "4"},
        { RemoteButton.Button5, "5"},
        { RemoteButton.Button6, "6"},
        { RemoteButton.Button7, "7"},
        { RemoteButton.Button8, "8"},
        { RemoteButton.Button9, "9"},
        { RemoteButton.Enter, "ENTER"},
        { RemoteButton.Up, "UP_ARROW"},
        { RemoteButton.Down, "DN_ARROW"},
        { RemoteButton.Left, "LEFT_ARROW"},
        { RemoteButton.Right, "RIGHT_ARROW"},
        { RemoteButton.Subtitle, "C"}
    };
    
    private readonly string _name;
    private readonly IROutputPort _port;
    private readonly ushort _pulseTimeInMs;
    private readonly bool _sendEnterAfterChannel;

    public SetTopBoxOverCrestronIr(string name, IROutputPort port, string irFileName, bool sendEnterAfterChannel = true, ushort pulseTimeInMs = 25)
    {
        _name = name;
        _port = port;
        _pulseTimeInMs = pulseTimeInMs;
        _sendEnterAfterChannel = sendEnterAfterChannel;
        port.LoadIRDriver($"{Directory.GetApplicationDirectory()}/{irFileName}");
    }
    public void ChannelUp() => Pulse("CH+");

    public void ChannelDown() => Pulse("CH-");
    
    public void VolumeUp() => Pulse("Vol+");

    public void VolumeDown() => Pulse("Vol-");

    public void SendIRCode(RemoteButton button) => Pulse(RemoteButtonMap[button]);

    public void SetChannel(int channel)
    {
        foreach (var c in channel.ToString())
        {
            Pulse($"{c}");
            Thread.Sleep(_pulseTimeInMs * 3);
        }
        if(_sendEnterAfterChannel)
            Pulse(RemoteButtonMap[RemoteButton.Enter]);
    }

    private void Pulse(string key)
    {
        Log($"Pulsing {key}");
        _port.PressAndRelease(key, _pulseTimeInMs);
    }
    
    private void Log(string message) => CrestronConsole.PrintLine($"{DateTime.Now} - {_name} - Mainline - {message}");
}