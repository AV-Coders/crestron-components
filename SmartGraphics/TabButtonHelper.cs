namespace AVCoders.Crestron.SmartGraphics;

public static class TabButtonHelper
{
    public static uint GetButtonId(uint sigNumber)
    {
        return (sigNumber + 1) / 2;
    }

    public static uint GetFeedbackSignal(uint buttonIndex)
    {
        return buttonIndex * 2;
    }
}