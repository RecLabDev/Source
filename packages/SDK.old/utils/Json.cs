using System;

using Newtonsoft.Json;

namespace Theta.SDK.Utils
{
    /// <summary>
    /// JSON serialization and validation utils.
    /// </summary>
    public static class JSON
    {
        /// <summary>
        /// Serialize an object to a JSON-formatted string.
        /// </summary>
        public static string SerializeObject<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            });
        }

        /// <summary>
        /// Deserialize a JSON-formatted string to a particular type.
        /// </summary>
        public static T DeserializeObject<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }

        /// <summary>
        /// Deserialize a JSON-formatted string to a particular type, but different.
        /// </summary>
        public static object DeserializeObject(string value, Type type)
        {
            return JsonConvert.DeserializeObject(value, type);
        }
    }
}
