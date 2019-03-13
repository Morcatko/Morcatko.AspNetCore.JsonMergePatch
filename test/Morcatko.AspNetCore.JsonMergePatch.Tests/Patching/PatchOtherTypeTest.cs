using Morcatko.AspNetCore.JsonMergePatch.Builder;
using System;
using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Patching
{
    public class PatchOtherTypeTest
    {
        class Model1
        {
            public int Integer { get; set; }
        }

        class Model2
        {
            public int Integer { get; set; }
        }

        [Fact]
        public void PatchesDifferentType()
        {
            var model1 = new Model1 { Integer = 3 };
            var model2 = new Model2 { Integer = 3 };
            var patch1 = PatchBuilder.Build<Model1>(new { Integer = 5 });

            patch1.ApplyTo(model2);

            Assert.Equal(5, model2.Integer);

            Assert.Throws<NotSupportedException>(() => patch1.ApplyTo(model1));
        }

        [Fact]
        public void SecondApplyFails()
        {
            var model = new Model1 { Integer = 3 };
            var patch = PatchBuilder.Build<Model1>(new { Integer = 5 });

            patch.ApplyTo(model);
            Assert.Throws<NotSupportedException>(() => patch.ApplyTo(model));
        }
    }
}
