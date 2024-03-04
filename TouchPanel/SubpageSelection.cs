using AVCoders.Crestron.Core;
using AVCoders.Crestron.SmartGraphics;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace AVCoders.Crestron.TouchPanel;

public enum SubpageSelectionType
{
    Toggling,
    Interlocked
}
public record SubpageButtonConfig(ushort ButtonMode, uint PopupPageJoin, VisibilityChanged? VisibilityEvent = null, string? Pin = null);

public class SubpageSelection
{
    private readonly string _name;
    private readonly List<BasicTriListWithSmartObject> _panels;
    private readonly List<SmartObject> _smartObjects;
    private readonly List<SubpageButtonConfig> _buttonConfig;
    private readonly uint[] _pages;
    private readonly SubpageSelectionType _subpageSelectionType;
    private readonly List<SubpageSelection> _subMenus;
    private readonly Pin? _pin;
    private readonly uint _closeJoin;
    private int _activePage;
    private bool _enableLogs;
    private readonly SubpageReferenceListHelper _srlHelper;
    private readonly uint[] _allSelectJoins;

    private const uint SelectJoin = 1;
    private const uint VisibilityJoin = 2;

    private const uint ModeJoin = 1;

    public static readonly uint JoinIncrement = 120;

    public SubpageSelection(string name, List<BasicTriListWithSmartObject> panels, SubpageSelectionType subpageSelectionType,
        List<SubpageButtonConfig> buttonConfig, uint[] pages, List<SubpageSelection> subMenus, uint smartObjectId, uint closeJoin,
        Pin? pin = null)
    {
        _smartObjects = new List<SmartObject>();
        _srlHelper = new SubpageReferenceListHelper(JoinIncrement, JoinIncrement, JoinIncrement);
        _name = name;
        _panels = panels;
        _panels.ForEach(panel =>
        {
            panel.SigChange += HandleButtonPress;
            _smartObjects.Add(panel.SmartObjects![smartObjectId]);
        });
        _smartObjects.ForEach(smartObject => smartObject.SigChange += ModalButtonPressed);
        _pages = pages;
        _buttonConfig = buttonConfig;
        _subpageSelectionType = subpageSelectionType;
        _subMenus = subMenus;
        _pin = pin;
        _closeJoin = closeJoin;

        _allSelectJoins = new uint[buttonConfig.Count];
        for (int i = 0; i < buttonConfig.Count; i++)
        {
            _allSelectJoins[i] = _srlHelper.BooleanJoinFor(i, SelectJoin);
        }

        ConfigurePopupButtons();
    }

    private void HandleButtonPress(BasicTriList currentDevice, SigEventArgs args)
    {
        if (args.Sig.Number != _closeJoin)
            return;
        if (args.Sig.Type != eSigType.Bool)
            return;
        if (args.Sig.BoolValue != true)
            return;
        ClearSubpages();
    }

    private void ModalButtonPressed(GenericBase currentDevice, SmartObjectEventArgs args)
    {
        if (args.Sig.Type != eSigType.Bool)
            return;
        if (args.Sig.BoolValue != true)
            return;
        Log($"Modal button {args.Sig.Number} pressed");
        HandleSubpages(args.Sig.Number);
    }


    private void ConfigurePopupButtons()
    {
        Log("Configuring modal buttons");
        _smartObjects.ForEach(x => x.UShortInput["Set Number of Items"].ShortValue = (short)_buttonConfig.Count);

        for (int i = 0; i < _buttonConfig.Count; i++)
        {
            _smartObjects.ForEach(x => x.BooleanInput[_srlHelper.BooleanJoinFor(i, VisibilityJoin)].BoolValue = true);
            _smartObjects.ForEach(x => x.UShortInput[_srlHelper.AnalogJoinFor(i, ModeJoin)].UShortValue = _buttonConfig[i].ButtonMode);
        }
    }

    private void HandleSubpages(uint sigNumber)
    {
        _subMenus.ForEach(x => x.ClearSubpages());
        var selection = GetArrayIndexFromButton(sigNumber);
        if (selection == _activePage && _subpageSelectionType == SubpageSelectionType.Toggling)
        {
            ClearSubpages();
            return;
        }

        if (_pin == null)
            ShowPopupPage(selection);
        else
            _pin.Authenticate(_buttonConfig[selection].Pin, () => ShowPopupPage(selection));
    }

    public void ShowPopupPage(int selection)
    {
        _activePage = selection;
        _buttonConfig[selection].VisibilityEvent?.Invoke(Visibility.Shown);
        CrestronPanel.Interlock(_panels, _buttonConfig[selection].PopupPageJoin, _pages);
        CrestronPanel.Interlock(_smartObjects, _srlHelper.BooleanJoinFor(selection, SelectJoin), _allSelectJoins);
        Log($"Showing modal {selection}");
    }

    public void ClearSubpages()
    {
        _activePage = -1;
        CrestronPanel.Interlock(_panels, 0, _pages);
        CrestronPanel.Interlock(_smartObjects, 0, _allSelectJoins);
        _subMenus.ForEach(menu => menu.ClearSubpages());
        foreach (SubpageButtonConfig subpageButton in _buttonConfig)
        {
            subpageButton.VisibilityEvent?.Invoke(Visibility.Hidden);
        }

        Log("Clearing Subpages");
    }

    private int GetArrayIndexFromButton(uint sigNumber) => (int)((sigNumber - 4001) / 10) - 1;

    public void EnableLogs(bool enable) => _enableLogs = enable;

    private void Log(string message)
    {
        if (_enableLogs)
            CrestronConsole.PrintLine($"{DateTime.Now} - {_name} - SubPageSelection - {message}");
    }

    public void PowerOff() => ClearSubpages();
}