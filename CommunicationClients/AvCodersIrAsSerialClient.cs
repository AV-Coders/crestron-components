using System.Text;
using AVCoders.Core;
using Crestron.SimplSharpPro;

namespace AVCoders.Crestron.CommunicationClients;

public class AvCodersIrAsSerialClient : SerialClient
{
    private readonly IROutputPort _irPort;
    private readonly Encoding _encoding;

    public AvCodersIrAsSerialClient(IROutputPort irPort, SerialSpec serialSpec, Encoding? encoding, string name) : base(name)
    {
        _irPort = irPort;
        _irPort.Register();
        _encoding = encoding ?? Encoding.ASCII;
        ConfigurePort(serialSpec);
    }

    public override void Send(String message) => _irPort.SendSerialData(message);

    public override void Send(byte[] bytes) => Send(bytes.ToString() ?? throw new InvalidCastException("Bytes can't be converted to a string"));

    public sealed override void ConfigurePort(SerialSpec serialSpec)
    {
        _irPort.SetIRSerialSpec(
            SerialMappings.IrBaudRatesMap[serialSpec.BaudRate],
            SerialMappings.IrDataBitsMap[serialSpec.DataBits],
            SerialMappings.IrParityMap[serialSpec.Parity],
            SerialMappings.IrStopBitsMap[serialSpec.StopBits],
            _encoding
            );
    }

    public override void Send(char[] chars) => _irPort.SendSerialData(chars.ToString() ?? throw new InvalidCastException("Chars is not a string"));
}