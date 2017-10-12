using Newtonsoft.Json;
using System;
using System.Linq;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Server.Models
{
    public class TestModel : IEquatable<TestModel>
    {
        public int Integer { get; set; }
        public string String { get; set; }
        public float Float { get; set; }
        public bool Boolean { get; set; }

        [JsonProperty("NewName")]
        public string Renamed { get; set; }
        public SubModel SubModel { get; set; }


        public bool Equals(TestModel other)
        {
            return this.Integer == other.Integer
                && this.String == other.String
                && this.Float == other.Float
                && this.Boolean == other.Boolean
                && this.Renamed == other.Renamed
                && ((this.SubModel == other.SubModel)
                    || this.SubModel.Equals(other.SubModel));
        }
    }

    public class SubModel : IEquatable<SubModel>
    {
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public int[] Numbers { get; set; }

        public bool Equals(SubModel other)
        {
            return this.Value1 == other.Value1
                && this.Value2 == other.Value2
                && ((this.Numbers == other.Numbers)
                    || Enumerable.SequenceEqual(this.Numbers, other.Numbers));
        }
    }
}
