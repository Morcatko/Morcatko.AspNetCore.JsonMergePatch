using System;

namespace Morcatko.AspNetCore.JsonMergePatch.TestsMvcCore
{
    public class TestModel : IEquatable<TestModel>
    {
        public int Id { get; set; }
        public int Integer { get; set; }

        public bool Equals(TestModel other) => Integer == other.Integer;
    }
}