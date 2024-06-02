using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenTK.Mathematics;
using PGK2.Engine.Components.Base;
using PGK2.Engine.Core;
using PGK2.Engine.SceneSystem;

namespace PGK2.Engine.Serialization.Converters
{
	internal class DeserializeContext
	{
		public static DeserializeContext CurrentContext;
		public Scene? Scene;
		public GameObject? GameObject;
	}

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
			Console.WriteLine("READING COMPONENTS LIST");
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
					string type = null;
					Dictionary<string, JsonElement> componentData = new Dictionary<string, JsonElement>();

					while (reader.Read())
					{
						if (reader.TokenType == JsonTokenType.EndObject)
						{
							break;
						}

						if (reader.TokenType == JsonTokenType.PropertyName)
						{
							string propertyName = reader.GetString();
							reader.Read(); // Move to the property value

							if (propertyName == "Type")
							{
								type = reader.GetString();
							}
							else if (propertyName == "Data")
							{
								componentData[propertyName] = JsonDocument.ParseValue(ref reader).RootElement;
							}
						}
					}

					if (!string.IsNullOrEmpty(type) && componentData.ContainsKey("Data"))
					{
						Type componentType = Type.GetType(type);
						if (componentType != null && typeof(Component).IsAssignableFrom(componentType))
						{
							var componentJson = componentData["Data"].GetRawText();
							var component = JsonSerializer.Deserialize(componentJson, componentType, options);
							components.Add(component as Component);
							(component as Component).gameObject = DeserializeContext.CurrentContext.GameObject;
							(component as Component).OnSceneTransfer?.Invoke(null);
							EngineWindow.StartQueue.Enqueue(component as Component);
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

				writer.WriteString("Type", component.GetType().AssemblyQualifiedName);
				writer.WritePropertyName("Data");
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
			Console.WriteLine("Reading GAMEOBJECT...");
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

			// Create a placeholder GameObject
			var gameObject = new GameObject("LOADING OBJECT", Guid.Empty);
			gameObject.MyScene = DeserializeContext.CurrentContext.Scene;
			DeserializeContext.CurrentContext.GameObject = gameObject;
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
							gameObject.name = reader.GetString();
							break;
						case "Id":
							gameObject.Id = Guid.Parse(reader.GetString());
							break;
						case "Components":
							components = JsonSerializer.Deserialize<GameObjectComponents>(ref reader, options);
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
			if (components != null)
				components.gameObject = gameObject;
			// Update the placeholder GameObject's properties
			gameObject.Components = components ?? new GameObjectComponents(gameObject);
			gameObject.transform = transform;
			gameObject.Tags = tags;
			gameObject.IsActiveSelf = isActiveSelf;
			return gameObject;
		}
		public override void Write(Utf8JsonWriter writer, GameObject value, JsonSerializerOptions options)
		{
			if (value != null)
			{
				writer.WriteStartObject();
				writer.WriteString("name", value.name);
				writer.WriteString("Id", value.Id.ToString());

				writer.WritePropertyName("Components");
				JsonSerializer.Serialize(writer, value.Components, options);

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
			Console.WriteLine("Reading GameObject List...");
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
			writer.WriteStartArray();
			foreach (var gameObject in value)
			{
				JsonSerializer.Serialize(writer, gameObject, options);
			}
			writer.WriteEndArray();
		}
	}
	public class SceneConverter : JsonConverter<Scene>
	{
		public override Scene Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			Console.WriteLine("Reading SCENE OBJECT...");
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				throw new JsonException("Expected StartObject token.");
			}
			var scene = new Scene();
			DeserializeContext.CurrentContext.Scene = scene;
			scene.SceneName = "Unnamed Scene";
			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
				{
					break;
				}

				if (reader.TokenType == JsonTokenType.PropertyName)
				{
					string propertyName = reader.GetString();
					reader.Read();
					switch (propertyName)
					{
						case "SceneName":
							scene.SceneName = reader.GetString();
							break;
						case "GameObjects":
							scene.AwaitingGameObjects = JsonSerializer.Deserialize<List<GameObject>>(ref reader, options);
							break;
					}
				}
			}

