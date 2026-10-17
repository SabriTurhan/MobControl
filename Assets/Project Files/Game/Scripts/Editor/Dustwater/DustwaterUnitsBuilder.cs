using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Watermelon.Dustwater
{
    // Builds the new units, weapons, enemies, bosses and level props as prefab variants of the template prefabs,
    // so they keep the template behaviours, animators, particles and health bars.
    public static class DustwaterUnitsBuilder
    {
        public const string MODELS = "Assets/Project Files/Game/Models/Dustwater";
        public const string PREFABS = "Assets/Project Files/Game/Prefabs/Dustwater";

        private const string TEMPLATE = "Assets/Project Files/Game/Prefabs/";
        private const string SOLDIER_PREFAB = TEMPLATE + "Player/Humanoid Characters/Default Humanoid Character.prefab";
        private const string ENEMY_PREFAB = TEMPLATE + "Enemy/Enemy Human Behavior.prefab";
        private const string ENEMY_HEAVY_PREFAB = TEMPLATE + "Enemy/Enemy Heavy Human Behavior.prefab";
        private const string RIFLE_PREFAB = TEMPLATE + "Guns/Rifle.prefab";
        private const string BULLET_PREFAB = TEMPLATE + "Player/Bullet/Bullet.prefab";
        private const string BARREL_PREFAB = TEMPLATE + "Gameplay/Obstacles/Barrel Obstacle.prefab";
        private const string MONEY_DROP_PREFAB = TEMPLATE + "Money Drop.prefab";

        private const string SOLDIER_BLUE = "Assets/Project Files/Game/Models/Basic/soldier_blue.fbx";
        private const string SOLDIER_RED = "Assets/Project Files/Game/Models/Basic/soldier_red.fbx";
        private const string SOLDIER_BIG_RED = "Assets/Project Files/Game/Models/Basic/soldier_big_red.fbx";

        public const string SHARPSHOOTER = PREFABS + "/Characters/Sharpshooter Character.prefab";
        public const string MEDIC = PREFABS + "/Characters/Medic Character.prefab";
        public const string GRENADIER = PREFABS + "/Characters/Grenadier Character.prefab";
        public const string SNIPER_GUN = PREFABS + "/Guns/Sniper Rifle.prefab";
        public const string LAUNCHER_GUN = PREFABS + "/Guns/Grenade Launcher.prefab";
        public const string GRENADE_BULLET = PREFABS + "/Guns/Grenade.prefab";
        public const string BANDIT_SNIPER = PREFABS + "/Enemies/Bandit Sniper.prefab";
        public const string BOSS_RATTLER = PREFABS + "/Enemies/Boss Rattler.prefab";
        public const string BOSS_JAWS = PREFABS + "/Enemies/Boss Big Jaws.prefab";
        public const string BOSS_VEX = PREFABS + "/Enemies/Boss Vex.prefab";
        public const string SANDBAGS = PREFABS + "/Obstacles/Sandbag Barricade.prefab";
        public const string SUPPLY_CRATE = PREFABS + "/Obstacles/Supply Crate.prefab";
        public const string SUPPLIES_DROP = PREFABS + "/Drops/Supplies Drop.prefab";

        [MenuItem("Tools/Dustwater/Art/Build Units, Weapons, Props", priority = 110)]
        public static void BuildAll()
        {
            SoldierDyes.Paint();
            DustwaterPalette.Generate();

            foreach (string folder in new[] { MODELS, PREFABS + "/Characters", PREFABS + "/Guns", PREFABS + "/Enemies", PREFABS + "/Obstacles", PREFABS + "/Drops" })
                Directory.CreateDirectory(folder);

            BuildGuns();
            BuildCharacters();
            BuildEnemies();
            BuildObstacles();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Dustwater] Units, weapons and props built");
        }

        #region Guns

        private static void BuildGuns()
        {
            Mesh grenadeMesh = SaveMesh(DustwaterProps.Grenade(), "Bullet_Grenade");
            GameObject grenade = Variant(BULLET_PREFAB, GRENADE_BULLET, root =>
            {
                root.name = "Grenade";
                Transform arc = new GameObject("Arc").transform;
                arc.SetParent(root.transform, false);
                Attach(arc, "Grenade", grenadeMesh, DustwaterPalette.GlowMaterial);

                // the trail follows the ground path, not the arc
                TrailRenderer trail = root.GetComponentInChildren<TrailRenderer>(true);
                if (trail != null) trail.enabled = false;

                BoxCollider collider = root.GetComponent<BoxCollider>();
                if (collider != null) collider.size = new Vector3(0.5f, 1.2f, 0.5f);

                SerializedObject bullet = new SerializedObject(root.GetComponent<BulletBehavior>());
                bullet.FindProperty("arcVisual").objectReferenceValue = arc;
                bullet.FindProperty("arcHeight").floatValue = 1.1f;
                bullet.ApplyModifiedPropertiesWithoutUndo();
            });

            BuildGun(SNIPER_GUN, "Sniper Rifle", SaveMesh(DustwaterProps.SniperRifle(), "Gun_SniperRifle"), new Vector3(0f, 0.06f, 0.95f), null);
            BuildGun(LAUNCHER_GUN, "Grenade Launcher", SaveMesh(DustwaterProps.GrenadeLauncher(), "Gun_GrenadeLauncher"), new Vector3(0f, 0.06f, 0.62f), grenade);
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

                Transform point = model.Find("Shooting Point");
                point.localPosition = shootPoint;
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

            Mesh sharpshooterBody = SaveMesh(SoldierDyes.Redye(soldierBlue, SoldierDyes.BLUE, SoldierDyes.SHARPSHOOTER_TAN, "Soldier_Sharpshooter"), "Soldier_Sharpshooter");
            Mesh medicBody = SaveMesh(SoldierDyes.Redye(soldierBlue, SoldierDyes.BLUE, SoldierDyes.MEDIC_WHITE, "Soldier_Medic"), "Soldier_Medic");
            Mesh grenadierBody = SaveMesh(SoldierDyes.Redye(soldierBlue, SoldierDyes.BLUE, SoldierDyes.GRENADIER_OLIVE, "Soldier_Grenadier"), "Soldier_Grenadier");

            Mesh boonie = SaveMesh(DustwaterProps.BoonieHat(), "Prop_BoonieHat");
            Mesh medicCross = SaveMesh(DustwaterProps.MedicHelmetCross(), "Prop_MedicHelmetCross");
            Mesh medicBag = SaveMesh(DustwaterProps.MedicBag(), "Prop_MedicBag");
            Mesh goggles = SaveMesh(DustwaterProps.Goggles(), "Prop_Goggles");
            Mesh grenadeBelt = SaveMesh(DustwaterProps.GrenadeBelt(), "Prop_GrenadeBelt");

            Variant(SOLDIER_PREFAB, SHARPSHOOTER, root =>
            {
                root.name = "Sharpshooter Character";
                SetBody(root, sharpshooterBody);
                AttachToBone(root, "Head", "Boonie Hat", boonie);
                CopyWeaponPlacement(root.GetComponent<CharacterBehavior>(), "rifle", "sniper");
            });

            Variant(SOLDIER_PREFAB, MEDIC, root =>
            {
                root.name = "Medic Character";
                SetBody(root, medicBody);
                AttachToBone(root, "Head", "Helmet Cross", medicCross);
                AttachToBone(root, "Spine2", "Medic Bag", medicBag);
            });

            Variant(SOLDIER_PREFAB, GRENADIER, root =>
            {
                root.name = "Grenadier Character";
                SetBody(root, grenadierBody);
                AttachToBone(root, "Head", "Goggles", goggles);
                AttachToBone(root, "Spine2", "Grenade Belt", grenadeBelt);
                CopyWeaponPlacement(root.GetComponent<CharacterBehavior>(), "rifle", "grenade_launcher");
            });
        }

        #endregion

        #region Enemies

        private static void BuildEnemies()
        {
            Mesh soldierRed = LoadMesh(SOLDIER_RED);
            Mesh soldierBigRed = LoadMesh(SOLDIER_BIG_RED);

            Mesh banditSniperBody = SaveMesh(SoldierDyes.Redye(soldierRed, SoldierDyes.RED, SoldierDyes.BANDIT_PLUM, "Soldier_BanditSniper"), "Soldier_BanditSniper");
            Mesh rattlerBody = SaveMesh(SoldierDyes.Redye(soldierBigRed, SoldierDyes.RED, SoldierDyes.BOSS_GOLD, "Soldier_Boss_Rattler"), "Soldier_Boss_Rattler");
            Mesh blackBody = SaveMesh(SoldierDyes.Redye(soldierBigRed, SoldierDyes.RED, SoldierDyes.BOSS_BLACK, "Soldier_Boss_Black"), "Soldier_Boss_Black");

            Mesh banditHat = SaveMesh(DustwaterProps.CowboyHat(Band.DarkWood, Band.ScorpionRed, 1f), "Prop_CowboyHat_Bandit");
            Mesh redBandana = SaveMesh(DustwaterProps.Bandana(Band.ScorpionRed), "Prop_Bandana_Red");
            Mesh rattlerHat = SaveMesh(DustwaterProps.CowboyHat(Band.Wood, Band.Gold, 1.05f), "Prop_CowboyHat_Rattler");
            Mesh vexHat = SaveMesh(DustwaterProps.CowboyHat(Band.Black, Band.ScorpionRed, 1.08f), "Prop_CowboyHat_Vex");
            Mesh steelJaw = SaveMesh(DustwaterProps.SteelJaw(), "Prop_SteelJaw");
            Mesh spikes = SaveMesh(DustwaterProps.ShoulderSpikes(), "Prop_ShoulderSpikes");
            Mesh badge = SaveMesh(DustwaterProps.ScorpionBadge(), "Prop_ScorpionBadge");

            Variant(ENEMY_PREFAB, BANDIT_SNIPER, root =>
            {
                root.name = "Bandit Sniper";
                SetBody(root, banditSniperBody);
                AttachToBone(root, "Head", "Hat", banditHat);
                AttachToBone(root, "Head", "Bandana", redBandana);
                CopyWeaponPlacement(root.GetComponent<EnemyBehavior>(), "rifle", "sniper");
                CopyWeaponPlacement(root.GetComponent<EnemyBehavior>(), "rifle", "bandit_sniper");
                AddSight(root, 34f);
            });

            Variant(ENEMY_HEAVY_PREFAB, BOSS_RATTLER, root =>
            {
                root.name = "Boss Rattler";
                SetBody(root, rattlerBody);
                AttachToBone(root, "Head", "Hat", rattlerHat);
                AttachToBone(root, "Head", "Bandana", redBandana);
                MakeBoss(root, "LIEUTENANT RATTLER", 1.7f, 24f);
            });

            Variant(ENEMY_HEAVY_PREFAB, BOSS_JAWS, root =>
            {
                root.name = "Boss Big Jaws";
                SetBody(root, blackBody);
                AttachToBone(root, "Head", "Steel Jaw", steelJaw);
                AttachToBone(root, "Spine2", "Spikes", spikes);
                MakeBoss(root, "BIG JAWS", 1.9f, 24f);
            });

            Variant(ENEMY_HEAVY_PREFAB, BOSS_VEX, root =>
            {
                root.name = "Boss Vex";
                SetBody(root, blackBody);
                AttachToBone(root, "Head", "Hat", vexHat);
                AttachToBone(root, "Head", "Bandana", redBandana);
                AttachToBone(root, "Spine2", "Badge", badge);
                AttachToBone(root, "Spine2", "Spikes", spikes);
                MakeBoss(root, "BOSS VEX", 2.1f, 26f);
            });
        }

        private static void MakeBoss(GameObject root, string bossName, float scale, float sightLength)
        {
            root.transform.localScale = Vector3.one * scale;

            EnemyBehavior enemy = root.GetComponent<EnemyBehavior>();
            CopyWeaponPlacement(enemy, "minigun", "boss_minigun");
            BossBehavior boss = root.AddComponent<BossBehavior>();
            boss.SetData(bossName, enemy);

            // the boss bar replaces the small overhead bar
            Transform healthbar = root.transform.Find("Healthbar Holder");
            if (healthbar != null) healthbar.gameObject.SetActive(false);

            AddSight(root, sightLength / scale);
        }

        private static void AddSight(GameObject root, float length)
        {
            GameObject sight = new GameObject("Sight");
            sight.layer = LayerMask.NameToLayer("Pickable");
            sight.transform.SetParent(root.transform, false);

            BoxCollider collider = sight.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(5f, 2f, length);
            collider.center = new Vector3(0f, 1f, length * 0.5f);

            EnemySight enemySight = sight.AddComponent<EnemySight>();
            enemySight.SetEnemy(root.GetComponent<EnemyBehavior>());
        }

        #endregion

        #region Obstacles

        private static void BuildObstacles()
        {
            Mesh sandbags = SaveMesh(DustwaterProps.Sandbags(6, 3), "Obstacle_Sandbags");
            Mesh crate = SaveMesh(DustwaterProps.SupplyCrate(), "Obstacle_SupplyCrate");
            Mesh pickup = SaveMesh(DustwaterProps.SuppliesPickup(), "Drop_Supplies");

            Variant(BARREL_PREFAB, SANDBAGS, root =>
            {
                root.name = "Sandbag Barricade";
                SetObstacleMesh(root, sandbags, new Vector3(0f, 0f, 0f));
                BoxCollider collider = root.GetComponent<BoxCollider>();
                collider.size = new Vector3(3.0f, 1.2f, 0.8f);
                collider.center = new Vector3(0f, 0.6f, 0f);
                Transform canvas = root.transform.Find("Visuals/Canvas");
                canvas.localPosition = new Vector3(0f, 0.75f, -0.2f);
            });

            Variant(BARREL_PREFAB, SUPPLY_CRATE, root =>
            {
                root.name = "Supply Crate";
                SetObstacleMesh(root, crate, Vector3.zero);
                BoxCollider collider = root.GetComponent<BoxCollider>();
                collider.size = new Vector3(1.3f, 1.2f, 1.1f);
                collider.center = new Vector3(0f, 0.6f, 0f);
                Transform canvas = root.transform.Find("Visuals/Canvas");
                canvas.localPosition = new Vector3(0f, 0.5f, -0.1f);
            });

            Variant(MONEY_DROP_PREFAB, SUPPLIES_DROP, root =>
            {
                root.name = "Supplies Drop";
                Transform model = root.transform.Find("Visuals/money_stack");
                model.name = "supplies";
                model.GetComponent<MeshFilter>().sharedMesh = pickup;
                model.GetComponent<MeshRenderer>().sharedMaterial = DustwaterPalette.Material;
                model.localScale = Vector3.one * 1.3f;
            });
        }

        private static void SetObstacleMesh(GameObject root, Mesh mesh, Vector3 position)
        {
            Transform model = root.transform.Find("Visuals/obstacle_barrel");
            model.name = "model";
            model.localPosition = position;
            model.localRotation = Quaternion.identity;
            model.GetComponent<MeshFilter>().sharedMesh = mesh;
            model.GetComponent<MeshRenderer>().sharedMaterial = DustwaterPalette.GlowMaterial;
        }

        #endregion

        #region Helpers

        public static GameObject Variant(string basePath, string outPath, Action<GameObject> modify)
        {
            GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePath);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
            try
            {
                modify(instance);
                return PrefabUtility.SaveAsPrefabAsset(instance, outPath);
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        public static Mesh LoadMesh(string fbxPath)
        {
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(fbxPath))
                if (asset is Mesh mesh) return mesh;

            throw new Exception("No mesh in " + fbxPath);
        }

        public static Mesh SaveMesh(Mesh mesh, string name)
        {
            string path = MODELS + "/" + name + ".asset";
            mesh.name = name;

            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                return mesh;
            }

            // keep the asset (and every reference to it), replace the data
            EditorUtility.CopySerialized(mesh, existing);
            existing.vertices = mesh.vertices;
            existing.normals = mesh.normals;
            existing.uv = mesh.uv;
            existing.triangles = mesh.triangles;
            existing.RecalculateBounds();
            EditorUtility.SetDirty(existing);
            Object.DestroyImmediate(mesh);

            return existing;
        }

        public static GameObject Attach(Transform parent, string name, Mesh mesh, Material material)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            child.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = child.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return child;
        }

        private static void SetBody(GameObject root, Mesh mesh)
        {
            SkinnedMeshRenderer body = root.GetComponentInChildren<SkinnedMeshRenderer>(true);
            body.sharedMesh = mesh;
        }

        private static void AttachToBone(GameObject root, string boneName, string name, Mesh mesh, Vector3? offset = null)
        {
            SkinnedMeshRenderer body = root.GetComponentInChildren<SkinnedMeshRenderer>(true);
            Transform bone = Array.Find(body.bones, b => b.name == boneName);
            GameObject prop = Attach(bone, name, mesh, DustwaterPalette.GlowMaterial);
            prop.transform.localPosition = offset ?? Vector3.zero;
            // props are authored for an unrotated bone at bind pose; undo the bone's bind rotation
            prop.transform.rotation = root.transform.rotation;
        }

        private static void CopyWeaponPlacement(Component receiver, string fromWeapon, string toWeapon)
        {
            SerializedObject serialized = new SerializedObject(receiver);
            SerializedProperty list = serialized.FindProperty("weaponPlacementData");

            int source = -1;
            for (int i = 0; i < list.arraySize; i++)
            {
                SerializedProperty element = list.GetArrayElementAtIndex(i);
                string id = element.FindPropertyRelative("weaponId").stringValue;
                if (id == toWeapon) list.DeleteArrayElementAtIndex(i--);
                else if (id == fromWeapon) source = i;
            }

            if (source < 0) return;

            list.InsertArrayElementAtIndex(source);
            list.MoveArrayElement(source + 1, list.arraySize - 1);
            list.GetArrayElementAtIndex(list.arraySize - 1).FindPropertyRelative("weaponId").stringValue = toWeapon;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        #endregion
    }
}
