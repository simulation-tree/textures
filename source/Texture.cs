using Data;
using Simulation;
using System;
using System.Diagnostics;
using System.Numerics;
using Textures.Components;
using Unmanaged;

namespace Textures
{
    public readonly struct Texture : IEntity
    {
        private readonly Entity entity;

        public readonly (uint width, uint height) Size
        {
            get
            {
                ThrowIfDataNotLoadedYet();
                IsTexture component = entity.GetComponent<IsTexture>();
                return (component.width, component.height);
            }
        }

        public readonly uint Width => Size.width;
        public readonly uint Height => Size.height;
        public readonly Span<Pixel> Pixels
        {
            get
            {
                ThrowIfDataNotLoadedYet();
                return entity.GetArray<Pixel>();
            }
        }

        public readonly ref Pixel this[uint x, uint y]
        {
            get
            {
                ThrowIfDataNotLoadedYet();
                Span<Pixel> pixels = entity.GetArray<Pixel>();
                uint index = y * Width + x;
                if (index >= pixels.Length)
                {
                    throw new ArgumentOutOfRangeException(null, "Position must be within the texture.");
                }

                return ref pixels[(int)index];
            }
        }

        World IEntity.World => entity;
        uint IEntity.Value => entity;

        public Texture(World world, uint existingEntity)
        {
            entity = new(world, existingEntity);
        }

        /// <summary>
        /// Creates a new empty texture with a set size.
        /// </summary>
        public Texture(World world, uint width, uint height, Pixel defaultPixel = default)
        {
            entity = new(world);
            entity.AddComponent(new IsTexture(width, height));

            uint pixelCount = width * height;
            Span<Pixel> pixels = entity.CreateArray<Pixel>(pixelCount);
            pixels.Fill(defaultPixel);
        }

        public Texture(World world, uint width, uint height, ReadOnlySpan<Pixel> pixels)
        {
            entity = new(world);
            entity.AddComponent(new IsTexture(width, height));
            entity.CreateArray(pixels);
        }

        /// <summary>
        /// Creates a texture+request that loads from the given address.
        /// </summary>
        public Texture(World world, ReadOnlySpan<char> address)
        {
            DataRequest request = new(world, address);
            entity = request;
            entity.AddComponent(new IsTextureRequest());
        }

        /// <summary>
        /// Creates a texture+request that loads from the given address.
        /// </summary>
        public Texture(World world, FixedString address)
        {
            DataRequest request = new(world, address);
            entity = request;
            entity.AddComponent(new IsTextureRequest());
        }

        public readonly override string ToString()
        {
            Span<char> buffer = stackalloc char[128];
            int length = ToString(buffer);
            return new string(buffer[..length]);
        }

        public readonly int ToString(Span<char> buffer)
        {
            int length = 0;
            Width.TryFormat(buffer, out int written);
            length += written;
            buffer[length++] = 'x';
            Height.TryFormat(buffer[length..], out written);
            length += written;
            buffer[length++] = ' ';
            buffer[length++] = '(';
            buffer[length++] = '`';
            length += entity.ToString(buffer[length..]);
            buffer[length++] = '`';
            buffer[length++] = ')';
            return length;

        }

        Query IEntity.GetQuery(World world)
        {
            return new(world, RuntimeType.Get<IsTexture>());
        }

        [Conditional("DEBUG")]
        private readonly void ThrowIfDataNotLoadedYet()
        {
            if (!entity.ContainsComponent<IsTexture>())
            {
                throw new InvalidOperationException($"Texture entity `{entity}` is not yet loaded");
            }
        }

        public readonly uint GetVersion()
        {
            return entity.GetComponent<IsTexture>().version;
        }

        public readonly Color Evaluate(Vector2 position)
        {
            ThrowIfDataNotLoadedYet();
            if (position.X < 0 || position.X > 1 || position.Y < 0 || position.Y > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Position must be normalized within the 0-1 range.");
            }

            (uint width, uint height) size = Size;
            int width = (int)size.width;
            int height = (int)size.height;
            Span<Pixel> pixels = Pixels;
            int maxWidth = width - 1;
            int maxHeight = height - 1;
            int x = (int)(position.X * maxWidth);
            int y = (int)(position.Y * maxHeight);
            int xx = Math.Min(x + 1, maxWidth);
            int yy = Math.Min(y + 1, maxHeight);
            Vector4 topLeft = pixels[y * width + x].AsVector4();
            Vector4 topRight = pixels[y * width + xx].AsVector4();
            Vector4 bottomLeft = pixels[yy * width + x].AsVector4();
            Vector4 bottomRight = pixels[yy * width + xx].AsVector4();
            float xFactor = position.X * maxWidth - x;
            float yFactor = position.Y * maxHeight - y;
            Vector4 top = Vector4.Lerp(topLeft, topRight, xFactor);
            Vector4 bottom = Vector4.Lerp(bottomLeft, bottomRight, xFactor);
            return new(Vector4.Lerp(top, bottom, yFactor));
        }

        public readonly Color Evaluate(float x, float y)
        {
            return Evaluate(new Vector2(x, y));
        }

        public static implicit operator Entity(Texture texture)
        {
            return texture.entity;
        }
    }
}
