using System.Numerics;
using Unmanaged;

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

        public unsafe readonly override string ToString()
        {
            USpan<char> buffer = stackalloc char[256];
            uint length = ToString(buffer);
            return buffer.GetSpan(length).ToString();
        }

        public readonly uint ToString(USpan<char> buffer)
        {
            uint length = r.ToString(buffer);
            buffer[length++] = ',';
            length += g.ToString(buffer.Slice(length));
            buffer[length++] = ',';
            length += b.ToString(buffer.Slice(length));
            buffer[length++] = ',';
            length += a.ToString(buffer.Slice(length));
            return length;
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
