using Data;
using Data.Components;
using System;
using System.Diagnostics;
using System.Numerics;
using Textures.Components;
using Unmanaged;
using Worlds;

namespace Textures
{
    public readonly struct Texture : IEntity, IEquatable<Texture>
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
        public readonly USpan<Pixel> Pixels
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
                USpan<Pixel> pixels = entity.GetArray<Pixel>();
                uint index = y * Width + x;
                if (index >= pixels.Length)
                {
                    throw new ArgumentOutOfRangeException(null, "Position must be within the texture.");
                }

                return ref pixels[index];
            }
        }

        readonly uint IEntity.Value => entity.value;
        readonly World IEntity.World => entity.world;
        readonly Definition IEntity.Definition => new Definition().AddComponentType<IsTexture>().AddArrayType<Pixel>();

        public Texture(World world, uint existingEntity)
        {
            entity = new(world, existingEntity);
        }

        /// <summary>
        /// Creates a new empty texture with a set size.
        /// </summary>
        public Texture(World world, uint width, uint height, Pixel defaultPixel = default)
        {
            entity = new Entity<IsTexture>(world, new IsTexture(width, height));

            uint pixelCount = width * height;
            USpan<Pixel> pixels = entity.CreateArray<Pixel>(pixelCount);
            pixels.Fill(defaultPixel);
        }

        public Texture(World world, uint width, uint height, USpan<Pixel> pixels)
        {
            entity = new Entity<IsTexture>(world, new IsTexture(width, height));
            entity.CreateArray(pixels);
        }

        /// <summary>
        /// Creates a texture+request that loads from the given address.
        /// </summary>
        public Texture(World world, USpan<char> address)
        {
            entity = new Entity<IsDataRequest, IsTextureRequest>(world, new IsDataRequest(address), new IsTextureRequest());
        }

        /// <summary>
        /// Creates a texture+request that loads from the given address.
        /// </summary>
        public Texture(World world, string address)
        {
            entity = new Entity<IsDataRequest, IsTextureRequest>(world, new IsDataRequest(address), new IsTextureRequest());
        }

        /// <summary>
        /// Creates a texture+request that loads from the given address.
        /// </summary>
        public Texture(World world, FixedString address)
        {
            entity = new Entity<IsDataRequest, IsTextureRequest>(world, new IsDataRequest(address), new IsTextureRequest());
        }

        public readonly void Dispose()
        {
            entity.Dispose();
        }

        public unsafe readonly override string ToString()
        {
            USpan<char> buffer = stackalloc char[128];
            uint length = ToString(buffer);
            return buffer.Slice(0, length).ToString();
        }

        public readonly uint ToString(USpan<char> buffer)
        {
            uint length = 0;
            length += Width.ToString(buffer.Slice(length));
            buffer[length++] = 'x';
            length += Height.ToString(buffer.Slice(length));
            buffer[length++] = ' ';
            buffer[length++] = '(';
            buffer[length++] = '`';
            length += entity.ToString(buffer.Slice(length));
            buffer[length++] = '`';
            buffer[length++] = ')';
            return length;
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
            uint width = size.width;
            uint height = size.height;
            USpan<Pixel> pixels = Pixels;
            uint maxWidth = width - 1;
            uint maxHeight = height - 1;
            uint x = (uint)(position.X * maxWidth);
            uint y = (uint)(position.Y * maxHeight);
            uint xx = Math.Min(x + 1, maxWidth);
            uint yy = Math.Min(y + 1, maxHeight);
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

        public readonly override bool Equals(object? obj)
        {
            return obj is Texture texture && Equals(texture);
        }

        public readonly bool Equals(Texture other)
        {
            return entity.Equals(other.entity);
        }

        public readonly override int GetHashCode()
        {
            return entity.GetHashCode();
        }

        public static implicit operator Entity(Texture texture)
        {
            return texture.entity;
        }

        public static bool operator ==(Texture left, Texture right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Texture left, Texture right)
        {
            return !(left == right);
        }
    }
}
