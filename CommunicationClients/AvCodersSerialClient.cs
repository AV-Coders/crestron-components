using AVCoders.Core;
using Crestron.SimplSharpPro;

namespace AVCoders.Crestron.CommunicationClients;

public class AvCodersSerialClient : SerialClient
{
    private readonly ComPort _comPort;

        private readonly Dictionary<SerialBaud, ComPort.eComBaudRates> _baudRatesMap = new()
        {
            { SerialBaud.Rate9600, ComPort.eComBaudRates.ComspecBaudRate9600 },
            { SerialBaud.Rate19200, ComPort.eComBaudRates.ComspecBaudRate19200 },
            { SerialBaud.Rate38400, ComPort.eComBaudRates.ComspecBaudRate38400 },
            { SerialBaud.Rate115200, ComPort.eComBaudRates.ComspecBaudRate115200 }
        };

        private readonly Dictionary<SerialDataBits, ComPort.eComDataBits> _dataBitsMap = new()
        {
            { SerialDataBits.DataBits7, ComPort.eComDataBits.ComspecDataBits7 }, 
            { SerialDataBits.DataBits8, ComPort.eComDataBits.ComspecDataBits8 }
        };


        private readonly Dictionary<SerialParity, ComPort.eComParityType> _parityMap = new()
        {
            { SerialParity.Even, ComPort.eComParityType.ComspecParityEven },
            { SerialParity.None, ComPort.eComParityType.ComspecParityNone },
            { SerialParity.Odd, ComPort.eComParityType.ComspecParityOdd }
        };
        

        private readonly Dictionary<SerialStopBits, ComPort.eComStopBits> _stopBitsMap = new() 
        {
            { SerialStopBits.Bits1, ComPort.eComStopBits.ComspecStopBits1 },
            { SerialStopBits.Bits2, ComPort.eComStopBits.ComspecStopBits2 },
        };

        private readonly Dictionary<SerialProtocol, ComPort.eComProtocolType> _protocolMap = new()
        {
            { SerialProtocol.Rs232, ComPort.eComProtocolType.ComspecProtocolRS232 },
            { SerialProtocol.Rs422, ComPort.eComProtocolType.ComspecProtocolRS422 },
            { SerialProtocol.Rs485, ComPort.eComProtocolType.ComspecProtocolRS485 },
        };

        public AvCodersSerialClient(ComPort comPort, SerialSpec serialSpec)
        {
            _comPort = comPort;
            ConfigurePort(serialSpec);
            _comPort.SerialDataReceived += ComPortOnSerialDataReceived;
            _comPort.Register();
        }

        private void ComPortOnSerialDataReceived(ComPort receivingComPort, ComPortSerialDataEventArgs args)
        {
            ResponseHandlers?.Invoke(args.SerialData);
        }

        public override void Send(String message)
        {
            _comPort.Send(message);
        }

        public override void Send(byte[] bytes)
        {
            Send(bytes.ToString() ?? throw new InvalidCastException("Bytes can't be a string"));
        }

        public sealed override void ConfigurePort(SerialSpec serialSpec)
        {
            _comPort.SetComPortSpec(
                _baudRatesMap[serialSpec.BaudRate], 
                _dataBitsMap[serialSpec.DataBits],
                _parityMap[serialSpec.Parity],
                _stopBitsMap[serialSpec.StopBits],
                _protocolMap[serialSpec.Protocol],
                ComPort.eComHardwareHandshakeType.ComspecHardwareHandshakeNone, // I've never seen this needed in any devices i've controlled
                ComPort.eComSoftwareHandshakeType.ComspecSoftwareHandshakeNone,
                false);
        }
}