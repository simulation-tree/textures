using Simulation;
using System;
using System.Numerics;
using Textures.Components;
using Unmanaged.Collections;

namespace Textures
{
    public readonly struct AtlasTexture : IDisposable
    {
        public readonly Texture texture;

        private readonly UnmanagedList<Sprite> sprites;

        public readonly bool IsDestroyed => texture.IsDestroyed;
        public readonly Span<Pixel> Pixels => texture.Pixels;
        public readonly Span<Sprite> Sprites => sprites.AsSpan();
        public readonly TextureSize Size => texture.Size;
        public readonly uint Width => texture.Width;
        public readonly uint Height => texture.Height;

        public AtlasTexture()
        {
            throw new InvalidOperationException("Cannot create an atlas texture without data.");
        }

        public AtlasTexture(World world, EntityID existingEntity)
        {
            texture = new(world, existingEntity);
            sprites = texture.entity.GetCollection<Sprite>();
        }

        public AtlasTexture(Texture existingTexture, ReadOnlySpan<Sprite> sprites)
        {
            texture = existingTexture;
            this.sprites = texture.entity.CreateCollection<Sprite>();
            this.sprites.AddDefault((uint)sprites.Length);
            sprites.CopyTo(this.sprites.AsSpan());
        }

        public AtlasTexture(World world, ReadOnlySpan<InputSprite> sprites, uint padding = 0)
        {
            //find the max sprite size
            int spriteCount = sprites.Length;
            using UnmanagedArray<Vector2> sizes = new((uint)spriteCount);
            Vector2 maxSpriteSize = default;
            for (uint i = 0; i < spriteCount; i++)
            {
                InputSprite inputSprite = sprites[(int)i];
                Vector2 spriteSize = inputSprite.size.AsVector2();
                sizes[i] = spriteSize;
                maxSpriteSize = Vector2.Max(maxSpriteSize, spriteSize);
            }

            //todo: find an algo for packing that auto finds the max size
            float spriteMaxDimension = Math.Max(maxSpriteSize.X, maxSpriteSize.Y) + padding;
            uint maxSpritesPerAxis = (uint)Math.Pow(2, Math.Ceiling(Math.Log2(spriteCount)));
            uint dimensionSize = (uint)(Math.Sqrt(maxSpritesPerAxis) * spriteMaxDimension);
            dimensionSize = (uint)Math.Pow(2, Math.Ceiling(Math.Log2(dimensionSize)));
            Vector2 atlasSize = new(dimensionSize, dimensionSize);

            texture = new(world, dimensionSize, dimensionSize);
            this.sprites = texture.entity.CreateCollection<Sprite>((uint)sprites.Length);

            Span<Pixel> pixels = texture.Pixels;
            for (uint i = 0; i < sprites.Length; i++)
            {
                InputSprite sprite = sprites[(int)i];
                char unicode = (char)i;
                uint atlasX = i % (maxSpritesPerAxis);
                uint atlasY = i / (maxSpritesPerAxis);
                Vector2 position = new(atlasX * (spriteMaxDimension), atlasY * (spriteMaxDimension));
                uint x = (uint)(position.X + padding);
                uint y = (uint)(position.Y + padding);
                uint gWidth = sprite.size.width;
                uint gHeight = sprite.size.height;
                Vector4 region = new(x / atlasSize.X, y / atlasSize.Y, gWidth / atlasSize.X, gHeight / atlasSize.Y);
                region.Y += region.W;
                region.W *= -1;

                this.sprites.Add(new(sprite.Name, region));

                ReadOnlySpan<Pixel> spritePixels = sprite.Pixels;
                for (uint p = 0; p < spritePixels.Length; p++)
                {
                    Pixel spritePixel = spritePixels[(int)p];
                    uint localX = p % gWidth;
                    uint localY = p / gWidth;
                    uint index = x + localX + (y + localY) * dimensionSize;
                    pixels[(int)index] = spritePixel;
                }

                sprite.Dispose();
            }
        }

        public readonly void Dispose()
        {
            texture.Dispose();
        }

        public readonly override string ToString()
        {
            return texture.ToString();
        }

        public readonly Pixel Get(uint x, uint y)
        {
            return texture.Get(x, y);
        }

        public readonly Color Evaluate(Vector2 position)
        {
            return texture.Evaluate(position);
        }

        public readonly Color Evaluate(float x, float y)
        {
            return texture.Evaluate(x, y);
        }

        public readonly struct InputSprite : IDisposable
        {
            public readonly TextureSize size;

            private readonly UnmanagedArray<char> name;
            private readonly UnmanagedArray<Pixel> pixels;

            public readonly ReadOnlySpan<char> Name => name.AsSpan();
            public readonly Span<Pixel> Pixels => pixels.AsSpan();

            public InputSprite()
            {
                throw new InvalidOperationException("Cannot create an input sprite without data.");
            }

            public InputSprite(ReadOnlySpan<char> name, TextureSize size, ReadOnlySpan<byte> inputData, Channels channels)
            {
                this.size = size;
                this.name = new(name);
                pixels = new((uint)inputData.Length);

                bool red = (channels & Channels.Red) == Channels.Red;
                bool green = (channels & Channels.Green) == Channels.Green;
                bool blue = (channels & Channels.Blue) == Channels.Blue;
                bool alpha = (channels & Channels.Alpha) == Channels.Alpha;
                for (uint i = 0; i < inputData.Length; i++)
                {
                    byte d = inputData[(int)i];
                    ref Pixel pixel = ref pixels.GetRef(i);
                    if (red) pixel.r = d;
                    if (green) pixel.g = d;
                    if (blue) pixel.b = d;
                    if (alpha) pixel.a = d;
                }
            }

            public InputSprite(ReadOnlySpan<char> name, TextureSize size)
            {
                this.size = size;
                this.name = new(name);
                pixels = new(size.width * size.height);
            }

            public readonly void Dispose()
            {
                pixels.Dispose();
                name.Dispose();
            }

            public readonly override string ToString()
            {
                return Name.ToString();
            }
        }
    }
}
