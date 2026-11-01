using System.Collections.Generic;

namespace SnapClone.Data.Definitions
{
    public class CardDef
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }
        public int BasePower { get; set; }
        public string Description { get; set; }
        public List<string> EffectIds { get; set; } = new List<string>();
        public List<string> Tags { get; set; } = new List<string>();
    }
    
    public class ZoneDef
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> EffectIds { get; set; } = new List<string>();
    }
    
    public class EffectDef
    {
        public string Id { get; set; }
        public string EffectType { get; set; }
        public EffectTrigger Trigger { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }
    
    public class PerkDef
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> EffectIds { get; set; } = new List<string>();
        public Dictionary<string, int> Modifiers { get; set; } = new Dictionary<string, int>();
    }
}
