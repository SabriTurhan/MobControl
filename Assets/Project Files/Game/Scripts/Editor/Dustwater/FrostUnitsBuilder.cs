using System.IO;
using UnityEditor;
using UnityEngine;
using static SquadCommand.Dustwater.DustwaterUnitsBuilder;

namespace SquadCommand.Dustwater
{
    // Act 2 (Frostpeak) units: Pyro / Guardian / Scout cards, the Burst Rifle and Rocket Launcher weapon
    // cards, the White Wolves (wolf, wolf heavy, wolf sniper), seven bosses and the icy obstacles.
    // Prefab variants of the template prefabs, like DustwaterUnitsBuilder.
    public static class FrostUnitsBuilder
    {
        private const string TEMPLATE = "Assets/Project Files/Game/Prefabs/";
        private const string SOLDIER_PREFAB = TEMPLATE + "Player/Humanoid Characters/Default Humanoid Character.prefab";
        private const string HEAVY_PREFAB = TEMPLATE + "Player/Humanoid Characters/Heavy Humanoid Character.prefab";
        private const string ENEMY_PREFAB = TEMPLATE + "Enemy/Enemy Human Behavior.prefab";
        private const string ENEMY_HEAVY_PREFAB = TEMPLATE + "Enemy/Enemy Heavy Human Behavior.prefab";
        private const string RIFLE_PREFAB = TEMPLATE + "Guns/Rifle.prefab";
        private const string BULLET_PREFAB = TEMPLATE + "Player/Bullet/Bullet.prefab";
        private const string BARREL_PREFAB = TEMPLATE + "Gameplay/Obstacles/Barrel Obstacle.prefab";

        private const string SOLDIER_BLUE = "Assets/Project Files/Game/Models/Basic/soldier_blue.fbx";
        private const string SOLDIER_BIG_BLUE = "Assets/Project Files/Game/Models/Basic/soldier_big_blue.fbx";
        private const string SOLDIER_RED = "Assets/Project Files/Game/Models/Basic/soldier_red.fbx";
        private const string SOLDIER_BIG_RED = "Assets/Project Files/Game/Models/Basic/soldier_big_red.fbx";

        public const string PYRO = PREFABS + "/Characters/Pyro Character.prefab";
        public const string GUARDIAN = PREFABS + "/Characters/Guardian Character.prefab";
        public const string SCOUT = PREFABS + "/Characters/Scout Character.prefab";

        public const string FLAMETHROWER = PREFABS + "/Guns/Flamethrower.prefab";
        public const string BURST_RIFLE = PREFABS + "/Guns/Burst Rifle.prefab";
        public const string ROCKET_LAUNCHER = PREFABS + "/Guns/Rocket Launcher.prefab";
        public const string FLAME_BULLET = PREFABS + "/Guns/Flame.prefab";
        public const string ROCKET_BULLET = PREFABS + "/Guns/Rocket.prefab";

        public const string WOLF = PREFABS + "/Enemies/Wolf.prefab";
        public const string WOLF_HEAVY = PREFABS + "/Enemies/Wolf Heavy.prefab";
        public const string WOLF_SNIPER = PREFABS + "/Enemies/Wolf Sniper.prefab";

        public const string ICE_WALL = PREFABS + "/Obstacles/Ice Wall.prefab";
        public const string SNOW_BARREL = PREFABS + "/Obstacles/Snow Barrel.prefab";

