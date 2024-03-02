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
        var actual = _subpageReferenceListHelper.GetBooleanSigInfo(join);
        
        Assert.Equal(new SubpageReferenceListJoinData(expectedIndex, expectedJoin), actual);
    }
    
    [Theory]
    [InlineData(11, 0, 1)]
    [InlineData(12, 0, 2)]
    [InlineData(25, 0, 15)]
    [InlineData(31, 1, 6)]
    [InlineData(41, 2, 1)]
    public void GetAnalogSigInfo_ReturnsTheCorrectSignalAndIndex(uint join, int expectedIndex, uint expectedJoin)
    {
        var actual = _subpageReferenceListHelper.GetAnalogSigInfo(join);
        
        Assert.Equal(new SubpageReferenceListJoinData(expectedIndex, expectedJoin), actual);
    }
    
    [Theory]
    [InlineData(11, 0, 1)]
    [InlineData(22, 0, 12)]
    [InlineData(25, 0, 15)]
    [InlineData(33, 1, 1)]
    [InlineData(54, 1, 22)]
    public void GetSerialSigInfo_ReturnsTheCorrectSignalAndIndex(uint join, int expectedIndex, uint expectedJoin)
    {
        var actual = _subpageReferenceListHelper.GetSerialSigInfo(join);
        
        Assert.Equal(new SubpageReferenceListJoinData(expectedIndex, expectedJoin), actual);
    }
    
    

    [Theory]
    [InlineData(4011, 0, 1)]
    [InlineData(4012, 0, 2)]
    [InlineData(4020, 0, 10)]
    [InlineData(4021, 1, 1)]
    [InlineData(4031, 2, 1)]
    public void BooleanJoinFor_ReturnsTheCorrectJoinNumber(uint expectedJoin, int index, uint join)
    {
        var actual = _subpageReferenceListHelper.BooleanJoinFor(index, join);
        
        Assert.Equal(expectedJoin, actual);
    }
    
    [Theory]
    [InlineData(11, 0, 1)]
    [InlineData(12, 0, 2)]
    [InlineData(25, 0, 15)]
    [InlineData(31, 1, 6)]
    [InlineData(41, 2, 1)]
    public void AnalogJoinFor_ReturnsTheCorrectJoinNumber(uint expectedJoin, int index, uint join)
    {
        var actual = _subpageReferenceListHelper.AnalogJoinFor(index, join);
        
        Assert.Equal(expectedJoin, actual);
    }
    
    [Theory]
    [InlineData(11, 0, 1)]
    [InlineData(22, 0, 12)]
    [InlineData(25, 0, 15)]
    [InlineData(33, 1, 1)]
    [InlineData(54, 1, 22)]
    public void SerialJoinFor_ReturnsTheCorrectJoinNumber(uint expectedJoin, int index, uint join)
    {
        var actual = _subpageReferenceListHelper.SerialJoinFor(index, join);
        
        Assert.Equal(expectedJoin, actual);
    }
}