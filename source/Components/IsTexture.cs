namespace Textures.Components
{
    public readonly struct IsTexture
    {
        public readonly uint version;
        public readonly uint width;
        public readonly uint height;

        public IsTexture(uint version, uint width, uint height)
        {
            this.version = version;
            this.width = width;
            this.height = height;
        }
    }
}