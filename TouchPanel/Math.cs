namespace AVCoders.Crestron.TouchPanel;

public static class Math
{
    public static int PercentageFromAnalog(ushort analog)
    {
        double scaledValue = (analog / 65535.0) * 100;

        return (int) scaledValue;
    }
    
    public static ushort PercentageToAnalog(int percentage)
    {
        return (ushort)(percentage * 655.35);
    }

    public static ushort PercentageToRange(ushort input, int max)
    {
        var floating = (max / 100.0);
        return (ushort)(input * floating);
    }

    public static ushort PercentageFromRange(ushort input, int max)
    {
        var floating = (double) input / max;
        return (ushort)(floating * 100);
    }
    
}