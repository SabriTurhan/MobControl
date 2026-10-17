using System;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class UnitStats
    {
        public float healthMult = 1f;
        public float damageMult = 1f;
        public float fireRateMult = 1f;
        public float rangeMult = 1f;
        public float healPerSecond = 0f;

        public static readonly UnitStats Default = new UnitStats();
    }

    public class CardReward
    {
        public bool isUnit;
        public string id;
        public int copies;
        public bool isNew;
        public CardRarity rarity;
    }

    /// <summary>
    /// Dustwater campaign state: commander cards, squad selection, town buildings and rewards.
    /// </summary>
    public static class Campaign
    {
        private const string SAVE_NAME = "Campaign Save";
        private const string LEVEL_NUMBER_SAVE_NAME = "Level Number Save";

        public static CampaignDatabase Database { get; private set; }
        public static bool IsInitialised { get; private set; }

        private static CampaignSave save;
        private static LevelNumberSave levelSave;

        public static event SimpleCallback OnChanged;

        public static void Init(CampaignDatabase database)
        {
            Database = database;
            save = SaveController.GetSaveObject<CampaignSave>(SAVE_NAME);
            levelSave = SaveController.GetSaveObject<LevelNumberSave>(LEVEL_NUMBER_SAVE_NAME);

            int buildingsCount = Enum.GetValues(typeof(BuildingType)).Length;
            if (save.buildings == null || save.buildings.Length < buildingsCount)
            {
                int[] buildings = new int[buildingsCount];
                if (save.buildings != null)
                    Array.Copy(save.buildings, buildings, save.buildings.Length);
                save.buildings = buildings;
            }

            // Command Post is the starting building
            if (save.buildings[(int)BuildingType.CommandPost] < 1)
                save.buildings[(int)BuildingType.CommandPost] = 1;

            if (GetUnitSave(database.StarterUnitId) == null)
                save.units.Add(new CardSave() { id = database.StarterUnitId, level = 1, copies = 0 });

            if (GetWeaponSave(database.StarterWeaponId) == null)
                save.weapons.Add(new CardSave() { id = database.StarterWeaponId, level = 1, copies = 0 });

            if (string.IsNullOrEmpty(save.weapon) || GetWeaponSave(save.weapon) == null)
                save.weapon = database.StarterWeaponId;

            if (save.lastIncomeTicks == 0)
                save.lastIncomeTicks = DateTime.Now.Ticks;

            ValidateSquad();

            IsInitialised = true;
        }

        public static void MarkChanged()
        {
            SaveController.MarkAsSaveIsRequired();
            OnChanged?.Invoke();
        }

        #region Progress

        /// <summary>Levels completed so far (= 0-based index of the next level).</summary>
        public static int LevelsCompleted => levelSave.LevelNumber;
        public static int CurrentLevelNumber => levelSave.LevelNumber + 1;

        public static int CurrentChapterIndex => Database.GetChapterIndex(CurrentLevelNumber);
        public static ChapterData CurrentChapter
        {
            get
            {
                int index = CurrentChapterIndex;
                return index >= 0 ? Database.Chapters[index] : null;
            }
        }

        public static bool IsIntroSeen
        {
            get => save.introSeen;
            set { save.introSeen = value; MarkChanged(); }
        }

        public static bool IsStoryBeatSeen(int beatId) => save.storyBeatsSeen.Contains(beatId);

        public static void MarkStoryBeatSeen(int beatId)
        {
            if (!save.storyBeatsSeen.Contains(beatId))
            {
                save.storyBeatsSeen.Add(beatId);
                MarkChanged();
            }
        }

        #endregion

        #region Cards

        public static CardSave GetUnitSave(string id) => save.units.Find(c => c.id == id);
        public static CardSave GetWeaponSave(string id) => save.weapons.Find(c => c.id == id);
        public static CardSave GetCardSave(bool isUnit, string id) => isUnit ? GetUnitSave(id) : GetWeaponSave(id);

        public static bool IsOwned(bool isUnit, string id) => GetCardSave(isUnit, id) != null;

        public static int MaxCardLevel => Database.CopiesPerLevel.Length + 1;

        public static int GetCopiesNeeded(int level)
        {
            int[] copies = Database.CopiesPerLevel;
            if (level - 1 < 0 || level - 1 >= copies.Length) return -1;
            return copies[level - 1];
        }

        public static int GetLevelUpMoney(int level)
        {
            int[] money = Database.MoneyPerLevel;
            if (level - 1 < 0 || level - 1 >= money.Length) return -1;
            return money[level - 1];
        }

        public static bool CanLevelUp(bool isUnit, string id)
        {
            CardSave card = GetCardSave(isUnit, id);
            if (card == null || card.level >= MaxCardLevel) return false;

            return card.copies >= GetCopiesNeeded(card.level) && CurrencyController.HasAmount(CurrencyType.Money, GetLevelUpMoney(card.level));
        }

        public static bool LevelUp(bool isUnit, string id)
        {
            if (!CanLevelUp(isUnit, id)) return false;

            CardSave card = GetCardSave(isUnit, id);
            CurrencyController.Substract(CurrencyType.Money, GetLevelUpMoney(card.level));
            card.copies -= GetCopiesNeeded(card.level);
            card.level++;

            MarkChanged();
            return true;
        }

        public static void AddCard(bool isUnit, string id, int copies, out bool isNew)
        {
            CardSave card = GetCardSave(isUnit, id);
            isNew = card == null;
            if (isNew)
            {
                card = new CardSave() { id = id, level = 1, copies = Mathf.Max(0, copies - 1) };
                if (isUnit) save.units.Add(card); else save.weapons.Add(card);
            }
            else
            {
                card.copies += copies;
            }

            MarkChanged();
        }

        public static float GetLevelFactor(int level) => 1f + Database.StatPerLevel * (Mathf.Max(1, level) - 1);

        public static UnitStats GetUnitStats(UnitCardData data)
        {
            if (data == null) return UnitStats.Default;

            CardSave card = GetUnitSave(data.id);
            float factor = GetLevelFactor(card != null ? card.level : 1);

            return new UnitStats()
            {
                healthMult = data.healthMult * factor,
                damageMult = data.damageMult * factor,
                fireRateMult = data.fireRateMult * (1f + (factor - 1f) * 0.5f),
                rangeMult = data.rangeMult,
                healPerSecond = data.healPerSecond * factor,
            };
        }

        public static UnitStats GetStatsForCharacter(string characterId)
        {
            if (!IsInitialised) return UnitStats.Default;

            return GetUnitStats(Database.GetUnitByCharacter(characterId));
        }

        /// <summary>Damage multiplier of the squad weapon card (level bonus).</summary>
        public static float GetWeaponDamageFactor(string gunId)
        {
            if (!IsInitialised) return 1f;

            for (int i = 0; i < Database.Weapons.Count; i++)
            {
                WeaponCardData weapon = Database.Weapons[i];
                if (weapon.gunId == gunId)
                {
                    CardSave card = GetWeaponSave(weapon.id);
                    return GetLevelFactor(card != null ? card.level : 1);
                }
            }

            return 1f;
        }

        public static bool IsCardAvailable(int unlockAfterLevel) => LevelsCompleted >= unlockAfterLevel;

        #endregion

        #region Squad

        public static int SquadSlots => Mathf.Max(1, Mathf.RoundToInt(GetBuildingValue(BuildingType.CommandPost, 2)));

        public static IReadOnlyList<string> Squad => save.squad;
        public static string SelectedWeaponId => save.weapon;

        public static string SquadGunId
        {
            get
            {
                WeaponCardData weapon = Database.GetWeapon(save.weapon);
                return weapon != null ? weapon.gunId : null;
            }
        }

        public static void SetSquad(List<string> unitIds)
        {
            save.squad = new List<string>(unitIds);
            ValidateSquad();
            MarkChanged();
        }

        public static void SetWeapon(string weaponId)
        {
            if (GetWeaponSave(weaponId) == null) return;

            save.weapon = weaponId;
            MarkChanged();
        }

        private static void ValidateSquad()
        {
            List<string> valid = new List<string>();
            int slots = SquadSlots;

            for (int i = 0; i < save.squad.Count && valid.Count < slots; i++)
            {
                string id = save.squad[i];
                UnitCardData data = Database.GetUnit(id);
                if (data == null || GetUnitSave(id) == null) continue;
                if (!data.isFiller && valid.Contains(id)) continue;

                valid.Add(id);
            }

            // Empty slots are filled with Rangers
            while (valid.Count < slots)
                valid.Add(Database.StarterUnitId);

            save.squad = valid;
        }

        #endregion

        #region Buildings

        public static int GetBuildingLevel(BuildingType type) => save.buildings[(int)type];

        public static bool IsBuildingUnlocked(BuildingType type)
        {
            BuildingData data = Database.GetBuilding(type);
            return data != null && LevelsCompleted >= data.unlockAfterLevel;
        }

        public static BuildingLevelData GetNextBuildingLevel(BuildingType type)
        {
            BuildingData data = Database.GetBuilding(type);
            if (data == null) return null;

            int level = GetBuildingLevel(type);
            return level < data.levels.Count ? data.levels[level] : null;
        }

        public static bool CanBuild(BuildingType type)
        {
            if (!IsBuildingUnlocked(type)) return false;

            BuildingLevelData next = GetNextBuildingLevel(type);
            if (next == null) return false;

            return CurrencyController.HasAmount(CurrencyType.Money, next.money) && CurrencyController.HasAmount(CurrencyType.Supplies, next.supplies);
        }

        public static bool Build(BuildingType type)
        {
            if (!CanBuild(type)) return false;

            if (type == BuildingType.Saloon)
                CollectIncome();

            BuildingLevelData next = GetNextBuildingLevel(type);
            CurrencyController.Substract(CurrencyType.Money, next.money);
            CurrencyController.Substract(CurrencyType.Supplies, next.supplies);
            save.buildings[(int)type]++;

            if (type == BuildingType.CommandPost)
                ValidateSquad();

            MarkChanged();
            return true;
        }

        public static float GetBuildingValue(BuildingType type, float defaultValue = 0f)
        {
            if (Database == null) return defaultValue;

            BuildingData data = Database.GetBuilding(type);
            int level = GetBuildingLevel(type);
            if (data == null || level <= 0 || data.levels.Count == 0) return defaultValue;

            return data.levels[Mathf.Min(level, data.levels.Count) - 1].value;
        }

        public static float HealthMultiplier => 1f + GetBuildingValue(BuildingType.Barracks) / 100f;
        public static float DamageMultiplier => 1f + GetBuildingValue(BuildingType.Armory) / 100f;
        public static float FireRateMultiplier => 1f + GetBuildingValue(BuildingType.Workshop) / 100f;
        public static float RangeMultiplier => 1f + GetBuildingValue(BuildingType.Watchtower) / 100f;
        public static int FreeRevives => Mathf.RoundToInt(GetBuildingValue(BuildingType.FieldHospital));
        public static float IncomePerHour => GetBuildingValue(BuildingType.Saloon);

        /// <summary>Saloon income waiting to be collected (capped at 8 hours).</summary>
        public static int PendingIncome
        {
            get
            {
                if (IncomePerHour <= 0) return 0;

                double hours = (DateTime.Now.Ticks - save.lastIncomeTicks) / (double)TimeSpan.TicksPerHour;
                hours = Math.Max(0, Math.Min(8, hours));
                return Mathf.FloorToInt((float)(hours * IncomePerHour));
            }
        }

        public static int CollectIncome()
        {
            int income = PendingIncome;
            save.lastIncomeTicks = DateTime.Now.Ticks;
            if (income > 0)
                CurrencyController.Add(CurrencyType.Money, income);

            MarkChanged();
            return income;
        }

        #endregion

        #region Rewards

        /// <summary>Rolls the chest card for a finished level and adds it to the collection.</summary>
        public static CardReward RollChestCard(int levelNumber, bool isBossLevel)
        {
            // A card whose unlock level was just reached is introduced first
            CardReward reward = GetIntroducedCard();
            if (reward == null)
            {
                CardRarity rarity = RollRarity(isBossLevel);
                reward = RollCardOfRarity(rarity);
            }

            if (reward == null) return null;

            if (isBossLevel)
                reward.copies += 2;

            AddCard(reward.isUnit, reward.id, reward.copies, out reward.isNew);
            return reward;
        }

        private static CardReward GetIntroducedCard()
        {
            for (int i = 0; i < Database.Units.Count; i++)
            {
                UnitCardData unit = Database.Units[i];
                if (GetUnitSave(unit.id) == null && IsCardAvailable(unit.unlockAfterLevel))
                    return new CardReward() { isUnit = true, id = unit.id, copies = 1, rarity = unit.rarity };
            }

            for (int i = 0; i < Database.Weapons.Count; i++)
            {
                WeaponCardData weapon = Database.Weapons[i];
                if (GetWeaponSave(weapon.id) == null && IsCardAvailable(weapon.unlockAfterLevel))
                    return new CardReward() { isUnit = false, id = weapon.id, copies = 1, rarity = weapon.rarity };
            }

            return null;
        }

        private static CardRarity RollRarity(bool isBossLevel)
        {
            float[] weights = Database.ChestRarityWeights;
            float total = 0;
            int start = isBossLevel ? 1 : 0;
            for (int i = start; i < weights.Length; i++) total += weights[i];

            float roll = UnityEngine.Random.value * total;
            for (int i = start; i < weights.Length; i++)
            {
                roll -= weights[i];
                if (roll <= 0) return (CardRarity)i;
            }

            return CardRarity.Common;
        }

        private static CardReward RollCardOfRarity(CardRarity rarity)
        {
            List<CardReward> pool = new List<CardReward>();

            // Fall back to lower rarities when nothing of this rarity is available yet
            for (int r = (int)rarity; r >= 0 && pool.Count == 0; r--)
            {
                for (int i = 0; i < Database.Units.Count; i++)
                {
                    UnitCardData unit = Database.Units[i];
                    if ((int)unit.rarity == r && IsCardAvailable(unit.unlockAfterLevel))
                        pool.Add(new CardReward() { isUnit = true, id = unit.id, copies = r == 0 ? 2 : 1, rarity = unit.rarity });
                }

                for (int i = 0; i < Database.Weapons.Count; i++)
                {
                    WeaponCardData weapon = Database.Weapons[i];
                    if ((int)weapon.rarity == r && IsCardAvailable(weapon.unlockAfterLevel))
                        pool.Add(new CardReward() { isUnit = false, id = weapon.id, copies = r == 0 ? 2 : 1, rarity = weapon.rarity });
                }
            }

            return pool.Count > 0 ? pool.GetRandomItem() : null;
        }

        #endregion
    }
}
