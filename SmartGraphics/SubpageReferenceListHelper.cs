using Crestron.SimplSharpPro;

namespace AVCoders.Crestron.SmartGraphics;

public record SubpageReferenceListJoinData(int Index, uint Join);

public class SubpageReferenceListHelper
{
    private readonly int _digitalJoinOffset;
    private readonly int _analogJoinOffset;
    private readonly int _serialJoinOffset;

    public static readonly int DigitalJoinOffset = 4010;
    public static readonly int OtherJoinOffset = 10;

    public SubpageReferenceListHelper(int digitalJoinOffset, int analogJoinOffset, int serialJoinOffset)
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
    public SubpageReferenceListJoinData ExtractListJoinNumberFrom(Sig sig)
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

    private SubpageReferenceListJoinData CalculateIndexAndJoin(uint sigNumber, int joinOffset, int sigOffset)
    {
        var rawNumber = sigNumber - sigOffset;
        int index = (int)(rawNumber - 1) / joinOffset;
        uint join = (uint)(rawNumber - (index * joinOffset));
        return new SubpageReferenceListJoinData(index, join);
    }
}