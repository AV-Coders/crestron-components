using AVCoders.Core;
using AVCoders.Crestron.SmartGraphics;

namespace AVCoders.Crestron.TouchPanel;

public enum MotorType
{
    Unknown,
    Roller,
    Curtain
}
public record MotorInfo(Motor.Motor Motor, MotorType Type);


public class MotorMenu : SrlPage
{
    private readonly List<MotorInfo> _motors;

    private const uint RaiseJoin = 1;
    private const uint LowerJoin = 2;
    private const uint StopJoin = 3;

    private const uint NameJoin = 4;

    public MotorMenu(string name, List<MotorInfo> motors, List<SmartObject> smartObjects) : base(name, smartObjects)
    {
        _motors = motors;
        
        smartObjects.ForEach(smartObject =>
        {
            smartObject.UShortInput["Set Number of Items"].ShortValue = (short)_motors.Count;
            smartObject.SigChange += HandleMotorPress;
            for (int i = 0; i < _motors.Count; i++)
            {
                smartObject.StringInput[SrlHelper.SerialJoinFor(i, NameJoin)].StringValue = _motors[i].Motor.Name;
                smartObject.StringInput[SrlHelper.SerialJoinFor(i, RaiseJoin)].StringValue = _motors[i].Type switch
                {
                    MotorType.Roller => "Up",
                    MotorType.Curtain => "Open",
                    _ => throw new ArgumentOutOfRangeException()
                };
                smartObject.StringInput[SrlHelper.SerialJoinFor(i, LowerJoin)].StringValue = _motors[i].Type switch
                {
                    MotorType.Roller => "Down",
                    MotorType.Curtain => "Close",
                    _ => throw new ArgumentOutOfRangeException()
                };
                ushort iconMode = _motors[i].Type switch
                {
                    MotorType.Roller => 0,
                    MotorType.Curtain => 1,
                    _ => throw new ArgumentOutOfRangeException()
                };
                smartObject.UShortInput[SrlHelper.AnalogJoinFor(i, RaiseJoin)].UShortValue = iconMode;
                smartObject.UShortInput[SrlHelper.AnalogJoinFor(i, LowerJoin)].UShortValue = iconMode;
            }
        });
    }

    private void HandleMotorPress(GenericBase currentDevice, SmartObjectEventArgs args)
    {
        if (args.Sig.Type != eSigType.Bool)
            return;
        if (args.Sig.BoolValue == false)
            return;
        Debug($"Button {args.Sig.Number} pressed");
        var joinInfo = SrlHelper.GetSigInfo(args.Sig);
        switch (joinInfo.Join)
        {
            case RaiseJoin:
                _motors[joinInfo.Index].Motor.Raise();
                Debug($"Raising");
                break;
            case LowerJoin:
                _motors[joinInfo.Index].Motor.Lower();
                Debug($"Lowering");
                break;
            case StopJoin:
                _motors[joinInfo.Index].Motor.Stop();
                Debug($"Stopping");
                break;
        }
    }

    public override void PowerOn() { }

    public override void PowerOff() { }
}