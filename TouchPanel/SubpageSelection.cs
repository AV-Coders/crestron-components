using AVCoders.Core;
using AVCoders.Crestron.SmartGraphics;
using Crestron.SimplSharpPro.DeviceSupport;
using Serilog;
using Serilog.Context;

namespace AVCoders.Crestron.TouchPanel;

public enum SubpageSelectionType
{
    Toggling,
    Interlocked
}
public record SubpageButtonConfig(ushort ButtonMode, uint PopupPageJoin, string Title, SubpageSelection? RelatedMenu = null, VisibilityChanged? VisibilityEvent = null, string? Pin = null);

public class SubpageSelection : DeviceBase
{
    private readonly List<BasicTriListWithSmartObject> _panels;
    private readonly List<SmartObject> _smartObjects;
    private readonly List<SubpageButtonConfig> _buttonConfig;
    private readonly uint[] _pages;
    private readonly SubpageSelectionType _subpageSelectionType;
    private readonly Pin? _pin;
    private readonly uint _closeJoin;
    private int _activePage;
    private readonly SubpageReferenceListHelper _srlHelper;
    private readonly uint[] _allSelectJoins;

    private const uint SelectJoin = 1;
    private const uint VisibilityJoin = 2;

    private const uint ModeJoin = 1;

    private const uint TitleJoin = 1;

    public static readonly uint JoinIncrement = 10;

    private int? _defaultPage = null;
    private bool _rememberSelection;

    public SubpageSelection(string name, List<BasicTriListWithSmartObject> panels, SubpageSelectionType subpageSelectionType,
        List<SubpageButtonConfig> buttonConfig, uint[] pages, uint smartObjectId, uint closeJoin, Pin? pin = null) : base(name)
    {
        _smartObjects = new List<SmartObject>();
        _srlHelper = new SubpageReferenceListHelper(JoinIncrement, JoinIncrement, JoinIncrement);
        _panels = panels;
        _panels.ForEach(panel =>
        {
            panel.SigChange += HandleButtonPress;
            _smartObjects.Add(panel.SmartObjects![smartObjectId]!);
        });
        _smartObjects.ForEach(smartObject => smartObject.SigChange += ModalButtonPressed);
        _pages = pages;
        _buttonConfig = buttonConfig;
        _subpageSelectionType = subpageSelectionType;
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
        if (!args.Sig.BoolValue)
            return;
        ClearSubpages();
    }

    private void ModalButtonPressed(GenericBase currentDevice, SmartObjectEventArgs args)
    {
        if (args.Sig.Type != eSigType.Bool)
            return;
        if (!args.Sig.BoolValue)
            return;
        if (args.Sig.Number < 4000)
            return;
        Debug($"Modal button {args.Sig.Number} pressed");
        HandleSubpages(args.Sig.Number);
    }


    private void ConfigurePopupButtons()
    {
        Debug("Configuring modal buttons");
        _smartObjects.ForEach(x => x.UShortInput["Set Number of Items"].ShortValue = (short)_buttonConfig.Count);

        for (int i = 0; i < _buttonConfig.Count; i++)
        {
            _smartObjects.ForEach(x => x.BooleanInput[_srlHelper.BooleanJoinFor(i, VisibilityJoin)].BoolValue = true);
            _smartObjects.ForEach(x => x.UShortInput[_srlHelper.AnalogJoinFor(i, ModeJoin)].UShortValue = _buttonConfig[i].ButtonMode);
            _smartObjects.ForEach(x => x.StringInput[_srlHelper.SerialJoinFor(i, TitleJoin)].StringValue = _buttonConfig[i].Title);
        }
    }

    private void HandleSubpages(uint sigNumber)
    {
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
        if(_activePage >= 0)
            HandleMenuItemVisibility(_buttonConfig[_activePage], Visibility.Hidden);
        _activePage = selection;
        if (_rememberSelection)
            _defaultPage = _activePage;
        HandleMenuItemVisibility(_buttonConfig[selection], Visibility.Shown);
        CrestronPanel.Interlock(_panels, _buttonConfig[selection].PopupPageJoin, _pages);
        CrestronPanel.Interlock(_smartObjects, _srlHelper.BooleanJoinFor(selection, SelectJoin), _allSelectJoins);
        Debug($"Showing modal {selection}");
    }

    public void ClearSubpages()
    {
        _activePage = -1;
        CrestronPanel.Interlock(_panels, 0, _pages);
        CrestronPanel.Interlock(_smartObjects, 0, _allSelectJoins);
        _buttonConfig.ForEach( button => HandleMenuItemVisibility(button, Visibility.Hidden));
        Debug("Clearing Subpages");
    }

    private void HandleMenuItemVisibility(SubpageButtonConfig button, Visibility visibility)
    {
        new Thread(_ =>
        {
            button.VisibilityEvent?.Invoke(visibility);
            switch (visibility)
            {
                case Visibility.Hidden:
                    button.RelatedMenu?.PowerOff();
                    break;
                case Visibility.Shown:
                    button.RelatedMenu?.PowerOn();
                    break;
            }
        }).Start();
    }

    private int GetArrayIndexFromButton(uint sigNumber) => (int)((sigNumber - 4001) / 10) - 1;

    public void SetDefaultPage(int? page) => _defaultPage = page;
    
    public void RememberSelection(bool remember)
    {
        _rememberSelection = remember;
    }

    public override void PowerOn()
    {
        if(_defaultPage != null)
            ShowPopupPage((int)_defaultPage);
    }

    public void Restore()
    {
        ShowPopupPage(_activePage);
    }

    public override void PowerOff() => ClearSubpages();
}