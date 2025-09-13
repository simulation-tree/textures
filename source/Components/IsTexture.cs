using System;

namespace Textures.Components
{
    public struct IsTexture : IEquatable<IsTexture>
    {
        public ushort version;
        public int width;
        public int height;

        public readonly (int width, int height) Dimensions => (width, height);
        public readonly int Length => width * height;

#if NET
        [Obsolete("Default constructor not supported", true)]
        public IsTexture()
        {
            throw new NotSupportedException("Default constructor not supported");
        }
#endif

        public IsTexture(ushort version, int width, int height)
        {
            this.version = version;
            this.width = width;
            this.height = height;
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is IsTexture texture && Equals(texture);
        }

        public readonly bool Equals(IsTexture other)
        {
            return version == other.version && width == other.width && height == other.height;
        }

        public readonly override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + version;
            hash = hash * 31 + width;
            hash = hash * 31 + height;
            return hash;
        }

        public static bool operator ==(IsTexture left, IsTexture right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(IsTexture left, IsTexture right)
        {
            return !(left == right);
        }
    }
}