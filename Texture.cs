using Data.Components;
using Data.Events;
using Simulation;
using System;
using System.Numerics;
using Textures.Components;
using Textures.Events;
using Unmanaged.Collections;

namespace Textures
{
    public readonly struct Texture : IDisposable
    {
        public readonly Entity entity;

        private readonly UnmanagedList<Pixel> pixels;

        public readonly bool IsDestroyed => entity.IsDestroyed;
        public readonly Span<Pixel> Pixels => pixels.AsSpan();
        public readonly TextureSize Size => entity.GetComponent<TextureSize>();
        public readonly uint Width => Size.width;
        public readonly uint Height => Size.height;

        public Texture(World world, EntityID existingEntity)
        {
            entity = new(world, existingEntity);
        }

        /// <summary>
        /// Creates a new empty texture with a set size.
        /// </summary>
        public Texture(World world, uint width, uint height)
        {
            entity = new(world);
            entity.AddComponent(new IsTexture(false));
            entity.AddComponent(new TextureSize(width, height));

            uint pixelCount = width * height;
            pixels = entity.CreateCollection<Pixel>(pixelCount);
            pixels.AddDefault(pixelCount);
        }

        public Texture(World world, uint width, uint height, ReadOnlySpan<Pixel> pixels)
        {
            entity = new(world);
            entity.AddComponent(new IsTexture(false));
            entity.AddComponent(new TextureSize(width, height));
            this.pixels = entity.CreateCollection<Pixel>((uint)pixels.Length);
            this.pixels.AddDefault((uint)pixels.Length);
            pixels.CopyTo(this.pixels.AsSpan());
        }

        /// <summary>
        /// Creates a texture that loads from the given address.
        /// </summary>
        public Texture(World world, ReadOnlySpan<char> address)
        {
            entity = new(world);
            entity.AddComponent(new IsDataRequest(address));
            entity.AddComponent(new TextureSize(0, 0));
            entity.AddComponent(new IsTexture());
            pixels = entity.CreateCollection<Pixel>();

            world.Submit(new DataUpdate());
            world.Submit(new TextureUpdate());
            world.Poll();
        }

        public readonly void Dispose()
        {
            entity.Dispose();
        }

        public readonly override string ToString()
        {
            return entity.ToString();
        }

        public readonly Pixel Get(uint x, uint y)
        {
            if (x >= Width || y >= Height)
            {
                throw new ArgumentOutOfRangeException(nameof(x), "Position is out of bounds.");
            }

            return pixels[y * Width + x];
        }

        /// <summary>
        /// Evaluates the color for the given normalized position.
        /// </summary>
        public readonly Color Evaluate(float x, float y)
        {
            return Evaluate(new(x, y));
        }

        /// <summary>
        /// Evaluates the color for the given normalized position.
        /// </summary>
        public readonly Color Evaluate(Vector2 position)
        {
            if (position.X < 0 || position.X > 1 || position.Y < 0 || position.Y > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Position must be normalized.");
            }

            uint maxWidth = Width - 1;
            uint maxHeight = Height - 1;
            uint x = (uint)(position.X * maxWidth);
            uint y = (uint)(position.Y * maxHeight);
            uint xx = Math.Min(x + 1, maxWidth);
            uint yy = Math.Min(y + 1, maxHeight);
            Vector4 topLeft = pixels[y * Width + x].AsVector4();
            Vector4 topRight = pixels[y * Width + xx].AsVector4();
            Vector4 bottomLeft = pixels[yy * Width + x].AsVector4();
            Vector4 bottomRight = pixels[yy * Width + xx].AsVector4();
            float xFactor = position.X * maxWidth - x;
            float yFactor = position.Y * maxHeight - y;
            Vector4 top = Vector4.Lerp(topLeft, topRight, xFactor);
            Vector4 bottom = Vector4.Lerp(bottomLeft, bottomRight, xFactor);
            return Color.CreateFromRGB(Vector4.Lerp(top, bottom, yFactor));
        }
    }
}
