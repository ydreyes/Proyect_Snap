namespace SnapClone.Persistence
{
    /// <summary>Documento run.json de la run activa o ultima terminada.</summary>
    public class JsonRunStore
    {
        public const string Key = "run.json";

        private readonly JsonDocumentStore _documents;

        public JsonRunStore(IStorage storage)
        {
            _documents = new JsonDocumentStore(storage);
        }

        public bool Exists() => _documents.Exists(Key);

        public RunDto Load()
        {
            var run = _documents.Load<RunDto>(Key,
                dto => dto.SchemaVersion == RunDto.CurrentSchemaVersion);

            run?.Normalize();
            return run;
        }

        public void Save(RunDto run)
        {
            run.SchemaVersion = RunDto.CurrentSchemaVersion;
            run.Normalize();
            _documents.Save(Key, run);
        }

        public void Delete() => _documents.Delete(Key);
    }
}
