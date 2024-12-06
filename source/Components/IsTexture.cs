using Worlds;

namespace Textures.Components
{
    [Component]
    public struct IsTexture
    {
        public uint width;
        public uint height;
        public uint version;

        public IsTexture(uint width, uint height)
        {
            this.version = default;
            this.width = width;
            this.height = height;
        }

        public IsTexture(uint width, uint height, uint version)
        {
            this.version = version;
            this.width = width;
            this.height = height;
        }
    }
}
