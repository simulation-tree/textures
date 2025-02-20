namespace Textures.Components
{
    public readonly struct IsTexture
    {
        public readonly uint version;
        public readonly uint width;
        public readonly uint height;

        public readonly (uint width, uint height) Dimensions => (width, height);
        public readonly uint Length => width * height;

        public IsTexture(uint version, uint width, uint height)
        {
            this.version = version;
            this.width = width;
            this.height = height;
        }
    }
}