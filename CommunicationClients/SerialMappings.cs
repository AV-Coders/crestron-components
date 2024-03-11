using AVCoders.Core;
using Crestron.SimplSharpPro;

namespace AVCoders.Crestron.CommunicationClients;

public static class SerialMappings
{
    public static readonly Dictionary<SerialBaud, ComPort.eComBaudRates> BaudRatesMap = new()
    {
        { SerialBaud.Rate9600, ComPort.eComBaudRates.ComspecBaudRate9600 },
        { SerialBaud.Rate19200, ComPort.eComBaudRates.ComspecBaudRate19200 },
        { SerialBaud.Rate38400, ComPort.eComBaudRates.ComspecBaudRate38400 },
        { SerialBaud.Rate115200, ComPort.eComBaudRates.ComspecBaudRate115200 }
    };

    public static readonly Dictionary<SerialDataBits, ComPort.eComDataBits> DataBitsMap = new()
    {
        { SerialDataBits.DataBits7, ComPort.eComDataBits.ComspecDataBits7 }, 
        { SerialDataBits.DataBits8, ComPort.eComDataBits.ComspecDataBits8 }
    };
    
    public static readonly Dictionary<SerialParity, ComPort.eComParityType> ParityMap = new()
    {
        { SerialParity.Even, ComPort.eComParityType.ComspecParityEven },
        { SerialParity.None, ComPort.eComParityType.ComspecParityNone },
        { SerialParity.Odd, ComPort.eComParityType.ComspecParityOdd }
    };

    public static readonly Dictionary<SerialStopBits, ComPort.eComStopBits> StopBitsMap = new() 
    {
        { SerialStopBits.Bits1, ComPort.eComStopBits.ComspecStopBits1 },
        { SerialStopBits.Bits2, ComPort.eComStopBits.ComspecStopBits2 },
    };

    public static readonly Dictionary<SerialProtocol, ComPort.eComProtocolType> ProtocolMap = new()
    {
        { SerialProtocol.Rs232, ComPort.eComProtocolType.ComspecProtocolRS232 },
        { SerialProtocol.Rs422, ComPort.eComProtocolType.ComspecProtocolRS422 },
        { SerialProtocol.Rs485, ComPort.eComProtocolType.ComspecProtocolRS485 },
    };
    
    public static readonly Dictionary<SerialBaud, eIRSerialBaudRates> IrBaudRatesMap = new()
    {
        { SerialBaud.Rate9600, eIRSerialBaudRates.ComspecBaudRate9600 },
        { SerialBaud.Rate19200, eIRSerialBaudRates.ComspecBaudRate19200 },
        { SerialBaud.Rate38400, eIRSerialBaudRates.ComspecBaudRate38400 },
        { SerialBaud.Rate115200, eIRSerialBaudRates.ComspecBaudRate115200 }
    };

    public static readonly Dictionary<SerialDataBits, eIRSerialDataBits> IrDataBitsMap = new()
    {
        { SerialDataBits.DataBits7, eIRSerialDataBits.ComspecDataBits7 }, 
        { SerialDataBits.DataBits8, eIRSerialDataBits.ComspecDataBits8 }
    };
    
    public static readonly Dictionary<SerialParity, eIRSerialParityType> IrParityMap = new()
    {
        { SerialParity.Even, eIRSerialParityType.ComspecParityEven },
        { SerialParity.None, eIRSerialParityType.ComspecParityNone },
        { SerialParity.Odd, eIRSerialParityType.ComspecParityOdd }
    };

    public static readonly Dictionary<SerialStopBits, eIRSerialStopBits> IrStopBitsMap = new() 
    {
        { SerialStopBits.Bits1, eIRSerialStopBits.ComspecStopBits1 },
        { SerialStopBits.Bits2, eIRSerialStopBits.ComspecStopBits2 },
    };
}