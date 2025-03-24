using AVCoders.Matrix;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Streaming;
using Stream = Crestron.SimplSharpPro.DeviceSupport.Stream;

namespace AvCoders.Crestron.Matrix;

public class NvxDecoder : NvxBase
{
    public NvxDecoder(string name, DmNvxBaseClass device) : base(name, device, AVoIPDeviceType.Decoder)
    {
        device.HdmiOut.StreamChange += HandleStreamChanges;
        if(device.Control.DeviceModeFeedback != eDeviceMode.Receiver)
            throw new InvalidOperationException($"The device at {Device.ID:x2} is not a Decoder");
        
        UpdateSyncState();
        UpdateResolution();
    }

    private void HandleStreamChanges(Stream stream, StreamEventArgs args)
    {
        switch (args.EventId)
        {
            case DMOutputEventIds.SyncDetectedEventId:
            {
                UpdateSyncState();
                return;
            }
            case DMOutputEventIds.ResolutionEventId:
            {
                UpdateResolution();
                return;
            }
        }
    }

    public void SetInput(string serverUrl)
    {
        Device.Control.ServerUrl.StringValue = serverUrl;
    }

    public void SetInput(DmNvxBaseClass source)
    {
        if(source.Control.DeviceModeFeedback == eDeviceMode.Receiver)
            throw new InvalidOperationException($"You can't route a receiver to a receiver.  Transmitter: {source.ID:x2}, Receiver: {Device.ID:x2}");
    }

    private void UpdateResolution() => OutputResolution = 
            $"{Device.HdmiOut.VideoAttributes.HorizontalResolutionFeedback.UShortValue}x{Device.HdmiOut.VideoAttributes.VerticalResolutionFeedback.UShortValue}:{Device.HdmiOut.VideoAttributes.FramesPerSecondFeedback.UShortValue}p";

    private void UpdateSyncState() => OutputConnectionStatus = 
            Device.HdmiOut.SyncDetectedFeedback.BoolValue ? ConnectionStatus.Connected : ConnectionStatus.Disconnected;
}