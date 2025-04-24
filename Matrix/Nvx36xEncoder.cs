using Crestron.SimplSharpPro.DM.Streaming;

namespace AvCoders.Crestron.Matrix;

public class Nvx36xEncoder : NvxEncoder
{
    public Nvx36xEncoder(string name, DmNvx36x device) : base(name, device)
    {
    }
}