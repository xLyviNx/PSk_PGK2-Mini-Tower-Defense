using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenTK.Mathematics;
using PGK2.Engine.Core;

namespace PGK2.Engine.Serialization.Converters
{
	public class Vector3Converter : JsonConverter<Vector3>
	{
		public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				throw new JsonException("Expected StartObject token.");
			}

			float x = 0, y = 0, z = 0;

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
				{
					break;
				}

				if (reader.TokenType == JsonTokenType.PropertyName)
				{
					var propertyName = reader.GetString();
					reader.Read(); // Move to the property value

					switch (propertyName)
					{
						case "X":
							x = reader.GetSingle();
							break;
						case "Y":
							y = reader.GetSingle();
							break;
						case "Z":
							z = reader.GetSingle();
							break;
							// Add more cases for other properties if needed
					}
				}
			}

			return new Vector3(x, y, z);
		}

		public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteNumber("X", value.X);
			writer.WriteNumber("Y", value.Y);
			writer.WriteNumber("Z", value.Z);
			writer.WriteEndObject();
		}
	}
	public class QuaternionConverter : JsonConverter<Quaternion>
	{
		public override Quaternion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				throw new JsonException("Expected StartObject token.");
			}

			float x = 0, y = 0, z = 0, w = 1;

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
				{
					break;
				}

				if (reader.TokenType == JsonTokenType.PropertyName)
				{
					var propertyName = reader.GetString();
					reader.Read(); // Move to the property value

					switch (propertyName)
					{
						case "X":
							x = reader.GetSingle();
							break;
						case "Y":
							y = reader.GetSingle();
							break;
						case "Z":
							z = reader.GetSingle();
							break;
						case "W":
							w = reader.GetSingle();
							break;
							// Add more cases for other properties if needed
					}
				}
			}

			return new Quaternion(x, y, z, w);
		}

		public override void Write(Utf8JsonWriter writer, Quaternion value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteNumber("X", value.X);
			writer.WriteNumber("Y", value.Y);
			writer.WriteNumber("Z", value.Z);
			writer.WriteNumber("W", value.W);
			writer.WriteEndObject();
		}
	}
	public class ComponentListConverter : JsonConverter<List<Component>>
	{
		public override List<Component> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			throw new NotImplementedException(); // Implement this if needed
		}

		public override void Write(Utf8JsonWriter writer, List<Component> value, JsonSerializerOptions options)
		{
			writer.WriteStartArray();
			foreach (var component in value)
			{
				writer.WriteStartObject();

				// Write component type
				writer.WritePropertyName("Type");
				writer.WriteStringValue(component.GetType().FullName);
				// Write component properties
				JsonSerializer.Serialize(writer, component, component.GetType(), options);
				writer.WriteEndObject();
			}

			writer.WriteEndArray();
		}
	}
	public static class Utf8JsonWriterExtensions
	{
		public static void WriteGuid(this Utf8JsonWriter writer, string propertyName, Guid value)
		{
			writer.WriteStartObject(propertyName);
			writer.WriteString("Id", value.ToString());
			writer.WriteEndObject();
		}
	}
	public class GameObjectConverter : JsonConverter<GameObject>
	{
		public override GameObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			// Implement deserialization logic if needed
			throw new NotImplementedException();
		}

		public override void Write(Utf8JsonWriter writer, GameObject value, JsonSerializerOptions options)
		{
			if (value != null)
			{
				writer.WriteStartObject();

				// Serialize GameObject.Id instead of the entire GameObject
				writer.WriteGuid("Id", value.Id);

				writer.WriteEndObject();
			}
			else
			{
				writer.WriteNullValue();
			}
		}
	}
	public class GameObjectListConverter : JsonConverter<List<GameObject>>
	{
		public override List<GameObject> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			throw new NotImplementedException(); // Implement this if needed
		}

		public override void Write(Utf8JsonWriter writer, List<GameObject> value, JsonSerializerOptions options)
		{
			writer.WriteStartArray();
			foreach (var gameObject in value)
			{
				writer.WriteStartObject();
				writer.WritePropertyName(gameObject.name);
				// Serialize GameObject properties
				JsonSerializer.Serialize(writer, gameObject, options);

				writer.WriteEndObject();
			}

			writer.WriteEndArray();
		}
	}
}
