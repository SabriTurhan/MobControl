using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Watermelon.Dustwater
{
    // Writes the 30 campaign missions (Level 001-030) from a design table. Every mission is a sequence of
    // rows (template spacing: first row at z 19.3, one row every 11.27, lanes at x -1.5 / 1.5, wide rows at
    // -2..2). Row patterns follow the mission's threats so the briefing intel matches the road:
    //   Armored -> heavy bandits, Swarm -> 5-wide crowds, Snipers -> bandit sniper nests behind sandbags,
    //   Barricades -> sandbag walls (drop Supplies), Attrition -> shooters on most rows.
    // Seeded per level, so re-running gives the same missions.
    public static class DustwaterLevelBuilder
    {
        private const string LEVELS_FOLDER = "Assets/Project Files/Data/Level System/Levels";
        private const float FIRST_ROW_Z = 19.33f;
        private const float ROW_STEP = 11.27f;

        private class Mission
        {
            public int number;
            public string operation;
            public ThreatType[] threats;
            public bool boss;
            public MovementModeType mode = MovementModeType.Classic;
        }

        private static Mission M(int number, string operation, params ThreatType[] threats) => new Mission() { number = number, operation = operation, threats = threats };

        private static readonly Mission[] MISSIONS =
        {
            M(1, "Dusty Crossroads"),
            M(2, "Tumbleweed Run"),
            M(3, "Sandbag Alley", ThreatType.Barricades),
            M(4, "Iron Vests", ThreatType.Armored),
            M(5, "Coyote Pack", ThreatType.Swarm),
            M(6, "Gas Station Rescue", ThreatType.Snipers),
            M(7, "Old Mill Road", ThreatType.Barricades, ThreatType.Armored),
            M(8, "Water Tower Watch", ThreatType.Attrition),
            M(9, "Rusty Rails", ThreatType.Snipers, ThreatType.Swarm),
            new Mission() { number = 10, operation = "Rattler's Roost", threats = new[] { ThreatType.Armored }, boss = true },

            M(11, "Main Street Entry", ThreatType.Attrition),
            M(12, "Saloon Doors", ThreatType.Swarm, ThreatType.Barricades),
            M(13, "Rooftop Rifles", ThreatType.Snipers),
            M(14, "Sheriff's Office", ThreatType.Armored, ThreatType.Attrition),
            M(15, "Market Mayhem", ThreatType.Barricades, ThreatType.Snipers),
            M(16, "Blacksmith Alley", ThreatType.Swarm, ThreatType.Armored),
            M(17, "Church Bell", ThreatType.Attrition, ThreatType.Snipers),
            M(18, "Stagecoach Stop", ThreatType.Barricades, ThreatType.Swarm),
            M(19, "Hotel Balconies", ThreatType.Armored, ThreatType.Snipers),
            new Mission() { number = 20, operation = "Jaws' Jailhouse", threats = new[] { ThreatType.Armored, ThreatType.Attrition }, boss = true },

            M(21, "Fort Outskirts", ThreatType.Swarm, ThreatType.Attrition),
            M(22, "Scorpion Trenches", ThreatType.Barricades, ThreatType.Armored),
            M(23, "Ammo Dump", ThreatType.Snipers, ThreatType.Swarm),
            M(24, "Barbed Gate", ThreatType.Attrition, ThreatType.Barricades),
            M(25, "Watchtower Line", ThreatType.Armored, ThreatType.Snipers),
            M(26, "Supply Raid", ThreatType.Swarm, ThreatType.Barricades),
            M(27, "Inner Wall", ThreatType.Snipers, ThreatType.Attrition),
            M(28, "Vex's Garage", ThreatType.Armored, ThreatType.Swarm),
            M(29, "Last Stand", ThreatType.Barricades, ThreatType.Snipers, ThreatType.Attrition),
            new Mission() { number = 30, operation = "Scorpion Throne", threats = new[] { ThreatType.Swarm, ThreatType.Snipers }, boss = true },
        };

        private static readonly Dictionary<int, (string id, float health, float damage, float fireRate)> BOSSES = new Dictionary<int, (string, float, float, float)>()
        {
            { 10, ("boss_rattler", 380f, 1.5f, 2.2f) },
            { 20, ("boss_jaws", 520f, 1.5f, 2.0f) },
            { 30, ("boss_vex", 6000f, 3f, 2.6f) },
        };

        #region Item lists

        private class Content
        {
            public readonly List<System.Action<SerializedProperty>> gates = new List<System.Action<SerializedProperty>>();
            public readonly List<System.Action<SerializedProperty>> boosters = new List<System.Action<SerializedProperty>>();
            public readonly List<System.Action<SerializedProperty>> obstacles = new List<System.Action<SerializedProperty>>();
            public readonly List<System.Action<SerializedProperty>> enemies = new List<System.Action<SerializedProperty>>();
        }

        private static void Base(SerializedProperty p, string id, Vector3 position)
        {
            p.FindPropertyRelative("id").stringValue = id;
            p.FindPropertyRelative("position").vector3Value = position;
        }

        private static void Drop(SerializedProperty p, DropableItemType type, CurrencyType currency, int count, float value)
        {
            p.FindPropertyRelative("dropType").intValue = (int)type;
            p.FindPropertyRelative("dropCurrencyType").intValue = (int)currency;
            p.FindPropertyRelative("dropItemsCount").intValue = count;
            p.FindPropertyRelative("dropItemValue").floatValue = value;
        }

        private static void Gate(Content c, Vector3 position, GateType type, OperationType operation, float value, string explicitId = "default_character", int characters = 1, bool grows = true)
        {
            string id = type switch
            {
                GateType.GiveCharacter => "gate_character",
                GateType.GiveWeapon => "gate_weapon",
                GateType.Health => "gate_health",
                GateType.Damage => "gate_damage",
                GateType.FireRate => "gate_fire_rate",
                GateType.Range => "gate_range",
                _ => "gate_money",
            };

            c.gates.Add(p =>
            {
                Base(p, id, position);
                p.FindPropertyRelative("gateType").intValue = (int)type;
                p.FindPropertyRelative("operationType").intValue = (int)operation;
                p.FindPropertyRelative("numericalValue").floatValue = value;
                p.FindPropertyRelative("updateOnHit").boolValue = grows && type != GateType.GiveWeapon && type != GateType.GiveCharacter;
                p.FindPropertyRelative("step").floatValue = type == GateType.Money ? 1f : 0.05f;
                p.FindPropertyRelative("explicitId").stringValue = explicitId;
                p.FindPropertyRelative("charactersAmount").intValue = characters;
            });
        }

        private static void Booster(Content c, Vector3 position, BoosterType type, OperationType operation, float value, string explicitId = "", int characters = 0)
        {
            string id = type switch
            {
                BoosterType.GiveCharacter => "booster_character",
                BoosterType.GiveWeapon => "booster_weapon",
                BoosterType.Health => "booster_health",
                BoosterType.Damage => "booster_damage",
                BoosterType.FireRate => "booster_fire_rate",
                _ => "booster_range",
            };

            c.boosters.Add(p =>
            {
                Base(p, id, position);
                p.FindPropertyRelative("boosterType").intValue = (int)type;
                p.FindPropertyRelative("operationType").intValue = (int)operation;
                p.FindPropertyRelative("numericalValue").floatValue = value;
                p.FindPropertyRelative("explicitId").stringValue = explicitId;
                p.FindPropertyRelative("charactersAmount").intValue = characters;
            });
        }

        private static void Obstacle(Content c, string id, Vector3 position, int health, DropableItemType drop, CurrencyType currency, int count, float value, float boosterHeight = 1f)
        {
            c.obstacles.Add(p =>
            {
                Base(p, id, position);
                Drop(p, drop, currency, count, value);
                p.FindPropertyRelative("health").intValue = Mathf.Max(1, health);
                p.FindPropertyRelative("boosterHeight").floatValue = boosterHeight;
            });
        }

        private static void Enemy(Content c, string id, Vector3 position, float health, float damage, float fireRate, string gun, DropableItemType drop = DropableItemType.Money, int count = 1, float value = 1)
        {
            c.enemies.Add(p =>
            {
                Base(p, id, position);
                Drop(p, drop, CurrencyType.Money, count, value);
                p.FindPropertyRelative("health").floatValue = Mathf.Round(health);
                p.FindPropertyRelative("damage").floatValue = damage;
                p.FindPropertyRelative("fireRate").floatValue = fireRate;
                p.FindPropertyRelative("gunId").stringValue = gun;
            });
        }

        #endregion

        #region Scaling

        // Expected squad damage per second at mission n with a normal progression (slots, buildings, card
        // levels, a counter pick). The squad's bullets converge on its lane and every row is in range for ~3s
        // at 4 m/s, so one object in the chosen lane should die in about Power(n) * 1-2.5 hp.
        private static float Power(int n) => 4f + 2.6f * (n - 1);

        private static float Bandit(int n) => Mathf.Max(2f, Power(n) * 0.65f);
        private static float Heavy(int n) => Power(n) * 1.25f + 5f;
        private static float Sniper(int n) => Power(n) * 0.8f;
        private static int Barrel(int n, System.Random r) => Mathf.Max(2, Mathf.RoundToInt(Power(n) * (0.8f + (float)r.NextDouble() * 0.5f)));
        private static int Sandbags(int n) => Mathf.RoundToInt(Power(n) * 2.2f + 6f);
        private static int Crate(int n) => Mathf.RoundToInt(Power(n) * 0.9f + 2f);
        private static float EnemyDamage(int n) => 1f + Mathf.Floor((n - 1) / 8f) * 0.5f;

        #endregion

        [MenuItem("Tools/Dustwater/Content/Build 30 Missions", priority = 210)]
        public static void BuildAll()
        {
            LevelsDatabase database = AssetDatabase.LoadAssetAtPath<LevelsDatabase>(DustwaterContentBuilder.LEVELS_DATABASE);
            SerializedObject databaseSo = new SerializedObject(database);
            SerializedProperty levels = databaseSo.FindProperty("levels");

            foreach (Mission mission in MISSIONS)
            {
                string path = $"{LEVELS_FOLDER}/Level {mission.number:000}.asset";
                LevelData level = AssetDatabase.LoadAssetAtPath<LevelData>(path);
                if (level == null)
                {
                    level = ScriptableObject.CreateInstance<LevelData>();
                    AssetDatabase.CreateAsset(level, path);
                }

                Write(level, mission);

                if (levels.arraySize < mission.number) levels.arraySize = mission.number;
                levels.GetArrayElementAtIndex(mission.number - 1).objectReferenceValue = level;
            }

            levels.arraySize = MISSIONS.Length;
            databaseSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            Debug.Log("[Dustwater] " + MISSIONS.Length + " missions written");
        }

        private static void Write(LevelData level, Mission mission)
        {
            int n = mission.number;
            System.Random random = new System.Random(n * 7919 + 17);
            Content content = new Content();

            int rows = mission.boss ? 13 : Mathf.Min(13, 9 + (n + 2) / 5);
            List<string> plan = Plan(mission, rows, random);

            for (int row = 0; row < plan.Count; row++)
                BuildRow(content, plan[row], FIRST_ROW_Z + row * ROW_STEP, n, mission, random);

            float finishZ = FIRST_ROW_Z + plan.Count * ROW_STEP + 2f;

            SerializedObject so = new SerializedObject(level);
            so.FindProperty("note").stringValue = "Mission " + n + ": " + mission.operation;
            so.FindProperty("movementMode").intValue = (int)mission.mode;
            so.FindProperty("cameraDataId").stringValue = "desert_camera";
            so.FindProperty("rewardCurrency").intValue = (int)CurrencyType.Money;
            so.FindProperty("rewardAmount").intValue = (20 + 4 * (n - 1)) * (mission.boss ? 2 : 1);
            so.FindProperty("playerSpawnPositionZ").floatValue = 5f;
            so.FindProperty("characterType").intValue = (int)CharacterType.Humanoid;
            so.FindProperty("operationName").stringValue = mission.operation;
            so.FindProperty("isBossLevel").boolValue = mission.boss;

            SerializedProperty threats = so.FindProperty("threats");
            threats.arraySize = mission.threats.Length;
            for (int i = 0; i < mission.threats.Length; i++)
                threats.GetArrayElementAtIndex(i).intValue = (int)mission.threats[i];

            Fill(so.FindProperty("gatesData"), content.gates);
            Fill(so.FindProperty("boostersData"), content.boosters);
            Fill(so.FindProperty("obstaclesData"), content.obstacles);
            Fill(so.FindProperty("enemiesData"), content.enemies);

            string road = DustwaterTownBuilder.RoadIdForMission(n);
            string environment = DustwaterTownBuilder.EnvironmentIdForMission(n);

            SerializedProperty finish = so.FindProperty("finishLevelData");
            finish.arraySize = 1;
            Base(finish.GetArrayElementAtIndex(0), "finish", new Vector3(0f, 0f, finishZ));

            SerializedProperty roads = so.FindProperty("roadsData");
            int roadCount = Mathf.CeilToInt((finishZ + 60f) / 10f) + 1;
            roads.arraySize = roadCount;
            for (int i = 0; i < roadCount; i++)
                Base(roads.GetArrayElementAtIndex(i), road, new Vector3(0f, 0f, (roadCount - 1 - i) * 10f));

            SerializedProperty environments = so.FindProperty("environmentLevelData");
            int environmentCount = Mathf.CeilToInt((finishZ + 40f) / 40f) + 1;
            environments.arraySize = environmentCount;
            for (int i = 0; i < environmentCount; i++)
                Base(environments.GetArrayElementAtIndex(i), environment, new Vector3(0f, 0f, (environmentCount - 1 - i) * 40f));

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(level);
        }

        private static void Fill(SerializedProperty list, List<System.Action<SerializedProperty>> items)
        {
            list.arraySize = items.Count;
            for (int i = 0; i < items.Count; i++)
                items[i](list.GetArrayElementAtIndex(i));
        }

        #region Planning

        // Row kinds: easy, barrels, gates, crate, armored, swarm, sniper, wall, ambush, mixed, boss
        private static List<string> Plan(Mission mission, int rows, System.Random random)
        {
            List<string> plan = new List<string>();
            for (int i = 0; i < rows; i++) plan.Add(null);

            plan[0] = "easy";
            plan[1] = "gates";
            int lastIndex = rows - 1;
            if (mission.boss)
            {
                plan[lastIndex] = "boss";
                plan[lastIndex - 1] = "gates";
                lastIndex -= 2;
            }

            // gates every ~3 rows, two supply crates
            for (int i = 4; i <= lastIndex; i += 3) plan[i] = "gates";
            Place(plan, "crate", 2, 2, lastIndex, random);

            // threat rows: ~3 per threat, early missions fewer
            int perThreat = mission.threats.Length >= 3 ? 2 : mission.number <= 3 ? 2 : 3;
            foreach (ThreatType threat in mission.threats)
            {
                string kind = threat switch
                {
                    ThreatType.Armored => "armored",
                    ThreatType.Swarm => "swarm",
                    ThreatType.Snipers => "sniper",
                    ThreatType.Barricades => "wall",
                    _ => "ambush",
                };
                Place(plan, kind, perThreat, 2, lastIndex, random);
            }

            for (int i = 0; i < plan.Count; i++)
                if (plan[i] == null) plan[i] = mission.number <= 2 || random.NextDouble() < 0.45 ? "barrels" : "mixed";

            return plan;
        }

        private static void Place(List<string> plan, string kind, int count, int from, int to, System.Random random)
        {
            List<int> free = new List<int>();
            for (int i = from; i <= to; i++) if (plan[i] == null) free.Add(i);

            for (int k = 0; k < count && free.Count > 0; k++)
            {
                // spread rows along the road instead of clumping
                int slot = Mathf.Clamp(Mathf.RoundToInt((k + 0.5f) / count * free.Count - 0.5f + (float)(random.NextDouble() - 0.5)), 0, free.Count - 1);
                plan[free[slot]] = kind;
                free.RemoveAt(slot);
            }
        }

        #endregion

        #region Rows

        private static void BuildRow(Content c, string kind, float z, int n, Mission mission, System.Random r)
        {
            float damage = EnemyDamage(n);
            bool attrition = System.Array.IndexOf(mission.threats, ThreatType.Attrition) >= 0;
            float shooterRate = attrition ? 0.65f : 0.5f;

            switch (kind)
            {
                case "easy":
                    Obstacle(c, "small_obstacle", new Vector3(-1.5f, 0f, z), Mathf.Max(2, Barrel(n, r) / 2), DropableItemType.Money, CurrencyType.Money, 2, 1);
                    Obstacle(c, "small_obstacle", new Vector3(1.5f, 0f, z), Mathf.Max(2, Barrel(n, r) / 2), DropableItemType.Money, CurrencyType.Money, 2, 1);
                    break;

                case "barrels":
                {
                    bool boosterOnTop = r.NextDouble() < 0.5;
                    float x = r.NextDouble() < 0.5 ? -1.5f : 1.5f;
                    Obstacle(c, "small_obstacle", new Vector3(x, 0f, z), Barrel(n, r), DropableItemType.Money, CurrencyType.Money, 2, 1 + n / 10);
                    Obstacle(c, "small_obstacle", new Vector3(-x, 0f, z), Barrel(n, r), r.NextDouble() < 0.3 ? DropableItemType.Heal : DropableItemType.Money, CurrencyType.Money, 1, 2);
                    if (boosterOnTop) RandomBooster(c, new Vector3(x, 1f, z), n, r);
                    break;
                }

                case "mixed":
                {
                    float x = r.NextDouble() < 0.5 ? -1.5f : 1.5f;
                    Obstacle(c, "small_obstacle", new Vector3(x, 0f, z), Barrel(n, r), DropableItemType.Money, CurrencyType.Money, 2, 1 + n / 10);
                    Enemy(c, "enemy", new Vector3(-x, 0f, z + 2f), Bandit(n), damage, shooterRate, r.NextDouble() < 0.6 ? "default_gun" : "none");
                    break;
                }

                case "crate":
                {
                    float x = r.NextDouble() < 0.5 ? -1.5f : 1.5f;
                    Obstacle(c, "supply_crate", new Vector3(x, 0f, z), Crate(n), DropableItemType.Money, CurrencyType.Supplies, 3 + n / 5, 1);
                    Obstacle(c, "small_obstacle", new Vector3(-x, 0f, z), Barrel(n, r), DropableItemType.Money, CurrencyType.Money, 2, 1);
                    break;
                }

                case "gates":
                    GatePair(c, z, n, mission, r);
                    break;

                case "armored":
                {
                    // a full heavy and a lighter one: shred one of them in time
                    bool strongLeft = r.NextDouble() < 0.5;
                    Enemy(c, "enemy_heavy", new Vector3(-1.5f, 0f, z), Heavy(n) * (strongLeft ? 1f : 0.6f), damage * 1.2f, 0.6f, strongLeft ? "minigun" : "none", DropableItemType.Money, 3, 1 + n / 10);
                    Enemy(c, "enemy_heavy", new Vector3(1.5f, 0f, z), Heavy(n) * (strongLeft ? 0.6f : 1f), damage * 1.2f, 0.6f, strongLeft ? "none" : "minigun", DropableItemType.Money, 3, 1 + n / 10);
                    break;
                }

                case "swarm":
                    for (int i = 0; i < 5; i++)
                    {
                        float x = -2f + i;
                        Enemy(c, "enemy", new Vector3(x, 0f, z + (i % 2) * 1.6f), Bandit(n) * 0.6f, damage, 0.45f, i % 2 == 0 ? "none" : "default_gun", DropableItemType.Money, 1, 1);
                    }
                    if (n >= 12)
                        for (int i = 0; i < 4; i++)
                            Enemy(c, "enemy", new Vector3(-1.5f + i, 0f, z + 4.5f), Bandit(n) * 0.6f, damage, 0.45f, "none", DropableItemType.None, 0, 0);
                    break;

                case "sniper":
                {
                    // nest: sandbags in front, the sniper behind them; the other lane stays open
                    float x = r.NextDouble() < 0.5 ? -1.5f : 1.5f;
                    Obstacle(c, "sandbags", new Vector3(x, 0f, z), Sandbags(n) / 3, DropableItemType.Money, CurrencyType.Supplies, 1, 1);
                    Enemy(c, "bandit_sniper", new Vector3(x, 0f, z + 3.5f), Sniper(n), damage * 1.6f, 0.45f, "bandit_sniper", DropableItemType.Money, 2, 2);
                    if (n >= 18) Enemy(c, "bandit_sniper", new Vector3(-x, 0f, z + 6f), Sniper(n), damage * 1.6f, 0.45f, "bandit_sniper", DropableItemType.Money, 2, 2);
                    break;
                }

                case "wall":
                {
                    // one side is a full wall, the other a lower stack: blast through or pick the weak side
                    bool strongLeft = r.NextDouble() < 0.5;
                    Obstacle(c, "sandbags", new Vector3(-1.5f, 0f, z), strongLeft ? Sandbags(n) : Sandbags(n) / 2, DropableItemType.Money, CurrencyType.Supplies, strongLeft ? 4 : 2, 1);
                    Obstacle(c, "sandbags", new Vector3(1.5f, 0f, z), strongLeft ? Sandbags(n) / 2 : Sandbags(n), DropableItemType.Money, CurrencyType.Supplies, strongLeft ? 2 : 4, 1);
                }
                    if (n >= 7) Enemy(c, "enemy", new Vector3(0f, 0f, z + 3f), Bandit(n), damage, shooterRate, "default_gun");
                    break;

                case "ambush":
                    // shooters on the flanks, one more further back from mission 15
                    // aimed rifles: they hit the squad wherever it runs
                    Enemy(c, "enemy", new Vector3(-2f, 0f, z), Bandit(n), damage, 0.6f, "bandit_rifle");
                    Enemy(c, "enemy", new Vector3(2f, 0f, z), Bandit(n), damage, 0.6f, "bandit_rifle");
                    if (n >= 15) Enemy(c, "enemy", new Vector3(0f, 0f, z + 5f), Bandit(n) * 0.7f, damage, 0.6f, "bandit_rifle");
                    break;

                case "boss":
                {
                    var boss = BOSSES[mission.number];
                    Enemy(c, boss.id, new Vector3(0f, 0f, z + 4f), boss.health, boss.damage, boss.fireRate, "boss_minigun", DropableItemType.Money, 10, 3);
                    Enemy(c, "enemy_heavy", new Vector3(-2f, 0f, z), Heavy(n) * 0.8f, damage, 0.6f, "none", DropableItemType.Money, 2, 2);
                    Enemy(c, "enemy_heavy", new Vector3(2f, 0f, z), Heavy(n) * 0.8f, damage, 0.6f, "none", DropableItemType.Money, 2, 2);
                    break;
                }
            }
        }

        // Left/right choice. One side is usually the better pick; some pairs hide a trap.
        private static void GatePair(Content c, float z, int n, Mission mission, System.Random r)
        {
            float growth = 1f + n * 0.04f;
            int roll = r.Next(6);
            bool flip = r.NextDouble() < 0.5;
            float left = flip ? 1.5f : -1.5f;
            float right = -left;

            switch (roll)
            {
                case 0:
                    Gate(c, new Vector3(left, 0f, z), GateType.GiveCharacter, OperationType.Plus, 1, "default_character", n >= 15 ? 2 : 1);
                    Gate(c, new Vector3(right, 0f, z), GateType.Damage, OperationType.Plus, Round(0.3f * growth));
                    break;
                case 1:
                    Gate(c, new Vector3(left, 0f, z), GateType.FireRate, OperationType.Multiply, Round(1.15f + 0.01f * n));
                    Gate(c, new Vector3(right, 0f, z), GateType.Range, OperationType.Plus, Round(1.5f + 0.05f * n));
                    break;
                case 2:
                    Gate(c, new Vector3(left, 0f, z), GateType.GiveWeapon, OperationType.Plus, 1, CounterWeapon(mission, r), 1, false);
                    Gate(c, new Vector3(right, 0f, z), GateType.Health, OperationType.Plus, Mathf.Round(3f * growth));
                    break;
                case 3:
                    Gate(c, new Vector3(left, 0f, z), GateType.Damage, OperationType.Multiply, Round(1.2f + 0.008f * n));
                    Gate(c, new Vector3(right, 0f, z), GateType.Money, OperationType.Plus, Mathf.Round(8f * growth));
                    break;
                case 4:
                    // trap: a shiny minus on one side
                    Gate(c, new Vector3(left, 0f, z), GateType.Damage, OperationType.Minus, Round(0.4f * growth));
                    Gate(c, new Vector3(right, 0f, z), GateType.GiveCharacter, OperationType.Plus, 1, n >= 8 && r.NextDouble() < 0.4 ? "heavy_character" : "default_character", 1);
                    break;
                default:
                    Gate(c, new Vector3(left, 0f, z), GateType.FireRate, OperationType.Plus, Round(0.4f * growth));
                    Gate(c, new Vector3(right, 0f, z), GateType.Health, OperationType.Multiply, Round(1.3f));
                    break;
            }
        }

        private static string CounterWeapon(Mission mission, System.Random r)
        {
            foreach (ThreatType threat in mission.threats)
            {
                if (threat == ThreatType.Barricades) return "shotgun";
                if (threat == ThreatType.Armored) return "rifle";
                if (threat == ThreatType.Swarm) return "minigun";
            }

            string[] any = { "rifle", "shotgun", "minigun" };
            return any[r.Next(any.Length)];
        }

        private static void RandomBooster(Content c, Vector3 position, int n, System.Random r)
        {
            switch (r.Next(4))
            {
                case 0: Booster(c, position, BoosterType.Damage, OperationType.Plus, Round(0.2f + 0.02f * n)); break;
                case 1: Booster(c, position, BoosterType.FireRate, OperationType.Plus, Round(0.3f + 0.02f * n)); break;
                case 2: Booster(c, position, BoosterType.Health, OperationType.Plus, Mathf.Round(3f + 0.2f * n)); break;
                default: Booster(c, position, BoosterType.GiveCharacter, OperationType.Plus, 1, "default_character", 1); break;
            }
        }

        private static float Round(float value) => Mathf.Round(value * 10f) / 10f;

        #endregion
    }
}
