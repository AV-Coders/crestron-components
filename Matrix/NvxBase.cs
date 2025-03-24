using AVCoders.Core;
using AVCoders.Matrix;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM.Streaming;

namespace AvCoders.Crestron.Matrix;

public class NvxCommunicationEmulator : CommunicationClient
{
    public NvxCommunicationEmulator(string name) : base(name)
    {
        ConnectionState = ConnectionState.Disconnected;
    }

    public void SetConnectionState(ConnectionState state) { ConnectionState = state; }

    public override void Send(string message) { }

    public override void Send(byte[] bytes) { }
}

public abstract class NvxBase : AVoIPEndpoint
{
    protected readonly DmNvxBaseClass Device;
    // protected readonly ThreadWorker PollWorker;

    protected NvxBase(string name, DmNvxBaseClass device, AVoIPDeviceType deviceType) : 
        base(name, deviceType, new NvxCommunicationEmulator(GetCommunicationClientName(deviceType, name)))
    {
        Device = device;
        Device.OnlineStatusChange += HandleDeviceOnlineStatus;
        
        HandleDeviceOnlineStatus(Device, new OnlineOfflineEventArgs(Device.IsOnline));
    }

    private void HandleDeviceOnlineStatus(GenericBase currentDevice, OnlineOfflineEventArgs args)
    {
        ((NvxCommunicationEmulator) CommunicationClient).SetConnectionState(args.DeviceOnLine ? ConnectionState.Connected : ConnectionState.Disconnected);
    }

    private static string GetCommunicationClientName(AVoIPDeviceType type, string name) => $"{name} {type.ToString()}";
}