using System;
using Worlds;

namespace Textures.Tests
{
    public class TextureEntityTests : TextureTests
    {
        [Test]
        public void CreateEmptyTexture()
        {
            using World world = CreateWorld();
            Span<Pixel> pixels = stackalloc Pixel[16];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Pixel(byte.MaxValue, 0, 0, byte.MaxValue);
            }

            Texture emptyTexture = new(world, 4, 4, pixels);
            Assert.That(emptyTexture.IsCompliant, Is.True);
            Assert.That(emptyTexture.Width, Is.EqualTo(4));
            Assert.That(emptyTexture.Height, Is.EqualTo(4));
            Pixel[] pixelsArray = emptyTexture.Pixels.ToArray();
            Assert.That(pixelsArray.Length, Is.EqualTo(4 * 4));
            foreach (Pixel pixel in pixelsArray)
            {
                Assert.That(pixel.r, Is.EqualTo(byte.MaxValue));
                Assert.That(pixel.g, Is.EqualTo(0));
                Assert.That(pixel.b, Is.EqualTo(0));
                Assert.That(pixel.a, Is.EqualTo(byte.MaxValue));
            }
        }

        [Test]
        public void CreateEmptyCubemap()
        {
            using World world = CreateWorld();
            Texture right = new(world, 4, 4);
            Assert.That(right.Width, Is.EqualTo(4));
            Assert.That(right.Height, Is.EqualTo(4));
            Texture left = new(world, 4, 4);
            Assert.That(left.Width, Is.EqualTo(4));
            Assert.That(left.Height, Is.EqualTo(4));
            Texture up = new(world, 4, 4);
            Texture down = new(world, 4, 4);
            Texture forward = new(world, 4, 4);
            Texture back = new(world, 4, 4);
            CubemapTexture cubemap = new(world, right, left, up, down, forward, back);

            Assert.That(cubemap.IsCompliant, Is.True);
        }
    }
}