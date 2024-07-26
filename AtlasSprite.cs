using System;
using System.Numerics;
using Unmanaged;

namespace Textures
{
    public struct AtlasSprite
    {
        public FixedString name;
        public Vector4 region;

        public AtlasSprite(ReadOnlySpan<char> name, Vector4 region)
        {
            this.name = name;
            this.region = region;
        }

        public AtlasSprite(FixedString name, Vector4 region)
        {
            this.name = name;
            this.region = region;
        }

        public readonly override string ToString()
        {
            Span<char> buffer = stackalloc char[128];
            int length = name.CopyTo(buffer);
            buffer[length++] = ' ';
            buffer[length++] = '[';
            region.X.TryFormat(buffer[length..], out length);
            buffer[length++] = ',';
            region.Y.TryFormat(buffer[length..], out length);
            buffer[length++] = ',';
            region.Z.TryFormat(buffer[length..], out length);
            buffer[length++] = ',';
            region.W.TryFormat(buffer[length..], out length);
            buffer[length++] = ']';
            return new string(buffer[..length]);
        }
    }
}
