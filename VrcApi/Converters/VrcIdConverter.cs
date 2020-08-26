using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VRC.API.Parsers
{
	public class VrcIdConverter : JsonConverter
	{
		public override bool CanRead => true;

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value == null || !(value is VrcId))
				throw new ArgumentNullException(nameof(value));

			writer.WriteValue(((VrcId) value).ToString());
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
			JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null) throw new ArgumentNullException(nameof(existingValue));

			if (reader.TokenType == JsonToken.String)
				return new VrcId((string) serializer.Deserialize(reader, typeof(string)));

			if (reader.TokenType == JsonToken.StartArray)
			{
				var token = JToken.Load(reader);
				string[] items = token.ToObject<string[]>();
				var guids = new List<VrcId>();
				for (int i = 0; i < items.Length; i++)
					guids.Add(new VrcId(items[i]));
				return guids.ToArray();
			}
			// @TODO: Implement me
			/*else if (reader.TokenType == JsonToken.EndArray)
			{
				Console.WriteLine("########[END]########");
				return new VrcId[1] { VrcId.Empty };
			}*/

			throw new ArgumentException("Not a parsable VrcId!");
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(VrcId).IsAssignableFrom(objectType);
		}
	}
}