        // id, prefab, display name (boss bar), body dye, scale
        public static readonly (string id, string path, string name, int dye, float scale)[] BOSSES =
        {
            ("boss_frostbite", PREFABS + "/Enemies/Boss Frostbite.prefab", "SGT. FROSTBITE", SoldierDyes.BOSS_GLACIER, 1.75f),
            ("boss_olaf", PREFABS + "/Enemies/Boss Olaf.prefab", "ICE HAMMER OLAF", SoldierDyes.BOSS_CRIMSON, 1.85f),
            ("boss_timber", PREFABS + "/Enemies/Boss Big Timber.prefab", "BIG TIMBER", SoldierDyes.BOSS_TIMBER, 2.0f),
            ("boss_snowjaw", PREFABS + "/Enemies/Boss Snowjaw.prefab", "MAYOR SNOWJAW", SoldierDyes.BOSS_LAVENDER, 1.85f),
            ("boss_crag", PREFABS + "/Enemies/Boss Crag.prefab", "FOREMAN CRAG", SoldierDyes.BOSS_CRYSTAL, 1.9f),
            ("boss_grimfang", PREFABS + "/Enemies/Boss Grimfang.prefab", "WARLORD GRIMFANG", SoldierDyes.BOSS_NIGHT, 2.05f),
            ("boss_vex_frost", PREFABS + "/Enemies/Boss Vex Frost.prefab", "BOSS VEX", SoldierDyes.BOSS_BLACK, 2.2f),
        };

        public static string BossPath(string id) => System.Array.Find(BOSSES, b => b.id == id).path;

        private static readonly Vector3 HEAVY_HAT_OFFSET = new Vector3(0f, -0.07f, 0f);

        [MenuItem("Tools/Dustwater/Art/Build Frostpeak Units, Weapons, Props", priority = 111)]
        public static void BuildAll()
        {
            SoldierDyes.Paint();
            DustwaterPalette.Generate();

            foreach (string folder in new[] { MODELS, PREFABS + "/Characters", PREFABS + "/Guns", PREFABS + "/Enemies", PREFABS + "/Obstacles" })
                Directory.CreateDirectory(folder);

            BuildGuns();
            BuildCharacters();
            BuildEnemies();
            BuildObstacles();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Dustwater] Frostpeak units, weapons and props built");
        }

        #region Guns

        private static void BuildGuns()
        {
            Mesh flameMesh = SaveMesh(FrostProps.FlameBall(), "Bullet_Flame");
            GameObject flame = Variant(BULLET_PREFAB, FLAME_BULLET, root =>
            {
                root.name = "Flame";
                Attach(root.transform, "Flame", flameMesh, DustwaterPalette.GlowMaterial);
                TrailRenderer trail = root.GetComponentInChildren<TrailRenderer>(true);
                if (trail != null) trail.enabled = false;
            });

            Mesh rocketMesh = SaveMesh(FrostProps.Rocket(), "Bullet_Rocket");
            GameObject rocket = Variant(BULLET_PREFAB, ROCKET_BULLET, root =>
            {
                root.name = "Rocket";
                Attach(root.transform, "Rocket", rocketMesh, DustwaterPalette.GlowMaterial);
                BoxCollider collider = root.GetComponent<BoxCollider>();
                if (collider != null) collider.size = new Vector3(0.4f, 1.0f, 0.6f);
            });

            BuildGun(FLAMETHROWER, "Flamethrower", SaveMesh(FrostProps.Flamethrower(), "Gun_Flamethrower"), new Vector3(0f, 0.04f, 0.66f), flame);
            BuildGun(BURST_RIFLE, "Burst Rifle", SaveMesh(FrostProps.BurstRifle(), "Gun_BurstRifle"), new Vector3(0f, 0.05f, 0.64f), null);
            BuildGun(ROCKET_LAUNCHER, "Rocket Launcher", SaveMesh(FrostProps.RocketLauncher(), "Gun_RocketLauncher"), new Vector3(0f, 0.08f, 0.66f), rocket);
        }

        private static void BuildGun(string path, string name, Mesh mesh, Vector3 shootPoint, GameObject bulletPrefab)
        {
            Variant(RIFLE_PREFAB, path, root =>
            {
                root.name = name;
                Transform model = root.transform.Find("rifle");
                model.name = "model";
                model.GetComponent<MeshFilter>().sharedMesh = mesh;
                model.GetComponent<MeshRenderer>().sharedMaterial = DustwaterPalette.GlowMaterial;

                model.Find("Shooting Point").localPosition = shootPoint;
                Transform muzzle = root.transform.Find("Muzzle");
                if (muzzle != null) muzzle.localPosition = shootPoint + new Vector3(0f, 0f, 0.04f);

                if (bulletPrefab != null)
                {
                    SerializedObject gun = new SerializedObject(root.GetComponent<GunBehavior>());
                    gun.FindProperty("bulletPrefab").objectReferenceValue = bulletPrefab;
                    gun.ApplyModifiedPropertiesWithoutUndo();
                }
            });
        }

