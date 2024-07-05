using System;
using System.Numerics;

namespace Textures
{
    public struct Pixel
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;

        public Pixel(byte r, byte g, byte b, byte a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public Pixel(uint value)
        {
            r = (byte)((value >> 24) & 0xFF);
            g = (byte)((value >> 16) & 0xFF);
            b = (byte)((value >> 8) & 0xFF);
            a = (byte)(value & 0xFF);
        }

        public readonly override string ToString()
        {
            Span<char> buffer = stackalloc char[256];
            r.TryFormat(buffer, out int length);
            buffer[length++] = ',';
            g.TryFormat(buffer[length..], out length);
            buffer[length++] = ',';
            b.TryFormat(buffer[length..], out length);
            buffer[length++] = ',';
            a.TryFormat(buffer[length..], out length);
            return new string(buffer[..length]);
        }

        public readonly Vector4 AsVector4()
        {
            return new Vector4(r / (float)byte.MaxValue, g / (float)byte.MaxValue, b / (float)byte.MaxValue, a / (float)byte.MaxValue);
        }

        public readonly uint AsUInt()
        {
            return ((uint)r << 24) | ((uint)g << 16) | ((uint)b << 8) | a;
        }
    }
}
