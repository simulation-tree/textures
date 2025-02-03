using Types;
using Worlds;
using Worlds.Tests;

namespace Textures.Tests
{
    public abstract class TextureTests : WorldTests
    {
        static TextureTests()
        {
            TypeRegistry.Load<TexturesTypeBank>();
        }

        protected override Schema CreateSchema()
        {
            Schema schema = base.CreateSchema();
            schema.Load<TexturesSchemaBank>();
            return schema;
        }
    }
}