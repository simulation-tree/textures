using System;
using System.Diagnostics;
using Textures.Components;
using Unmanaged;
using Worlds;

namespace Textures
{
    public readonly partial struct CubemapTexture : IEntity
    {
        public readonly (uint width, uint height) Dimensions => GetComponent<IsTexture>().Dimensions;
        
        public readonly USpan<Pixel> Right
        {
            get
            {
                USpan<Pixel> pixels = GetArray<Pixel>().AsSpan();
                uint faceLength = GetComponent<IsTexture>().Length;
                return pixels.Slice(faceLength * 4, faceLength);
            }
        }

        public readonly USpan<Pixel> Left
        {
            get
            {
                USpan<Pixel> pixels = GetArray<Pixel>().AsSpan();
                uint faceLength = GetComponent<IsTexture>().Length;
                return pixels.Slice(faceLength * 5, faceLength);
            }
        }

        public readonly USpan<Pixel> Up
        {
            get
            {
                USpan<Pixel> pixels = GetArray<Pixel>().AsSpan();
                uint faceLength = GetComponent<IsTexture>().Length;
                return pixels.Slice(faceLength * 2, faceLength);
            }
        }

        public readonly USpan<Pixel> Down
        {
            get
            {
                USpan<Pixel> pixels = GetArray<Pixel>().AsSpan();
                uint faceLength = GetComponent<IsTexture>().Length;
                return pixels.Slice(faceLength * 3, faceLength);
            }
        }

        public readonly USpan<Pixel> Forward
        {
            get
            {
                USpan<Pixel> pixels = GetArray<Pixel>().AsSpan();
                uint faceLength = GetComponent<IsTexture>().Length;
                return pixels.Slice(faceLength * 0, faceLength);
            }
        }

        public readonly USpan<Pixel> Back
        {
            get
            {
                USpan<Pixel> pixels = GetArray<Pixel>().AsSpan();
                uint faceLength = GetComponent<IsTexture>().Length;
                return pixels.Slice(faceLength * 1, faceLength);
            }
        }

        public CubemapTexture(World world, ASCIIText256 address, TimeSpan timeout = default)
        {
            this.world = world;
            value = world.CreateEntity(new IsTextureRequest(TextureType.Cubemap, address, timeout));
        }

        public CubemapTexture(World world, Texture right, Texture left, Texture up, Texture down, Texture forward, Texture back)
        {
            ThrowIfSizeMismatch(right, left);
            ThrowIfSizeMismatch(right, up);
            ThrowIfSizeMismatch(right, down);
            ThrowIfSizeMismatch(right, forward);
            ThrowIfSizeMismatch(right, back);

            (uint width, uint height) = right.Dimensions;
            this.world = world;
            value = world.CreateEntity(new IsTexture(1, width, height));
            AddTag<IsCubemapTexture>();

            uint faceLength = width * height;
            uint totalLength = faceLength * 6;
            USpan<Pixel> pixels = CreateArray<Pixel>(totalLength).AsSpan();
            CopyTo(forward.Pixels, pixels.Slice(faceLength * 4, faceLength), width, height);
            CopyTo(back.Pixels, pixels.Slice(faceLength * 5, faceLength), width, height);
            CopyTo(up.Pixels, pixels.Slice(faceLength * 2, faceLength), width, height);
            CopyTo(down.Pixels, pixels.Slice(faceLength * 3, faceLength), width, height);
            CopyTo(right.Pixels, pixels.Slice(faceLength * 0, faceLength), width, height);
            CopyTo(left.Pixels, pixels.Slice(faceLength * 1, faceLength), width, height);
        }

        public CubemapTexture(World world, uint width, uint height, USpan<Pixel> right, USpan<Pixel> left, USpan<Pixel> up, USpan<Pixel> down, USpan<Pixel> forward, USpan<Pixel> back)
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

            uint faceLength = width * height;
            uint totalLength = faceLength * 6;
            USpan<Pixel> pixels = CreateArray<Pixel>(totalLength).AsSpan();
            CopyTo(forward, pixels.Slice(faceLength * 0, faceLength), width, height);
            CopyTo(back, pixels.Slice(faceLength * 1, faceLength), width, height);
            CopyTo(up, pixels.Slice(faceLength * 2, faceLength), width, height);
            CopyTo(down, pixels.Slice(faceLength * 3, faceLength), width, height);
            CopyTo(right, pixels.Slice(faceLength * 4, faceLength), width, height);
            CopyTo(left, pixels.Slice(faceLength * 5, faceLength), width, height);
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsTexture>();
            archetype.AddArrayType<Pixel>();
            archetype.AddTagType<IsCubemapTexture>();
        }

        private static void CopyTo(USpan<Pixel> source, USpan<Pixel> destination, uint width, uint height)
        {
            for (uint i = 0; i < destination.Length; i++)
            {
                uint x = i % width;
                uint y = i / width;
                y = height - y - 1; //flip y
                destination[i] = source[y * width + x];
            }
        }

        [Conditional("DEBUG")]
        private static void ThrowIfSizeMismatch(Texture a, Texture b)
        {
            (uint width, uint height) aSize = a.Dimensions;
            (uint width, uint height) bSize = b.Dimensions;
            if (aSize.width != bSize.width || aSize.height != bSize.height)
            {
                throw new InvalidOperationException($"Cubemap texture {a} does not match {b}");
            }
        }

        [Conditional("DEBUG")]
        private static void ThrowIfSizeMismatch(uint width, uint height, USpan<Pixel> pixels)
        {
            uint length = width * height;
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