        #endregion

        #region Characters

        private static void BuildCharacters()
        {
            Mesh soldierBlue = LoadMesh(SOLDIER_BLUE);
            Mesh soldierBigBlue = LoadMesh(SOLDIER_BIG_BLUE);

            Mesh pyroBody = SaveMesh(SoldierDyes.Redye(soldierBlue, SoldierDyes.BLUE, SoldierDyes.PYRO_CHARCOAL, "Soldier_Pyro"), "Soldier_Pyro");
            Mesh guardianBody = SaveMesh(SoldierDyes.Redye(soldierBigBlue, SoldierDyes.BLUE, SoldierDyes.GUARDIAN_STEEL, "Soldier_Guardian"), "Soldier_Guardian");
            Mesh scoutBody = SaveMesh(SoldierDyes.Redye(soldierBlue, SoldierDyes.BLUE, SoldierDyes.SCOUT_SNOW, "Soldier_Scout"), "Soldier_Scout");

            Mesh gasMask = SaveMesh(FrostProps.GasMask(), "Prop_GasMask");
            Mesh tanks = SaveMesh(FrostProps.FuelTanks(), "Prop_FuelTanks");
            Mesh helmet = SaveMesh(FrostProps.SteelHelmet(), "Prop_SteelHelmet");
            Mesh shield = SaveMesh(FrostProps.TowerShield(), "Prop_TowerShield");
            Mesh hood = SaveMesh(FrostProps.FurHood(), "Prop_FurHood");
            Mesh blueScarf = SaveMesh(FrostProps.Scarf(Band.RangerBlue), "Prop_Scarf_Blue");

            // new squad weapons in the hands of every squad-weapon character (ranger, heavy-based guardian)
            foreach (string path in new[] { SOLDIER_PREFAB, HEAVY_PREFAB })
            {
                GameObject contents = PrefabUtility.LoadPrefabContents(path);
                CopyWeaponPlacement(contents.GetComponent<CharacterBehavior>(), "rifle", "burst_rifle");
                CopyWeaponPlacement(contents.GetComponent<CharacterBehavior>(), "rifle", "rocket_launcher");
                PrefabUtility.SaveAsPrefabAsset(contents, path);
                PrefabUtility.UnloadPrefabContents(contents);
            }

            Variant(SOLDIER_PREFAB, PYRO, root =>
            {
                root.name = "Pyro Character";
                SetBody(root, pyroBody);
                AttachToBone(root, "Head", "Gas Mask", gasMask);
                AttachToBone(root, "Spine2", "Fuel Tanks", tanks);
                CopyWeaponPlacement(root.GetComponent<CharacterBehavior>(), "rifle", "flamethrower");
            });

            Variant(HEAVY_PREFAB, GUARDIAN, root =>
            {
                root.name = "Guardian Character";
                SetBody(root, guardianBody);
                AttachToBone(root, "Head", "Helmet", helmet, HEAVY_HAT_OFFSET);
                AttachToBone(root, "Spine2", "Shield", shield, new Vector3(-0.22f, 0f, 0.05f));
            });

            Variant(SOLDIER_PREFAB, SCOUT, root =>
            {
                root.name = "Scout Character";
                SetBody(root, scoutBody);
                AttachToBone(root, "Head", "Fur Hood", hood);
                AttachToBone(root, "Head", "Scarf", blueScarf);
            });
        }

        #endregion

        #region Enemies

