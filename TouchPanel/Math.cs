namespace AVCoders.Crestron.TouchPanel;

public static class Math
{
    public static int AnalogToPercentage(ushort analog)
    {
        double scaledValue = (analog / 65535.0) * 100;

        return (int) scaledValue;
    }
    
    public static ushort PercentageToAnalog(int percentage)
    {
        return (ushort)(percentage * 655.35);
    }
}