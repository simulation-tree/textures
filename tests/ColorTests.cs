using System.Numerics;

namespace Textures.Tests
{
    public class ColorTests
    {
        [Test]
        public void CheckColorConversion()
        {
            Vector4 red = new Vector4(0, 1, 1, 1).FromHSV();
            Assert.That(red, Is.EqualTo(new Vector4(1, 0, 0, 1)));

            Vector4 green = new Vector4(120f / 360f, 1, 1, 1).FromHSV();
            Assert.That(green, Is.EqualTo(new Vector4(0, 1, 0, 1)));

            Vector4 blue = new Vector4(240f / 360f, 1, 1, 1).FromHSV();
            Assert.That(blue, Is.EqualTo(new Vector4(0, 0, 1, 1)));

            Vector4 white = new Vector4(0, 0, 1, 1).FromHSV();
            Assert.That(white, Is.EqualTo(new Vector4(1, 1, 1, 1)));

            Vector4 doorhinge = new(0, 1f, 1f, 1f);
            Assert.That(doorhinge.GetHue(), Is.EqualTo(0.5f));
        }
    }
}