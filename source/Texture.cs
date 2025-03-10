using System;
using System.Diagnostics;
using System.Numerics;
using Textures.Components;
using Unmanaged;
using Worlds;

namespace Textures
{
    public readonly partial struct Texture : IEntity
    {
        public readonly bool IsLoaded
        {
            get
            {
                if (TryGetComponent(out IsTextureRequest request))
                {
                    return request.status == IsTextureRequest.Status.Loaded;
                }

                return IsCompliant;
            }
        }

        public readonly TextureType Type
        {
            get
            {
                if (TryGetComponent(out IsTextureRequest request))
                {
                    return request.type;
                }

                if (ContainsTag<IsAtlasTexture>())
                {
                    return TextureType.Atlas;
                }
                else if (ContainsTag<CubemapTexture>())
                {
                    return TextureType.Cubemap;
                }
                else
                {
                    return TextureType.Default;
                }
            }
        }

        public readonly Span<Pixel> Pixels
        {
            get
            {
                ThrowIfNotLoaded();

                return GetArray<Pixel>().AsSpan();
            }
        }

        public readonly (int width, int height) Dimensions
        {
            get
            {
                ThrowIfNotLoaded();

                IsTexture component = GetComponent<IsTexture>();
                return (component.width, component.height);
            }
        }

        public readonly int Width
        {
            get
            {
                ThrowIfNotLoaded();

                return GetComponent<IsTexture>().width;
            }
        }

        public readonly int Height
        {
            get
            {
                ThrowIfNotLoaded();

                return GetComponent<IsTexture>().height;
            }
        }

        /// <summary>
        /// Creates a new empty texture with a set size.
        /// </summary>
        public Texture(World world, int width, int height, Pixel defaultPixel = default)
        {
            this.world = world;
            value = world.CreateEntity(new IsTexture(1, width, height));

            int pixelCount = width * height;
            Span<Pixel> pixels = CreateArray<Pixel>(pixelCount).AsSpan();
            pixels.Fill(defaultPixel);
        }

        /// <summary>
        /// Creates a new empty texture with the given <paramref name="pixels"/>.
        /// </summary>
        public Texture(World world, int width, int height, ReadOnlySpan<Pixel> pixels)
        {
            this.world = world;
            value = world.CreateEntity(new IsTexture(1, width, height));
            CreateArray(pixels);
        }

        /// <summary>
        /// Creates a request to load a texture at runtime from
        /// the given <paramref name="address"/>.
        /// </summary>
        public Texture(World world, ASCIIText256 address, TimeSpan timeout = default)
        {
            this.world = world;
            value = world.CreateEntity(new IsTextureRequest(TextureType.Default, address, timeout));
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsTexture>();
            archetype.AddArrayType<Pixel>();
        }

        public unsafe readonly override string ToString()
        {
            Span<char> buffer = stackalloc char[128];
            int length = ToString(buffer);
            return buffer.Slice(0, length).ToString();
        }

        public readonly int ToString(Span<char> destination)
        {
            (int width, int height) = Dimensions;
            int length = 0;
            length += width.ToString(destination.Slice(length));
            destination[length++] = 'x';
            length += height.ToString(destination.Slice(length));
            destination[length++] = ' ';
            destination[length++] = '(';
            destination[length++] = '`';
            length += value.ToString(destination.Slice(length));
            destination[length++] = '`';
            destination[length++] = ')';
            return length;
        }

        public readonly Vector4 Evaluate(Vector2 position)
        {
            ThrowIfNotLoaded();
            ThrowIfOutOfBounds(position);

            (int width, int height) = Dimensions;
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

        public readonly ref Pixel GetPixelAt(int x, int y)
        {
            ThrowIfNotLoaded();

            Span<Pixel> pixels = Pixels;
            int width = GetComponent<IsTexture>().width;
            int index = y * width + x;
            return ref pixels[index];
        }

        public readonly Pixel SetPixelAt(int x, int y, Pixel pixel)
        {
            ThrowIfNotLoaded();

            ref Pixel currentPixel = ref GetPixelAt(x, y);
            Pixel previousPixel = currentPixel;
            currentPixel = pixel;
            return previousPixel;
        }

        [Conditional("DEBUG")]
        private readonly void ThrowIfNotLoaded()
        {
            if (!IsLoaded)
            {
                throw new InvalidOperationException($"Texture entity `{value}` is not yet loaded");
            }
        }

        [Conditional("DEBUG")]
        private static void ThrowIfOutOfBounds(Vector2 position)
        {
            if (position.X < 0 || position.X > 1 || position.Y < 0 || position.Y > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(position), $"Position `{position}` is out of bounds");
            }
        }
    }
}