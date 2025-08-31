using AVCoders.Core;
using Crestron.SimplSharpPro;

namespace AVCoders.Crestron.SmartGraphics;

public abstract class SrlPage : DeviceBase
{
    public const uint DefaultJoinIncrement = 10;
    
    protected readonly SubpageReferenceListHelper SrlHelper;
    protected readonly List<SmartObject> SmartObjects;
    protected readonly uint JoinIncrement;

    public SrlPage(string name, List<SmartObject> smartObjects, uint joinIncrement = DefaultJoinIncrement) : base(name, CommunicationClient.None)
    {
        SmartObjects = smartObjects;
        JoinIncrement = joinIncrement;
        SrlHelper = new SubpageReferenceListHelper(joinIncrement, joinIncrement, joinIncrement);
    }
}