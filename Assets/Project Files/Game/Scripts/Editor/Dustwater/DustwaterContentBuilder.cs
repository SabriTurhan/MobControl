using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Watermelon.Dustwater
{
    // Registers the Dustwater content in the template databases (guns, characters, enemies, obstacles,
    // Supplies currency, particles) and writes the Campaign Database (cards, buildings, chapters, threats).
    // Re-runnable: entries are matched by id and overwritten.
    public static class DustwaterContentBuilder
    {
        public const string LEVELS_DATABASE = "Assets/Project Files/Data/Level System/Levels Database.asset";
        public const string CURRENCIES_DATABASE = "Assets/Project Files/Data/Currencies Database.asset";
        public const string CAMPAIGN_DATABASE = "Assets/Project Files/Data/Campaign/Campaign Database.asset";
        public const string GAME_SCENE = "Assets/Project Files/Game/Scenes/Game.unity";
        private const string EXPLOSION_PARTICLE = "Assets/Project Files/Game/Prefabs/Particles/Explosion.prefab";

        [MenuItem("Tools/Dustwater/Content/Register Content + Campaign Database", priority = 200)]
        public static void BuildAll()
        {
            RegisterLevelsDatabase();
            RegisterCurrency();
            BuildCampaignDatabase();
            BuildUIStyle();
            SetupGameScene();

            AssetDatabase.SaveAssets();
            Debug.Log("[Dustwater] Content registered");
        }

        #region Levels database

        private static void RegisterLevelsDatabase()
        {
            LevelsDatabase database = AssetDatabase.LoadAssetAtPath<LevelsDatabase>(LEVELS_DATABASE);
            SerializedObject so = new SerializedObject(database);

            SerializedProperty guns = so.FindProperty("gunsData");
            AnimationClip riflePose = FindGun(guns, "rifle").FindPropertyRelative("poseAnimation").objectReferenceValue as AnimationClip;

            SetGun(guns, "sniper", "Sniper Rifle", DustwaterUnitsBuilder.SNIPER_GUN, riflePose, fireRate: 0.4f, damage: 4.5f, range: 14f, speed: 45f, count: 1, spread: 0f, pierce: 2, splash: 0f);
            SetGun(guns, "grenade_launcher", "Grenade Launcher", DustwaterUnitsBuilder.LAUNCHER_GUN, riflePose, fireRate: 0.45f, damage: 2.6f, range: 3f, speed: 15f, count: 1, spread: 0f, pierce: 0, splash: 2.2f);
            AnimationClip minigunPose = FindGun(guns, "minigun").FindPropertyRelative("poseAnimation").objectReferenceValue as AnimationClip;
            SetGun(guns, "boss_minigun", "Boss Minigun", "Assets/Project Files/Game/Prefabs/Guns/Minigun.prefab", minigunPose, fireRate: 1f, damage: 1f, range: 17f, speed: 16f, count: 1, spread: 8f, pierce: 0, splash: 0f, aim: true, unblockable: true);
            SetGun(guns, "bandit_sniper", "Bandit Sniper Rifle", DustwaterUnitsBuilder.SNIPER_GUN, riflePose, fireRate: 1f, damage: 1f, range: 26f, speed: 32f, count: 1, spread: 0f, pierce: 0, splash: 0f, aim: true, unblockable: true);
            SetGun(guns, "bandit_rifle", "Bandit Rifle", "Assets/Project Files/Game/Prefabs/Guns/Rifle.prefab", riflePose, fireRate: 1f, damage: 1f, range: 14f, speed: 20f, count: 1, spread: 0f, pierce: 0, splash: 0f, aim: true, unblockable: false);

            SerializedProperty characters = so.FindProperty("charactersData");
            SetCharacter(characters, "sharpshooter_character", "Sharpshooter", DustwaterUnitsBuilder.SHARPSHOOTER, "sniper");
            SetCharacter(characters, "medic_character", "Medic", DustwaterUnitsBuilder.MEDIC, "default_gun");
            SetCharacter(characters, "grenadier_character", "Grenadier", DustwaterUnitsBuilder.GRENADIER, "grenade_launcher");

            SerializedProperty enemies = so.FindProperty("enemiesData");
            SetSimple(enemies, "bandit_sniper", DustwaterUnitsBuilder.BANDIT_SNIPER);
            SetSimple(enemies, "boss_rattler", DustwaterUnitsBuilder.BOSS_RATTLER);
            SetSimple(enemies, "boss_jaws", DustwaterUnitsBuilder.BOSS_JAWS);
            SetSimple(enemies, "boss_vex", DustwaterUnitsBuilder.BOSS_VEX);

            SerializedProperty obstacles = so.FindProperty("obstaclesData");
            SetSimple(obstacles, "sandbags", DustwaterUnitsBuilder.SANDBAGS);
            SetSimple(obstacles, "supply_crate", DustwaterUnitsBuilder.SUPPLY_CRATE);

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(database);

            // template bandits carry the aimed bandit rifle in ambush rows: same hand placement as the rifle
            foreach (string enemy in new[] { "Assets/Project Files/Game/Prefabs/Enemy/Enemy Human Behavior.prefab", "Assets/Project Files/Game/Prefabs/Enemy/Enemy Heavy Human Behavior.prefab" })
            {
                GameObject contents = PrefabUtility.LoadPrefabContents(enemy);
                DustwaterUnitsBuilder.CopyWeaponPlacement(contents.GetComponent<EnemyBehavior>(), "rifle", "bandit_rifle");
                PrefabUtility.SaveAsPrefabAsset(contents, enemy);
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static SerializedProperty FindById(SerializedProperty list, string id, bool create)
        {
            for (int i = 0; i < list.arraySize; i++)
            {
                SerializedProperty element = list.GetArrayElementAtIndex(i);
                if (element.FindPropertyRelative("id").stringValue == id) return element;
            }

            if (!create) return null;

            list.InsertArrayElementAtIndex(list.arraySize);
            SerializedProperty created = list.GetArrayElementAtIndex(list.arraySize - 1);
            created.FindPropertyRelative("id").stringValue = id;
            return created;
        }

        private static SerializedProperty FindGun(SerializedProperty guns, string id) => FindById(guns, id, false);

        private static void SetGun(SerializedProperty guns, string id, string displayName, string prefabPath, AnimationClip pose, float fireRate, float damage, float range, float speed, int count, float spread, int pierce, float splash, bool aim = false, bool unblockable = false)
        {
            SerializedProperty gun = FindById(guns, id, true);
            gun.FindPropertyRelative("prefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            gun.FindPropertyRelative("isDefault").boolValue = false;
            gun.FindPropertyRelative("fireRateMultiplier").floatValue = fireRate;
            gun.FindPropertyRelative("damageMultiplier").floatValue = damage;
            gun.FindPropertyRelative("bulletRangeMultiplier").floatValue = range;
            gun.FindPropertyRelative("bulletSpeed").floatValue = speed;
            gun.FindPropertyRelative("bulletsCount").intValue = count;
            gun.FindPropertyRelative("spread").floatValue = spread;
            gun.FindPropertyRelative("pierce").intValue = pierce;
            gun.FindPropertyRelative("splashRadius").floatValue = splash;
            gun.FindPropertyRelative("aimAtSquad").boolValue = aim;
            gun.FindPropertyRelative("unblockable").boolValue = unblockable;
            gun.FindPropertyRelative("poseAnimation").objectReferenceValue = pose;
            gun.FindPropertyRelative("displayName").stringValue = displayName;
        }

        private static void SetCharacter(SerializedProperty characters, string id, string displayName, string prefabPath, string lockedGun)
        {
            SerializedProperty character = FindById(characters, id, true);
            character.FindPropertyRelative("prefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            character.FindPropertyRelative("characterType").enumValueIndex = (int)CharacterType.Humanoid;
            character.FindPropertyRelative("isDefault").boolValue = false;
            character.FindPropertyRelative("isGunLocked").boolValue = !string.IsNullOrEmpty(lockedGun);
            character.FindPropertyRelative("lockedGunId").stringValue = lockedGun;
            character.FindPropertyRelative("displayName").stringValue = displayName;
            character.FindPropertyRelative("shootingImpactType").enumValueIndex = (int)ShootingImpactType.Animation;
        }

        private static void SetSimple(SerializedProperty list, string id, string prefabPath)
        {
            SerializedProperty element = FindById(list, id, true);
            element.FindPropertyRelative("prefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        #endregion

        #region Currency

        private static void RegisterCurrency()
        {
            Object database = AssetDatabase.LoadAssetAtPath<Object>(CURRENCIES_DATABASE);
            SerializedObject so = new SerializedObject(database);
            SerializedProperty currencies = so.FindProperty("currencies");

            SerializedProperty supplies = null;
            for (int i = 0; i < currencies.arraySize; i++)
            {
                SerializedProperty element = currencies.GetArrayElementAtIndex(i);
                if (element.FindPropertyRelative("currencyType").enumValueIndex == (int)CurrencyType.Supplies) supplies = element;
            }

            if (supplies == null)
            {
                currencies.InsertArrayElementAtIndex(0);
                currencies.MoveArrayElement(0, currencies.arraySize - 1);
                supplies = currencies.GetArrayElementAtIndex(currencies.arraySize - 1);
            }

            supplies.FindPropertyRelative("currencyType").enumValueIndex = (int)CurrencyType.Supplies;
            supplies.FindPropertyRelative("defaultAmount").intValue = 0;
            supplies.FindPropertyRelative("data.dropPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(DustwaterUnitsBuilder.SUPPLIES_DROP);

            Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(DustwaterIcons.SUPPLIES_ICON);
            if (icon != null) supplies.FindPropertyRelative("icon").objectReferenceValue = icon;

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(database);
        }

        #endregion

        #region Game scene

        private static void SetupGameScene()
        {
            var scene = EditorSceneManager.OpenScene(GAME_SCENE, OpenSceneMode.Single);

            GameController gameController = Object.FindFirstObjectByType<GameController>();
            CampaignController campaign = gameController.GetComponent<CampaignController>();
            if (campaign == null) campaign = gameController.gameObject.AddComponent<CampaignController>();

            SerializedObject campaignSo = new SerializedObject(campaign);
            campaignSo.FindProperty("database").objectReferenceValue = AssetDatabase.LoadAssetAtPath<CampaignDatabase>(CAMPAIGN_DATABASE);
            campaignSo.FindProperty("uiStyle").objectReferenceValue = AssetDatabase.LoadAssetAtPath<DustwaterUIStyle>(UI_STYLE);
            campaignSo.ApplyModifiedPropertiesWithoutUndo();

            ParticlesController particles = Object.FindFirstObjectByType<ParticlesController>();
            SerializedObject particlesSo = new SerializedObject(particles);
            SerializedProperty list = particlesSo.FindProperty("particles");
            SerializedProperty explosion = null;
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).FindPropertyRelative("particleName").stringValue == "Grenade Explosion") explosion = list.GetArrayElementAtIndex(i);

            if (explosion == null)
            {
                list.InsertArrayElementAtIndex(list.arraySize);
                explosion = list.GetArrayElementAtIndex(list.arraySize - 1);
            }

            explosion.FindPropertyRelative("particleName").stringValue = "Grenade Explosion";
            explosion.FindPropertyRelative("particlePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(EXPLOSION_PARTICLE);
            particlesSo.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        #endregion

        #region UI style

        public const string UI_STYLE = "Assets/Project Files/Data/Campaign/Dustwater UI Style.asset";

        private static void BuildUIStyle()
        {
            DustwaterUIStyle style = AssetDatabase.LoadAssetAtPath<DustwaterUIStyle>(UI_STYLE);
            if (style == null)
            {
                style = ScriptableObject.CreateInstance<DustwaterUIStyle>();
                AssetDatabase.CreateAsset(style, UI_STYLE);
            }

            const string GU = "Assets/Project Files/Game/Images/Base/General UI/";
            const string ST = "Assets/Project Files/Game/Images/Base/Store/";
            const string CO = "Assets/Watermelon Core/Core Resources/Images/";
            const string FONTS = "Assets/Project Files/Game/Fonts/FredokaOne/FredokaOne 50/";

            // the SDF version (localization builder) when present: outline presets also apply to fallback glyphs
            TMPro.TMP_FontAsset sdf = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(DustwaterLocalizationBuilder.SDF_FONT);
            style.font = sdf != null ? sdf : AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(FONTS + "FredokaOne 50.asset");
            style.fontOutline = sdf != null ? sdf : AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(FONTS + "FredokaOne 50 Outline.asset");
            style.fontMaterial = sdf != null ? AssetDatabase.LoadAssetAtPath<Material>(DustwaterLocalizationBuilder.MAT_PLAIN) : null;
            style.fontOutlineMaterial = sdf != null ? AssetDatabase.LoadAssetAtPath<Material>(DustwaterLocalizationBuilder.MAT_OUTLINE) : null;

            style.panelWhite = S(ST + "item_background.png");
            style.panelBlue = S(GU + "panel_blue.png");
            style.panelDark = S(GU + "panel_dark.png");
            style.panelRed = S(GU + "panel_red.png");
            style.panelOutlinedDark = S(GU + "panel_outlined_dark.png");
            style.itemBackground = S(ST + "item_background.png");
            style.itemFrame = S(ST + "item_frame.png");
            style.cardCommon = S(ST + "panel_blue_store.png");
            style.cardRare = S(ST + "panel_purple_store.png");
            style.cardEpic = S(ST + "panel_yellow_white.png");
            style.headerOrange = S(ST + "header_orange.png");
            style.roundedBack = S(CO + "core_universal_back.png");
            style.square = S(CO + "core_square.png");
            style.circle = S(CO + "core_universal_back.png");
            style.shine = S(GU + "grad_shine_round.png");

            style.buttonGreen = S(GU + "btn_green.png");
            style.buttonOrange = S(GU + "btn_orange.png");
            style.buttonRed = S(GU + "btn_red.png");
            style.buttonGray = S(GU + "btn_gray.png");
            style.buttonPurple = S(GU + "btn_purple.png");
            style.buttonClose = S(GU + "btn_close.png");

            style.money = S("Assets/Project Files/Game/Images/Base/Currency/money.png");
            style.supplies = S(DustwaterIcons.SUPPLIES_ICON);
            style.star = S(GU + "ic_star_small.png");
            style.checkmark = S(GU + "checkmark.png");
            style.questionMark = S(ST + "questionmark.png");
            style.plus = S(GU + "ic_plus.png");
            style.store = S(GU + "ic_store.png");
            style.heart = S(GU + "heart.png");
            style.town = DustwaterIcons.Icon("building_Saloon") ?? S(GU + "ic_store.png");
            style.squad = DustwaterIcons.Portrait("ranger");
            style.battle = S("Assets/Project Files/Game/Images/Base/Gates/booster_rifle.png");
            style.chest = DustwaterIcons.Icon("chest");
            style.commanderPortrait = DustwaterIcons.Portrait("ranger");
            style.commanderName = "SGT. MASON";

            EditorUtility.SetDirty(style);
        }

        private static Sprite S(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);

        #endregion

        #region Campaign database

        private static void BuildCampaignDatabase()
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(CAMPAIGN_DATABASE));

            CampaignDatabase database = AssetDatabase.LoadAssetAtPath<CampaignDatabase>(CAMPAIGN_DATABASE);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<CampaignDatabase>();
                AssetDatabase.CreateAsset(database, CAMPAIGN_DATABASE);
            }

            database.Units.Clear();
            database.Units.Add(Unit("ranger", "Ranger", CardRarity.Common, "default_character", 0, true, "Reliable rifleman. Fights with your squad weapon.", 1f, 1f, 1f, 1f, 0f));
            database.Units.Add(Unit("heavy", "Heavy Gunner", CardRarity.Rare, "heavy_character", 3, false, "Double health. Shreds crowds with his minigun.", 2f, 1f, 1f, 1f, 0f, ThreatType.Swarm, ThreatType.Attrition));
            database.Units.Add(Unit("sharpshooter", "Sharpshooter", CardRarity.Rare, "sharpshooter_character", 5, false, "Long range shots that pierce through 3 bandits.", 0.9f, 1f, 1f, 1f, 0f, ThreatType.Armored, ThreatType.Snipers));
            database.Units.Add(Unit("medic", "Medic", CardRarity.Rare, "medic_character", 7, false, "Heals the whole squad over time.", 1f, 0.6f, 1f, 1f, 0.5f, ThreatType.Attrition));
            database.Units.Add(Unit("grenadier", "Grenadier", CardRarity.Epic, "grenadier_character", 15, false, "Lobs grenades that blast everything nearby.", 1.1f, 1f, 1f, 1f, 0f, ThreatType.Barricades, ThreatType.Swarm));

            database.Weapons.Clear();
            database.Weapons.Add(Weapon("pistol", "Pistol", CardRarity.Common, "default_gun", 0, "Basic sidearm. Better than nothing."));
            database.Weapons.Add(Weapon("shotgun", "Shotgun", CardRarity.Common, "shotgun", 2, "Three pellets per shot. Wrecks barricades.", ThreatType.Barricades));
            database.Weapons.Add(Weapon("rifle", "Rifle", CardRarity.Rare, "rifle", 7, "Double damage per bullet. Cuts through armor.", ThreatType.Armored));
            database.Weapons.Add(Weapon("minigun", "Minigun", CardRarity.Epic, "minigun", 12, "Triple fire rate. Mows down crowds.", ThreatType.Swarm));

            database.Buildings.Clear();
            database.Buildings.Add(Building(BuildingType.CommandPost, "Command Post", "Your headquarters. Every level adds a squad slot.", "{0} squad slots", 0,
                L(0, 0, 2), L(150, 10, 3), L(500, 35, 4), L(1200, 80, 5)));
            database.Buildings.Add(Building(BuildingType.Barracks, "Barracks", "Bunks and training. Tougher soldiers.", "+{0}% squad health", 2,
                L(60, 5, 10), L(150, 15, 20), L(320, 30, 35), L(600, 55, 50), L(1000, 90, 70)));
            database.Buildings.Add(Building(BuildingType.Armory, "Armory", "Better ammo for every gun.", "+{0}% damage", 4,
                L(80, 6, 10), L(180, 18, 20), L(360, 34, 35), L(650, 60, 50), L(1100, 95, 70)));
            database.Buildings.Add(Building(BuildingType.Workshop, "Workshop", "Oiled and tuned weapons shoot faster.", "+{0}% fire rate", 7,
                L(120, 10, 8), L(260, 24, 16), L(480, 42, 25), L(800, 70, 35), L(1300, 110, 45)));
            database.Buildings.Add(Building(BuildingType.FieldHospital, "Field Hospital", "Patches up the squad once it falls.", "{0} free revive per mission", 10,
                L(300, 25, 1), L(900, 70, 2)));
            database.Buildings.Add(Building(BuildingType.Saloon, "Saloon", "Grateful townsfolk pay for protection.", "+{0} cash per hour", 13,
                L(400, 30, 30), L(800, 60, 60), L(1400, 100, 100), L(2200, 150, 150)));
            database.Buildings.Add(Building(BuildingType.Watchtower, "Watchtower", "Spot bandits early and shoot further.", "+{0}% range", 20,
                L(900, 80, 10), L(1600, 130, 20), L(2600, 200, 30)));

            database.Chapters.Clear();
            database.Chapters.Add(Chapter("OUTSKIRTS", "Gas station and water tower", 1, 10, "Lieutenant Rattler", "boss_rattler", "Rangers in MY town? The boys will fill you with holes!",
                new[] { "The Red Scorpions hold the road into Dustwater.", "Clear the outskirts, Commander!" },
                new[] { "Rattler is down! The outskirts are free.", "Townsfolk are coming back. Time to rebuild!" }));
            database.Chapters.Add(Chapter("MAIN STREET", "Saloon and market", 11, 20, "Big Jaws", "boss_jaws", "Main Street is mine. Nobody walks past Big Jaws!",
                new[] { "Main Street is the heart of Dustwater.", "Big Jaws guards it with an iron grip." },
                new[] { "Big Jaws is finished!", "Only the Scorpion Fort remains." }));
            database.Chapters.Add(Chapter("SCORPION FORT", "Vex's stronghold", 21, 30, "Boss Vex", "boss_vex", "You took my town, Commander. You will not take my fort!",
                new[] { "Vex hides in his fort at the edge of town.", "This is the final push, Commander!" },
                new[] { "Vex runs off into the desert. Dustwater is free!", "Keep patrolling the roads, Commander." }));

            database.Threats.Clear();
            database.Threats.Add(Threat(ThreatType.Armored, "ARMORED", "Heavy bandits with thick vests.", new Color32(120, 128, 150, 255)));
            database.Threats.Add(Threat(ThreatType.Swarm, "SWARM", "Lots of bandits at once.", new Color32(255, 140, 50, 255)));
            database.Threats.Add(Threat(ThreatType.Snipers, "SNIPERS", "Bandit snipers fire from far away.", new Color32(160, 70, 140, 255)));
            database.Threats.Add(Threat(ThreatType.Barricades, "BARRICADES", "Sandbag walls block the road.", new Color32(200, 160, 90, 255)));
            database.Threats.Add(Threat(ThreatType.Attrition, "AMBUSH", "A long road under constant fire.", new Color32(230, 70, 70, 255)));

            DustwaterIcons.AssignIcons(database);

            EditorUtility.SetDirty(database);
        }

        private static UnitCardData Unit(string id, string name, CardRarity rarity, string characterId, int unlock, bool filler, string description, float health, float damage, float fireRate, float range, float heal, params ThreatType[] counters)
        {
            return new UnitCardData()
            {
                id = id, displayName = name, rarity = rarity, characterId = characterId, unlockAfterLevel = unlock, isFiller = filler,
                description = description, healthMult = health, damageMult = damage, fireRateMult = fireRate, rangeMult = range, healPerSecond = heal,
                counters = new List<ThreatType>(counters),
            };
        }

        private static WeaponCardData Weapon(string id, string name, CardRarity rarity, string gunId, int unlock, string description, params ThreatType[] counters)
        {
            return new WeaponCardData() { id = id, displayName = name, rarity = rarity, gunId = gunId, unlockAfterLevel = unlock, description = description, counters = new List<ThreatType>(counters) };
        }

        private static BuildingLevelData L(int money, int supplies, float value) => new BuildingLevelData() { money = money, supplies = supplies, value = value };

        private static BuildingData Building(BuildingType type, string name, string description, string format, int unlock, params BuildingLevelData[] levels)
        {
            return new BuildingData() { type = type, displayName = name, description = description, valueFormat = format, unlockAfterLevel = unlock, levels = new List<BuildingLevelData>(levels) };
        }

        private static ChapterData Chapter(string title, string subtitle, int first, int last, string boss, string bossPortrait, string taunt, string[] intro, string[] outro)
        {
            return new ChapterData()
            {
                title = title, subtitle = subtitle, firstLevel = first, lastLevel = last, bossName = boss, bossPortrait = DustwaterIcons.Portrait(bossPortrait), bossTaunt = taunt,
                introLines = new List<string>(intro), outroLines = new List<string>(outro),
            };
        }

        private static ThreatInfo Threat(ThreatType type, string name, string description, Color color)
        {
            return new ThreatInfo() { type = type, displayName = name, description = description, color = color };
        }

        #endregion
    }
}
