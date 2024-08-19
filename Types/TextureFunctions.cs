using Simulation;
using System.Threading;
using System.Threading.Tasks;
using Textures;

public static class TextureFunctions
{
    /// <summary>
    /// Awaits until pixel data is available on the texture.
    /// </summary>
    public static async Task UntilLoaded<T>(this T texture, CancellationToken cancellation) where T : unmanaged, ITexture
    {
        World world = texture.World;
        eint textureEntity = texture.Value;
        while (!world.ContainsList<Pixel>(textureEntity))
        {
            await Task.Delay(1, cancellation);
        }
    }
}