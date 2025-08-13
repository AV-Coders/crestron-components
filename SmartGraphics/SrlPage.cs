using AVCoders.Core;
using Crestron.SimplSharpPro;

namespace AVCoders.Crestron.SmartGraphics;

public abstract class SrlPage : DeviceBase
{
    public const uint DefaultJoinIncrement = 10;
    
    protected readonly SubpageReferenceListHelper _srlHelper;
    protected readonly List<SmartObject> SmartObjects;
    
    public SrlPage(string name, List<SmartObject> smartObjects, uint joinIncrement = DefaultJoinIncrement) : base(name)
    {
        SmartObjects = smartObjects;
        _srlHelper = new SubpageReferenceListHelper(joinIncrement, joinIncrement, joinIncrement);
    }
}