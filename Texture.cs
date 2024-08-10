using Data.Components;
using Data.Events;
using Simulation;
using System;
using Textures.Components;
using Textures.Events;
using Unmanaged;
using Unmanaged.Collections;

namespace Textures
{
    public readonly struct Texture : ITexture, IDisposable
    {
        private readonly Entity entity;

        World IEntity.World => entity.world;
        eint IEntity.Value => entity.value;

        public Texture(World world, eint existingEntity)
        {
            entity = new(world, existingEntity);
        }

        /// <summary>
        /// Creates a new empty texture with a set size.
        /// </summary>
        public Texture(World world, uint width, uint height, Pixel defaultPixel = default)
        {
            entity = new(world);
            entity.AddComponent(new IsTexture());
            entity.AddComponent(new TextureSize(width, height));

            uint pixelCount = width * height;
            UnmanagedList<Pixel> list = entity.CreateList<Entity, Pixel>(pixelCount);
            list.AddRepeat(defaultPixel, pixelCount);
        }

        public Texture(World world, uint width, uint height, ReadOnlySpan<Pixel> pixels)
        {
            entity = new(world);
            entity.AddComponent(new IsTexture());
            entity.AddComponent(new TextureSize(width, height));

            UnmanagedList<Pixel> list = entity.CreateList<Entity, Pixel>((uint)pixels.Length);
            list.AddDefault((uint)pixels.Length);
            pixels.CopyTo(list.AsSpan());
        }

        /// <summary>
        /// Creates a texture that loads from the given address.
        /// </summary>
        public Texture(World world, ReadOnlySpan<char> address)
        {
            entity = new(world);
            entity.AddComponent(new IsDataRequest(address));
            entity.AddComponent(new TextureSize(0, 0));
            entity.AddComponent(new IsTexture());
            entity.CreateList<Entity, Pixel>();

            world.Submit(new DataUpdate());
            world.Submit(new TextureUpdate());
            world.Poll();
        }

        public readonly void Dispose()
        {
            entity.Dispose();
        }

        public readonly override string ToString()
        {
            return entity.ToString();
        }

        public static Query GetQuery(World world)
        {
            return new(world, RuntimeType.Get<IsTexture>());
        }
    }
}
