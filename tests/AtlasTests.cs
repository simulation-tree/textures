using System;
using Worlds;

namespace Textures.Tests
{
    public class AtlasTests : TextureTests
    {
        [Test]
        public void CreateAtlasTextureFromSprites()
        {
            using World world = CreateWorld();
            Span<AtlasTexture.InputSprite> sprites = stackalloc AtlasTexture.InputSprite[4];
            AtlasTexture.InputSprite a = new("r", 32, 32);
            for (int i = 0; i < a.Pixels.Length; i++)
            {
                a.Pixels[i] = new(byte.MaxValue, 0, 0, 0);
            }

            AtlasTexture.InputSprite b = new("g", 32, 32);
            for (int i = 0; i < b.Pixels.Length; i++)
            {
                b.Pixels[i] = new(0, byte.MaxValue, 0, 0);
            }

            AtlasTexture.InputSprite c = new("b", 32, 32);
            for (int i = 0; i < c.Pixels.Length; i++)
            {
                c.Pixels[i] = new(0, 0, byte.MaxValue, 0);
            }

            AtlasTexture.InputSprite d = new("y", 32, 32);
            for (int i = 0; i < d.Pixels.Length; i++)
            {
                d.Pixels[i] = new(byte.MaxValue, byte.MaxValue, 0, 0);
            }

            sprites[0] = a;
            sprites[1] = b;
            sprites[2] = c;
            sprites[3] = d;

            AtlasTexture atlas = new(world, sprites);
            Assert.That(atlas.Width, Is.EqualTo(64));
            Assert.That(atlas.Height, Is.EqualTo(64));
            Assert.That(atlas.Sprites.Length, Is.EqualTo(4));
        }
    }
}