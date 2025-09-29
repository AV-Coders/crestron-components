using System.Text;
using AVCoders.CommunicationClients;
using AVCoders.Core;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using Serilog;

namespace AVCoders.Crestron.CommunicationClients;

public class CrestronMulticastClient : IMulticastClient
{
    private UDPServer _server;
    
    private readonly Queue<QueuedPayload<byte[]>> _sendQueue = new();

    public CrestronMulticastClient(string name, string host, ushort port)
        : base(host, port, name, CommandStringFormat.Ascii)
    {
        _server = new UDPServer(IPAddress.Parse(host), port, 500, EthernetAdapterType.EthernetLANAdapter);
        _server.ClearIncomingDataBuffer = true;
        _server.EnableUDPServer();
        ConnectionState = ConnectionState.Connected;
        
        ReceiveThreadWorker.Restart();
        SendQueueWorker.Restart();
    }

    public override void Send(string message)
    {
        Send(Encoding.UTF8.GetBytes(message));
        InvokeRequestHandlers(message);
    }

    public override void Send(byte[] bytes)
    {
        using (PushProperties("Send"))
        {
            try
            {
                _server.SendData(bytes, bytes.Length, Host, Port);
                InvokeRequestHandlers(bytes);
            }
            catch (Exception e)
            {
                LogException(e, "There was an issue sending.");
                _sendQueue.Enqueue(new QueuedPayload<byte[]>(DateTime.Now, bytes));
            }
        }
    }

    protected override async Task Receive(CancellationToken token)
    {
        using (PushProperties("Receive"))
        {
            while (!_server.DataAvailable)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), token);
            }
            while (_server.ReceiveData() > 0)
            {
                string response = Encoding.UTF8.GetString(_server.IncomingDataBuffer);
                InvokeResponseHandlers(response, _server.IncomingDataBuffer);
            }
            await Task.Delay(TimeSpan.FromSeconds(1), token);
        }
    }

    protected override async Task ProcessSendQueue(CancellationToken token)
    {
        while (_sendQueue.Count > 0)
        {
            try
            {
                var item = _sendQueue.Dequeue();
                if (Math.Abs((DateTime.Now - item.Timestamp).TotalSeconds) < QueueTimeout)
                    _server.SendData(item.Payload, item.Payload.Length, Host, Port);
            }
            catch (Exception e)
            {
                LogException(e, "There was an issue re-sending a message from the queue");
            }
            
        }
        await Task.Delay(TimeSpan.FromSeconds(2), token);
    }

    protected override Task CheckConnectionState(CancellationToken token) => ConnectionStateWorker.Stop();

    public override void Connect() { }

    public override void Reconnect() { }

    public override void Disconnect() { }
}