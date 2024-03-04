using AVCoders.Core;
using AVCoders.Crestron.Core;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace AVCoders.Crestron.TouchPanel;

public class Shutdown : SubPage
{
    public SubpageSelection? Controller = null;
    public PowerStateHandler? PowerStateHandlers;
    private readonly string _name;
    private readonly List<SmartObject> _smartObjects;
    private readonly ShutdownMode _mode;
    private readonly ushort _countdownTime;
    private ushort _timeRemaining;
    private bool _enableLogs;
    private bool _timerRunning;
    
    public const uint ShutdownConfirm = 4011;
    public const uint ShutdownCancel = 4012;

    public Shutdown(string name, List<SmartObject> smartObjects, ShutdownMode mode, ushort countdownTime = 10)
    {
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
        Log($"Button {args.Sig.Number} pressed");
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
            Log("Shutdown thread finished.");
        }).Start();
    }

    private void ShutdownTick()
    {
        _timeRemaining--;
        UpdateRemainingTimeString();
        Log($"Shutdown tick.  Time remaining: {_timeRemaining}");

        if (_timeRemaining < 1)
            DoShutdown();
    }

    private void DoShutdown()
    {
        Log($"Shutting down");
        StopTimer();
        PowerStateHandlers?.Invoke(PowerState.Off);
        UpdateRemainingTimeString();
    }

    private void StopTimer()
    {
        Log($"Stopping timer");
        _timerRunning = false;
        _timeRemaining = _countdownTime;
        UpdateRemainingTimeString();
    }

    private void HandleVisibilityChange(Visibility visibility)
    {
        switch (visibility)
        {
            case Visibility.Shown:
                Log($"Visible");
                _timerRunning = true;
                _timeRemaining = _countdownTime;
                if(_mode == ShutdownMode.Countdown)
                    CreateShutdownTimer();
                break;
            case Visibility.Hidden:
                Log($"Hidden");
                StopTimer();
                break;
        }
    }

    private void UpdateRemainingTimeString() => _smartObjects.ForEach(smartObject => smartObject.UShortInput[11].UShortValue = _timeRemaining);

    public void EnableLogs(bool enable) => _enableLogs = enable;

    private void Log(string message)
    {
        if(_enableLogs)
            CrestronConsole.PrintLine($"{DateTime.Now} - {_name} - Shutdown - {message}");
    }
}