using Crestron.SimplSharpPro.DM.Streaming;

namespace AvCoders.Crestron.Matrix;

public class NvxExxEncoder : NvxEncoder
{
    public NvxExxEncoder(string name, DmNvxE3x device) : base(name, device)
    {
    }
}