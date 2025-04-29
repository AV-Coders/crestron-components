using AVCoders.Core;
using Crestron.SimplSharpPro;

namespace AVCoders.Crestron.CommunicationClients;

public class CrestronGenericDeviceCommunicationClient : IpComms
{
    public CrestronGenericDeviceCommunicationClient(GenericDevice device) : base(String.Empty, 0, device.Description)
    {
        device.OnlineStatusChange += HandleOnlineStatusChange;
    }

    private void HandleOnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
    {
        ConnectionState = args.DeviceOnLine? ConnectionState.Connected : ConnectionState.Disconnected;
    }

    public override void Send(string message) { }

    public override void Send(byte[] bytes) { }

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

    public override void SetHost(string host) { }

    public override void SetPort(ushort port) { }

    public override void Connect() { }

    public override void Reconnect() { }

    public override void Disconnect() { }
}