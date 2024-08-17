using Data.Components;
using Data.Events;
using Simulation;
using System;
using System.Numerics;
using Textures.Components;
using Textures.Events;
using Unmanaged;
using Unmanaged.Collections;

namespace Textures
{
    public readonly struct Texture : ITexture, IDisposable
    {
        private readonly Entity entity;

        public readonly (uint width, uint height) Size
        {
            get
            {
                TextureSize size = entity.GetComponent<TextureSize>();
                return (size.width, size.height);
            }
        }

        public readonly uint Width => Size.width;
        public readonly uint Height => Size.height;
        public readonly Span<Pixel> Pixels => entity.GetList<Pixel>().AsSpan();

        World IEntity.World => entity.world;
        eint IEntity.Value => entity.value;

        public Texture(World world, eint existingEntity)
        {
            entity = new(world, existingEntity);
        }

        /// <summary>
        /// Creates a new empty texture with a set size.
        /// </summary>
        public Texture(World world, uint width, uint height, Pixel defaultPixel = default)
        {
            entity = new(world);
            entity.AddComponent(new IsTexture());
            entity.AddComponent(new TextureSize(width, height));

            uint pixelCount = width * height;
            UnmanagedList<Pixel> list = entity.CreateList<Pixel>(pixelCount);
            list.AddRepeat(defaultPixel, pixelCount);
        }

        public Texture(World world, uint width, uint height, ReadOnlySpan<Pixel> pixels)
        {
            entity = new(world);
            entity.AddComponent(new IsTexture());
            entity.AddComponent(new TextureSize(width, height));

            UnmanagedList<Pixel> list = entity.CreateList<Pixel>((uint)pixels.Length);
            list.AddDefault((uint)pixels.Length);
            pixels.CopyTo(list.AsSpan());
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
            entity.CreateList<Pixel>();

            world.Submit(new DataUpdate());
            world.Submit(new TextureUpdate());
            world.Poll();
        }

        public Texture(World world, FixedString address)
        {
            entity = new(world);
            entity.AddComponent(new IsDataRequest(address));
            entity.AddComponent(new TextureSize(0, 0));
            entity.AddComponent(new IsTexture());
            entity.CreateList<Pixel>();

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

        Query IEntity.GetQuery(World world)
        {
            return new(world, RuntimeType.Get<IsTexture>());
        }

        public readonly bool IsRequesting()
        {
            return entity.ContainsComponent<IsDataRequest>();
        }

        public readonly FixedString GetRequestAddress()
        {
            return entity.GetComponent<IsDataRequest>().address;
        }

        public readonly uint GetVersion()
        {
            return entity.GetComponent<IsTexture>().version;
        }

        public readonly Pixel Get(uint x, uint y)
        {
            UnmanagedList<Pixel> pixels = entity.GetList<Pixel>();
            (uint width, uint height) = Size;
            if (x >= width || y >= height)
            {
                throw new ArgumentOutOfRangeException(null, "Position must be within the texture.");
            }

            return pixels[y * width + x];
        }

        public readonly void Set(uint x, uint y, Pixel pixel)
        {
            UnmanagedList<Pixel> pixels = entity.GetList<Pixel>();
            (uint width, uint height) = Size;
            if (x >= width || y >= height)
            {
                throw new ArgumentOutOfRangeException(null, "Position must be within the texture.");
            }

            pixels[y * width + x] = pixel;
        }

        public readonly Vector4 Evaluate(Vector2 position)
        {
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
            return Vector4.Lerp(top, bottom, yFactor);
        }

        public readonly Vector4 Evaluate(float x, float y)
        {
            return Evaluate(new Vector2(x, y));
        }

        public static implicit operator Entity(Texture texture)
        {
            return texture.entity;
        }
    }
}
