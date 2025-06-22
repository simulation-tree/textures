using System;
using System.Diagnostics;
using Textures.Components;

namespace Textures
{
    public static class TextureRequestFlagsExtensions
    {
        public static TextureType GetTextureType(this IsTextureRequest.Flags flags)
        {
            ThrowIfUnknownType(flags);

            if ((flags & IsTextureRequest.Flags.CubemapTexture) != 0)
            {
                return TextureType.Cubemap;
            }
            else if ((flags & IsTextureRequest.Flags.AtlasTexture) != 0)
            {
                return TextureType.Atlas;
            }

            return TextureType.Flat;
        }

        [Conditional("DEBUG")]
        private static void ThrowIfUnknownType(IsTextureRequest.Flags flags)
        {
            bool standardTexture = (flags & IsTextureRequest.Flags.FlatTexture) != 0;
            bool cubemapTexture = (flags & IsTextureRequest.Flags.CubemapTexture) != 0;
            bool atlasTexture = (flags & IsTextureRequest.Flags.AtlasTexture) != 0;
            if (standardTexture && cubemapTexture && atlasTexture)
            {
                throw new NotSupportedException("Texture cannot be standard, cubemap, and atlas texture at the same time");
            }
            else if (!standardTexture && !cubemapTexture && !atlasTexture)
            {
                throw new NotSupportedException("Texture must be either standard, cubemap, or atlas texture");
            }
            else
            {
                if (standardTexture && cubemapTexture)
                {
                    throw new NotSupportedException("Texture cannot be both standard and cubemap texture at the same time");
                }
                else if (standardTexture && atlasTexture)
                {
                    throw new NotSupportedException("Texture cannot be both standard and atlas texture at the same time");
                }
                else if (cubemapTexture && atlasTexture)
                {
                    throw new NotSupportedException("Texture cannot be both cubemap and atlas texture at the same time");
                }
            }
        }
    }
}
