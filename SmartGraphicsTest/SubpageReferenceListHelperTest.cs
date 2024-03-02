namespace AVCoders.Crestron.SmartGraphics.Tests;

public class SubpageReferenceListHelperTest
{
    private readonly SubpageReferenceListHelper _subpageReferenceListHelper = new(10, 15, 22);

    [Theory]
    [InlineData(4011, 0, 1)]
    [InlineData(4012, 0, 2)]
    [InlineData(4020, 0, 10)]
    [InlineData(4021, 1, 1)]
    [InlineData(4031, 2, 1)]
    public void GetBooleanSigInfo_ReturnsTheCorrectSignalAndIndex(uint join, int expectedIndex, uint expectedJoin)
    {
        var foo = _subpageReferenceListHelper.GetBooleanSigInfo(join);
        
        Assert.Equal(new SubpageReferenceListJoinData(expectedIndex, expectedJoin), foo);
    }
    
    [Theory]
    [InlineData(11, 0, 1)]
    [InlineData(12, 0, 2)]
    [InlineData(25, 0, 15)]
    [InlineData(31, 1, 6)]
    [InlineData(41, 2, 1)]
    public void GetAnalogSigInfo_ReturnsTheCorrectSignalAndIndex(uint join, int expectedIndex, uint expectedJoin)
    {
        var foo = _subpageReferenceListHelper.GetAnalogSigInfo(join);
        
        Assert.Equal(new SubpageReferenceListJoinData(expectedIndex, expectedJoin), foo);
    }
    
    [Theory]
    [InlineData(11, 0, 1)]
    [InlineData(22, 0, 12)]
    [InlineData(25, 0, 15)]
    [InlineData(33, 1, 1)]
    [InlineData(54, 1, 22)]
    public void GetSerialSigInfo_ReturnsTheCorrectSignalAndIndex(uint join, int expectedIndex, uint expectedJoin)
    {
        var foo = _subpageReferenceListHelper.GetSerialSigInfo(join);
        
        Assert.Equal(new SubpageReferenceListJoinData(expectedIndex, expectedJoin), foo);
    }
}