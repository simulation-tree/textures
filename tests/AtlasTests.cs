using System;
using Textures.Components;
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

        [Test]
        public void BleedingSprites()
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

            AtlasTexture atlas = new(world, sprites, 4, IsTextureRequest.Flags.BleedPixels);
            Assert.That(atlas.Width, Is.EqualTo(128));
            Assert.That(atlas.Height, Is.EqualTo(128));
            Assert.That(atlas.Sprites.Length, Is.EqualTo(4));

            Texture texture = atlas;
            Pixel bottomLeft = texture.GetPixelAt(32, 32);
            Pixel topLeft = texture.GetPixelAt(32, 64);
            Pixel bottomRight = texture.GetPixelAt(64, 32);
            Pixel topRight = texture.GetPixelAt(64, 64);

            //check left side
            Assert.That(texture.GetPixelAt(3, 32), Is.EqualTo(bottomLeft));
            Assert.That(texture.GetPixelAt(2, 32), Is.EqualTo(bottomLeft));
            Assert.That(texture.GetPixelAt(1, 32), Is.EqualTo(bottomLeft));
            Assert.That(texture.GetPixelAt(0, 32), Is.EqualTo(bottomLeft));

            //check right side
            Assert.That(texture.GetPixelAt(36, 32), Is.EqualTo(bottomLeft));
            Assert.That(texture.GetPixelAt(37, 32), Is.EqualTo(bottomLeft));
            Assert.That(texture.GetPixelAt(38, 32), Is.EqualTo(bottomLeft));
            Assert.That(texture.GetPixelAt(39, 32), Is.EqualTo(bottomLeft));

            //check above
            Assert.That(texture.GetPixelAt(32, 0), Is.EqualTo(bottomLeft));
            Assert.That(texture.GetPixelAt(32, 1), Is.EqualTo(bottomLeft));
            Assert.That(texture.GetPixelAt(32, 2), Is.EqualTo(bottomLeft));
            Assert.That(texture.GetPixelAt(32, 3), Is.EqualTo(bottomLeft));

            //check below
            Assert.That(texture.GetPixelAt(32, 36), Is.EqualTo(bottomLeft));
            Assert.That(texture.GetPixelAt(32, 37), Is.EqualTo(bottomLeft));
            Assert.That(texture.GetPixelAt(32, 38), Is.EqualTo(bottomLeft));
            Assert.That(texture.GetPixelAt(32, 39), Is.EqualTo(bottomLeft));

            //check bottom left
            Assert.That(texture.GetPixelAt(0, 0), Is.EqualTo(bottomLeft));
            Assert.That(texture.GetPixelAt(3, 0), Is.EqualTo(bottomLeft));
            Assert.That(texture.GetPixelAt(0, 3), Is.EqualTo(bottomLeft));
            Assert.That(texture.GetPixelAt(3, 3), Is.EqualTo(bottomLeft));

            Assert.That(texture.GetPixelAt(32, 40), Is.EqualTo(topLeft));

            Assert.That(texture.GetPixelAt(40, 32), Is.EqualTo(bottomRight));
            Assert.That(texture.GetPixelAt(41, 32), Is.EqualTo(bottomRight));
            Assert.That(texture.GetPixelAt(42, 32), Is.EqualTo(bottomRight));
            Assert.That(texture.GetPixelAt(43, 32), Is.EqualTo(bottomRight));
            Assert.That(texture.GetPixelAt(64, 32), Is.EqualTo(bottomRight));
        }
    }
}