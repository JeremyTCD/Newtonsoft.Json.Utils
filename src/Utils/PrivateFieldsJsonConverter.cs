using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace JeremyTCD.PipelinesCE.PluginAndConfigTools
{
    // TODO move this to utils
    /// <summary>
    /// Serializes private fields in root object. Stores extra or missing fields in json when deserializing. 
    /// Always serializes and deserializes nulls (does not consider a field to be missing if it has null as its value).
    /// 
    /// Inspired by https://stackoverflow.com/questions/30300740/how-to-configure-json-net-deserializer-to-track-missing-properties
    /// </summary>
    public class PrivateFieldsJsonConverter : JsonConverter
    {
        public List<FieldInfo> MissingFields { get; } = new List<FieldInfo>();
        public List<KeyValuePair<string, JToken>> ExtraFields { get; } = new List<KeyValuePair<string, JToken>>();

        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public void Clear()
        {
            MissingFields.Clear();
            ExtraFields.Clear();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            existingValue = existingValue ?? Activator.CreateInstance(objectType, true);

            JObject jObject = JObject.Load(reader);
            FieldInfo[] privateFields = objectType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (FieldInfo field in privateFields)
            {
                JToken jToken = jObject[field.Name];

                // Missing field
                if (jToken == null)
                {
                    MissingFields.Add(field);
                    continue;
                }

                jObject.Remove(field.Name);
                object value = jToken.ToObject(field.FieldType);

                field.SetValue(existingValue, value);
            }

            foreach (KeyValuePair<string, JToken> pair in jObject)
            {
                // Extra field
                ExtraFields.Add(pair);
            }

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Type objectType = value.GetType();
            FieldInfo[] privateFields = objectType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            // Create a serializer that isn't associated with this converter to serialize nested objects
            // https://github.com/JamesNK/Newtonsoft.Json/issues/386
            JsonSerializer secondarySerializer = new JsonSerializer();

            writer.WriteStartObject();

            foreach (FieldInfo field in privateFields)
            {
                writer.WritePropertyName(field.Name);
                object fieldValue = field.GetValue(value);
                secondarySerializer.Serialize(writer, fieldValue);
            }

            writer.WriteEndObject();
        }
    }
}
