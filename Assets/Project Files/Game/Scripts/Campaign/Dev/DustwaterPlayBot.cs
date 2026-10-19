#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// Balance bot: plays missions with the expected progression of a player who reached them
    /// (squad slots, building levels, card levels, counter picks) and logs the outcome to Temp/playbot.log.
    /// A mission counts as won when the squad crosses the finish line. Run from play mode:
    /// DustwaterPlayBot.Launch(1, 30, 4f)
    /// </summary>
    public class DustwaterPlayBot : MonoBehaviour
    {
        private static readonly float[] LANES = { -2.2f, -1.5f, 0f, 1.5f, 2.2f };

        private int from;
        private int to;
        private float timeScale;
        private string logPath;

        public static DustwaterPlayBot Launch(int fromMission, int toMission, float timeScale = 4f)
        {
            DustwaterPlayBot bot = new GameObject("Dustwater Play Bot").AddComponent<DustwaterPlayBot>();
            bot.from = fromMission;
            bot.to = toMission;
            bot.timeScale = timeScale;
            bot.logPath = Path.Combine(Application.dataPath, "../Temp/playbot.log");
            Campaign.IsIntroSeen = true;
            File.WriteAllText(bot.logPath, "");
            bot.StartCoroutine(bot.Run());
            return bot;
        }

        private void Log(string line)
        {
            File.AppendAllText(logPath, line + "\n");
            Debug.Log("[PlayBot] " + line);
        }

        private IEnumerator Run()
        {
            for (int mission = from; mission <= to; mission++)
            {
                SetupProgress(mission);
                yield return PlayMission(mission);
            }

            Time.timeScale = 1f;
            Log("DONE");
            Destroy(gameObject);
        }

        #region Progression

        private static void SetupProgress(int mission)
        {
            int completed = mission - 1;
            CampaignDatabase db = Campaign.Database;

            int[] buildings = new int[db.Buildings.Count];
            for (int i = 0; i < db.Buildings.Count; i++)
                buildings[i] = BotBuildingLevel(db.Buildings[i], completed);

            Dictionary<string, int> units = new Dictionary<string, int>();
            foreach (UnitCardData unit in db.Units)
            {
                if (unit.unlockAfterLevel > completed) continue;
                units[unit.id] = unit.isFiller ? Mathf.Min(15, 1 + completed / 4) : Mathf.Min(15, 1 + (completed - unit.unlockAfterLevel) / 5);
            }

            Dictionary<string, int> weapons = new Dictionary<string, int>();
            foreach (WeaponCardData weapon in db.Weapons)
            {
                if (weapon.unlockAfterLevel > completed) continue;
                weapons[weapon.id] = Mathf.Min(15, 1 + (completed - weapon.unlockAfterLevel) / 4);
            }

            Campaign.DevSetProgress(completed, buildings, units, weapons);

            // squad: counters for the mission's threats first, then the toughest units, then Rangers
            LevelData level = GetLevel(mission);
            List<ThreatType> threats = level != null ? level.Threats : new List<ThreatType>();
            List<string> squad = new List<string>();
            foreach (ThreatType threat in threats)
                foreach (UnitCardData unit in db.Units)
                    if (!unit.isFiller && units.ContainsKey(unit.id) && unit.counters.Contains(threat) && !squad.Contains(unit.id) && squad.Count < Campaign.SquadSlots)
                    {
                        squad.Add(unit.id);
                        break;
                    }

            foreach (string preferred in new[] { "guardian", "heavy", "medic", "pyro", "scout", "sharpshooter", "grenadier" })
                if (units.ContainsKey(preferred) && !squad.Contains(preferred) && squad.Count < Campaign.SquadSlots - 1) squad.Add(preferred);

            while (squad.Count < Campaign.SquadSlots) squad.Add(db.StarterUnitId);
            Campaign.SetSquad(squad);

            string weaponId = "pistol";
            foreach (string candidate in new[] { "shotgun", "rifle", "minigun", "burst_rifle" })
                if (weapons.ContainsKey(candidate)) weaponId = candidate;
            foreach (ThreatType threat in threats)
                foreach (WeaponCardData weapon in db.Weapons)
                    if (weapons.ContainsKey(weapon.id) && weapon.counters.Contains(threat)) { weaponId = weapon.id; goto picked; }
            picked:
            Campaign.SetWeapon(weaponId);
        }

        // a player who spends on the town as soon as plots open (no hospital or saloon/tavern)
        private static int BotBuildingLevel(BuildingData building, int completed)
        {
            if (building.act == 0)
            {
                switch (building.type)
                {
                    case BuildingType.CommandPost: return 1 + (completed >= 3 ? 1 : 0) + (completed >= 8 ? 1 : 0) + (completed >= 16 ? 1 : 0);
                    case BuildingType.Barracks: return completed >= 2 ? Mathf.Min(5, 1 + (completed - 2) / 5) : 0;
                    case BuildingType.Armory: return completed >= 4 ? Mathf.Min(5, 1 + (completed - 4) / 5) : 0;
                    case BuildingType.Workshop: return completed >= 7 ? Mathf.Min(5, 1 + (completed - 7) / 5) : 0;
                    case BuildingType.Watchtower: return completed >= 20 ? Mathf.Min(3, 1 + (completed - 20) / 4) : 0;
                    default: return 0;
                }
            }

            if (completed < building.unlockAfterLevel || building.type == BuildingType.FieldHospital || building.type == BuildingType.Saloon) return 0;
            int step = building.type == BuildingType.CommandPost ? 14 : 8;
            return Mathf.Min(building.levels.Count, 1 + (completed - building.unlockAfterLevel) / step);
        }

        private static LevelData GetLevel(int mission)
        {
            List<LevelData> levels = LevelController.Database.Levels;
            return mission - 1 < levels.Count ? levels[mission - 1] : null;
        }

        #endregion

        #region Play

        private IEnumerator PlayMission(int mission)
        {
            ForceGameplayInactive();
            UIController.HidePage<UIGame>();
            UIController.HidePage<UIGameOver>();
            UIController.HidePage<UIComplete>();
            GameController.ReturnToMainMenu();
            yield return new WaitForSecondsRealtime(0.3f);

            LevelData level = LevelController.LevelData;
            int suppliesBefore = CurrencyController.Get(CurrencyType.Supplies);
            GameController.StartLevel();
            Time.timeScale = timeScale;

            PlayerBehavior player = PlayerBehavior.Instance;
            int startSquad = player.Characters.Count;

            // damage breakdown: contact (hit point = the character) vs bullets
            float contactDamage = 0f, bulletDamage = 0f;
            System.Action<CharacterBehavior, float, Vector3> onDamaged = (character, amount, point) =>
            {
                if (character.IsImmortal) return;
                if ((point - character.transform.position).sqrMagnitude < 0.0001f) contactDamage += amount;
                else bulletDamage += amount;
            };
            CharacterBehavior.Damaged += onDamaged;
            float startTime = Time.time;
            float bossTime = 0f;
            string result = "TIMEOUT";

            while (Time.time - startTime < 180f)
            {
                if (LevelController.StageId == 2) { result = "WIN"; break; }
                if (player.Characters.Count == 0) { result = "FAIL"; break; }

                if (MovementManager.AdvanceMultiplier < 0.5f) bossTime += Time.deltaTime;
                Steer(player);

                float z = player.transform.position.z;
                if (mission == TraceMission && z >= TraceFromZ && z <= TraceToZ && Time.time - lastTrace > 0.2f)
                {
                    lastTrace = Time.time;
                    Trace(player);
                }
                yield return null;
            }

            CharacterBehavior.Damaged -= onDamaged;

            float hp = 0f, maxHp = 0f;
            foreach (CharacterBehavior character in player.Characters)
            {
                hp += character.Health.CurrentHealth;
                maxHp += character.Health.MaxHealth;
            }

            Log($"M{mission:00} {level.OperationName,-20} {string.Join("+", level.Threats),-30} {result,-7} squad {startSquad}->{player.Characters.Count} hp {hp:0}/{maxHp:0} t {Time.time - startTime:0.0}s boss {bossTime:0.0}s supplies +{CurrencyController.Get(CurrencyType.Supplies) - suppliesBefore} dmg contact {contactDamage:0} bullets {bulletDamage:0} at z {player.transform.position.z:0} squad[{string.Join(",", Campaign.Squad)}] gun {Campaign.SelectedWeaponId}");

            Time.timeScale = 1f;
            yield return new WaitForSecondsRealtime(0.2f);
        }

        public static int TraceMission = -1;
        public static float TraceFromZ, TraceToZ;
        private float lastTrace;

        private void Trace(PlayerBehavior player)
        {
            float z = player.transform.position.z;
            string line = $"  z{z:0.0} [";
            foreach (CharacterBehavior c in player.Characters)
                line += $"{c.Data.Id.Substring(0, 2)}{c.Health.CurrentHealth:0}@{c.transform.position.x:0.0} ";
            line += "]";
            foreach (EnemyBehavior e in FindObjectsByType<EnemyBehavior>(FindObjectsSortMode.None))
            {
                float dz = e.transform.position.z - z;
                if (dz > -2f && dz < 18f && e.Health != null) line += $" E({e.transform.position.x:0.0},dz{dz:0},hp{e.Health.CurrentHealth:0})";
            }
            foreach (ObstacleBehavior o in FindObjectsByType<ObstacleBehavior>(FindObjectsSortMode.None))
            {
                float dz = o.transform.position.z - z;
                if (dz > -2f && dz < 18f) line += $" O({o.transform.position.x:0.0},dz{dz:0},hp{o.Health:0})";
            }
            foreach (GateBehavior g in FindObjectsByType<GateBehavior>(FindObjectsSortMode.None))
            {
                float dz = g.transform.position.z - z;
                if (dz > -2f && dz < 18f) line += $" G({g.transform.position.x:0.0},dz{dz:0},{g.Data.GateType})";
            }
            Log(line);
        }

        private static void ForceGameplayInactive()
        {
            PropertyInfo property = typeof(GameController).GetProperty("IsGameplayActive", BindingFlags.Public | BindingFlags.Static);
            property.SetValue(null, false);
        }

        private static readonly FieldInfo MODE_FIELD = typeof(MovementManager).GetField("currentMovementMode", BindingFlags.NonPublic | BindingFlags.Static);

        // pick the lane with the best upcoming gates and the least HP in the way
        private void Steer(PlayerBehavior player)
        {
            MovementMode mode = MODE_FIELD.GetValue(null) as MovementMode;
            if (mode == null) return;

            float playerZ = player.transform.position.z;
            float bestScore = float.MinValue;
            float bestX = 0f;

            // boss standoff: line up with the boss
            if (BossBehavior.Active != null && MovementManager.AdvanceMultiplier < 0.5f)
            {
                float target = BossBehavior.Active.transform.position.x;
                mode.ProcessPointerInput(new Vector2(Mathf.Clamp(target - mode.Position.x, -10f * Time.deltaTime, 10f * Time.deltaTime), 0f));
                return;
            }

            float dps = 0f;
            foreach (CharacterBehavior character in player.Characters) dps += character.Damage * character.FireRate;

            foreach (float lane in LANES)
            {
                float score = -Mathf.Abs(lane - mode.Position.x) * 0.5f;

                foreach (ObstacleBehavior obstacle in FindObjectsByType<ObstacleBehavior>(FindObjectsSortMode.None))
                    score -= Threat(obstacle.transform.position, obstacle.Health, lane, playerZ, dps);

                foreach (EnemyBehavior enemy in FindObjectsByType<EnemyBehavior>(FindObjectsSortMode.None))
                    if (enemy.Health != null && !enemy.Health.IsDepleted) score -= Threat(enemy.transform.position, enemy.Health.CurrentHealth, lane, playerZ, dps);

                foreach (GateBehavior gate in FindObjectsByType<GateBehavior>(FindObjectsSortMode.None))
                {
                    float dz = gate.transform.position.z - playerZ;
                    if (dz < 0.5f || dz > 14f || Mathf.Abs(gate.transform.position.x - lane) > 1.2f) continue;

                    float value = gate is NumericalGateBehavior numerical ? (numerical.IsPositive ? 3f : -6f) : 5f;
                    score += value * 4f / Mathf.Max(1f, dz);
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestX = lane;
                }
            }

            float step = Mathf.Clamp(bestX - mode.Position.x, -10f * Time.deltaTime, 10f * Time.deltaTime);
            mode.ProcessPointerInput(new Vector2(step, 0f));
        }

        // HP that will still be standing when the squad reaches it in this lane (aligned targets get shot)
        private static float Threat(Vector3 position, float health, float lane, float playerZ, float dps)
        {
            float dz = position.z - playerZ;
            float dx = Mathf.Abs(position.x - lane);
            if (dz < 0.5f || dz > 18f || health <= 0f || dx > 2.0f) return 0f;

            float shotDown = dx < 0.8f ? dps * dz / 4f * 0.8f : 0f;
            return Mathf.Max(0f, health - shotDown) / Mathf.Max(1f, dz * 0.4f) + (dx < 0.8f ? 0f : 0.5f);
        }

        #endregion
    }
}
#endif
