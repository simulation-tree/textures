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
        public readonly Span<Pixel> Pixels => GetArray<Pixel>();
        public readonly (int width, int height) Dimensions => As<Texture>().Dimensions;
        public readonly int Width => GetComponent<IsTexture>().width;
        public readonly int Height => GetComponent<IsTexture>().height;
        public readonly ReadOnlySpan<AtlasSprite> Sprites => GetArray<AtlasSprite>();
        public readonly int SpriteCount => GetArrayLength<AtlasSprite>();
        public readonly AtlasSprite this[int index] => GetArrayElement<AtlasSprite>(index);
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
        public AtlasTexture(World world, ReadOnlySpan<InputSprite> sprites, int padding = 0, IsTextureRequest.Flags flags = default)
        {
            //find the max sprite size
            int spriteCount = sprites.Length;
            Span<Vector2> sizes = stackalloc Vector2[spriteCount];
            for (int s = 0; s < spriteCount; s++)
            {
                InputSprite inputSprite = sprites[s];
                sizes[s] = new(inputSprite.width, inputSprite.height);
            }

            RecursivePacker packer = new();
            Span<Vector2> positions = stackalloc Vector2[spriteCount];
            Vector2 maxSize = packer.Pack(sizes, positions, padding);
            int atlasWidth = (int)maxSize.X;
            int atlasHeight = (int)maxSize.Y;
            this.world = world;
            value = world.CreateEntity(new IsTexture(1, atlasWidth, atlasHeight));
            Span<AtlasSprite> spritesList = CreateArray<AtlasSprite>(spriteCount);
            Span<Pixel> atlasPixels = CreateArray<Pixel>(atlasWidth * atlasHeight);
            for (int s = 0; s < spriteCount; s++)
            {
                InputSprite sprite = sprites[s];
                Vector2 position = positions[s];
                Vector2 size = sizes[s];
                int spriteX = (int)position.X;
                int spriteY = (int)position.Y;
                int spriteWidth = (int)size.X;
                int spriteHeight = (int)size.Y;
                Span<Pixel> spritePixels = sprite.Pixels;
                for (int p = 0; p < spritePixels.Length; p++)
                {
                    Texture.GetPosition(p, spriteWidth, out int px, out int py);
                    atlasPixels[Texture.GetIndex(spriteX + px, spriteY + py, atlasWidth)] = spritePixels[p];
                }

                Vector4 region = default;
                region.X = spriteX / (float)atlasWidth;
                region.Y = spriteY / (float)atlasHeight;
                region.Z = region.X + spriteWidth / (float)atlasWidth;
                region.W = region.Y + spriteHeight / (float)atlasHeight;

                //flip y
                if ((flags & IsTextureRequest.Flags.FlipY) != 0)
                {
                    region.Y += region.W;
                    region.W *= -1;
                }

                //bleed the sprite
                if (padding > 0 && (flags & IsTextureRequest.Flags.BleedPixels) != 0)
                {
                    //left
                    for (int y = -padding; y < spriteHeight + padding; y++)
                    {
                        Pixel spritePixel = spritePixels[Texture.GetIndex(0, Math.Min(0, Math.Max(spriteHeight - 1, y)), spriteWidth)];
                        for (int o = 1; o <= padding; o++)
                        {
                            ref Pixel atlasPixel = ref atlasPixels[Texture.GetIndex(spriteX - o, spriteY + y, atlasWidth)];
                            atlasPixel = atlasPixel == default ? spritePixel : Pixel.Average(atlasPixel, spritePixel);
                        }
                    }

                    //right
                    for (int y = -padding; y < spriteHeight + padding; y++)
                    {
                        Pixel spritePixel = spritePixels[Texture.GetIndex(spriteWidth - 1, Math.Min(0, Math.Max(spriteHeight - 1, y)), spriteWidth)];
                        for (int o = 0; o < padding; o++)
                        {
                            ref Pixel atlasPixel = ref atlasPixels[Texture.GetIndex(spriteX + spriteWidth + o, spriteY + y, atlasWidth)];
                            atlasPixel = atlasPixel == default ? spritePixel : Pixel.Average(atlasPixel, spritePixel);
                        }
                    }

                    //down
                    for (int x = -padding; x < spriteWidth + padding; x++)
                    {
                        Pixel spritePixel = spritePixels[Texture.GetIndex(Math.Min(0, Math.Max(spriteWidth - 1, x)), 0, spriteWidth)];
                        for (int o = 1; o <= padding; o++)
                        {
                            ref Pixel atlasPixel = ref atlasPixels[Texture.GetIndex(spriteX + x, spriteY - o, atlasWidth)];
                            atlasPixel = atlasPixel == default ? spritePixel : Pixel.Average(atlasPixel, spritePixel);
                        }
                    }

                    //up
                    for (int x = -padding; x < spriteWidth + padding; x++)
                    {
                        Pixel spritePixel = spritePixels[Texture.GetIndex(Math.Min(0, Math.Max(spriteWidth - 1, x)), spriteHeight - 1, spriteWidth)];
                        for (int o = 0; o < padding; o++)
                        {
                            ref Pixel atlasPixel = ref atlasPixels[Texture.GetIndex(spriteX + x, spriteY + spriteHeight + o, atlasWidth)];
                            atlasPixel = atlasPixel == default ? spritePixel : Pixel.Average(atlasPixel, spritePixel);
                        }
                    }
                }

                spritesList[s] = new(sprite.name, region);
                sprite.Dispose();
            }
        }

        public readonly override string ToString()
        {
            Span<char> buffer = stackalloc char[64];
            int length = ToString(buffer);
            return buffer.Slice(0, length).ToString();
        }

        public readonly int ToString(Span<char> destination)
        {
            return As<Texture>().ToString(destination);
        }

        public readonly bool TryGetSprite(ReadOnlySpan<char> name, out AtlasSprite sprite)
        {
            return TryGetSprite(new ASCIIText256(name), out sprite);
        }

        public readonly bool TryGetSprite(ASCIIText256 name, out AtlasSprite sprite)
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

        public readonly bool ContainsSprite(ASCIIText256 name)
        {
            ReadOnlySpan<AtlasSprite> sprites = Sprites;
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i].name.Equals(name))
                {
                    return true;
                }
            }

            return false;
        }

        public readonly AtlasSprite GetSprite(ReadOnlySpan<char> name)
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
            public readonly int width;
            public readonly int height;

            private readonly Array<Pixel> pixels;

            /// <summary>
            /// All pixels of this sprite.
            /// </summary>
            public readonly Span<Pixel> Pixels => pixels.AsSpan();

            public ref Pixel this[int index] => ref pixels[index];
            public ref Pixel this[int x, int y] => ref pixels[x + (y * width)];

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
            public InputSprite(ReadOnlySpan<char> name, int width, int height, ReadOnlySpan<byte> channelData, Channels channels)
                : this(new ASCIIText256(name), width, height, channelData, channels)
            {
            }

            /// <summary>
            /// A sprite with preset data spread out across
            /// the given channel mask.
            /// </summary>
            public InputSprite(ASCIIText256 name, int width, int height, ReadOnlySpan<byte> channelData, Channels channels)
            {
                this.width = width;
                this.height = height;
                this.name = name;
                pixels = new(channelData.Length);

                bool red = (channels & Channels.Red) == Channels.Red;
                bool green = (channels & Channels.Green) == Channels.Green;
                bool blue = (channels & Channels.Blue) == Channels.Blue;
                bool alpha = (channels & Channels.Alpha) == Channels.Alpha;
                for (int i = 0; i < channelData.Length; i++)
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
            public InputSprite(ASCIIText256 name, int width, int height, ReadOnlySpan<Pixel> pixels)
            {
                this.width = width;
                this.height = height;
                this.name = name;
                this.pixels = new(pixels);
            }

            /// <summary>
            /// A sprite with preset data.
            /// </summary>
            public InputSprite(ReadOnlySpan<char> name, int width, int height, ReadOnlySpan<Pixel> pixels)
                : this(new ASCIIText256(name), width, height, pixels)
            {
            }

            /// <summary>
            /// A blank sprite with default pixels.
            /// </summary>
            public InputSprite(ASCIIText256 name, int width, int height)
            {
                this.width = width;
                this.height = height;
                this.name = name;
                pixels = new(width * height);
            }

            /// <summary>
            /// A blank sprite with default pixels.
            /// </summary>
            public InputSprite(ReadOnlySpan<char> name, int width, int height)
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
