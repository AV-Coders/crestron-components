using Crestron.SimplSharpPro;

namespace AVCoders.Crestron.SmartGraphics;

public record SubpageReferenceListJoinData(int Index, uint Join);

public class SubpageReferenceListHelper
{
    private readonly uint _digitalJoinOffset;
    private readonly uint _analogJoinOffset;
    private readonly uint _serialJoinOffset;

    public static readonly uint DigitalJoinOffset = 4010;
    public static readonly uint OtherJoinOffset = 10;

    public SubpageReferenceListHelper(uint digitalJoinOffset, uint analogJoinOffset, uint serialJoinOffset)
    {
        _digitalJoinOffset = digitalJoinOffset;
        _analogJoinOffset = analogJoinOffset;
        _serialJoinOffset = serialJoinOffset;
    }

    /// <summary>
    /// Extracts the list index and join number from a given Sig.
    /// </summary>
    /// <param name="sig">The Sig object from which to extract the list join number.</param>
    /// <returns>The SubpageReferenceListJoinData containing the extracted index and join number.</returns>
    /// <exception cref="InvalidDataException">Thrown when encountering an unsupported Sig type.</exception>
    public SubpageReferenceListJoinData GetSigInfo(Sig sig)
    {
        return sig.Type switch
        { 
            eSigType.Bool => GetBooleanSigInfo(sig.Number),
            eSigType.UShort => GetAnalogSigInfo(sig.Number),
            eSigType.String => GetSerialSigInfo(sig.Number),
            _ => throw new InvalidDataException($"Unsupported Sig type: {sig.Type.ToString()}.  Number: {sig.Number}")
        };
    }

    /// <summary>
    /// Gets the SubpageReferenceListJoinData for an analog Sig.
    /// </summary>
    /// <param name="sigNumber">The number of the analog Sig from the Subpage Reference List.</param>
    /// <returns>A SubpageReferenceListJoinData containing the list index and VT Pro join number.</returns>
    public SubpageReferenceListJoinData GetAnalogSigInfo(uint sigNumber) =>
        CalculateIndexAndJoin(sigNumber, _analogJoinOffset, OtherJoinOffset);

    /// <summary>
    /// Gets the SubpageReferenceListJoinData for a serial Sig.
    /// </summary>
    /// <param name="sigNumber">The number of the serial Sig from the Subpage Reference List.</param>
    /// <returns>A SubpageReferenceListJoinData containing the list index and VT Pro join number.</returns>
    public SubpageReferenceListJoinData GetSerialSigInfo(uint sigNumber) =>
        CalculateIndexAndJoin(sigNumber, _serialJoinOffset, OtherJoinOffset);

    /// <summary>
    /// Gets the SubpageReferenceListJoinData for a boolean Sig.
    /// </summary>
    /// <param name="sigNumber">The number of the boolean Sig from the Subpage Reference List.</param>
    /// <returns>A SubpageReferenceListJoinData containing the list index and VT Pro join number.</returns>
    public SubpageReferenceListJoinData GetBooleanSigInfo(uint sigNumber) =>
        CalculateIndexAndJoin(sigNumber, _digitalJoinOffset, DigitalJoinOffset);

    private SubpageReferenceListJoinData CalculateIndexAndJoin(uint sigNumber, uint joinOffset, uint sigOffset)
    {
        var rawNumber = sigNumber - sigOffset;
        int index = (int)((rawNumber - 1) / joinOffset);
        uint join = (uint)(rawNumber - (index * joinOffset));
        return new SubpageReferenceListJoinData(index, join);
    }

    /// <summary>
    /// Returns the BooleanInput for a Subpage reference list, based on the VT Pro join number and the instance
    /// </summary>
    /// <param name="index">The zero-based index of the Subpage reference list.</param>
    /// <param name="join">The digital join from VT Pro.</param>
    /// <returns>A uint to be used as the BooleanInput index for a Subpage reference list.</returns>
    public uint BooleanJoinFor(int index, uint join) =>
        CalculateSmartGraphicsJoin(index, join, _digitalJoinOffset, DigitalJoinOffset);

    

    /// <summary>
    /// Returns the UShortInput for a Subpage reference list, based on the VT Pro join number and the instance
    /// </summary>
    /// <param name="index">The zero-based index of the Subpage reference list.</param>
    /// <param name="join">The analog join from VT Pro.</param>
    /// <returns>A uint to be used as the UShortInput index for a Subpage reference list.</returns>
    public uint AnalogJoinFor(int index, uint join) =>
        CalculateSmartGraphicsJoin(index, join, _analogJoinOffset, OtherJoinOffset);
    
    

    /// <summary>
    /// Returns the StringInput for a Subpage reference list, based on the VT Pro join number and the instance
    /// </summary>
    /// <param name="index">The zero-based index of the Subpage reference list.</param>
    /// <param name="join">The serial join from VT Pro.</param>
    /// <returns>A uint to be used as the StringInput index for a Subpage reference list.</returns>
    public uint SerialJoinFor(int index, uint join) =>
        CalculateSmartGraphicsJoin(index, join, _serialJoinOffset, OtherJoinOffset);
    
    private uint CalculateSmartGraphicsJoin(int index, uint join, uint joinOffset, uint sigOffset)
    {
        uint rawNumber = (uint)(index * joinOffset) + join;
        return rawNumber + sigOffset;
    }
}