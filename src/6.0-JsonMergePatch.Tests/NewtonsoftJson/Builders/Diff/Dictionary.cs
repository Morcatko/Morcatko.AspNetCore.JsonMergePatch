using System.Collections.Generic;
using Morcatko.AspNetCore.JsonMergePatch.NewtonsoftJson.Builders;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.NewtonsoftJson.Builders.Diff
{
    public class Dictionary
    {
        class SimpleModelWithDictionary
        {
            public Dictionary<string, string> Dic { get; set; } = new Dictionary<string, string>();
        }

        [Fact]
        public void NoChange()
        {
            var source = new SimpleModelWithDictionary();
            var patched = new SimpleModelWithDictionary();

            var diff = DiffBuilder.Build(source, patched);

            Assert.Null(diff);
        }

        [Fact]
        public void EmptyToNull()
        {
            var source = new SimpleModelWithDictionary();
            var patched = new SimpleModelWithDictionary() { Dic = null };

            var diff = DiffBuilder.Build(source, patched);

            Assert.True(JObject.DeepEquals(
                             JObject.Parse("{Dic:null}"),
                             diff));
        }

        [Fact]
        public void NullToEmpty()
        {
            var source = new SimpleModelWithDictionary() { Dic = null };
            var patched = new SimpleModelWithDictionary();

            var diff = DiffBuilder.Build(source, patched);

            Assert.True(JObject.DeepEquals(
                             JObject.Parse("{Dic:{}}"),
                             diff));
        }

        [Fact]
        public void KeyAdded()
        {
            var source = new SimpleModelWithDictionary();
            var patched = new SimpleModelWithDictionary()
            {
                Dic = new Dictionary<string, string>()
                {
                    { "key1", "val1" }
                }
            };

            var diff = DiffBuilder.Build(source, patched);

            Assert.True(JObject.DeepEquals(
                             JObject.Parse("{Dic:{\"key1\": \"val1\"}}"),
                             diff));
        }

        [Fact]
        public void KeyMissing()
        {
            var source = new SimpleModelWithDictionary()
            {
                Dic = new Dictionary<string, string>()
                {
                    { "key1", "val1" }
                }
            };
            var patched = new SimpleModelWithDictionary();

            var diff = DiffBuilder.Build(source, patched);

            Assert.True(JObject.DeepEquals(
                             JObject.Parse("{Dic:{\"key1\": null}}"),
                             diff));
        }

        [Fact]
        public void ValueSetToNull()
        {
            var source = new SimpleModelWithDictionary()
            {
                Dic = new Dictionary<string, string>()
                {
                    { "key1", "val1" }
                }
            };
            var patched = new SimpleModelWithDictionary()
            {
                Dic = new Dictionary<string, string>()
                {
                    { "key1", null }
                }
            };

            var diff = DiffBuilder.Build(source, patched);

            Assert.True(JObject.DeepEquals(
                             JObject.Parse("{Dic:{\"key1\": null}}"),
                             diff));
        }

        [Fact]
        public void ValueChanged()
        {
            var source = new SimpleModelWithDictionary()
            {
                Dic = new Dictionary<string, string>()
                {
                    { "key1", "val1" }
                }
            };
            var patched = new SimpleModelWithDictionary()
            {
                Dic = new Dictionary<string, string>()
                {
                    { "key1", "val_changed" }
                }
            };

            var diff = DiffBuilder.Build(source, patched);

            Assert.True(JObject.DeepEquals(
                             JObject.Parse("{Dic:{\"key1\": \"val_changed\"}}"),
                             diff));
        }
    }
}
