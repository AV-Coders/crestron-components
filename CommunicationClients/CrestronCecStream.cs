using System.Text;
using AVCoders.Core;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using CrestronStream = Crestron.SimplSharpPro.DeviceSupport.Stream;

namespace AVCoders.Crestron.CommunicationClients;

/// <summary>
/// A <see cref="SerialClient"/> over a Crestron DM <see cref="Cec"/> object (for example
/// <c>DmNvx360.HdmiIn[1].StreamCec</c>). Frames the endpoint receives on the CEC bus are delivered
/// through <see cref="CommunicationClient.ResponseHandlers"/> / <see cref="CommunicationClient.ResponseByteHandlers"/>;
/// frames given to <see cref="Send(string)"/> are put on the bus verbatim.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Cec.CecChange"/> fires for three distinct things, distinguished only by
/// <see cref="CecEventArgs.EventId"/>: <see cref="CecEventIds.CecMessageReceivedEventId"/> (a frame arrived
/// and <see cref="Cec.Received"/> changed), <see cref="CecEventIds.ErrorFeedbackEventId"/> (the
/// <see cref="Cec.ErrorFeedback"/> NACK flag toggled) and <see cref="CecEventIds.PhysicalAddressEventId"/>
/// (<see cref="Cec.PhysicalAddress"/> changed). <see cref="Cec.Received"/> is a latched sig that keeps the
/// last frame, so only the <c>CecMessageReceivedEventId</c> event is forwarded as a response. Forwarding on
/// every event replayed the stale last frame to consumers whenever a NACK or physical-address change came
/// through - observed on DM-NVX-360 / 4-series, where a replayed Standby broadcast was mistaken for a fresh
/// user action.
/// </para>
/// <para>
/// NACKs are reported through <see cref="NackHandlers"/> and logged at Debug; they are never delivered as
/// responses.
/// </para>
/// <para>
/// <see cref="CommunicationClient.ConnectionState"/> follows the online status of the Crestron device that
/// owns the CEC object, resolved from <see cref="Cec.Owner"/> (for NVX HDMI inputs that is the
/// <see cref="CrestronStream"/> whose <see cref="CrestronStream.Owner"/> is the device) or supplied
/// explicitly via the constructor's <c>device</c> parameter.
/// </para>
/// </remarks>
public class CrestronCecStream : SerialClient
{
    private readonly Cec _stream;
    private readonly GenericBase? _device;

    /// <summary>
    /// Raised with the new value of <see cref="Cec.ErrorFeedback"/> whenever it changes: <c>true</c> when the
    /// last frame sent got no ACK from the connected device (wrong logical address, or nothing listening),
    /// <c>false</c> once a frame is sent successfully again.
    /// </summary>
    public BoolHandler? NackHandlers;

    /// <summary>
    /// Wraps a Crestron DM <see cref="Cec"/> object.
    /// </summary>
    /// <param name="stream">The CEC object, e.g. <c>DmNvx360.HdmiIn[1].StreamCec</c>.</param>
    /// <param name="name">The client name used for logging.</param>
    /// <param name="device">
    /// The Crestron device whose online status drives <see cref="CommunicationClient.ConnectionState"/>.
    /// When <c>null</c> the device is resolved from <see cref="Cec.Owner"/>; pass it explicitly for CEC objects
    /// whose owner chain does not lead back to a <see cref="GenericBase"/>.
    /// </param>
    public CrestronCecStream(Cec stream, string name, GenericBase? device = null)
        : base(name, "CEC Stream", 0, CommandStringFormat.Hex)
    {
        using (PushProperties("Constructor"))
        {
            _stream = stream;
            _device = device ?? ResolveOwningDevice(stream);

            if (_device == null)
            {
                LogWarning("No owning device found for the CEC object (owner is {OwnerType}); ConnectionState will not be tracked",
                    stream.Owner?.GetType().Name ?? "null");
            }
            else
            {
                // Seed from the current status; a device registered before this client is built may already
                // be online, and OnlineStatusChange only fires on transitions.
                ConnectionState = _device.IsOnline ? ConnectionState.Connected : ConnectionState.Disconnected;
                _device.OnlineStatusChange += HandleOnlineStatusChange;
            }

            _stream.CecChange += HandleCecResponse;
        }
    }

    /// <summary>
    /// Walks <see cref="Cec.Owner"/> back to the Crestron device. DM endpoints (NVX, DM-RMC, cards) build their
    /// CEC objects with either the device itself or the HDMI <see cref="CrestronStream"/> as owner.
    /// </summary>
    private static GenericBase? ResolveOwningDevice(Cec stream) => stream.Owner switch
    {
        GenericBase device => device,
        CrestronStream { Owner: GenericBase device } => device,
        _ => null
    };

    private void HandleOnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
    {
        ConnectionState = args.DeviceOnLine ? ConnectionState.Connected : ConnectionState.Disconnected;
    }

    /// <summary>
    /// Filters <see cref="Cec.CecChange"/> by event id. Only <see cref="CecEventIds.CecMessageReceivedEventId"/>
    /// carries a new frame; the other ids leave <see cref="Cec.Received"/> holding the previous frame, so
    /// forwarding on them would replay stale data.
    /// </summary>
    private void HandleCecResponse(Cec cecDevice, CecEventArgs args)
    {
        switch (args.EventId)
        {
            case CecEventIds.CecMessageReceivedEventId:
                string received = cecDevice.Received.StringValue;
                InvokeResponseHandlers(received, Encoding.Latin1.GetBytes(received));
                break;
            case CecEventIds.ErrorFeedbackEventId:
                HandleNack(cecDevice.ErrorFeedback.BoolValue);
                break;
            case CecEventIds.PhysicalAddressEventId:
                using (PushProperties("HandleCecResponse"))
                {
                    LogDebug("Physical address changed: {PhysicalAddress}",
                        BitConverter.ToString(Encoding.Latin1.GetBytes(cecDevice.PhysicalAddress.StringValue)));
                }
                break;
            default:
                using (PushProperties("HandleCecResponse"))
                {
                    LogDebug("Ignoring CEC event id {EventId}", args.EventId);
                }
                break;
        }
    }

    private void HandleNack(bool nack)
    {
        using (PushProperties("HandleNack"))
        {
            LogDebug(nack
                ? "CEC NACK: the last frame was not acknowledged"
                : "CEC NACK cleared");
            try
            {
                NackHandlers?.Invoke(nack);
            }
            catch (Exception e)
            {
                LogException(e, "A NACK handler threw an exception");
            }
        }
    }

    public override void Send(string message)
    {
        _stream.Send.StringValue = message;
        InvokeRequestHandlers(message);
    }

    public override void Send(byte[] bytes)
    {
        _stream.Send.CharacterArrayValue = bytes.Select(b => (char)b).ToArray();
        InvokeRequestHandlers(bytes);
    }

    public override void ConfigurePort(SerialSpec serialSpec)
    {
        using (PushProperties("ConfigurePort"))
        {
            LogError("This port can't be configured");
        }
    }

    public override void Send(char[] chars) => _stream.Send.CharacterArrayValue = chars;
}
