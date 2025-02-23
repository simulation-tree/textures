using System;

namespace Textures.Components
{
    public readonly struct IsTexture : IEquatable<IsTexture>
    {
        public readonly uint version;
        public readonly uint width;
        public readonly uint height;

        public readonly (uint width, uint height) Dimensions => (width, height);
        public readonly uint Length => width * height;

#if NET
        [Obsolete("Default constructor not supported", true)]
        public IsTexture()
        {
            throw new NotSupportedException("Default constructor not supported");
        }
#endif

        public IsTexture(uint version, uint width, uint height)
        {
            this.version = version;
            this.width = width;
            this.height = height;
        }

        public readonly IsTexture IncrementVersion(uint width, uint height)
        {
            return new IsTexture(version + 1, width, height);
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
            return HashCode.Combine(version, width, height);
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