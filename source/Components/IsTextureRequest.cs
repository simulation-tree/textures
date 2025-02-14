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

        public IsTextureRequest(FixedString address, TimeSpan timeout)
        {
            this.address = address;
            this.timeout = timeout;
            duration = TimeSpan.Zero;
            status = Status.Submitted;
        }

        private IsTextureRequest(FixedString address, TimeSpan timeout, TimeSpan duration, Status status)
        {
            this.address = address;
            this.timeout = timeout;
            this.duration = duration;
            this.status = status;
        }

        public readonly IsTextureRequest BecomeLoaded()
        {
            return new(address, timeout, duration, Status.Loaded);
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