using System.Text;
using AVCoders.Core;
using Crestron.SimplSharpPro;

namespace AVCoders.Crestron.CommunicationClients;

public class AvCodersIrAsSerialClient : SerialClient
{
    private readonly IROutputPort _irPort;
    private readonly Encoding _encoding;

    public AvCodersIrAsSerialClient(IROutputPort irPort, SerialSpec serialSpec, Encoding? encoding, string name, CommandStringFormat commandStringFormat) 
        : base(name, irPort.DeviceName, (ushort)irPort.ID, commandStringFormat)
    {
        _irPort = irPort;
        switch (_irPort.Register())
        {
            case eDeviceRegistrationUnRegistrationResponse.Failure:
                ConnectionState = ConnectionState.Error;
                LogError("Failed to register IR port {IrPortName}.  Reason: {reason}", _irPort.DeviceName,
                    _irPort.DeviceRegistrationFailureReason);
                // Registration is one-shot with no retry, so this is raised as ongoing
                // directly rather than via the momentary/threshold path.
                RaiseOngoingIssue(ConnectionIssueKey,
                    $"Failed to register IR port {_irPort.DeviceName}: {_irPort.DeviceRegistrationFailureReason}",
                    IssueSeverity.Critical);
                break;
            default:
                ConnectionState = ConnectionState.Connected;
                break;
        }
        _encoding = encoding ?? Encoding.ASCII;
        ConfigurePort(serialSpec);
    }

    public override void Send(String message)
    {
        _irPort.SendSerialData(message);
        InvokeRequestHandlers(message);       
    }

    public override void Send(byte[] bytes)
    {
        Send(bytes.ToString() ?? throw new InvalidCastException("Bytes can't be converted to a string"));
        InvokeRequestHandlers(bytes);
    }

    public sealed override void ConfigurePort(SerialSpec serialSpec)
    {
        _irPort.SetIRSerialSpec(
            SerialMappings.ConvertIrBaudRate(serialSpec.BaudRate),
            SerialMappings.ConvertIrDataBits(serialSpec.DataBits),
            SerialMappings.ConvertIrParity(serialSpec.Parity),
            SerialMappings.ConvertIrStopBits(serialSpec.StopBits),
            _encoding
            );
    }

    public override void Send(char[] chars) => _irPort.SendSerialData(chars.ToString() ?? throw new InvalidCastException("Chars is not a string"));
}