using BinPacking;
using Data;
using Simulation;
using System;
using System.Numerics;
using Textures.Components;
using Unmanaged;
using Unmanaged.Collections;

namespace Textures
{
    public readonly struct AtlasTexture : IEntity
    {
        private readonly Texture texture;

        public readonly ReadOnlySpan<AtlasSprite> Sprites => ((Entity)texture).GetArray<AtlasSprite>();
        public readonly (uint width, uint height) Size => texture.Size;
        public readonly uint Width => texture.Width;
        public readonly uint Height => texture.Height;
        public readonly uint SpriteCount => ((Entity)texture).GetArrayLength<AtlasSprite>();
        public readonly AtlasSprite this[uint index] => ((Entity)texture).GetArrayElement<AtlasSprite>(index);

        World IEntity.World => (Entity)texture;
        uint IEntity.Value => (Entity)texture;

#if NET
        [Obsolete("Default constructor not available.", true)]
        public AtlasTexture()
        {
            throw new InvalidOperationException("Cannot create an atlas texture without data.");
        }
#endif

        public AtlasTexture(World world, uint existingEntity)
        {
            texture = new(world, existingEntity);
        }

        /// <summary>
        /// Creates a new atlas from the given input sprites
        /// into the smallest size possible.
        /// </summary>
        public AtlasTexture(World world, ReadOnlySpan<InputSprite> sprites, uint padding = 0)
        {
            //find the max sprite size
            uint spriteCount = (uint)sprites.Length;
            using UnmanagedArray<Vector2> sizes = new(spriteCount);
            Vector2 maxSpriteSize = default;
            for (uint i = 0; i < spriteCount; i++)
            {
                InputSprite inputSprite = sprites[(int)i];
                Vector2 spriteSize = new(inputSprite.width, inputSprite.height);
                sizes[i] = spriteSize;
                maxSpriteSize = Vector2.Max(maxSpriteSize, spriteSize);
            }

            RecursivePacker packer = new();
            Span<Vector2> positions = stackalloc Vector2[(int)spriteCount];
            Vector2 maxSize = packer.Pack(sizes.AsSpan(), positions, padding);
            uint atlasWidth = (uint)maxSize.X;
            uint atlasHeight = (uint)maxSize.Y;
            texture = new(world, atlasWidth, atlasHeight);
            Span<AtlasSprite> spritesList = ((Entity)texture).CreateArray<AtlasSprite>(spriteCount);
            Span<Pixel> pixels = texture.Pixels;
            for (int i = 0; i < spriteCount; i++)
            {
                InputSprite sprite = sprites[i];
                Vector2 position = positions[i];
                Vector2 size = sizes[(uint)i];
                uint x = (uint)position.X;
                uint y = (uint)position.Y;
                uint width = (uint)size.X;
                uint height = (uint)size.Y;
                Span<Pixel> spritePixels = sprite.Pixels;
                for (uint p = 0; p < spritePixels.Length; p++)
                {
                    Pixel spritePixel = spritePixels[(int)p];
                    uint px = p % width;
                    uint py = p / width;
                    uint index = x + px + ((y + py) * atlasWidth);
                    pixels[(int)index] = spritePixel;
                }

                Vector4 uv = new(x / (float)atlasWidth, y / (float)atlasHeight, width / (float)atlasWidth, height / (float)atlasHeight);
                uv.Y += uv.W;
                uv.W *= -1;
                spritesList[i] = new(sprite.name, uv);
                sprite.Dispose();
            }
        }

        public readonly override string ToString()
        {
            return texture.ToString();
        }

        readonly Query IEntity.GetQuery(World world)
        {
            //todo: either make the query say that it looks for entities with a list,
            //or have a component that says "i have a list", because a texture is the same as an atlas texture according to this...
            return new(world, RuntimeType.Get<IsTexture>());
        }

        public readonly bool TryGetSprite(ReadOnlySpan<char> name, out AtlasSprite sprite)
        {
            ReadOnlySpan<AtlasSprite> sprites = Sprites;
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i].name.Equals(name))
                {
                    sprite = sprites[i];
                    return true;
                }
            }

            sprite = default;
            return false;
        }

        public readonly AtlasSprite GetSprite(ReadOnlySpan<char> name)
        {
            if (!TryGetSprite(name, out AtlasSprite sprite))
            {
                throw new InvalidOperationException($"Sprite named `{name.ToString()}` not found in atlas texture `{texture}`");
            }

            return sprite;
        }

        public readonly Color Evaluate(Vector2 position)
        {
            return texture.Evaluate(position);
        }

        public readonly Color Evaluate(float x, float y)
        {
            return texture.Evaluate(x, y);
        }

        public static implicit operator Texture(AtlasTexture atlasTexture)
        {
            return atlasTexture.texture;
        }

        public static implicit operator Entity(AtlasTexture atlasTexture)
        {
            return atlasTexture.texture;
        }

        public readonly struct InputSprite : IDisposable
        {
            public readonly FixedString name;
            public readonly uint width;
            public readonly uint height;

            private readonly UnmanagedArray<Pixel> pixels;

            /// <summary>
            /// All pixels of this sprite.
            /// </summary>
            public readonly Span<Pixel> Pixels => pixels.AsSpan();

            public ref Pixel this[uint index] => ref pixels.GetRef(index);
            public ref Pixel this[uint x, uint y] => ref pixels.GetRef(x + (y * width));

#if NET
            [Obsolete("Default constructor not available", true)]
            public InputSprite()
            {
                throw new InvalidOperationException("Cannot create an input sprite without data.");
            }
#endif

            /// <summary>
            /// A sprite with preset data spread out across
            /// the given channel mask.
            /// </summary>
            public InputSprite(ReadOnlySpan<char> name, uint width, uint height, Channels channels, ReadOnlySpan<byte> channelData)
            {
                this.width = width;
                this.height = height;
                this.name = new(name);
                pixels = new((uint)channelData.Length);

                bool red = (channels & Channels.Red) == Channels.Red;
                bool green = (channels & Channels.Green) == Channels.Green;
                bool blue = (channels & Channels.Blue) == Channels.Blue;
                bool alpha = (channels & Channels.Alpha) == Channels.Alpha;
                for (uint i = 0; i < channelData.Length; i++)
                {
                    byte d = channelData[(int)i];
                    ref Pixel pixel = ref pixels.GetRef(i);
                    if (red) pixel.r = d;
                    if (green) pixel.g = d;
                    if (blue) pixel.b = d;
                    if (alpha) pixel.a = d;
                }
            }

            /// <summary>
            /// A sprite with preset data.
            /// </summary>
            public InputSprite(ReadOnlySpan<char> name, uint width, uint height, ReadOnlySpan<Pixel> pixels)
            {
                this.width = width;
                this.height = height;
                this.name = new(name);
                this.pixels = new(pixels);
            }

            /// <summary>
            /// A blank sprite with default pixels.
            /// </summary>
            public InputSprite(ReadOnlySpan<char> name, uint width, uint height)
            {
                this.width = width;
                this.height = height;
                this.name = new(name);
                pixels = new(width * height);
            }

            public readonly void Dispose()
            {
                pixels.Dispose();
            }

            public readonly override string ToString()
            {
                return name.ToString();
            }
        }
    }
}
