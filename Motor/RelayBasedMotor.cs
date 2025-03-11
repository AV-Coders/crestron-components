using AVCoders.Motor;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace AVCoders.Crestron.Motor;

public record Relay(uint Raise, uint Lower);

public class RelayBasedMotor : AVCoders.Motor.Motor
{
    private readonly Relay _relays;
    private readonly CrestronControlSystem _cs;
    private readonly int _holdSeconds;
    private readonly int _debounceTimeMilliseconds;
    private Action? _queuedAction;
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
    }

    private void TriggerRaiseRelays()
    {
        ClearRelays();
        _cs.RelayPorts[_relays.Raise]!.State = true;
        ClearRelaysAfterMoveCompleted();
    }

    public override void Raise()
    {
        _cancellationTokenSource.Cancel();
        Guid thisMove = Guid.NewGuid();
        
        if (CurrentMoveAction == RelayAction.Raise)
            Log("Not triggering the same move direction twice");
        else
        {
            Log($"Raising, move id {thisMove}");
            TriggerRaiseRelays();
        
            CreateMoveTimer(thisMove);
            CurrentMoveId = thisMove;
            CurrentMoveAction = RelayAction.Raise;
        }
    }

    private void TriggerLowerRelays()
    {
        ClearRelays();
        _cs.RelayPorts[_relays.Lower]!.State = true;
        ClearRelaysAfterMoveCompleted();
    }

    public override void Lower()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        Guid thisMove = Guid.NewGuid();
        if (CurrentMoveAction == RelayAction.Lower)
            Log("Not triggering the same move direction twice");
        else
        {
            Log($"Lowering, move id {thisMove}");
            TriggerLowerRelays();

            if (CurrentMoveId != Guid.Empty)
                _queuedAction = TriggerLowerRelays;
        
            CreateMoveTimer(thisMove);
            CurrentMoveId = thisMove;
            CurrentMoveAction = RelayAction.Lower;
        }
    }

    public override void Stop()
    {
        _cancellationTokenSource.Cancel();
        _queuedAction = null;
        ClearRelays();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    private void ClearRelaysAfterMoveCompleted()
    {
        new Task(() =>
        {
            Task.Delay(TimeSpan.FromSeconds(_holdSeconds), _cancellationTokenSource.Token).Wait(_cancellationTokenSource.Token);
            ClearRelays();
        }).Start();
    }

    private void ClearRelays()
    {
        _cs.RelayPorts[_relays.Raise]!.State = false;
        _cs.RelayPorts[_relays.Lower]!.State = false;
        Task.Delay(TimeSpan.FromMilliseconds(_debounceTimeMilliseconds), _cancellationTokenSource.Token).Wait();
    }

    private new void Log(string message) => CrestronConsole.PrintLine($"{DateTime.Now} - {Name} - RelayBasedMotor - {message}");
}