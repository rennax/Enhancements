using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Enhancements.Converters
{
    public class Vector3Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Vector3) == objectType;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var vec = (Vector3)value;
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            serializer.Serialize(writer, vec.x);
            writer.WritePropertyName("y");
            serializer.Serialize(writer, vec.y);
            writer.WritePropertyName("z");
            serializer.Serialize(writer, vec.z);
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject j = JObject.Load(reader);

            Vector3 vec = new Vector3()
            {
                x = (float)j["x"],
                y = (float)j["y"],
                z = (float)j["z"],
            };

            serializer.Populate(j.CreateReader(), vec);

            return vec;
        }
    }
}
