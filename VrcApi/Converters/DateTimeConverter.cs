using System;
using Newtonsoft.Json;

namespace VRC.API.Parsers
{
	public class VrcDateTimeConverter : JsonConverter
	{
		public override bool CanRead => true;

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(((DateTime?) value)?.ToString() ?? "none");
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
			JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}

			if (reader.TokenType == JsonToken.String)
			{
				if (DateTime.TryParse((string) serializer.Deserialize(reader, typeof(string)), out var outVal))
					return outVal;
			}
			else if (reader.TokenType == JsonToken.Date)
			{
				return serializer.Deserialize(reader, typeof(DateTime));
			}

			return null;
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(DateTime?);
		}
	}
}