using System;
using System.Numerics;
using Unmanaged;

namespace Textures
{
    public struct AtlasSprite
    {
        /// <summary>
        /// Unique name of the sprite.
        /// </summary>
        public ASCIIText256 name;

        /// <summary>
        /// Texture coordinates for where this sprite is
        /// within its original atlas texture.
        /// </summary>
        public Vector4 region;

        public AtlasSprite(ReadOnlySpan<char> name, Vector4 region)
        {
            this.name = new(name);
            this.region = region;
        }

        public AtlasSprite(ASCIIText256 name, Vector4 region)
        {
            this.name = name;
            this.region = region;
        }

        public unsafe readonly override string ToString()
        {
            Span<char> buffer = stackalloc char[128];
            int length = ToString(buffer);
            return buffer.Slice(0, length).ToString();
        }

        public readonly int ToString(Span<char> buffer)
        {
            int length = name.CopyTo(buffer);
            buffer[length++] = ' ';
            buffer[length++] = '[';
            length += region.X.ToString(buffer.Slice(length));
            buffer[length++] = ',';
            length += region.Y.ToString(buffer.Slice(length));
            buffer[length++] = ',';
            length += region.Z.ToString(buffer.Slice(length));
            buffer[length++] = ',';
            length += region.W.ToString(buffer.Slice(length));
            buffer[length++] = ']';
            return length;
        }
    }
}
