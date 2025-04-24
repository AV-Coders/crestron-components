using AVCoders.Matrix;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Streaming;
using Serilog;
using Stream = Crestron.SimplSharpPro.DeviceSupport.Stream;

namespace AvCoders.Crestron.Matrix;

public abstract class NvxEncoder : NvxBase
{
    public NvxEncoder(string name, DmNvxBaseClass device) : base(name, device, AVoIPDeviceType.Encoder)
    {
        if (device.Control.DeviceModeFeedback != eDeviceMode.Transmitter)
            Log.Fatal($"The device at {Device.ID:x2} is not an Encoder");
        device.HdmiIn[1]!.StreamChange += HandleStreamChanges;
        device.BaseEvent += HandleBaseEvent;

        UpdateSyncState();
        UpdateResolution();
    }

    private void HandleBaseEvent(GenericBase device, BaseEventArgs args)
    {
        switch (args.EventId)
        {
            case DMInputEventIds.DeviceModeFeedbackEventId:
                if (Device.Control.DeviceModeFeedback != eDeviceMode.Transmitter)
                    Log.Fatal($"The device at {Device.ID:x2} is not an Encoder");
                break;
        }
    }

    private void HandleStreamChanges(Stream stream, StreamEventArgs args)
    {
        switch (args.EventId)
        {
            case DMInputEventIds.SourceSyncEventId:
            {
                UpdateSyncState();
                return;
            }
            case DMInputEventIds.ResolutionEventId:
            {
                UpdateResolution();
                return;
            }
        }
    }

    private void UpdateResolution()
    {
        try
        {
            InputResolution =
                $"{Device.HdmiIn[1]!.VideoAttributes.HorizontalResolutionFeedback.UShortValue}x{Device.HdmiIn[1]!.VideoAttributes.VerticalResolutionFeedback.UShortValue}:{Device.HdmiIn[1]!.VideoAttributes.FramesPerSecondFeedback.UShortValue}p";
        }
        catch (Exception e)
        {
            OutputResolution = "Exception";
            LogException(e);
        }
    }

    private void UpdateSyncState()
    {
        try
        {
            InputConnectionStatus =
                Device.HdmiIn[1]!.SyncDetectedFeedback.BoolValue
                    ? ConnectionStatus.Connected
                    : ConnectionStatus.Disconnected;
        }
        catch (Exception e)
        {
            LogException(e);
        }
    }

    protected override Task Poll(CancellationToken token)
    {
        StreamAddress = Device.Control.ServerUrlFeedback.StringValue;
        return Task.CompletedTask;
    }
}