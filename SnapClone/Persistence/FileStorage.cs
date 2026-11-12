using System;
using System.IO;

namespace SnapClone.Persistence
{
    /// <summary>
    /// Implementacion de escritorio/Android sobre archivos. La ruta raiz la
    /// decide el host: en Android se pasa FilesDir; en escritorio se usa la
    /// carpeta de datos locales del usuario.
    /// </summary>
    public class FileStorage : IStorage
    {
        private readonly string _root;

        public FileStorage(string rootDirectory = null)
        {
            _root = string.IsNullOrWhiteSpace(rootDirectory)
                ? Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SnapClone")
                : rootDirectory;
        }

        public string RootDirectory => _root;

        private string PathFor(string key) => Path.Combine(_root, key);

        public bool Exists(string key) => File.Exists(PathFor(key));

        public string Read(string key)
        {
            string path = PathFor(key);
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }

        public void Write(string key, string content)
        {
            string path = PathFor(key);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, content);
        }

        /// <summary>
        /// Escribe el contenido en un temporal, conserva el documento anterior
        /// como .bak y mueve el temporal sobre el destino. Si algo falla, el
        /// destino conserva el documento anterior.
        /// </summary>
        public void Replace(string key, string content)
        {
            string path = PathFor(key);
            string temp = path + ".tmp";
            string backup = path + ".bak";

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(temp, content);

            if (File.Exists(path))
                File.Copy(path, backup, true);

            File.Move(temp, path, true);
        }

        public void Delete(string key)
        {
            string path = PathFor(key);
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
