using AVCoders.Core;
using Crestron.SimplSharpPro;

namespace AVCoders.Crestron.CommunicationClients;

public class CrestronGenericDeviceCommunicationClient : IpComms
{
    public CrestronGenericDeviceCommunicationClient(GenericDevice device) 
        : base(String.Empty, 0, device.Description, CommandStringFormat.Ascii)
    {
        device.OnlineStatusChange += HandleOnlineStatusChange;
    }

    private void HandleOnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
    {
        ConnectionState = args.DeviceOnLine? ConnectionState.Connected : ConnectionState.Disconnected;
    }

    public override void Send(string message) => throw new NotSupportedException("This client does not support sending.");

    public override void Send(byte[] bytes) => throw new NotSupportedException("This client does not support sending.");

    protected override Task Receive(CancellationToken token)
    {
        ReceiveThreadWorker.Stop();
        return Task.CompletedTask;
    }

    protected override Task ProcessSendQueue(CancellationToken token) 
    {
        SendQueueWorker.Stop();
        return Task.CompletedTask;
    }

    protected override Task CheckConnectionState(CancellationToken token) 
    {
        ConnectionStateWorker.Stop();
        return Task.CompletedTask;
    }

    public override void SetHost(string host) => throw new NotSupportedException("This client does not support changing the host.");

    public override void SetPort(ushort port) => throw new NotSupportedException("This client does not support changing the port.");

    public override void Connect() => throw new NotSupportedException("This client does not support connecting.");

    public override void Reconnect() => throw new NotSupportedException("This client does not support reconnecting.");

    public override void Disconnect() => throw new NotSupportedException("This client does not support disconnecting.");
}