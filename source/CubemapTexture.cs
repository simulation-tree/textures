using System;
using System.Diagnostics;
using Textures.Components;
using Unmanaged;
using Worlds;

namespace Textures
{
    public readonly partial struct CubemapTexture : IEntity
    {
        public readonly (int width, int height) Dimensions => GetComponent<IsTexture>().Dimensions;

        public readonly Span<Pixel> Right
        {
            get
            {
                Span<Pixel> pixels = GetArray<Pixel>();
                int faceLength = GetComponent<IsTexture>().Length;
                return pixels.Slice(faceLength * 4, faceLength);
            }
        }

        public readonly Span<Pixel> Left
        {
            get
            {
                Span<Pixel> pixels = GetArray<Pixel>();
                int faceLength = GetComponent<IsTexture>().Length;
                return pixels.Slice(faceLength * 5, faceLength);
            }
        }

        public readonly Span<Pixel> Up
        {
            get
            {
                Span<Pixel> pixels = GetArray<Pixel>();
                int faceLength = GetComponent<IsTexture>().Length;
                return pixels.Slice(faceLength * 2, faceLength);
            }
        }

        public readonly Span<Pixel> Down
        {
            get
            {
                Span<Pixel> pixels = GetArray<Pixel>();
                int faceLength = GetComponent<IsTexture>().Length;
                return pixels.Slice(faceLength * 3, faceLength);
            }
        }

        public readonly Span<Pixel> Forward
        {
            get
            {
                Span<Pixel> pixels = GetArray<Pixel>();
                int faceLength = GetComponent<IsTexture>().Length;
                return pixels.Slice(faceLength * 0, faceLength);
            }
        }

        public readonly Span<Pixel> Back
        {
            get
            {
                Span<Pixel> pixels = GetArray<Pixel>();
                int faceLength = GetComponent<IsTexture>().Length;
                return pixels.Slice(faceLength * 1, faceLength);
            }
        }

        public CubemapTexture(World world, ASCIIText256 address, double timeout = default, IsTextureRequest.Flags flags = IsTextureRequest.Flags.FlipY)
        {
            this.world = world;
            flags |= IsTextureRequest.Flags.CubemapTexture;
            value = world.CreateEntity(new IsTextureRequest(flags, address, timeout));
        }

        public CubemapTexture(World world, Texture right, Texture left, Texture up, Texture down, Texture forward, Texture back, IsTextureRequest.Flags flags = IsTextureRequest.Flags.FlipY)
        {
            ThrowIfSizeMismatch(right, left);
            ThrowIfSizeMismatch(right, up);
            ThrowIfSizeMismatch(right, down);
            ThrowIfSizeMismatch(right, forward);
            ThrowIfSizeMismatch(right, back);

            (int width, int height) = right.Dimensions;
            this.world = world;
            value = world.CreateEntity(new IsTexture(1, width, height));
            AddTag<IsCubemapTexture>();

            int faceLength = width * height;
            int totalLength = faceLength * 6;
            Span<Pixel> pixels = CreateArray<Pixel>(totalLength);
            CopyTo(forward.Pixels, pixels.Slice(faceLength * 4, faceLength), width, height, flags);
            CopyTo(back.Pixels, pixels.Slice(faceLength * 5, faceLength), width, height, flags);
            CopyTo(up.Pixels, pixels.Slice(faceLength * 2, faceLength), width, height, flags);
            CopyTo(down.Pixels, pixels.Slice(faceLength * 3, faceLength), width, height, flags);
            CopyTo(right.Pixels, pixels.Slice(faceLength * 0, faceLength), width, height, flags);
            CopyTo(left.Pixels, pixels.Slice(faceLength * 1, faceLength), width, height, flags);
        }

        public CubemapTexture(World world, int width, int height, Span<Pixel> right, Span<Pixel> left, Span<Pixel> up, Span<Pixel> down, Span<Pixel> forward, Span<Pixel> back, IsTextureRequest.Flags flags = IsTextureRequest.Flags.FlipY)
        {
            ThrowIfSizeMismatch(width, height, right);
            ThrowIfSizeMismatch(width, height, left);
            ThrowIfSizeMismatch(width, height, up);
            ThrowIfSizeMismatch(width, height, down);
            ThrowIfSizeMismatch(width, height, forward);
            ThrowIfSizeMismatch(width, height, back);

            this.world = world;
            value = world.CreateEntity(new IsTexture(1, width, height));
            AddTag<IsCubemapTexture>();

            int faceLength = width * height;
            int totalLength = faceLength * 6;
            Span<Pixel> pixels = CreateArray<Pixel>(totalLength);
            CopyTo(forward, pixels.Slice(faceLength * 0, faceLength), width, height, flags);
            CopyTo(back, pixels.Slice(faceLength * 1, faceLength), width, height, flags);
            CopyTo(up, pixels.Slice(faceLength * 2, faceLength), width, height, flags);
            CopyTo(down, pixels.Slice(faceLength * 3, faceLength), width, height, flags);
            CopyTo(right, pixels.Slice(faceLength * 4, faceLength), width, height, flags);
            CopyTo(left, pixels.Slice(faceLength * 5, faceLength), width, height, flags);
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsTexture>();
            archetype.AddArrayType<Pixel>();
            archetype.AddTagType<IsCubemapTexture>();
        }

        private static void CopyTo(ReadOnlySpan<Pixel> source, Span<Pixel> destination, int width, int height, IsTextureRequest.Flags flags)
        {
            if ((flags & IsTextureRequest.Flags.FlipY) != 0)
            {
                for (int i = 0; i < destination.Length; i++)
                {
                    Texture.GetPosition(i, width, out int x, out int y);
                    y = height - y - 1;
                    destination[i] = source[Texture.GetIndex(x, y, width)];
                }
            }
            else
            {
                source.CopyTo(destination);
            }
        }

        [Conditional("DEBUG")]
        private static void ThrowIfSizeMismatch(Texture a, Texture b)
        {
            (int width, int height) aSize = a.Dimensions;
            (int width, int height) bSize = b.Dimensions;
            if (aSize.width != bSize.width || aSize.height != bSize.height)
            {
                throw new InvalidOperationException($"Cubemap texture {a} does not match {b}");
            }
        }

        [Conditional("DEBUG")]
        private static void ThrowIfSizeMismatch(int width, int height, Span<Pixel> pixels)
        {
            int length = width * height;
            if (pixels.Length != length)
            {
                throw new InvalidOperationException($"Cubemap texture size {width}x{height} does not match pixel count {pixels.Length}");
            }
        }

        public static implicit operator Texture(CubemapTexture cubemap)
        {
            return cubemap.As<Texture>();
        }
    }
}