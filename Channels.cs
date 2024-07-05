using System;

namespace Textures
{
    [Flags]
    public enum Channels : byte
    {
        Red = 1,
        Green = 2,
        Blue = 4,
        Alpha = 8
    }
}
