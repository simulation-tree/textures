# Textures

Definitions for images, and for atlases with their sprites.

### Importing images

```cs
Texture texture = new(world, "*/texture.png");
while (!texture.IsCompliant)
{
    simulator.Broadcast(new DataUpdate()); //to load the bytes
    simulator.Broadcast(new TextureUpdate()); //load import the texture from the bytes
}

Assert.That(texture.Width, Is.EqualTo(1024));
Assert.That(texture.Height, Is.EqualTo(1024));

ReadOnlySpan<Pixel> pixels = texture.Pixels;
Assert.That(pixels.Length, Is.EqualTo(1024 * 1024));
```

### Evaluating pixels

When textures are loaded, their colours can be evaluated at floating point coordinates, or exact coordinates:
```cs
Color color = texture.Evaluate(0.5f, 0.5f);
Pixel exactPixel = texture[512, 512];
```

### Creating atlases from inputs

An `AtlasTexture` can be created from a series of sprites, each with individual pixel data into
the smallest possible texture:
```cs
Span<AtlasTexture.InputSprite> sprites = stackalloc AtlasTexture.InputSprite[1];
AtlasTexture.InputSprite firstSprite = new("firstSprite", 32, 32);
Span<Pixel> firstSpritePixels = firstSprite.Pixels;
firstSpritePixels.Fill(new Pixel(255, 0, 0, 255));
sprites[0] = firstSprite;

AtlasTexture atlas = new(world, sprites);
if (atlas.TryGetSprite("firstSprite", out AtlasSprite sprite))
{
    Vector4 region = sprite.region;
}

Console.WriteLine($"Atlas texture size: {atlas.Width}x{atlas.Height}");
```