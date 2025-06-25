using System;
using System.Numerics;

namespace Textures
{
    public struct Pixel : IEquatable<Pixel>
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
            int length = ToString(buffer);
            return buffer.Slice(0, length).ToString();
        }

        public readonly int ToString(Span<char> destination)
        {
            int length = r.ToString(destination);
            destination[length++] = ',';
            length += g.ToString(destination.Slice(length));
            destination[length++] = ',';
            length += b.ToString(destination.Slice(length));
            destination[length++] = ',';
            length += a.ToString(destination.Slice(length));
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

        public readonly int AsInt()
        {
            return (r << 24) | (g << 16) | (b << 8) | a;
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is Pixel pixel && Equals(pixel);
        }

        public readonly bool Equals(Pixel other)
        {
            return r == other.r && g == other.g && b == other.b && a == other.a;
        }

        public readonly override int GetHashCode()
        {
            return AsInt();
        }

        public static Pixel Average(Pixel first, Pixel second)
        {
            byte r = (byte)((first.r + second.r) * 0.5f);
            byte g = (byte)((first.g + second.g) * 0.5f);
            byte b = (byte)((first.b + second.b) * 0.5f);
            byte a = (byte)((first.a + second.a) * 0.5f);
            return new Pixel(r, g, b, a);
        }

        public static bool operator ==(Pixel left, Pixel right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Pixel left, Pixel right)
        {
            return !(left == right);
        }
    }
}