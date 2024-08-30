using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hasmer {
    /// <summary>
    /// Utility for working with embedded resources.
    /// </summary>
    public class ResourceManager {
        /// <summary>
        /// Loads an embedded resource and returns its contents.
        /// </summary>
        /// <param name="name">The name of the resource, without the ".json" extension.</param>
        public static string ReadEmbeddedResource(string name) {
            Assembly assembly = Assembly.GetExecutingAssembly();

            using Stream stream = assembly.GetManifestResourceStream("Hasmer.Resources." + name + ".json");
            if (stream == null) {
                throw new Exception("embedded resource does not exist: " + name);
            }
            using StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Loads an embedded resource as a JSON object.
        /// </summary>
        /// <param name="name">The name of the resource, without the ".json" extension.</param>
        public static T ReadEmbeddedResource<T>(string name) {
            string str = ReadEmbeddedResource(name);
            return JsonConvert.DeserializeObject<T>(str);
        }

        /// <summary>
        /// Loads a JSON embedded resource.
        /// </summary>
        /// <param name="name">The name of the resource, without the ".json" extension.</param>
        public static JObject LoadJsonObject(string name) {
            return JObject.Parse(ReadEmbeddedResource(name));
        }
    }
}
