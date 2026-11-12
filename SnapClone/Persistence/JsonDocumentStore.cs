using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SnapClone.Persistence
{
    /// <summary>
    /// Serializa un documento JSON con escritura atomica y lectura con
    /// fallback al .bak. La validacion de schema la decide el store.
    /// </summary>
    public class JsonDocumentStore
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly IStorage _storage;

        public JsonDocumentStore(IStorage storage)
        {
            _storage = storage;
        }

        public bool Exists(string key) => _storage.Exists(key);

        public T Load<T>(string key, Func<T, bool> isValid) where T : class
        {
            T document = TryRead(key, isValid);
            if (document != null) return document;

            return TryRead(key + ".bak", isValid);
        }

        public void Save<T>(string key, T value)
        {
            string json = JsonSerializer.Serialize(value, Options);
            _storage.Replace(key, json);
        }

        public void Delete(string key)
        {
            _storage.Delete(key);
            _storage.Delete(key + ".bak");
            _storage.Delete(key + ".tmp");
        }

        private T TryRead<T>(string key, Func<T, bool> isValid) where T : class
        {
            if (!_storage.Exists(key)) return null;

            try
            {
                string json = _storage.Read(key);
                if (string.IsNullOrWhiteSpace(json)) return null;

                T document = JsonSerializer.Deserialize<T>(json, Options);
                return document != null && (isValid == null || isValid(document))
                    ? document
                    : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
