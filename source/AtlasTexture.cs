using BinPacker;
using Data;
using Simulation;
using System;
using System.Diagnostics;
using System.Numerics;
using Textures.Components;
using Unmanaged;
using Unmanaged.Collections;

namespace Textures
{
    public readonly struct AtlasTexture : IEntity
    {
        public readonly Texture texture;

        public readonly USpan<AtlasSprite> Sprites => texture.entity.GetArray<AtlasSprite>();
        public readonly (uint width, uint height) Size => texture.Size;
        public readonly uint Width => texture.Width;
        public readonly uint Height => texture.Height;
        public readonly uint SpriteCount => texture.entity.GetArrayLength<AtlasSprite>();
        public readonly AtlasSprite this[uint index] => texture.entity.GetArrayElementRef<AtlasSprite>(index);
        public readonly AtlasSprite this[FixedString name] => GetSprite(name);

        readonly uint IEntity.Value => texture.entity.value;
        readonly World IEntity.World => texture.entity.world;
        readonly Definition IEntity.Definition => new Definition().AddComponentType<IsTexture>().AddArrayTypes<Pixel, AtlasSprite>();

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
        public AtlasTexture(World world, USpan<InputSprite> sprites, uint padding = 0)
        {
            //find the max sprite size
            uint spriteCount = sprites.Length;
            using UnmanagedArray<Vector2> sizes = new(spriteCount);
            Vector2 maxSpriteSize = default;
            for (uint i = 0; i < spriteCount; i++)
            {
                InputSprite inputSprite = sprites[i];
                Vector2 spriteSize = new(inputSprite.width, inputSprite.height);
                sizes[i] = spriteSize;
                maxSpriteSize = Vector2.Max(maxSpriteSize, spriteSize);
            }

            RecursivePacker packer = new();
            USpan<Vector2> positions = stackalloc Vector2[(int)spriteCount];
            Vector2 maxSize = packer.Pack(sizes.AsSpan(), positions, padding);
            uint atlasWidth = (uint)maxSize.X;
            uint atlasHeight = (uint)maxSize.Y;
            texture = new(world, atlasWidth, atlasHeight);
            USpan<AtlasSprite> spritesList = texture.entity.CreateArray<AtlasSprite>(spriteCount);
            USpan<Pixel> pixels = texture.Pixels;
            for (uint i = 0; i < spriteCount; i++)
            {
                InputSprite sprite = sprites[i];
                Vector2 position = positions[i];
                Vector2 size = sizes[i];
                uint x = (uint)position.X;
                uint y = (uint)position.Y;
                uint width = (uint)size.X;
                uint height = (uint)size.Y;
                USpan<Pixel> spritePixels = sprite.Pixels;
                for (uint p = 0; p < spritePixels.Length; p++)
                {
                    Pixel spritePixel = spritePixels[p];
                    uint px = p % width;
                    uint py = p / width;
                    uint index = x + px + ((y + py) * atlasWidth);
                    pixels[index] = spritePixel;
                }

                Vector4 region = default;
                region.X = x / (float)atlasWidth;
                region.Y = y / (float)atlasHeight;
                region.Z = region.X + width / (float)atlasWidth;
                region.W = region.Y + height / (float)atlasHeight;

                //flip y
                //region.Y += region.W;
                //region.W *= -1;

                spritesList[i] = new(sprite.name, region);
                sprite.Dispose();
            }
        }

        public readonly override string ToString()
        {
            return texture.ToString();
        }

        public readonly bool TryGetSprite(USpan<char> name, out AtlasSprite sprite)
        {
            return TryGetSprite(new FixedString(name), out sprite);
        }

        public readonly bool TryGetSprite(FixedString name, out AtlasSprite sprite)
        {
            USpan<AtlasSprite> sprites = Sprites;
            for (uint i = 0; i < sprites.Length; i++)
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

        public readonly bool ContainsSprite(FixedString name)
        {
            USpan<AtlasSprite> sprites = Sprites;
            for (uint i = 0; i < sprites.Length; i++)
            {
                if (sprites[i].name.Equals(name))
                {
                    return true;
                }
            }

            return false;
        }

        public readonly AtlasSprite GetSprite(USpan<char> name)
        {
            ThrowIfSpriteIsMissing(new FixedString(name));
            TryGetSprite(name, out AtlasSprite sprite);
            return sprite;
        }

        public readonly AtlasSprite GetSprite(FixedString name)
        {
            ThrowIfSpriteIsMissing(name);
            TryGetSprite(name, out AtlasSprite sprite);
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

        [Conditional("DEBUG")]
        private readonly void ThrowIfSpriteIsMissing(FixedString name)
        {
            if (!ContainsSprite(name))
            {
                throw new InvalidOperationException($"Sprite named `{name}` not found in atlas texture `{texture}`");
            }
        }

        public static implicit operator Texture(AtlasTexture atlasTexture)
        {
            return atlasTexture.texture;
        }

        public static implicit operator Entity(AtlasTexture atlasTexture)
        {
            return atlasTexture.AsEntity();
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
            public readonly USpan<Pixel> Pixels => pixels.AsSpan();

            public ref Pixel this[uint index] => ref pixels[index];
            public ref Pixel this[uint x, uint y] => ref pixels[x + (y * width)];

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
            public InputSprite(USpan<char> name, uint width, uint height, Channels channels, USpan<byte> channelData)
                : this(new FixedString(name), width, height, channels, channelData)
            {
            }

            /// <summary>
            /// A sprite with preset data spread out across
            /// the given channel mask.
            /// </summary>
            public InputSprite(FixedString name, uint width, uint height, Channels channels, USpan<byte> channelData)
            {
                this.width = width;
                this.height = height;
                this.name = name;
                pixels = new(channelData.Length);

                bool red = (channels & Channels.Red) == Channels.Red;
                bool green = (channels & Channels.Green) == Channels.Green;
                bool blue = (channels & Channels.Blue) == Channels.Blue;
                bool alpha = (channels & Channels.Alpha) == Channels.Alpha;
                for (uint i = 0; i < channelData.Length; i++)
                {
                    byte d = channelData[i];
                    ref Pixel pixel = ref pixels[i];
                    if (red) pixel.r = d;
                    if (green) pixel.g = d;
                    if (blue) pixel.b = d;
                    if (alpha) pixel.a = d;
                }
            }

            /// <summary>
            /// A sprite with preset data.
            /// </summary>
            public InputSprite(FixedString name, uint width, uint height, USpan<Pixel> pixels)
            {
                this.width = width;
                this.height = height;
                this.name = name;
                this.pixels = new(pixels);
            }

            /// <summary>
            /// A sprite with preset data.
            /// </summary>
            public InputSprite(USpan<char> name, uint width, uint height, USpan<Pixel> pixels)
                : this(new FixedString(name), width, height, pixels)
            {
            }

            /// <summary>
            /// A blank sprite with default pixels.
            /// </summary>
            public InputSprite(FixedString name, uint width, uint height)
            {
                this.width = width;
                this.height = height;
                this.name = name;
                pixels = new(width * height);
            }

            /// <summary>
            /// A blank sprite with default pixels.
            /// </summary>
            public InputSprite(USpan<char> name, uint width, uint height)
                : this(new FixedString(name), width, height)
            {
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
