namespace AVCoders.Crestron.SmartGraphics.Tests;

public class TabButtonHelperTest
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(3, 2)]
    [InlineData(5, 3)]
    [InlineData(7, 4)]
    [InlineData(9, 5)]
    [InlineData(11, 6)]
    [InlineData(13, 7)]
    public void GetButtonId_ReturnsTheButtonId(uint input, uint expected)
    {
        Assert.Equal(expected, TabButtonHelper.GetButtonId(input));
    }
    
    [Theory]
    [InlineData(2, 1)]
    [InlineData(4, 2)]
    [InlineData(6, 3)]
    [InlineData(8, 4)]
    [InlineData(10, 5)]
    [InlineData(12, 6)]
    [InlineData(14, 7)]
    public void GetFeedbackSignal_ReturnsTheSignalNumber(uint expected, uint input)
    {
        Assert.Equal(expected, TabButtonHelper.GetFeedbackSignal(input));
    }
}