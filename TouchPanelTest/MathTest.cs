using Math = AVCoders.Crestron.TouchPanel.Math;

namespace TouchPanelTest;


public class MathTest
{
    [Fact]
    public void PercentageFromAnalog_ReturnsMinimum()
    {
        Assert.Equal(0, Math.PercentageFromAnalog(0));
    }
    
    [Fact]
    public void PercentageFromAnalog_ReturnsMaximum()
    {
        Assert.Equal(100, Math.PercentageFromAnalog(65535));
    }
    
    [Fact]
    public void PercentageFromAnalog_HalfWay()
    {
        Assert.Equal(50, Math.PercentageFromAnalog(32768));
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

    [Theory]
    [InlineData(65, 65, 100)]
    [InlineData(0, 65, 0)]
    [InlineData(7, 65, 10)]
    [InlineData(33, 65, 50)]
    public void PercentageFromRange_CalculatesTheCorrectValue(ushort input, int max, ushort expected)
    {
        Assert.Equal(expected, Math.PercentageFromRange(input, max));
    }
    
    [Theory]
    [InlineData(100, 65, 65)]
    [InlineData(0, 65, 0)]
    [InlineData(50, 65, 32)]
    public void PercentageToRange_CalculatesTheCorrectValue(ushort input, int max, ushort expected)
    {
        Assert.Equal(expected, Math.PercentageToRange(input, max));
    }
    
}