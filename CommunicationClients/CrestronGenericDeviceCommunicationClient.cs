using AVCoders.Core;
using Crestron.SimplSharpPro;

namespace AVCoders.Crestron.CommunicationClients;

public class CrestronGenericDeviceCommunicationClient : IpComms
{
    public CrestronGenericDeviceCommunicationClient(GenericDevice device)
        : base(device.ConnectedIpList?.FirstOrDefault()?.DeviceIpAddress ?? String.Empty, (ushort) device.ID, device.Description, CommandStringFormat.Ascii)
    {
        ConnectionState = ConnectionState.Disconnected;
        device.OnlineStatusChange += HandleOnlineStatusChange;
        if (device.ConnectedIpList != null)
            device.IpInformationChange += HandleIpInformationChange;
    }

    private void HandleOnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
    {
        ConnectionState = args.DeviceOnLine? ConnectionState.Connected : ConnectionState.Disconnected;
    }

    private void HandleIpInformationChange(GenericBase currentDevice, ConnectedIpEventArgs args)
    {
        if (args.Connected)
            Host = args.DeviceIpAddress;
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

    public override void Connect() => throw new NotSupportedException("This client does not support connecting.");

    public override void Reconnect() => throw new NotSupportedException("This client does not support reconnecting.");

    public override void Disconnect() => throw new NotSupportedException("This client does not support disconnecting.");
}