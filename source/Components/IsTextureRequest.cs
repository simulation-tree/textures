using System;
using Unmanaged;

namespace Textures.Components
{
    public struct IsTextureRequest
    {
        public readonly Flags flags;
        public ASCIIText256 address;
        public double timeout;
        public double duration;
        public Status status;

        public IsTextureRequest(Flags flags, ASCIIText256 address, double timeout)
        {
            this.flags = flags;
            this.address = address;
            this.timeout = timeout;
            duration = 0;
            status = Status.Submitted;
        }

        public enum Status : byte
        {
            Submitted,
            Loading,
            Loaded,
            NotFound
        }

        [Flags]
        public enum Flags : byte
        {
            None = 0,
            FlipY = 1,
            FlatTexture = 2,
            CubemapTexture = 4,
            AtlasTexture = 8
        }
    }
}