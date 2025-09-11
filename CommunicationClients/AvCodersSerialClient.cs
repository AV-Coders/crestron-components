using AVCoders.Core;
using Crestron.SimplSharpPro;

namespace AVCoders.Crestron.CommunicationClients;

public class AvCodersSerialClient : SerialClient
{
    private readonly ComPort _comPort;

    public AvCodersSerialClient(ComPort comPort, SerialSpec serialSpec, string name, CommandStringFormat commandStringFormat) 
        : base(name, comPort.DeviceName, (ushort)comPort.ID, commandStringFormat)
    {
        _comPort = comPort;
        _comPort.Register();
        ConfigurePort(serialSpec);
        _comPort.SerialDataReceived += ComPortOnSerialDataReceived;
    }

    private void ComPortOnSerialDataReceived(ComPort receivingComPort, ComPortSerialDataEventArgs args) => ResponseHandlers?.Invoke(args.SerialData);

    public override void Send(string message)
    {
        _comPort.Send(message);
        InvokeRequestHandlers(message);
    }

    public override void Send(byte[] bytes)
    {
        Send(bytes.ToString() ?? throw new InvalidCastException("Bytes can't be a string"));
        InvokeRequestHandlers(bytes);
    }

    public override void Send(char[] chars) => Send(new string(chars));

    public sealed override void ConfigurePort(SerialSpec serialSpec)
    {
        _comPort.SetComPortSpec(
            SerialMappings.ConvertBaudRate(serialSpec.BaudRate),
            SerialMappings.ConvertDataBits(serialSpec.DataBits),
            SerialMappings.ConvertParity(serialSpec.Parity),
            SerialMappings.ConvertStopBits(serialSpec.StopBits),
            SerialMappings.ConvertProtocol(serialSpec.Protocol),
            ComPort.eComHardwareHandshakeType
                .ComspecHardwareHandshakeNone, // I've never seen this needed in any devices i've controlled
            ComPort.eComSoftwareHandshakeType.ComspecSoftwareHandshakeNone,
            false);
    }
}