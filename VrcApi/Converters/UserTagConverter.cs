using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VRC.API.Parsers
{
	public class ApiUserTagConverter : JsonConverter
	{
		public override bool CanRead => true;

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value == null || !(value is ApiUserTag))
				throw new ArgumentNullException(nameof(value));

			writer.WriteValue(((ApiUserTag) value).ToString());
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
			JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null) throw new ArgumentNullException(nameof(existingValue));

			if (reader.TokenType == JsonToken.String)
				return new ApiUserTag((string) serializer.Deserialize(reader, typeof(string)));

			if (reader.TokenType == JsonToken.StartArray)
			{
				var token = JToken.Load(reader);
				var items = token.ToObject<string[]>();
				var tags = new List<ApiUserTag>();
				for (var i = 0; i < items.Length; i++)
					tags.Add(new ApiUserTag(items[i]));
				return tags.ToArray();
			}
			// @TODO: Implement me
			/*else if (reader.TokenType == JsonToken.EndArray)
			{
				Console.WriteLine("########[END]########");
				return new ApiUserTag[1] { ApiUserTag.Empty };
			}*/

			throw new ArgumentException("Not a parsable ApiUserTag!");
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(VrcId).IsAssignableFrom(objectType);
		}
	}
}