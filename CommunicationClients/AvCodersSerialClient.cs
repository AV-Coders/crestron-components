using AVCoders.Core;
using Crestron.SimplSharpPro;

namespace AVCoders.Crestron.CommunicationClients;

public class AvCodersSerialClient : SerialClient
{
    private readonly ComPort _comPort;

        private readonly Dictionary<SerialBaud, ComPort.eComBaudRates> _baudRatesMap;
        private readonly Dictionary<SerialDataBits, ComPort.eComDataBits> _dataBitsMap;
        private readonly Dictionary<SerialParity, ComPort.eComParityType> _parityMap;
        private readonly Dictionary<SerialStopBits, ComPort.eComStopBits> _stopBitsMap;
        private readonly Dictionary<SerialProtocol, ComPort.eComProtocolType> _protocolMap;

        public AvCodersSerialClient(ComPort comPort, SerialSpec serialSpec)
        {
            _comPort = comPort;
            
            _baudRatesMap = new Dictionary<SerialBaud, ComPort.eComBaudRates>();
            _baudRatesMap.Add(SerialBaud.Rate9600, ComPort.eComBaudRates.ComspecBaudRate9600);
            _baudRatesMap.Add(SerialBaud.Rate19200, ComPort.eComBaudRates.ComspecBaudRate19200);
            _baudRatesMap.Add(SerialBaud.Rate38400, ComPort.eComBaudRates.ComspecBaudRate38400);
            _baudRatesMap.Add(SerialBaud.Rate115200, ComPort.eComBaudRates.ComspecBaudRate115200);

            _dataBitsMap = new Dictionary<SerialDataBits, ComPort.eComDataBits>();
            _dataBitsMap.Add(SerialDataBits.DataBits7, ComPort.eComDataBits.ComspecDataBits7);
            _dataBitsMap.Add(SerialDataBits.DataBits8, ComPort.eComDataBits.ComspecDataBits8);

            _parityMap = new Dictionary<SerialParity, ComPort.eComParityType>();
            _parityMap.Add(SerialParity.Even, ComPort.eComParityType.ComspecParityEven);
            _parityMap.Add(SerialParity.None, ComPort.eComParityType.ComspecParityNone);
            _parityMap.Add(SerialParity.Odd, ComPort.eComParityType.ComspecParityOdd);

            _stopBitsMap = new Dictionary<SerialStopBits, ComPort.eComStopBits>();
            _stopBitsMap.Add(SerialStopBits.Bits1, ComPort.eComStopBits.ComspecStopBits1);
            _stopBitsMap.Add(SerialStopBits.Bits2, ComPort.eComStopBits.ComspecStopBits2);

            _protocolMap = new Dictionary<SerialProtocol, ComPort.eComProtocolType>();
            _protocolMap.Add(SerialProtocol.Rs232, ComPort.eComProtocolType.ComspecProtocolRS232);
            _protocolMap.Add(SerialProtocol.Rs422, ComPort.eComProtocolType.ComspecProtocolRS422);
            _protocolMap.Add(SerialProtocol.Rs485, ComPort.eComProtocolType.ComspecProtocolRS485);
            
            ConfigurePort(serialSpec);
            _comPort.SerialDataReceived += ComPortOnSerialDataReceived;
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
            if(bytes.ToString() != null)
                _comPort.Send(bytes.ToString());
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