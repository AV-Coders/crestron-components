using AVCoders.Core;
using Crestron.SimplSharpPro;

namespace AVCoders.Crestron.CommunicationClients;

public static class SerialMappings
{
    public static ComPort.eComBaudRates ConvertBaudRate(SerialBaud baudRate)
    {
        return baudRate switch
        {
            SerialBaud.Rate300 => ComPort.eComBaudRates.ComspecBaudRate300,
            SerialBaud.Rate1200 => ComPort.eComBaudRates.ComspecBaudRate1200,
            SerialBaud.Rate2400 => ComPort.eComBaudRates.ComspecBaudRate2400,
            SerialBaud.Rate4800 => ComPort.eComBaudRates.ComspecBaudRate4800,
            SerialBaud.Rate9600 => ComPort.eComBaudRates.ComspecBaudRate9600,
            SerialBaud.Rate19200 => ComPort.eComBaudRates.ComspecBaudRate19200,
            SerialBaud.Rate38400 => ComPort.eComBaudRates.ComspecBaudRate38400,
            SerialBaud.Rate115200 => ComPort.eComBaudRates.ComspecBaudRate115200,
            _ => throw new ArgumentOutOfRangeException(nameof(baudRate), baudRate, null)
        };
    }

    public static ComPort.eComDataBits ConvertDataBits(SerialDataBits dataBits)
    {
        return dataBits switch
        {
            SerialDataBits.DataBits7 => ComPort.eComDataBits.ComspecDataBits7,
            SerialDataBits.DataBits8 => ComPort.eComDataBits.ComspecDataBits8,
            _ => throw new ArgumentOutOfRangeException(nameof(dataBits), dataBits, null)
        };
    }

    public static ComPort.eComParityType ConvertParity(SerialParity parity)
    {
        return parity switch
        {
            SerialParity.Even => ComPort.eComParityType.ComspecParityEven,
            SerialParity.None => ComPort.eComParityType.ComspecParityNone,
            SerialParity.Odd => ComPort.eComParityType.ComspecParityOdd,
            _ => throw new ArgumentOutOfRangeException(nameof(parity), parity, null)
        };
    }
    
    public static ComPort.eComStopBits ConvertStopBits(SerialStopBits stopBits)
    {
        return stopBits switch
        {
            SerialStopBits.Bits1 => ComPort.eComStopBits.ComspecStopBits1,
            SerialStopBits.Bits2 => ComPort.eComStopBits.ComspecStopBits2,
            _ => throw new ArgumentOutOfRangeException(nameof(stopBits), stopBits, null)
        };
    }

    public static ComPort.eComProtocolType ConvertProtocol(SerialProtocol protocol)
    {
        return protocol switch
        {
            SerialProtocol.Rs232 => ComPort.eComProtocolType.ComspecProtocolRS232,
            SerialProtocol.Rs422 => ComPort.eComProtocolType.ComspecProtocolRS422,
            SerialProtocol.Rs485 => ComPort.eComProtocolType.ComspecProtocolRS485,
            _ => throw new ArgumentOutOfRangeException(nameof(protocol), protocol, null)
        };
    }

    public static eIRSerialBaudRates ConvertIrBaudRate(SerialBaud baudRate)
    {
        return baudRate switch
        {
            SerialBaud.Rate9600 => eIRSerialBaudRates.ComspecBaudRate9600,
            SerialBaud.Rate19200 => eIRSerialBaudRates.ComspecBaudRate19200,
            SerialBaud.Rate38400 => eIRSerialBaudRates.ComspecBaudRate38400,
            SerialBaud.Rate115200 => eIRSerialBaudRates.ComspecBaudRate115200,
            _ => throw new ArgumentOutOfRangeException(nameof(baudRate), baudRate, null)
        };
    }

    public static eIRSerialDataBits ConvertIrDataBits(SerialDataBits dataBits)
    {
        return dataBits switch
        {
            SerialDataBits.DataBits7 => eIRSerialDataBits.ComspecDataBits7,
            SerialDataBits.DataBits8 => eIRSerialDataBits.ComspecDataBits8,
            _ => throw new ArgumentOutOfRangeException(nameof(dataBits), dataBits, null)
        };
    }

    public static eIRSerialParityType ConvertIrParity(SerialParity parity)
    {
        return parity switch
        {
            SerialParity.Even => eIRSerialParityType.ComspecParityEven,
            SerialParity.None => eIRSerialParityType.ComspecParityNone,
            SerialParity.Odd => eIRSerialParityType.ComspecParityOdd,
            _ => throw new ArgumentOutOfRangeException(nameof(parity), parity, null)
        };
    }

    public static eIRSerialStopBits ConvertIrStopBits(SerialStopBits stopBits)
    {
        return stopBits switch
        {
            SerialStopBits.Bits1 => eIRSerialStopBits.ComspecStopBits1,
            SerialStopBits.Bits2 => eIRSerialStopBits.ComspecStopBits2,
            _ => throw new ArgumentOutOfRangeException(nameof(stopBits), stopBits, null)
        };
    }
}