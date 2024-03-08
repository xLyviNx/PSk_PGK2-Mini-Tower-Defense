using System.Reflection;
using System.Xml.Serialization;
using System.Xml;

public class XmlSerializableFieldWriter
{
	public static void WriteXmlSerializableFields(XmlWriter writer, object obj)
	{
		Type type = obj.GetType();

		foreach (var propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
		{
			object value = propertyInfo.GetValue(obj);
			writer.WriteStartElement(propertyInfo.Name);
			WriteXmlSerializableValue(writer, value);
			writer.WriteEndElement();
		}
	}

	private static void WriteXmlSerializableValue(XmlWriter writer, object value)
	{
		Type type = value.GetType();

		if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type.IsEnum)
		{
			writer.WriteString(value.ToString());
		}
		else if (type == typeof(DateTime))
		{
			writer.WriteString(XmlConvert.ToString((DateTime)value, XmlDateTimeSerializationMode.RoundtripKind));
		}
		else if (type == typeof(Guid))
		{
			writer.WriteString(value.ToString());
		}
		else
		{
			XmlSerializer serializer = new XmlSerializer(type);
			serializer.Serialize(writer, value);
		}
	}
}