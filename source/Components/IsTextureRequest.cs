using System;
using Unmanaged;

namespace Textures.Components
{
    public struct IsTextureRequest
    {
        public FixedString address;
        public TimeSpan timeout;
        public TimeSpan duration;
        public Status status;
        public readonly TextureType type;

        public IsTextureRequest(TextureType type, FixedString address, TimeSpan timeout)
        {
            this.address = address;
            this.timeout = timeout;
            duration = TimeSpan.Zero;
            status = Status.Submitted;
            this.type = type;
        }

        private IsTextureRequest(TextureType type, FixedString address, TimeSpan timeout, TimeSpan duration, Status status)
        {
            this.address = address;
            this.timeout = timeout;
            this.duration = duration;
            this.status = status;
            this.type = type;
        }

        public readonly IsTextureRequest BecomeLoaded()
        {
            return new(type, address, timeout, duration, Status.Loaded);
        }

        public enum Status : byte
        {
            Submitted,
            Loading,
            Loaded,
            NotFound
        }
    }
}