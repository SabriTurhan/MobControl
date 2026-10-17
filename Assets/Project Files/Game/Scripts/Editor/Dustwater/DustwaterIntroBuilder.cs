using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Watermelon.Dustwater
{
    // Visual-only actors for the intro (gameplay prefabs with every script, collider, health bar and
    // particle removed, holding their gun model) and the intro object in the Game scene.
    public static class DustwaterIntroBuilder
    {
        private const string ACTORS = DustwaterUnitsBuilder.PREFABS + "/Actors";
        private const string TEMPLATE = "Assets/Project Files/Game/Prefabs/";

        [MenuItem("Tools/Dustwater/Content/Build Intro Cinematic", priority = 220)]
        public static void BuildAll()
        {
            Directory.CreateDirectory(ACTORS);

            GameObject bandit = Actor(TEMPLATE + "Enemy/Enemy Human Behavior.prefab", "Bandit Actor", TEMPLATE + "Guns/Pistol.prefab", "default_gun");
            GameObject vex = Actor(DustwaterUnitsBuilder.BOSS_VEX, "Vex Actor", TEMPLATE + "Guns/Minigun.prefab", "minigun");
            List<GameObject> rangers = new List<GameObject>()
            {
                Actor(DustwaterUnitsBuilder.SHARPSHOOTER, "Sharpshooter Actor", DustwaterUnitsBuilder.SNIPER_GUN, "sniper"),
                Actor(TEMPLATE + "Player/Humanoid Characters/Default Humanoid Character.prefab", "Ranger Actor", TEMPLATE + "Guns/Rifle.prefab", "rifle"),
                Actor(TEMPLATE + "Player/Humanoid Characters/Heavy Humanoid Character.prefab", "Heavy Actor", TEMPLATE + "Guns/Minigun.prefab", "minigun"),
                Actor(DustwaterUnitsBuilder.MEDIC, "Medic Actor", TEMPLATE + "Guns/Pistol.prefab", "default_gun"),
            };

            var scene = EditorSceneManager.OpenScene(DustwaterContentBuilder.GAME_SCENE, OpenSceneMode.Single);
            GameObject old = GameObject.Find("Dustwater Intro");
            if (old != null) Object.DestroyImmediate(old);

            GameObject intro = new GameObject("Dustwater Intro");
            DustwaterIntroPlayer player = intro.AddComponent<DustwaterIntroPlayer>();
            SerializedObject so = new SerializedObject(player);
            so.FindProperty("town").objectReferenceValue = Object.FindFirstObjectByType<DustwaterTownView>(FindObjectsInactive.Include);
            so.FindProperty("banditActor").objectReferenceValue = bandit;
            so.FindProperty("vexActor").objectReferenceValue = vex;
            so.FindProperty("truckPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(DustwaterUnitsBuilder.PREFABS + "/Town/Bandit Truck.prefab");
            SerializedProperty list = so.FindProperty("rangerActors");
            list.arraySize = rangers.Count;
            for (int i = 0; i < rangers.Count; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = rangers[i];
            so.FindProperty("vexPortrait").objectReferenceValue = DustwaterIcons.Portrait("boss_vex");
            so.FindProperty("sergeantPortrait").objectReferenceValue = DustwaterIcons.Portrait("ranger");
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Dustwater] Intro built");
        }

        private static GameObject Actor(string sourcePath, string name, string gunPath, string placementId)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath));
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            instance.name = name;

            // gun model in the hand, placed like in the game
            IWeaponPlacementReceiver receiver = instance.GetComponent<IWeaponPlacementReceiver>();
            if (receiver != null && gunPath != null)
            {
                GameObject gunPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(gunPath);
                GameObject gun = (GameObject)PrefabUtility.InstantiatePrefab(gunPrefab, receiver.GunHolder);
                PrefabUtility.UnpackPrefabInstance(gun, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                gun.transform.localPosition = Vector3.zero;
                gun.transform.localRotation = Quaternion.identity;
                foreach (WeaponPlacementData placement in receiver.WeaponPlacementData)
                    if (placement.WeaponId == placementId) placement.Apply(gun.transform);
            }

            // strip gameplay: scripts, physics, health bars, effects, sights
            foreach (Transform child in instance.GetComponentsInChildren<Transform>(true))
            {
                if (child == null) continue;
                if (child.name == "Healthbar Holder" || child.name == "Sight" || child.GetComponent<ParticleSystem>() != null)
                    Object.DestroyImmediate(child.gameObject);
            }

            foreach (MonoBehaviour behaviour in instance.GetComponentsInChildren<MonoBehaviour>(true))
                Object.DestroyImmediate(behaviour);
            foreach (Collider collider in instance.GetComponentsInChildren<Collider>(true))
                Object.DestroyImmediate(collider);
            foreach (Rigidbody body in instance.GetComponentsInChildren<Rigidbody>(true))
                Object.DestroyImmediate(body);

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(instance, ACTORS + "/" + name + ".prefab");
            Object.DestroyImmediate(instance);
            return saved;
        }
    }
}
