using System;
using Textures;

public static class AtlasTextureFunctions
{
    public static Texture AsTexture<T>(this T atlasTexture) where T : IAtlasTexture
    {
        return new Texture(atlasTexture.GetWorld(), atlasTexture.GetEntityValue());
    }

    public static ReadOnlySpan<AtlasSprite> GetSprites<T>(this T atlasTexture) where T : IAtlasTexture
    {
        return atlasTexture.GetList<T, AtlasSprite>().AsSpan();
    }

    public static bool TryGetSprite<T>(this T atlasTexture, ReadOnlySpan<char> name, out AtlasSprite sprite) where T : IAtlasTexture
    {
        ReadOnlySpan<AtlasSprite> sprites = atlasTexture.GetSprites();
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

    public static AtlasSprite GetSprite<T>(this T atlasTexture, ReadOnlySpan<char> name) where T : IAtlasTexture
    {
        if (!atlasTexture.TryGetSprite(name, out AtlasSprite sprite))
        {
            throw new InvalidOperationException($"Sprite named `{name.ToString()}` not found in atlas texture `{atlasTexture}`");
        }

        return sprite;
    }

    public static AtlasSprite GetSprite<T>(this T atlasTexture, uint index) where T : IAtlasTexture
    {
        ReadOnlySpan<AtlasSprite> sprites = atlasTexture.GetSprites();
        if (index >= sprites.Length)
        {
            throw new IndexOutOfRangeException($"Index {index} out of range");
        }

        return sprites[(int)index];
    }

    public static AtlasSprite GetSprite<T>(this T atlasTexture, int index) where T : IAtlasTexture
    {
        if (index < 0)
        {
            throw new IndexOutOfRangeException($"Index {index} out of range");
        }

        return atlasTexture.GetSprite((uint)index);
    }
}
