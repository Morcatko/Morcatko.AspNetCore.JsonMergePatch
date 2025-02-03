using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests
{
	public class TestModelBase : IEquatable<TestModelBase>, IEquatable<NewtonsoftTestModel>, IEquatable<SystemTextTestModel>
	{
		public int Id { get; set; }
		public int Integer { get; set; }
		[Required]
		public string String { get; set; }
		public virtual float Float { get; set; }

		public bool Boolean { get; set; }

		public virtual string Renamed { get; set; }
		public SubModel SubModel { get; set; }

		public SimpleEnum SimpleEnum { get; set; }

		public virtual ValueEnum ValueEnum { get; set; }

		public DateTimeOffset? Date { get; set; }

		public decimal? NullableDecimal { get; set; }

		public float[] ArrayOfFloats { get; set; } = new float[0];

		public Dictionary<string, SubModel> SubModels { get; set; } = new Dictionary<string, SubModel>();

		public bool Equals(TestModelBase other)
		{
			//We are not comparing Id
			return this.Integer == other.Integer
				&& this.String == other.String
				&& this.Float == other.Float
				&& this.Boolean == other.Boolean
				&& this.Renamed == other.Renamed
				&& this.SimpleEnum == other.SimpleEnum
				&& this.ValueEnum == other.ValueEnum
				&& this.NullableDecimal == other.NullableDecimal
				&& this.Date == other.Date
				&& this.Date.GetValueOrDefault().Offset == other.Date.GetValueOrDefault().Offset
				&& Enumerable.SequenceEqual(this.ArrayOfFloats, other.ArrayOfFloats)
				&& Enumerable.SequenceEqual(this.SubModels?.Keys, other.SubModels?.Keys)
				&& Enumerable.SequenceEqual(this.SubModels?.Values, other.SubModels?.Values)
				&& ((this.SubModel == other.SubModel)
					|| this.SubModel.Equals(other.SubModel));
		}

		public bool Equals(NewtonsoftTestModel other) => Equals(other as TestModelBase);
		public bool Equals(SystemTextTestModel other) => Equals(other as TestModelBase);
	}

	public class NewtonsoftTestModel : TestModelBase
	{
		[JsonProperty("NewName")]
		public override string Renamed { get; set; }

		[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
		public override ValueEnum ValueEnum { get; set; }
	}

	public class SystemTextTestModel : TestModelBase
	{
		//[JsonPropertyName("NewName")]
		public override string Renamed { get; set; }
	}

	public class SubModel : IEquatable<SubModel>
	{
		public string Value1 { get; set; }
		public string Value2 { get; set; }
		public int[] Numbers { get; set; }

		public SubSubModel SubSubModel { get; set; }

		public bool Equals(SubModel other)
		{
			return this.Value1 == other.Value1
				&& this.Value2 == other.Value2
				&& this.SubSubModel == other.SubSubModel
				&& ((this.Numbers == other.Numbers)
					|| Enumerable.SequenceEqual(this.Numbers, other.Numbers));
		}
	}

	public class SubSubModel : IEquatable<SubSubModel>
	{
		public string Value1 { get; set; }

		public override bool Equals(object obj)
		{
			var model = obj as SubSubModel;
			return model != null &&
				   Value1 == model.Value1;
		}

		public override int GetHashCode()
		{
			return -1092109975 + EqualityComparer<string>.Default.GetHashCode(Value1);
		}

		public static bool operator ==(SubSubModel model1, SubSubModel model2)
		{
			return EqualityComparer<SubSubModel>.Default.Equals(model1, model2);
		}

		public static bool operator !=(SubSubModel model1, SubSubModel model2)
		{
			return !(model1 == model2);
		}

		public bool Equals(SubSubModel other)
		{
			return this.Value1 == other.Value1;
		}


	}

	public enum SimpleEnum
	{
		zero = 0,
		one = 1,
		two = 2
	}

	public enum ValueEnum
	{
		[EnumMember(Value = "Meter")]
		m,
		[EnumMember(Value = "Feet")]
		ft,
		[EnumMember(Value = "Inch")]
		i
	}
}
