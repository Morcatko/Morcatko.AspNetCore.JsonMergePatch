using System.Reflection;
using Morcatko.AspNetCore.JsonMergePatch.NewtonsoftJson.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.NewtonsoftJson.Patching {
    public class MyClass 
    {
        [JsonProperty("attributes", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public virtual MyAttributes Attributes { get; set; }
    }

    public class MyAttributes 
    {
        [JsonProperty("name", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public virtual string Name { get; set; }
    }

    // see https://github.com/RicoSuter/NSwag/issues/1991#issuecomment-518600843
    class DisallowNullContractResolver : DefaultContractResolver
    {
        protected override Newtonsoft.Json.Serialization.JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var jsonProp = base.CreateProperty(member, memberSerialization);
            if (jsonProp.Required == Required.DisallowNull)
                jsonProp.Required = Required.AllowNull;
            return jsonProp;
        }
    }    

    public class PropertyRemovalTest {
        [Fact]
        public void PropertyRemovalWorks() {
            var myObj = new MyClass() {
                Attributes = new MyAttributes() {
                    Name = "Test"
                }
            };

            var jsonPatch = @"{
                ""attributes"": null
            }";

            var jsonMergePatch = PatchBuilder.Build<MyClass>(
                jsonPatch,
                new JsonSerializerSettings() { ContractResolver = new DisallowNullContractResolver() },
                new JsonMergePatchOptions() { EnableDelete = true });
            jsonMergePatch.ApplyTo(myObj);

            Assert.Null(myObj.Attributes);            
        }
    }
}
