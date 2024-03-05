using Crestron.SimplSharpPro.DeviceSupport;

namespace AVCoders.Crestron.TouchPanel;

public record NotificationJoins(uint Notify, uint Warn, uint[] RelatedPages, uint MessageText, uint Close);

public class Notifications
{
    private readonly NotificationJoins _joins;
    private readonly BasicTriList _panel;
    private Guid _currentWarning;

    public Notifications(BasicTriList panel, NotificationJoins joins)
    {
        _joins = joins;
        _panel = panel;
        _panel.SigChange += PanelButtonPressed;
    }

    public void Notify(string message, int timeout = 3000)
    {
        _panel.StringInput[_joins.MessageText].StringValue = message;
        SetBanner(_joins.Notify);
        ClearTimer(x => SetBanner(0), timeout);
    }

    private static void ClearTimer(CTimerCallbackFunction action, int timeout) => new CTimer(action, timeout);

    public Guid Warn(string message)
    {
        _currentWarning = Guid.NewGuid();
        _panel.StringInput[_joins.MessageText].StringValue = message;
        SetBanner(_joins.Warn);
        return _currentWarning;
    }

    public void ClearWarning(Guid wanringId)
    {
        if (_currentWarning == wanringId)
        {
            SetBanner(0);
            _currentWarning = Guid.Empty;
        }
    }

    private void PanelButtonPressed(BasicTriList currentDevice, SigEventArgs args)
    {
        if (args.Sig.Type != eSigType.Bool)
            return;
        if (args.Sig.BoolValue == false)
            return;
        if (args.Sig.Number == _joins.Close)
        {
            SetBanner(0);
            _currentWarning = Guid.Empty;
        }
    }

    private void SetBanner(uint activeBanner)
    {
        CrestronPanel.Interlock(_panel, activeBanner, _joins.RelatedPages);
    }
}