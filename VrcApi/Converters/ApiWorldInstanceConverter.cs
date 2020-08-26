using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VRC.API.Parsers
{
	public class ApiWorldInstanceConverter : JsonConverter
	{
		public override bool CanRead => true;

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value == null || !(value is ApiWorldInstance))
				throw new ArgumentNullException(nameof(value));

			var instance = (ApiWorldInstance)value;

			// Retarded
			writer.WriteValue($"[\"{instance.id}\", {instance.occupants}]");
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
			JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
				throw new ArgumentNullException(nameof(existingValue));

			// Ok so dont judge me faggot, i havent done C# in ages

			if (reader.TokenType != JsonToken.StartArray)
				throw new ArgumentException("Not a parsable ApiWorldInstance!");

			if (!reader.Read() || reader.TokenType != JsonToken.String)
				throw new ArgumentException("Not a parsable ApiWorldInstance!");

			ApiWorldInstance instance = new ApiWorldInstance();

			instance.id = (string)serializer.Deserialize(reader, typeof(string));

			if (!reader.Read() || reader.TokenType != JsonToken.Integer)
				throw new ArgumentException("Not a parsable ApiWorldInstance!");

			instance.occupants = (int)serializer.Deserialize(reader, typeof(int));

			if (!reader.Read() || reader.TokenType != JsonToken.EndArray)
				throw new ArgumentException("Not a parsable ApiWorldInstance!");

			return instance;
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(VrcId).IsAssignableFrom(objectType);
		}
	}
}