			return scene;
		}

		public override void Write(Utf8JsonWriter writer, Scene value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteString("SceneName", value.SceneName);
			writer.WritePropertyName("GameObjects");
			JsonSerializer.Serialize(writer, value.GameObjects, options);
			writer.WriteEndObject();
		}
	}
	public class GameObjectComponentsConverter : JsonConverter<GameObjectComponents>
	{
		public override GameObjectComponents Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			Console.WriteLine("Reading GameObject Components Object...");
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				throw new JsonException("Expected StartObject token.");
			}

			List<Component> components = null;

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
						case "All":
							components = JsonSerializer.Deserialize<List<Component>>(ref reader, options);
							break;
					}
				}
			}

			return new GameObjectComponents { All = components };
		}

		public override void Write(Utf8JsonWriter writer, GameObjectComponents value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("All");
			JsonSerializer.Serialize(writer, value.All, options);
			writer.WriteEndObject();
		}
	}
	public class ChildrenContainerConverter : JsonConverter<ChildrenContainer>
	{
		public override ChildrenContainer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			Console.WriteLine("Reading CHILDREN CONTAINER Object...");
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				throw new JsonException("Expected StartObject token.");
			}

			var childrenContainer = new ChildrenContainer();
			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
				{
					break;
				}

				if (reader.TokenType == JsonTokenType.PropertyName)
				{
					var propertyName = reader.GetString();
					reader.Read();

					switch (propertyName)
					{
						case "All":
							childrenContainer._loaded = JsonSerializer.Deserialize<List<Guid>>(ref reader, options);
							break;
					}
				}
			}
			Console.WriteLine($"LOADED {childrenContainer._loaded.Count} CHILDREN ");
			return childrenContainer;
		}

		public override void Write(Utf8JsonWriter writer, ChildrenContainer value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("All");
			JsonSerializer.Serialize(writer, value.All, options);
			writer.WriteEndObject();
		}
	}
	public class TransformComponentConverter : JsonConverter<TransformComponent>
	{
		public override TransformComponent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			Console.WriteLine("Reading Transform Component Object...");

			if (reader.TokenType != JsonTokenType.StartObject)
			{
				throw new JsonException("Expected StartObject token.");
			}

			var transformComponent = new TransformComponent();

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
				{
					break;
				}

				if (reader.TokenType == JsonTokenType.PropertyName)
				{
					var propertyName = reader.GetString();
					reader.Read();

					switch (propertyName)
					{
						case "LocalPosition":
							transformComponent.LocalPosition = JsonSerializer.Deserialize<Vector3>(ref reader, options);
							break;
						case "LocalScale":
							transformComponent.LocalScale = JsonSerializer.Deserialize<Vector3>(ref reader, options);
							break;
						case "Pitch":
							transformComponent.Pitch = reader.GetSingle();
							break;
						case "Yaw":
							transformComponent.Yaw = reader.GetSingle();
							break;
						case "Roll":
							transformComponent.Roll = reader.GetSingle();
							break;
						case "Children":
							transformComponent.Children = JsonSerializer.Deserialize<ChildrenContainer>(ref reader, options);
							break;
					}
				}
			}
			return transformComponent;
		}

		public override void Write(Utf8JsonWriter writer, TransformComponent value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();

			writer.WritePropertyName("LocalPosition");
			JsonSerializer.Serialize(writer, value.LocalPosition, options);

			writer.WritePropertyName("LocalScale");
			JsonSerializer.Serialize(writer, value.LocalScale, options);

			writer.WritePropertyName("Pitch");
			writer.WriteNumberValue(value.Pitch);

			writer.WritePropertyName("Yaw");
			writer.WriteNumberValue(value.Yaw);

			writer.WritePropertyName("Roll");
			writer.WriteNumberValue(value.Roll);

			writer.WritePropertyName("Children");
			JsonSerializer.Serialize(writer, value.Children, options);

			// Serialize other properties if needed

			writer.WriteEndObject();
		}
	}

}
