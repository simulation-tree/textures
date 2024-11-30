using Worlds;

namespace Textures.Components
{
    [Component]
    public struct IsTextureRequest
    {
        public uint version;

        public IsTextureRequest(uint version)
        {
            this.version = version;
        }
    }
}
