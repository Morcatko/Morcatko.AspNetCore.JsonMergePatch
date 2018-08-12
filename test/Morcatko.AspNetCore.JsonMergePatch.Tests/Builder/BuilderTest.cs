using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Builder
{
    public class BuilderTest
    {
        class SimpleClass
        {
            public int Integer { get; set; }
            public string String { get; set; }
        }

        [Fact]
        public void BuildPatch()
        {
            var source = new SimpleClass();
            var patched = new SimpleClass() { Integer = 7 };
            var patch = JsonMergePatchDocument.Build(source, patched);

            var result = new SimpleClass();
            patch.ApplyTo(result);

            Assert.Equal(7, result.Integer);
            Assert.Null(result.String);
        }
    }
}