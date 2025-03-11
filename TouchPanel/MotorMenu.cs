using AVCoders.Crestron.SmartGraphics;

namespace AVCoders.Crestron.TouchPanel;

public class MotorMenu
{
    private readonly List<Motor.Motor> _motors;
    private readonly SubpageReferenceListHelper _srlHelper;
    private readonly string _name;

    private const uint RaiseJoin = 1;
    private const uint LowerJoin = 2;
    private const uint StopJoin = 3;

    private const uint NameJoin = 1;

    public MotorMenu(string name, List<Motor.Motor> motors, List<SmartObject> smartObjects)
    {
        _name = name;
        _motors = motors;
        _srlHelper = new SubpageReferenceListHelper(10, 10, 10);
        
        smartObjects.ForEach(smartObject =>
        {
            smartObject.UShortInput["Set Number of Items"].ShortValue = (short)_motors.Count;
            smartObject.SigChange += HandleMotorPress;
            for (int i = 0; i < _motors.Count; i++)
            {
                smartObject.StringInput[_srlHelper.SerialJoinFor(i, NameJoin)].StringValue = _motors[i].Name;
            }
        });
    }

    private void HandleMotorPress(GenericBase currentDevice, SmartObjectEventArgs args)
    {
        if (args.Sig.Type != eSigType.Bool)
            return;
        if (args.Sig.BoolValue == false)
            return;
        Log($"Button {args.Sig.Number} pressed");
        var joinInfo = _srlHelper.GetSigInfo(args.Sig);
        switch (joinInfo.Join)
        {
            case RaiseJoin:
                _motors[joinInfo.Index].Raise();
                Log($"Raising");
                break;
            case LowerJoin:
                _motors[joinInfo.Index].Lower();
                Log($"Lowering");
                break;
            case StopJoin:
                _motors[joinInfo.Index].Stop();
                Log($"Stopping");
                break;
        }
    }

    private void Log(string message) => CrestronConsole.PrintLine($"{DateTime.Now} - {_name} - Motor Menu - {message}");
}