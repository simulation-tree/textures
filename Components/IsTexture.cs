namespace Textures.Components
{
    public struct IsTexture
    {
        public uint version;

        public IsTexture()
        {
            version = 0;
        }

        public IsTexture(uint version)
        {
            this.version = version;
        }
    }
}
