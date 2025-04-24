using Crestron.SimplSharpPro.DM.Streaming;

namespace AvCoders.Crestron.Matrix;

public class Nvx36XSeriesEncoder : NvxEncoder
{
    public Nvx36XSeriesEncoder(string name, DmNvx36x device) : base(name, device)
    {
    }
}