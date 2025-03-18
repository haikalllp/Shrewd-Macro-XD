using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace NotesAndTasks.Models
{
    /// <summary>
    /// Custom JSON converter for InputBinding objects to ensure proper serialization
    /// </summary>
    public class InputBindingConverter : JsonConverter<InputBinding>
    {
        /// <summary>
        /// Reads and converts the JSON to an InputBinding
        /// </summary>
        public override InputBinding Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected start of object");
            }

            var result = new InputBinding();
            
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("Expected property name");
                }

                string propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "Key":
                        if (reader.TokenType == JsonTokenType.String)
                        {
                            string keyName = reader.GetString();
                            if (Enum.TryParse<Keys>(keyName, out Keys key))
                            {
                                result.Key = key;
                            }
                        }
                        else if (reader.TokenType == JsonTokenType.Number)
                        {
                            int keyValue = reader.GetInt32();
                            result.Key = (Keys)keyValue;
                        }
                        break;

                    case "Type":
                        if (reader.TokenType == JsonTokenType.String)
                        {
                            string typeName = reader.GetString();
                            if (Enum.TryParse<InputType>(typeName, out InputType type))
                            {
                                result.Type = type;
                            }
                        }
                        else if (reader.TokenType == JsonTokenType.Number)
                        {
                            int typeValue = reader.GetInt32();
                            result.Type = (InputType)typeValue;
                        }
                        break;

                    case "DisplayName":
                        if (reader.TokenType == JsonTokenType.String)
                        {
                            result.DisplayName = reader.GetString();
                        }
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Writes the InputBinding as JSON with enum values as strings
        /// </summary>
        public override void Write(Utf8JsonWriter writer, InputBinding value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            
            writer.WritePropertyName("Key");
            writer.WriteStringValue(value.Key.ToString());
            
            writer.WritePropertyName("Type");
            writer.WriteStringValue(value.Type.ToString());
            
            writer.WritePropertyName("DisplayName");
            writer.WriteStringValue(value.DisplayName ?? value.Key.ToString());
            
            writer.WriteEndObject();
        }
    }
} 