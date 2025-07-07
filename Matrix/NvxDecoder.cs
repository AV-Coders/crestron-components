using AVCoders.Core;
using AVCoders.Matrix;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Streaming;
using Serilog;
using Stream = Crestron.SimplSharpPro.DeviceSupport.Stream;

namespace AvCoders.Crestron.Matrix;

public class NvxDecoder : NvxBase
{
    public NvxDecoder(string name, DmNvxBaseClass device) : base(name, device, AVEndpointType.Decoder)
    {
        if(device.Control.DeviceModeFeedback != eDeviceMode.Receiver)
            Log.Fatal($"The device at {Device.ID:x2} is not a Decoder");
        device.HdmiOut.StreamChange += HandleStreamChanges;
        device.BaseEvent += HandleBaseEvent;
        
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

    private void HandleBaseEvent(GenericBase device, BaseEventArgs args)
    {
        switch (args.EventId)
        {
            case DMInputEventIds.DeviceModeFeedbackEventId:
                if(Device.Control.DeviceModeFeedback != eDeviceMode.Receiver)
                    Log.Fatal($"The device at {Device.ID:x2} is not a Decoder");
                break;
        }
    }

    public void SetInput(string serverUrl)
    {
        Device.Control.ServerUrl.StringValue = serverUrl;
    }

    public void SetInput(DmNvxBaseClass source)
    {
        if(source.Control.DeviceModeFeedback == eDeviceMode.Receiver)
            throw new InvalidOperationException($"You can't route a decoder to a decoder.  Encoder: {source.ID:x2}, Decoder: {Device.ID:x2}");
        SetInput(source.Control.ServerUrl.StringValue);
    }

    private void UpdateResolution()
    {
        try
        {
            OutputResolution =
                $"{Device.HdmiOut.VideoAttributes.HorizontalResolutionFeedback.UShortValue}x{Device.HdmiOut.VideoAttributes.VerticalResolutionFeedback.UShortValue}:{Device.HdmiOut.VideoAttributes.FramesPerSecondFeedback.UShortValue}p";
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
            OutputConnectionStatus =
                Device.HdmiOut.SyncDetectedFeedback.BoolValue 
                    ? ConnectionState.Connected 
                    : ConnectionState.Disconnected;
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