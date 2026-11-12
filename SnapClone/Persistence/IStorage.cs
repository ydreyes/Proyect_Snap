namespace SnapClone.Persistence
{
    /// <summary>
    /// Superficie minima de almacenamiento. El core no conoce rutas ni el
    /// sistema de archivos; el host inyecta una implementacion.
    /// </summary>
    public interface IStorage
    {
        bool Exists(string key);
        string Read(string key);
        void Write(string key, string content);
        void Replace(string key, string content);
        void Delete(string key);
    }
}
