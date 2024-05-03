using Math = AVCoders.Crestron.TouchPanel.Math;

namespace TouchPanelTest;


public class MathTest
{
    [Fact]
    public void AnalogToPercentage_ReturnsMinimum()
    {
        Assert.Equal(0, Math.AnalogToPercentage(0));
    }
    
    [Fact]
    public void AnalogToPercentage_ReturnsMaximum()
    {
        Assert.Equal(100, Math.AnalogToPercentage(65535));
    }
    
    [Fact]
    public void AnalogToPercentage_HalfWay()
    {
        Assert.Equal(50, Math.AnalogToPercentage(32768));
    }
    
    [Fact]
    public void PercentageToAnalog_ReturnsMinimum()
    {
        Assert.Equal(0, Math.PercentageToAnalog(0));
    }
    
    [Fact]
    public void PercentageToAnalog_ReturnsMaximum()
    {
        Assert.Equal(65535, Math.PercentageToAnalog(100));
    }
    
    [Fact]
    public void PercentageToAnalog_HalfWay()
    {
        Assert.Equal(32767, Math.PercentageToAnalog(50));
    }
    
    
}