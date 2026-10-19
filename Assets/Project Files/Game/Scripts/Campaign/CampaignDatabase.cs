using System.Collections.Generic;
using UnityEngine;

namespace SquadCommand
{
    [CreateAssetMenu(fileName = "Campaign Database", menuName = "Data/Campaign/Campaign Database")]
    public class CampaignDatabase : ScriptableObject
    {
        [SerializeField] List<UnitCardData> units = new List<UnitCardData>();
        public List<UnitCardData> Units => units;

        [SerializeField] List<WeaponCardData> weapons = new List<WeaponCardData>();
        public List<WeaponCardData> Weapons => weapons;

        [SerializeField] List<BuildingData> buildings = new List<BuildingData>();
        public List<BuildingData> Buildings => buildings;

        [SerializeField] List<ChapterData> chapters = new List<ChapterData>();
        public List<ChapterData> Chapters => chapters;

        [SerializeField] List<ThreatInfo> threats = new List<ThreatInfo>();
        public List<ThreatInfo> Threats => threats;

        [Header("Cards")]
        [Tooltip("Copies needed to go from level i+1 to i+2")]
        [SerializeField] int[] copiesPerLevel = new int[] { 2, 3, 4, 6, 8, 10, 14, 18, 24 };
        public int[] CopiesPerLevel => copiesPerLevel;
        [SerializeField] int[] moneyPerLevel = new int[] { 30, 60, 100, 150, 220, 300, 400, 520, 660 };
        public int[] MoneyPerLevel => moneyPerLevel;
        [Tooltip("Stat bonus per card level above 1")]
        [SerializeField] float statPerLevel = 0.12f;
        public float StatPerLevel => statPerLevel;
        [Tooltip("Chest rarity weights: common, rare, epic")]
        [SerializeField] float[] chestRarityWeights = new float[] { 70, 25, 5 };
        public float[] ChestRarityWeights => chestRarityWeights;

        [Header("Squad")]
        [SerializeField] string starterUnitId = "ranger";
        public string StarterUnitId => starterUnitId;
        [SerializeField] string starterWeaponId = "pistol";
        public string StarterWeaponId => starterWeaponId;

        [Header("Patrols (after the last chapter)")]
        [Tooltip("Enemy/obstacle health multiplier added per patrol level")]
        [SerializeField] float patrolHealthGrowth = 0.08f;
        public float PatrolHealthGrowth => patrolHealthGrowth;

        public UnitCardData GetUnit(string id)
        {
            for (int i = 0; i < units.Count; i++)
                if (units[i].id == id) return units[i];
            return null;
        }

        public UnitCardData GetUnitByCharacter(string characterId)
        {
            for (int i = 0; i < units.Count; i++)
                if (units[i].characterId == characterId) return units[i];
            return null;
        }

        public WeaponCardData GetWeapon(string id)
        {
            for (int i = 0; i < weapons.Count; i++)
                if (weapons[i].id == id) return weapons[i];
            return null;
        }

        public BuildingData GetBuilding(BuildingType type)
        {
            for (int i = 0; i < buildings.Count; i++)
                if (buildings[i].type == type) return buildings[i];
            return null;
        }

        public ThreatInfo GetThreat(ThreatType type)
        {
            for (int i = 0; i < threats.Count; i++)
                if (threats[i].type == type) return threats[i];
            return null;
        }

        /// <summary>Chapter index for a 1-based level number, -1 = patrol (after the campaign).</summary>
        public int GetChapterIndex(int levelNumber)
        {
            for (int i = 0; i < chapters.Count; i++)
            {
                if (levelNumber >= chapters[i].firstLevel && levelNumber <= chapters[i].lastLevel)
                    return i;
            }
            return -1;
        }
    }
}
