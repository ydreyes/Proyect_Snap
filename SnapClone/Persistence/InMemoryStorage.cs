using System;
using System.Collections.Generic;

namespace SnapClone.Persistence
{
    /// <summary>
    /// Almacenamiento en memoria para pruebas y arranques sin disco. Tambien
    /// permite inyectar fallos de escritura de forma determinista.
    /// </summary>
    public class InMemoryStorage : IStorage
    {
        private readonly Dictionary<string, string> _files =
            new Dictionary<string, string>(StringComparer.Ordinal);

        public bool FailWrites { get; set; }

        public IReadOnlyDictionary<string, string> Files => _files;

        public bool Exists(string key) => _files.ContainsKey(key);

        public string Read(string key) =>
            _files.TryGetValue(key, out string value) ? value : null;

        public void Write(string key, string content)
        {
            if (FailWrites)
                throw new InvalidOperationException("Fallo de escritura simulado.");

            _files[key] = content;
        }

        public void Replace(string key, string content)
        {
            if (FailWrites)
                throw new InvalidOperationException("Fallo de escritura simulado.");

            if (_files.TryGetValue(key, out string previous))
                _files[key + ".bak"] = previous;

            _files[key] = content;
        }

        public void Delete(string key) => _files.Remove(key);
    }
}
