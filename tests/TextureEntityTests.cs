using Unmanaged;
using Worlds;

namespace Textures.Tests
{
    public class TextureEntityTests : TextureTests
    {
        [Test]
        public void CreateEmptyTexture()
        {
            using World world = CreateWorld();
            USpan<Pixel> pixels = stackalloc Pixel[16];
            for (uint i = 0; i < pixels.Length; i++)
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
    }
}