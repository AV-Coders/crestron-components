using AVCoders.Core;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;

namespace AVCoders.Crestron.CommunicationClients;

public class AvCodersWakeOnLan : IWakeOnLan
{
    private readonly UDPServer _server = new();

    public void Wake(string mac)
    {
        var wolPacket = BuildMagicPacket(ParseMacAddress(mac));
        for (int i = 0; i < 3; i++)
        {
            _server.SendData(wolPacket, wolPacket.Length, IPAddress.Broadcast.ToString(), 7);
            _server.SendData(wolPacket, wolPacket.Length, IPAddress.Broadcast.ToString(), 9);
            Thread.Sleep(300);
        }
    }
    
    private byte[] BuildMagicPacket(byte[] macAddress)
    {
        if (macAddress.Length != 6) throw new ArgumentException("The MAC is invalid");

        List<byte> magic = new List<byte>{ 0xff, 0xff, 0xff, 0xff, 0xff, 0xff};

        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                magic.Add(macAddress[j]);
            }
        }
        return magic.ToArray();
    }

    private static byte[] ParseMacAddress(string text)
    {
        string[] tokens = text.Split(new char[] { ':', '-' });

        byte[] bytes = new byte[6];
        for (int i = 0; i < 6; i++)
        {
            bytes[i] = Convert.ToByte(tokens[i], 16);
        }
        return bytes;
    }
}