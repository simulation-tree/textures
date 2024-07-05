namespace Textures.Components
{
    public struct IsTexture
    {
        /// <summary>
        /// When the underlying <see cref="byte"/> collection on the entity has changed,
        /// indicating that the <see cref="Pixel"/> collection should be updated.
        /// </summary>
        public bool changed;

        public IsTexture()
        {
            changed = true;
        }

        public IsTexture(bool changed)
        {
            this.changed = changed;
        }
    }
}
