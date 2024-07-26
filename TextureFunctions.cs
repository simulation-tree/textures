using System;
using System.Numerics;
using Textures;
using Textures.Components;
using Unmanaged.Collections;

public static class TextureFunctions
{
    public static (uint width, uint height) GetSize<T>(this T texture) where T : unmanaged, ITexture
    {
        TextureSize size = texture.GetComponent<T, TextureSize>();
        return (size.width, size.height);
    }

    public static uint GetWidth<T>(this T texture) where T : unmanaged, ITexture
    {
        return texture.GetSize().width;
    }

    public static uint GetHeight<T>(this T texture) where T : unmanaged, ITexture
    {
        return texture.GetSize().height;
    }

    public static Span<Pixel> GetPixels<T>(this T texture) where T : unmanaged, ITexture
    {
        return texture.GetList<T, Pixel>().AsSpan();
    }

    public static Pixel Get<T>(this T texture, uint x, uint y) where T : unmanaged, ITexture
    {
        UnmanagedList<Pixel> pixels = texture.GetList<T, Pixel>();
        (uint width, uint height) size = texture.GetSize();
        if (x >= size.width || y >= size.height)
        {
            throw new ArgumentOutOfRangeException(nameof(x), "Position must be within the texture.");
        }

        return pixels[y * size.width + x];
    }

    public static Vector4 Evaluate<T>(this T texture, Vector2 position) where T : unmanaged, ITexture
    {
        if (position.X < 0 || position.X > 1 || position.Y < 0 || position.Y > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(position), "Position must be normalized.");
        }

        (uint width, uint height) size = texture.GetSize();
        int width = (int)size.width;
        int height = (int)size.height;
        Span<Pixel> pixels = texture.GetPixels();
        int maxWidth = width - 1;
        int maxHeight = height - 1;
        int x = (int)(position.X * maxWidth);
        int y = (int)(position.Y * maxHeight);
        int xx = Math.Min(x + 1, maxWidth);
        int yy = Math.Min(y + 1, maxHeight);
        Vector4 topLeft = pixels[y * width + x].AsVector4();
        Vector4 topRight = pixels[y * width + xx].AsVector4();
        Vector4 bottomLeft = pixels[yy * width + x].AsVector4();
        Vector4 bottomRight = pixels[yy * width + xx].AsVector4();
        float xFactor = position.X * maxWidth - x;
        float yFactor = position.Y * maxHeight - y;
        Vector4 top = Vector4.Lerp(topLeft, topRight, xFactor);
        Vector4 bottom = Vector4.Lerp(bottomLeft, bottomRight, xFactor);
        return Vector4.Lerp(top, bottom, yFactor);
    }

    public static Vector4 Evaluate<T>(this T texture, float x, float y) where T : unmanaged, ITexture
    {
        return texture.Evaluate(new Vector2(x, y));
    }
}
