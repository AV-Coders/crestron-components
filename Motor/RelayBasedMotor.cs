using AVCoders.Motor;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Serilog;

namespace AVCoders.Crestron.Motor;

public record Relay(uint Raise, uint Lower);

public class RelayBasedMotor : AVCoders.Motor.Motor
{
    private readonly Relay _relays;
    private readonly CrestronControlSystem _cs;
    private readonly int _holdSeconds;
    private readonly int _debounceTimeMilliseconds;
    private CancellationTokenSource _cancellationTokenSource = new ();

    public RelayBasedMotor(string name, Relay relays, CrestronControlSystem cs, RelayAction powerOnAction, int moveSeconds = 25, int holdSeconds = 1, int debounceTimeMilliseconds = 500)
        : base(name, powerOnAction, moveSeconds)
    {
        if (!cs.SupportsRelay)
            throw new InvalidOperationException("This control system doesn't support relays");
        _relays = relays;
        _cs = cs;
        _holdSeconds = holdSeconds;
        _debounceTimeMilliseconds = debounceTimeMilliseconds;

        if(cs.RelayPorts[_relays.Raise]!.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
            throw new InvalidOperationException($"Raise relay registration failed.  Reason - {cs.RelayPorts[_relays.Raise]!.DeviceRegistrationFailureReason.ToString()}");
        if(cs.RelayPorts[_relays.Lower]!.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
            throw new InvalidOperationException($"Lower relay registration failed.  Reason - {cs.RelayPorts[_relays.Lower]!.DeviceRegistrationFailureReason.ToString()}");
    }

    ~RelayBasedMotor()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }

    public override void Raise()
    {
        if (CurrentMoveAction == RelayAction.Raise)
            Log.Information("Not triggering the same move direction twice");
        else
            TriggerRelay(RelayAction.Raise);
    }

    public override void Lower()
    {
        if (CurrentMoveAction == RelayAction.Lower)
            Log.Information("Not triggering the same move direction twice");
        else
            TriggerRelay(RelayAction.Lower);
    }

    private void TriggerRelay(RelayAction action)
    {
        Guid thisMove = Guid.NewGuid();
        Log.Information($"Action: {action.ToString()}, move id: {thisMove}");
        CancelAndCreateANewToken();
        ClearRelays();
        _cs.RelayPorts[action == RelayAction.Lower? _relays.Lower : _relays.Raise]!.State = true;
        new Task(() =>
        {
            Task.Delay(TimeSpan.FromSeconds(_holdSeconds), _cancellationTokenSource.Token).Wait(_cancellationTokenSource.Token);
            ClearRelays();
        }).Start();
        CreateMoveTimer(thisMove, ClearRelays);
        CurrentMoveId = thisMove;
        CurrentMoveAction = action;
    }

    public override void Stop()
    {
        CancelAndCreateANewToken();
        ClearRelays();
        CurrentMoveAction = RelayAction.None;
    }

    private void ClearRelays()
    {
        _cs.RelayPorts[_relays.Raise]!.State = false;
        _cs.RelayPorts[_relays.Lower]!.State = false;
        Task.Delay(TimeSpan.FromMilliseconds(_debounceTimeMilliseconds), _cancellationTokenSource.Token).Wait();
    }

    private void CancelAndCreateANewToken()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
    }
}