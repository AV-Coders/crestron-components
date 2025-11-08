using AVCoders.MediaPlayer;
using Crestron.SimplSharpPro;
using Serilog;
using Directory = Crestron.SimplSharp.CrestronIO.Directory;

namespace Core;

public class StrongSrt5432OverCrestronIr : ISetTopBox
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

    public StrongSrt5432OverCrestronIr(string name, IROutputPort port)
    {
        _name = name;
        _port = port;
        _pulseTimeInMs = 25;
        _sendEnterAfterChannel = true;
        port.LoadIRDriver($"{Directory.GetApplicationDirectory()}/Strong SRT-5432.ir");
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

    public void ToggleSubtitles()
    {
        SendIRCode(RemoteButton.Subtitle);
            Thread.Sleep(_pulseTimeInMs * 3);
        SendIRCode(RemoteButton.Down);
            Thread.Sleep(_pulseTimeInMs * 3);
        SendIRCode(RemoteButton.Enter);
    }

    private void Pulse(string key)
    {
        Log.Information($"Pulsing {key}");
        _port.PressAndRelease(key, _pulseTimeInMs);
    }
}