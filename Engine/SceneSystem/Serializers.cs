using Newtonsoft.Json;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGK2.Engine.SceneSystem
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class SerializeFieldAttribute : Attribute
	{
	}
	public class Matrix4Converter : JsonConverter<Matrix4>
	{
		public override Matrix4 ReadJson(JsonReader reader, Type objectType, Matrix4 existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			// Implement the deserialization logic for Matrix4
			throw new NotImplementedException();
		}

		public override void WriteJson(JsonWriter writer, Matrix4 value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					writer.WritePropertyName($"M{i}{j}");
					writer.WriteValue(value[i, j]);
				}
			}
			writer.WriteEndObject();
		}
	}

	public class Vector4Converter : JsonConverter<Vector4>
	{
		public override Vector4 ReadJson(JsonReader reader, Type objectType, Vector4 existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			// Implement the deserialization logic for Vector4
			throw new NotImplementedException();
		}

		public override void WriteJson(JsonWriter writer, Vector4 value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("X");
			writer.WriteValue(value.X);
			writer.WritePropertyName("Y");
			writer.WriteValue(value.Y);
			writer.WritePropertyName("Z");
			writer.WriteValue(value.Z);
			writer.WritePropertyName("W");
			writer.WriteValue(value.W);
			writer.WriteEndObject();
		}
	}

	public class Vector3Converter : JsonConverter<Vector3>
	{
		public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			// Implement the deserialization logic for Vector3
			throw new NotImplementedException();
		}

		public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("X");
			writer.WriteValue(value.X);
			writer.WritePropertyName("Y");
			writer.WriteValue(value.Y);
			writer.WritePropertyName("Z");
			writer.WriteValue(value.Z);
			writer.WriteEndObject();
		}
	}

	public class QuaternionConverter : JsonConverter<Quaternion>
	{
		public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			// Implement the deserialization logic for Quaternion
			throw new NotImplementedException();
		}

		public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("X");
			writer.WriteValue(value.X);
			writer.WritePropertyName("Y");
			writer.WriteValue(value.Y);
			writer.WritePropertyName("Z");
			writer.WriteValue(value.Z);
			writer.WritePropertyName("W");
			writer.WriteValue(value.W);
			writer.WriteEndObject();
		}
	}
}
