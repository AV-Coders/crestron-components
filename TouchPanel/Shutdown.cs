using AVCoders.Core;
using Microsoft.Extensions.Logging;

namespace AVCoders.Crestron.TouchPanel;

public class Shutdown : SubPage, IDevice
{
    public SubpageSelection? Controller = null;
    public PowerStateHandler? PowerStateHandlers;
    private readonly ILogger _logger;
    private readonly string _name;
    private readonly List<SmartObject> _smartObjects;
    private readonly ShutdownMode _mode;
    private readonly ushort _countdownTime;
    private ushort _timeRemaining;
    private bool _timerRunning;
    
    public const uint ShutdownConfirm = 4011;
    public const uint ShutdownCancel = 4012;

    public Shutdown(string name, List<SmartObject> smartObjects, ShutdownMode mode, ushort countdownTime = 10)
    {
        _logger = LogBase.LoggerFactory.CreateLogger<Shutdown>();
        _smartObjects = smartObjects;
        _mode = mode;
        _name = name;
        _countdownTime = countdownTime;
        _timeRemaining = countdownTime;
        smartObjects.ForEach(x => x.SigChange += HandlePowerButtonPress);
        VisibilityChanged += HandleVisibilityChange;
    }

    private void HandlePowerButtonPress(GenericBase currentDevice, SmartObjectEventArgs args)
    {
        if (args.Sig.Type != eSigType.Bool)
            return;
        if (!args.Sig.BoolValue)
            return;
        Debug($"Button {args.Sig.Number} pressed");
        switch (args.Sig.Number)
        {
            case ShutdownConfirm:
                DoShutdown();
                break;
            case ShutdownCancel:
                StopTimer();
                Controller?.ClearSubpages();
                break;
        }
    }

    private void CreateShutdownTimer()
    {
        new Thread(_ =>
        {
            while (_timerRunning)
            {
                Thread.Sleep(1000);
                if(_timerRunning)
                    ShutdownTick();
            }
            Debug("Shutdown thread finished.");
        }).Start();
    }

    private void ShutdownTick()
    {
        _timeRemaining--;
        UpdateRemainingTimeString();
        Debug($"Shutdown tick.  Time remaining: {_timeRemaining}");

        if (_timeRemaining < 1)
            DoShutdown();
    }

    private void DoShutdown()
    {
        Debug($"Shutting down");
        StopTimer();
        PowerStateHandlers?.Invoke(PowerState.Off);
        UpdateRemainingTimeString();
    }

    private void StopTimer()
    {
        Debug($"Stopping timer");
        _timerRunning = false;
        _timeRemaining = _countdownTime;
        UpdateRemainingTimeString();
    }

    private void HandleVisibilityChange(Visibility visibility)
    {
        switch (visibility)
        {
            case Visibility.Shown:
                Debug($"Visible");
                _timerRunning = true;
                _timeRemaining = _countdownTime;
                if(_mode == ShutdownMode.Countdown)
                    CreateShutdownTimer();
                break;
            case Visibility.Hidden:
                Debug($"Hidden");
                StopTimer();
                break;
        }
    }

    private void UpdateRemainingTimeString() => _smartObjects.ForEach(smartObject => smartObject.UShortInput[11].UShortValue = _timeRemaining);
    
    private void Debug(string message)
    {
        using (_logger.BeginScope(new Dictionary<string, object>
               { ["Class"] = GetType().Name, ["InstanceName"] = _name }))
        {
            _logger.LogDebug("{Message}", message);
        }
    }

    public void PowerOn() => UpdateRemainingTimeString();

    public void PowerOff() => UpdateRemainingTimeString();

    public PowerState GetCurrentPowerState() => PowerState.On;

    public CommunicationState GetCurrentCommunicationState() => CommunicationState.Okay;
}