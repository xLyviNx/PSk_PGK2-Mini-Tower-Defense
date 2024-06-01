using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenTK.Mathematics;
using PGK2.Engine.Components.Base;
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
			var components = new List<Component>();

			if (reader.TokenType != JsonTokenType.StartArray)
			{
				throw new JsonException("Expected StartArray token.");
			}

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndArray)
				{
					break;
				}

				if (reader.TokenType == JsonTokenType.StartObject)
				{
					// Read the type information first
					string type = null;
					while (reader.Read())
					{
						if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == "Type")
						{
							reader.Read();
							type = reader.GetString();
							break;
						}
					}

					// Deserialize the component based on the type information
					if (!string.IsNullOrEmpty(type))
					{
						Type componentType = Type.GetType(type);
						if (componentType != null && typeof(Component).IsAssignableFrom(componentType))
						{
							var component = (Component)JsonSerializer.Deserialize(ref reader, componentType, options);
							components.Add(component);
						}
					}
				}
			}

			return components;
		}

		public override void Write(Utf8JsonWriter writer, List<Component> value, JsonSerializerOptions options)
		{
			writer.WriteStartArray();
			foreach (var component in value)
			{
				writer.WriteStartObject();

				// Write type information
				writer.WriteString("Type", component.GetType().AssemblyQualifiedName);

				// Serialize the component
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
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				throw new JsonException("Expected StartObject token.");
			}

			string name = null;
			Guid id = Guid.Empty;
			GameObjectComponents components = null;
			TransformComponent transform = null;
			TagsContainer tags = null;
			bool isActiveSelf = true;

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
						case "name":
							name = reader.GetString();
							break;
						case "Id":
							id = Guid.Parse(reader.GetString());
							break;
						case "Components":
							components = JsonSerializer.Deserialize<GameObjectComponents>(ref reader, options);
							break;
						case "transform":
							transform = JsonSerializer.Deserialize<TransformComponent>(ref reader, options);
							break;
						case "Tags":
							tags = JsonSerializer.Deserialize<TagsContainer>(ref reader, options);
							break;
						case "IsActiveSelf":
							isActiveSelf = reader.GetBoolean();
							break;
					}
				}
			}

			var gameObject = new GameObject(name, id)
			{
				Components = components ?? new GameObjectComponents(null),
				transform = transform,
				Tags = tags,
				IsActiveSelf = isActiveSelf
			};

			return gameObject;
		}

		public override void Write(Utf8JsonWriter writer, GameObject value, JsonSerializerOptions options)
		{
			Console.WriteLine("GameObjectConverter.Write called");
			if (value != null)
			{
				writer.WriteStartObject();
				writer.WriteString("name", value.name);
				writer.WriteString("Id", value.Id.ToString());

				writer.WritePropertyName("Components");
				JsonSerializer.Serialize(writer, value.Components.All, options);

				writer.WritePropertyName("transform");
				JsonSerializer.Serialize(writer, value.transform, options);

				writer.WritePropertyName("Tags");
				JsonSerializer.Serialize(writer, value.Tags, options);

				writer.WriteBoolean("IsActiveSelf", value.IsActiveSelf);

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
			Console.WriteLine("GameObjectListConverter.Read called");
			if (reader.TokenType != JsonTokenType.StartArray)
			{
				throw new JsonException("Expected StartArray token.");
			}

			var gameObjects = new List<GameObject>();

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndArray)
				{
					break;
				}

				// Deserialize each GameObject within the array
				var gameObject = JsonSerializer.Deserialize<GameObject>(ref reader, options);
				gameObjects.Add(gameObject);
			}

			return gameObjects;
		}

		public override void Write(Utf8JsonWriter writer, List<GameObject> value, JsonSerializerOptions options)
		{
			Console.WriteLine("GameObjectListConverter.Write called");
			writer.WriteStartArray();
			foreach (var gameObject in value)
			{
				JsonSerializer.Serialize(writer, gameObject, options);
			}
			writer.WriteEndArray();
		}
	}
}
