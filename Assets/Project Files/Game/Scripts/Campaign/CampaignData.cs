using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public enum CardRarity
    {
        Common = 0,
        Rare = 1,
        Epic = 2,
    }

    public enum ThreatType
    {
        Armored = 0,
        Swarm = 1,
        Snipers = 2,
        Barricades = 3,
        Attrition = 4,
    }

    public enum BuildingType
    {
        CommandPost = 0,
        Barracks = 1,
        Armory = 2,
        Workshop = 3,
        FieldHospital = 4,
        Saloon = 5,
        Watchtower = 6,
    }

    [System.Serializable]
    public class UnitCardData
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public CardRarity rarity;
        [Tooltip("Character id in the Levels Database")]
        public string characterId;
        public Sprite portrait;
        [Tooltip("Levels completed before the card can drop from chests (first drop is guaranteed)")]
        public int unlockAfterLevel;
        [Tooltip("Ranger: can fill any number of squad slots. Others: one slot each")]
        public bool isFiller;

        [Header("Base multipliers (level 1)")]
        public float healthMult = 1f;
        public float damageMult = 1f;
        public float fireRateMult = 1f;
        public float rangeMult = 1f;
        [Tooltip("Medic: hp restored to every squad member per second (level 1)")]
        public float healPerSecond = 0f;

        public List<ThreatType> counters = new List<ThreatType>();
    }

    [System.Serializable]
    public class WeaponCardData
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public CardRarity rarity;
        [Tooltip("Gun id in the Levels Database")]
        public string gunId;
        public Sprite portrait;
        [Tooltip("Levels completed before the card can drop from chests (first drop is guaranteed)")]
        public int unlockAfterLevel;

        public List<ThreatType> counters = new List<ThreatType>();
    }

    [System.Serializable]
    public class BuildingLevelData
    {
        public int money;
        public int supplies;
        [Tooltip("Meaning depends on the building: slots, percent bonus, revives, income per hour")]
        public float value;
    }

    [System.Serializable]
    public class BuildingData
    {
        public BuildingType type;
        public string displayName;
        [TextArea] public string description;
        [Tooltip("Text with {0} = current value, e.g. \"+{0}% squad health\"")]
        public string valueFormat = "{0}";
        public Sprite icon;
        [Tooltip("Levels completed before the plot can be built")]
        public int unlockAfterLevel;
        [Tooltip("Level 0 = not built. levels[0] = cost/value of level 1")]
        public List<BuildingLevelData> levels = new List<BuildingLevelData>();
    }

    [System.Serializable]
    public class ChapterData
    {
        public string title;
        public string subtitle;
        [Tooltip("1-based, inclusive")]
        public int firstLevel;
        public int lastLevel;
        public string bossName;
        public Sprite bossPortrait;
        [TextArea] public string bossTaunt;
        [TextArea] public List<string> introLines = new List<string>();
        [TextArea] public List<string> outroLines = new List<string>();
    }

    [System.Serializable]
    public class ThreatInfo
    {
        public ThreatType type;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        public Color color = Color.white;
    }
}
