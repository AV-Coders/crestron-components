using AVCoders.Core;
using Crestron.SimplSharpPro.DM;
using Serilog;

namespace AVCoders.Crestron.CommunicationClients;

public class CrestronCecStream: SerialClient
{
    private readonly Cec _stream;

    public CrestronCecStream(Cec stream, string name) : base(name, "CEC Steram", 0, CommandStringFormat.Hex)
    {
        _stream = stream;
        _stream.CecChange += HandleCecResponse;
    }

    private void HandleCecResponse(Cec cecDevice, CecEventArgs args) => ResponseHandlers?.Invoke(cecDevice.Received.StringValue);

    public override void Send(string message) => _stream.Send.StringValue = message;

    public override void Send(byte[] bytes) => _stream.Send.CharacterArrayValue = bytes.Select(b => (char)b).ToArray();

    public override void ConfigurePort(SerialSpec serialSpec)
    {
        using (PushProperties("ConfigurePort"))
        {
            Log.Error("This port can't be configured");
        }
    }

    public override void Send(char[] chars) => _stream.Send.CharacterArrayValue = chars;
}