using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using OpenTK.Mathematics;

namespace PGK2.Engine.SceneSystem
{
	public class SceneContractResolver : DefaultContractResolver
	{
		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			return type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
					   .Where(m => m.GetCustomAttribute<SerializeFieldAttribute>() != null)
					   .Select(m => CreateProperty(m, memberSerialization))
					   .ToList();
		}

		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			JsonProperty property = base.CreateProperty(member, memberSerialization);

			if (property.PropertyType == typeof(Vector3))
			{
				property.Converter = new Vector3Converter();
			}
			else if (property.PropertyType == typeof(Quaternion))
			{
				property.Converter = new QuaternionConverter();
			}
			else if (property.PropertyType == typeof(Vector4))
			{
				property.Converter = new Vector4Converter();
			}
			else if (property.PropertyType == typeof(Matrix4))
			{
				property.Converter = new Matrix4Converter();
			}

			return property;
		}

		protected override List<MemberInfo> GetSerializableMembers(Type objectType)
		{
			var members = base.GetSerializableMembers(objectType);
			return members.Where(m => m.GetCustomAttribute<SerializeFieldAttribute>() != null).ToList();
		}
	}
}
