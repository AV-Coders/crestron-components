using AVCoders.Core;
using Crestron.SimplSharpPro;

namespace AVCoders.Crestron.CommunicationClients;

public class AvCodersSerialClient : SerialClient
{
    private readonly ComPort _comPort;

    public AvCodersSerialClient(ComPort comPort, SerialSpec serialSpec)
    {
        _comPort = comPort;
        _comPort.Register();
        ConfigurePort(serialSpec);
        _comPort.SerialDataReceived += ComPortOnSerialDataReceived;
    }

    private void ComPortOnSerialDataReceived(ComPort receivingComPort, ComPortSerialDataEventArgs args) => ResponseHandlers?.Invoke(args.SerialData);

    public override void Send(String message) => _comPort.Send(message);

    public override void Send(byte[] bytes) => Send(bytes.ToString() ?? throw new InvalidCastException("Bytes can't be a string"));
    public override void Send(char[] chars) => _comPort.Send(chars, chars.Length);

    public sealed override void ConfigurePort(SerialSpec serialSpec)
    {
        _comPort.SetComPortSpec(
            SerialMappings.BaudRatesMap[serialSpec.BaudRate],
            SerialMappings.DataBitsMap[serialSpec.DataBits],
            SerialMappings.ParityMap[serialSpec.Parity],
            SerialMappings.StopBitsMap[serialSpec.StopBits],
            SerialMappings.ProtocolMap[serialSpec.Protocol],
            ComPort.eComHardwareHandshakeType
                .ComspecHardwareHandshakeNone, // I've never seen this needed in any devices i've controlled
            ComPort.eComSoftwareHandshakeType.ComspecSoftwareHandshakeNone,
            false);
    }
}