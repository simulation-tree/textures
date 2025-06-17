using System;
using Unmanaged;

namespace Textures.Components
{
    public struct IsTextureRequest
    {
        public ASCIIText256 address;
        public double timeout;
        public double duration;
        public Status status;
        public readonly Flags flags;
        public readonly TextureType type;

        public IsTextureRequest(TextureType type, ASCIIText256 address, double timeout, Flags flags)
        {
            this.address = address;
            this.timeout = timeout;
            duration = 0;
            status = Status.Submitted;
            this.type = type;
            this.flags = flags;
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
            FlipY = 1
        }
    }
}