﻿using AVCoders.Core;
using AVCoders.Matrix;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
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
        Device.PreviewImage.DmNvxPreviewImagePropertyChange += HandlePreviewImageChange;
        Device.OnlineStatusChange += HandleDeviceOnlineStatus;
        
        
        HandleDeviceOnlineStatus(Device, new OnlineOfflineEventArgs(Device.IsOnline));
    }

    private void HandlePreviewImageChange(object sender, GenericEventArgs args)
    {
        switch (args.EventId)
        {
            case DMOutputEventIds.PreviewImageEnabledEventId:
                UpdatePreviewImageUrl();
                return;
            case DMOutputEventIds.PreviewImageDisabledEventId:
                PreviewUrl = String.Empty;
                return;
        }
    }

    private void UpdatePreviewImageUrl()
    {
        ushort maxVertRes = 0;
        uint indexOfMaxRes = 0;
        for (uint i = 0; i < Device.PreviewImage.PreviewImages.ImageDetails.Count; i++)
        {
            if (Device.PreviewImage.PreviewImages.ImageDetails[i]!.HeightFeedback.UShortValue > maxVertRes)
            {
                maxVertRes = Device.PreviewImage.PreviewImages.ImageDetails[i]!.HeightFeedback.UShortValue;
                indexOfMaxRes = i;
            }
        }

        PreviewUrl = Device.PreviewImage.PreviewImages.ImageDetails[indexOfMaxRes]?.Ipv4PathFeedback.StringValue ?? String.Empty;
    }

    private void HandleDeviceOnlineStatus(GenericBase currentDevice, OnlineOfflineEventArgs args)
    {
        ((NvxCommunicationEmulator) CommunicationClient).SetConnectionState(args.DeviceOnLine ? ConnectionState.Connected : ConnectionState.Disconnected);
    }

    private static string GetCommunicationClientName(AVoIPDeviceType type, string name) => $"{name} {type.ToString()}";
}