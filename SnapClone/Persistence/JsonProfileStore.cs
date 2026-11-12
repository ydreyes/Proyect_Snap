namespace SnapClone.Persistence
{
    /// <summary>Documento profile.json con defaults de primera ejecucion.</summary>
    public class JsonProfileStore
    {
        public const string Key = "profile.json";

        private readonly JsonDocumentStore _documents;

        public JsonProfileStore(IStorage storage)
        {
            _documents = new JsonDocumentStore(storage);
        }

        public bool Exists() => _documents.Exists(Key);

        public ProfileDto Load()
        {
            var profile = _documents.Load<ProfileDto>(Key,
                dto => dto.SchemaVersion == ProfileDto.CurrentSchemaVersion);

            if (profile == null)
                return ProfileDto.CreateDefault(StarterSet.CardIds);

            profile.Normalize();
            return profile;
        }

        public void Save(ProfileDto profile)
        {
            profile.SchemaVersion = ProfileDto.CurrentSchemaVersion;
            profile.Normalize();
            _documents.Save(Key, profile);
        }

        public void Delete() => _documents.Delete(Key);
    }
}