        private static void BuildEnemies()
        {
            Mesh soldierRed = LoadMesh(SOLDIER_RED);
            Mesh soldierBigRed = LoadMesh(SOLDIER_BIG_RED);

            Mesh wolfBody = SaveMesh(SoldierDyes.Redye(soldierRed, SoldierDyes.RED, SoldierDyes.WOLF_NAVY, "Soldier_Wolf"), "Soldier_Wolf");
            Mesh wolfHeavyBody = SaveMesh(SoldierDyes.Redye(soldierBigRed, SoldierDyes.RED, SoldierDyes.WOLF_ICE, "Soldier_WolfHeavy"), "Soldier_WolfHeavy");
            Mesh wolfSniperBody = SaveMesh(SoldierDyes.Redye(soldierRed, SoldierDyes.RED, SoldierDyes.WOLF_TEAL, "Soldier_WolfSniper"), "Soldier_WolfSniper");

            Mesh ushanka = SaveMesh(FrostProps.Ushanka(Band.Fur, Band.WolfGrey), "Prop_Ushanka_Wolf");
            Mesh greyScarf = SaveMesh(FrostProps.Scarf(Band.WolfGrey), "Prop_Scarf_Grey");
            Mesh horned = SaveMesh(FrostProps.HornedHelmet(Band.Steel), "Prop_HornedHelmet_Steel");
            Mesh greyCape = SaveMesh(FrostProps.FurCape(Band.WolfGrey), "Prop_FurCape_Grey");
            Mesh wolfHood = SaveMesh(FrostProps.WolfHood(), "Prop_WolfHood");

            Variant(ENEMY_PREFAB, WOLF, root =>
            {
                root.name = "Wolf";
                SetBody(root, wolfBody);
                AttachToBone(root, "Head", "Ushanka", ushanka);
                AttachToBone(root, "Head", "Scarf", greyScarf);
            });

            Variant(ENEMY_HEAVY_PREFAB, WOLF_HEAVY, root =>
            {
                root.name = "Wolf Heavy";
                SetBody(root, wolfHeavyBody);
                AttachToBone(root, "Head", "Horned Helmet", horned, HEAVY_HAT_OFFSET);
                AttachToBone(root, "Spine2", "Cape", greyCape);
            });

            Variant(ENEMY_PREFAB, WOLF_SNIPER, root =>
            {
                root.name = "Wolf Sniper";
                SetBody(root, wolfSniperBody);
                AttachToBone(root, "Head", "Wolf Hood", wolfHood);
                CopyWeaponPlacement(root.GetComponent<EnemyBehavior>(), "rifle", "sniper");
                CopyWeaponPlacement(root.GetComponent<EnemyBehavior>(), "rifle", "bandit_sniper");
                AddSight(root, 30f);
            });

            // bosses: big body in the boss dye plus a signature outfit
            Mesh snowHat = SaveMesh(FrostProps.Ushanka(Band.Snow, Band.IceBlue), "Prop_Ushanka_Frostbite");
            Mesh timberHat = SaveMesh(FrostProps.Ushanka(Band.FurDark, Band.Maroon), "Prop_Ushanka_Timber");
            Mesh whiteBeard = SaveMesh(FrostProps.Beard(Band.Snow), "Prop_Beard_White");
            Mesh redBeard = SaveMesh(FrostProps.Beard(Band.Orange), "Prop_Beard_Red");
            Mesh brownBeard = SaveMesh(FrostProps.Beard(Band.DarkWood), "Prop_Beard_Brown");
            Mesh ironHorns = SaveMesh(FrostProps.HornedHelmet(Band.Iron), "Prop_HornedHelmet_Iron");
            Mesh navyCape = SaveMesh(FrostProps.FurCape(Band.Navy), "Prop_FurCape_Navy");
            Mesh furCape = SaveMesh(FrostProps.FurCape(Band.FurDark), "Prop_FurCape_Fur");
            Mesh crown = SaveMesh(FrostProps.IceCrown(), "Prop_IceCrown");
            Mesh steelHelmet = SaveMesh(FrostProps.SteelHelmet(), "Prop_SteelHelmet");
            Mesh orangeScarf = SaveMesh(FrostProps.Scarf(Band.Orange), "Prop_Scarf_Orange");
            Mesh gasMask = SaveMesh(FrostProps.GasMask(), "Prop_GasMask");
            Mesh steelJaw = SaveMesh(DustwaterProps.SteelJaw(), "Prop_SteelJaw");
            Mesh spikes = SaveMesh(DustwaterProps.ShoulderSpikes(), "Prop_ShoulderSpikes");
            Mesh vexHat = SaveMesh(DustwaterProps.CowboyHat(Band.Black, Band.ScorpionRed, 1.08f), "Prop_CowboyHat_Vex");
            Mesh redBandana = SaveMesh(DustwaterProps.Bandana(Band.ScorpionRed), "Prop_Bandana_Red");
            Mesh badge = SaveMesh(DustwaterProps.ScorpionBadge(), "Prop_ScorpionBadge");

            foreach (var boss in BOSSES)
            {
                Mesh body = SaveMesh(SoldierDyes.Redye(soldierBigRed, SoldierDyes.RED, boss.dye, "Soldier_Boss_" + boss.dye), "Soldier_Boss_" + boss.dye);
                Variant(ENEMY_HEAVY_PREFAB, boss.path, root =>
                {
                    root.name = Path.GetFileNameWithoutExtension(boss.path);
                    SetBody(root, body);
                    switch (boss.id)
                    {
                        case "boss_frostbite":
                            AttachToBone(root, "Head", "Hat", snowHat, HEAVY_HAT_OFFSET);
                            AttachToBone(root, "Head", "Beard", whiteBeard);
                            break;
                        case "boss_olaf":
                            AttachToBone(root, "Head", "Helmet", ironHorns, HEAVY_HAT_OFFSET);
                            AttachToBone(root, "Head", "Beard", redBeard);
                            AttachToBone(root, "Spine2", "Cape", furCape);
                            break;
                        case "boss_timber":
                            AttachToBone(root, "Head", "Hat", timberHat, HEAVY_HAT_OFFSET);
                            AttachToBone(root, "Head", "Beard", brownBeard);
                            break;
                        case "boss_snowjaw":
                            AttachToBone(root, "Head", "Crown", crown, HEAVY_HAT_OFFSET);
                            AttachToBone(root, "Head", "Steel Jaw", steelJaw);
                            AttachToBone(root, "Spine2", "Cape", navyCape);
                            break;
                        case "boss_crag":
                            AttachToBone(root, "Head", "Helmet", steelHelmet, HEAVY_HAT_OFFSET);
                            AttachToBone(root, "Head", "Gas Mask", gasMask);
                            AttachToBone(root, "Head", "Scarf", orangeScarf);
                            break;
                        case "boss_grimfang":
                            AttachToBone(root, "Head", "Wolf Hood", wolfHood, HEAVY_HAT_OFFSET);
                            AttachToBone(root, "Spine2", "Cape", greyCape);
                            AttachToBone(root, "Spine2", "Spikes", spikes);
                            break;
                        default:
                            AttachToBone(root, "Head", "Hat", vexHat, HEAVY_HAT_OFFSET);
                            AttachToBone(root, "Head", "Bandana", redBandana);
                            AttachToBone(root, "Spine2", "Cape", furCape);
                            AttachToBone(root, "Spine2", "Badge", badge);
                            break;
                    }
                    MakeBoss(root, boss.name, boss.scale, 26f);
                });
            }
        }

        #endregion

        #region Obstacles

        private static void BuildObstacles()
        {
            Mesh wall = SaveMesh(FrostProps.IceWall(6, 3), "Obstacle_IceWall");
            Mesh barrel = SaveMesh(FrostProps.IceBarrel(), "Obstacle_SnowBarrel");

            Variant(BARREL_PREFAB, ICE_WALL, root =>
            {
                root.name = "Ice Wall";
                SetObstacleMesh(root, wall, Vector3.zero);
                BoxCollider collider = root.GetComponent<BoxCollider>();
                collider.size = new Vector3(3.0f, 1.2f, 0.8f);
                collider.center = new Vector3(0f, 0.6f, 0f);
                root.transform.Find("Visuals/Canvas").localPosition = new Vector3(0f, 0.75f, -0.3f);
            });

            Variant(BARREL_PREFAB, SNOW_BARREL, root =>
            {
                root.name = "Snow Barrel";
                SetObstacleMesh(root, barrel, Vector3.zero);
            });
        }

        #endregion
    }
}
