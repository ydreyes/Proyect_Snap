using System.Collections.Generic;
using SnapClone.Data.Definitions;

namespace SnapClone.Data.Registries
{
    public class CardRegistry
    {
        private Dictionary<string, CardDef> _cards = new Dictionary<string, CardDef>();
        
        public void Register(CardDef card)
        {
            _cards[card.Id] = card;
        }
        
        public CardDef Get(string id)
        {
            return _cards.TryGetValue(id, out CardDef card) ? card : null;
        }
        
        public List<CardDef> GetAll()
        {
            return new List<CardDef>(_cards.Values);
        }
    }
    
    public class ZoneRegistry
    {
        private Dictionary<string, ZoneDef> _zones = new Dictionary<string, ZoneDef>();
        
        public void Register(ZoneDef zone)
        {
            _zones[zone.Id] = zone;
        }
        
        public ZoneDef Get(string id)
        {
            return _zones.TryGetValue(id, out ZoneDef zone) ? zone : null;
        }
        
        public List<ZoneDef> GetAll()
        {
            return new List<ZoneDef>(_zones.Values);
        }
    }
    
    public class EffectRegistry
    {
        private Dictionary<string, EffectDef> _effects = new Dictionary<string, EffectDef>();
        
        public void Register(EffectDef effect)
        {
            _effects[effect.Id] = effect;
        }
        
        public EffectDef Get(string id)
        {
            return _effects.TryGetValue(id, out EffectDef effect) ? effect : null;
        }
        
        public List<EffectDef> GetAll()
        {
            return new List<EffectDef>(_effects.Values);
        }
    }
    
    public class PerkRegistry
    {
        private Dictionary<string, PerkDef> _perks = new Dictionary<string, PerkDef>();
        
        public void Register(PerkDef perk)
        {
            _perks[perk.Id] = perk;
        }
        
        public PerkDef Get(string id)
        {
            return _perks.TryGetValue(id, out PerkDef perk) ? perk : null;
        }
        
        public List<PerkDef> GetAll()
        {
            return new List<PerkDef>(_perks.Values);
        }
    }
}
