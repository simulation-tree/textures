using Worlds;

namespace Textures.Components
{
    [Component]
    public struct IsTexture
    {
        public uint version;
        public uint width;
        public uint height;

        public IsTexture(uint width, uint height)
        {
            this.version = default;
            this.width = width;
            this.height = height;
        }
    }
}
