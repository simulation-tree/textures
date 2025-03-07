using BinPacker;
using Collections.Generic;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Textures.Components;
using Unmanaged;
using Worlds;

namespace Textures
{
    public readonly partial struct AtlasTexture : IEntity
    {
        public readonly USpan<Pixel> Pixels => GetArray<Pixel>().AsSpan();
        public readonly (uint width, uint height) Dimensions => As<Texture>().Dimensions;
        public readonly uint Width => GetComponent<IsTexture>().width;
        public readonly uint Height => GetComponent<IsTexture>().height;
        public readonly USpan<AtlasSprite> Sprites => GetArray<AtlasSprite>().AsSpan();
        public readonly uint SpriteCount => GetArrayLength<AtlasSprite>();
        public readonly AtlasSprite this[uint index] => GetArrayElement<AtlasSprite>(index);
        public readonly AtlasSprite this[ASCIIText256 name] => GetSprite(name);

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsTexture>();
            archetype.AddArrayType<Pixel>();
            archetype.AddArrayType<AtlasSprite>();
            archetype.AddTagType<IsAtlasTexture>();
        }

        /// <summary>
        /// Creates a new atlas from the given input <paramref name="sprites"/>
        /// into the smallest size possible.
        /// <para>
        /// All of the given <paramref name="sprites"/> will be disposed after
        /// this call.
        /// </para>
        /// </summary>
        [SkipLocalsInit]
        public AtlasTexture(World world, USpan<InputSprite> sprites, uint padding = 0)
        {
            //find the max sprite size
            uint spriteCount = sprites.Length;
            USpan<Vector2> sizes = stackalloc Vector2[(int)spriteCount];
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
            Vector2 maxSize = packer.Pack(sizes, positions, padding);
            uint atlasWidth = (uint)maxSize.X;
            uint atlasHeight = (uint)maxSize.Y;
            this.world = world;
            value = world.CreateEntity(new IsTexture(1, atlasWidth, atlasHeight));
            USpan<AtlasSprite> spritesList = CreateArray<AtlasSprite>(spriteCount).AsSpan();
            USpan<Pixel> pixels = CreateArray<Pixel>(atlasWidth * atlasHeight).AsSpan();
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
            USpan<char> buffer = stackalloc char[64];
            uint length = ToString(buffer);
            return buffer.GetSpan(length).ToString();
        }

        public readonly uint ToString(USpan<char> buffer)
        {
            return As<Texture>().ToString(buffer);
        }

        public readonly bool TryGetSprite(USpan<char> name, out AtlasSprite sprite)
        {
            return TryGetSprite(new ASCIIText256(name), out sprite);
        }

        public readonly bool TryGetSprite(ASCIIText256 name, out AtlasSprite sprite)
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

        public readonly bool ContainsSprite(ASCIIText256 name)
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
            ThrowIfSpriteIsMissing(new ASCIIText256(name));

            TryGetSprite(name, out AtlasSprite sprite);
            return sprite;
        }

        public readonly AtlasSprite GetSprite(ASCIIText256 name)
        {
            ThrowIfSpriteIsMissing(name);

            TryGetSprite(name, out AtlasSprite sprite);
            return sprite;
        }

        [Conditional("DEBUG")]
        private readonly void ThrowIfSpriteIsMissing(ASCIIText256 name)
        {
            if (!ContainsSprite(name))
            {
                throw new InvalidOperationException($"Sprite named `{name}` not found in atlas texture `{value}`");
            }
        }

        public static implicit operator Texture(AtlasTexture atlasTexture)
        {
            return atlasTexture.As<Texture>();
        }

        public readonly struct InputSprite : IDisposable
        {
            public readonly ASCIIText256 name;
            public readonly uint width;
            public readonly uint height;

            private readonly Array<Pixel> pixels;

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
            public InputSprite(USpan<char> name, uint width, uint height, USpan<byte> channelData, Channels channels)
                : this(new ASCIIText256(name), width, height, channelData, channels)
            {
            }

            /// <summary>
            /// A sprite with preset data spread out across
            /// the given channel mask.
            /// </summary>
            public InputSprite(ASCIIText256 name, uint width, uint height, USpan<byte> channelData, Channels channels)
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
            /// Creates an input sprite from the given <paramref name="texture"/>.
            /// </summary>
            public InputSprite(ASCIIText256 name, Texture texture)
            {
                (width, height) = texture.Dimensions;
                this.name = name;
                pixels = new(texture.Pixels);
            }

            /// <summary>
            /// A sprite with preset data.
            /// </summary>
            public InputSprite(ASCIIText256 name, uint width, uint height, USpan<Pixel> pixels)
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
                : this(new ASCIIText256(name), width, height, pixels)
            {
            }

            /// <summary>
            /// A blank sprite with default pixels.
            /// </summary>
            public InputSprite(ASCIIText256 name, uint width, uint height)
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
                : this(new ASCIIText256(name), width, height)
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
