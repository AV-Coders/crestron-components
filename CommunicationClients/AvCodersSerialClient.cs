using System.Text;
using AVCoders.Core;
using Crestron.SimplSharpPro;
using Serilog;

namespace AVCoders.Crestron.CommunicationClients;

public class AvCodersSerialClient : SerialClient
{
    private readonly ComPort _comPort;

    public AvCodersSerialClient(ComPort comPort, SerialSpec serialSpec, string name, CommandStringFormat commandStringFormat) 
        : base(name, comPort.DeviceName, (ushort)comPort.ID, commandStringFormat)
    {
        using (PushProperties("Constructor"))
        {
            _comPort = comPort;
            switch (_comPort.Register())
            {
                case eDeviceRegistrationUnRegistrationResponse.Failure:
                    ConnectionState = ConnectionState.Error;
                    Log.Error("Failed to register com port {ComPortName}.  Reason: {reason}", _comPort.DeviceName,
                        _comPort.DeviceRegistrationFailureReason);
                    break;
                default:
                    ConnectionState = ConnectionState.Connected;
                    break;
            }

            ConfigurePort(serialSpec);
            _comPort.SerialDataReceived += ComPortOnSerialDataReceived;
        }
    }

    private void ComPortOnSerialDataReceived(ComPort receivingComPort, ComPortSerialDataEventArgs args)
    {
        InvokeResponseHandlers(args.SerialData, Encoding.Latin1.GetBytes(args.SerialData));
    }

    public override void Send(string message)
    {
        _comPort.Send(message);
        InvokeRequestHandlers(message);
    }

    public override void Send(byte[] bytes)
    {
        Send(Encoding.Latin1.GetString(bytes));
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
                .ComspecHardwareHandshakeNone, // I've never seen this needed in any devices I've controlled
            ComPort.eComSoftwareHandshakeType.ComspecSoftwareHandshakeNone,
            false);
    }
}