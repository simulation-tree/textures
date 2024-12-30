using System.Numerics;
using Unmanaged;
using Worlds;

namespace Textures
{
    [ArrayElement]
    public struct AtlasSprite
    {
        /// <summary>
        /// Unique name of the sprite.
        /// </summary>
        public FixedString name;

        /// <summary>
        /// Texture coordinates for where this sprite is
        /// within its original atlas texture.
        /// </summary>
        public Vector4 region;

        public AtlasSprite(USpan<char> name, Vector4 region)
        {
            this.name = new(name);
            this.region = region;
        }

        public AtlasSprite(FixedString name, Vector4 region)
        {
            this.name = name;
            this.region = region;
        }

        public unsafe readonly override string ToString()
        {
            USpan<char> buffer = stackalloc char[128];
            uint length = ToString(buffer);
            return buffer.Slice(0, length).ToString();
        }

        public readonly uint ToString(USpan<char> buffer)
        {
            uint length = name.CopyTo(buffer);
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
