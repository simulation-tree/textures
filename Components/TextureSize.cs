using System;

namespace Textures.Components
{
    public readonly struct TextureSize
    {
        public readonly uint width;
        public readonly uint height;

        public TextureSize(uint width, uint height)
        {
            this.width = width;
            this.height = height;
        }

        public readonly override string ToString()
        {
            Span<char> buffer = stackalloc char[64];
            width.TryFormat(buffer, out int length);
            buffer[length++] = 'x';
            height.TryFormat(buffer[length..], out int length2);
            return new string(buffer[..(length + length2)]);
        }
    }
}
