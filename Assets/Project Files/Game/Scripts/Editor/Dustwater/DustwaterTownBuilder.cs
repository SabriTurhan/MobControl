using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Watermelon.Dustwater
{
    // Town art: building prefabs, the three road environments (Outskirts / Main Street / Scorpion Fort)
    // and the 3D town diorama shown in the Town tab and the intro cinematic.
    public static class DustwaterTownBuilder
    {
        private const string MODELS = DustwaterUnitsBuilder.MODELS + "/Town";
        private const string PREFABS = DustwaterUnitsBuilder.PREFABS + "/Town";
        private const string ENVIRONMENTS = DustwaterUnitsBuilder.PREFABS + "/Environments";
        private const string DESERT_ENVIRONMENT = "Assets/Project Files/Game/Prefabs/Gameplay/Environment/Environment Desert.prefab";

        public const string ENV_OUTSKIRTS = "environment_outskirts";
        public const string ENV_TOWN = "environment_town";
        public const string ENV_FORT = "environment_fort";

        public static string RoadIdForMission(int mission) => "road_desert";

        public static string EnvironmentIdForMission(int mission)
        {
            if (mission <= 10) return ENV_OUTSKIRTS;
            if (mission <= 20) return ENV_TOWN;
            return ENV_FORT;
        }

        private static readonly Dictionary<string, GameObject> pieces = new Dictionary<string, GameObject>();

        [MenuItem("Tools/Dustwater/Art/Build Town (pieces, environments, diorama)", priority = 130)]
        public static void BuildAll()
        {
            Directory.CreateDirectory(MODELS);
            Directory.CreateDirectory(PREFABS);
            Directory.CreateDirectory(ENVIRONMENTS);

            BuildPieces();
            BuildEnvironments();
            RegisterEnvironments();
            BuildDiorama();

            AssetDatabase.SaveAssets();
            Debug.Log("[Dustwater] Town built");
        }

        #region Pieces

        private static void BuildPieces()
        {
            pieces.Clear();

            Piece("Command Post", DustwaterBuildings.CommandPost());
            Piece("Barracks", DustwaterBuildings.Barracks());
            Piece("Armory", DustwaterBuildings.Armory());
            Piece("Workshop", DustwaterBuildings.Workshop());
            Piece("Field Hospital", DustwaterBuildings.FieldHospital());
            Piece("Saloon", DustwaterBuildings.Saloon());
            Piece("Watchtower", DustwaterBuildings.Watchtower());
            Piece("Water Tower", DustwaterBuildings.WaterTower());
            Piece("Gas Station", DustwaterBuildings.GasStation());
            Piece("Construction Site", DustwaterBuildings.ConstructionSite());
            Piece("Bandit Camp", DustwaterBuildings.BanditCamp());
            Piece("Flag Ranger", DustwaterBuildings.FlagPole(Band.RangerBlue, 4f, false));
            Piece("Flag Scorpion", DustwaterBuildings.FlagPole(Band.ScorpionRed, 4f, true));
            Piece("Fence", DustwaterBuildings.Fence(4f));
            Piece("Cactus A", DustwaterBuildings.Cactus(1.8f));
            Piece("Cactus B", DustwaterBuildings.Cactus(2.6f));
            Piece("Rock A", DustwaterBuildings.Rock(1.2f, 3));
            Piece("Rock B", DustwaterBuildings.Rock(1.8f, 7));
            Piece("Mesa A", DustwaterBuildings.Mesa(14f, 7f, 11));
            Piece("Mesa B", DustwaterBuildings.Mesa(10f, 9f, 23));
            Piece("Lamp", DustwaterBuildings.Lamp());
            Piece("Wagon", DustwaterBuildings.Wagon());
            Piece("Bandit Truck", DustwaterBuildings.Truck(Band.ScorpionRed));
            Piece("Palisade", DustwaterBuildings.Palisade(10f));
            Piece("Adobe A", DustwaterBuildings.AdobeHouse(4.2f, 3.6f, 3.0f, 1));
            Piece("Adobe B", DustwaterBuildings.AdobeHouse(3.4f, 3.2f, 3.4f, 2));
            Piece("Store Blue", DustwaterBuildings.FalseFront(4.4f, 4f, 3.4f, Band.Slate, Band.RangerBlue, 3));
            Piece("Store Red", DustwaterBuildings.FalseFront(4.0f, 4f, 3.2f, Band.Wood, Band.ScorpionRed, 4));
            Piece("Store Green", DustwaterBuildings.FalseFront(4.6f, 4f, 3.6f, Band.Bone, Band.Olive, 5));
            Piece("Store Gold", DustwaterBuildings.FalseFront(4.2f, 4f, 3.3f, Band.Teal, Band.Gold, 6));
        }

        private static void Piece(string name, Mesh mesh)
        {
            Mesh saved = DustwaterUnitsBuilder.SaveMesh(mesh, "Town/" + name.Replace(" ", "_"));

            GameObject go = new GameObject(name);
            go.AddComponent<MeshFilter>().sharedMesh = saved;
            go.AddComponent<MeshRenderer>().sharedMaterial = DustwaterPalette.Material;

            string path = PREFABS + "/" + name + ".prefab";
            pieces[name] = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        private static GameObject Place(Transform parent, string piece, Vector3 position, float yaw = 0f, float scale = 1f, string name = null)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(pieces[piece], parent);
            instance.name = name ?? piece;
            instance.transform.localPosition = position;
            instance.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            instance.transform.localScale = Vector3.one * scale;
            return instance;
        }

        #endregion

        #region Road environments

        private static void BuildEnvironments()
        {
            // Outskirts: the template desert with bandit flags, a camp and a wrecked truck
            DustwaterUnitsBuilder.Variant(DESERT_ENVIRONMENT, ENVIRONMENTS + "/Environment Outskirts.prefab", root =>
            {
                root.name = "Environment Outskirts";
                Transform extra = new GameObject("Dustwater").transform;
                extra.SetParent(root.transform, false);
                Place(extra, "Flag Scorpion", new Vector3(-6.5f, 0f, 8f), 20f);
                Place(extra, "Flag Scorpion", new Vector3(7f, 0f, 29f), 160f);
                Place(extra, "Bandit Camp", new Vector3(-13f, 0f, 22f), 70f);
                Place(extra, "Bandit Truck", new Vector3(12f, 0f, 13f), 200f).transform.localRotation = Quaternion.Euler(0f, 200f, 8f);
                Place(extra, "Cactus B", new Vector3(9f, 0f, 36f), 30f);
                Place(extra, "Rock B", new Vector3(-10f, 0f, 2f));
            });

            // Main Street: storefronts and adobe houses on both sides, lamps and barrels
            DustwaterUnitsBuilder.Variant(DESERT_ENVIRONMENT, ENVIRONMENTS + "/Environment Town.prefab", root =>
            {
                root.name = "Environment Town";
                Transform props = root.transform.Find("Prop");
                if (props != null) props.gameObject.SetActive(false);

                Transform street = new GameObject("Main Street").transform;
                street.SetParent(root.transform, false);
                string[] left = { "Store Blue", "Adobe A", "Store Red", "Saloon" };
                string[] right = { "Store Gold", "Store Green", "Adobe B", "Store Red" };
                for (int i = 0; i < 4; i++)
                {
                    Place(street, left[i], new Vector3(-9.5f, 0f, 5f + i * 10f), -90f);
                    Place(street, right[i], new Vector3(9.5f, 0f, 0f + i * 10f), 90f);
                    Place(street, "Lamp", new Vector3(-5.3f, 0f, i * 10f), 180f);
                    Place(street, "Lamp", new Vector3(5.3f, 0f, 5f + i * 10f), 0f);
                }
                Place(street, "Wagon", new Vector3(6.2f, 0f, 24f), 8f);
                Place(street, "Flag Scorpion", new Vector3(-6.2f, 0f, 18f), 0f);
                Place(street, "Water Tower", new Vector3(-16f, 0f, 30f), 0f);
            });

            // Scorpion Fort: palisade walls along the road, watchtowers, bandit tents behind
            DustwaterUnitsBuilder.Variant(DESERT_ENVIRONMENT, ENVIRONMENTS + "/Environment Fort.prefab", root =>
            {
                root.name = "Environment Fort";
                Transform props = root.transform.Find("Prop");
                if (props != null) props.gameObject.SetActive(false);

                Transform fort = new GameObject("Fort").transform;
                fort.SetParent(root.transform, false);
                for (int i = 0; i < 4; i++)
                {
                    Place(fort, "Palisade", new Vector3(-6.8f, 0f, 5f + i * 10f), 90f);
                    Place(fort, "Palisade", new Vector3(6.8f, 0f, 5f + i * 10f), -90f);
                }
                Place(fort, "Watchtower", new Vector3(-9.5f, 0f, 20f), 90f);
                Place(fort, "Watchtower", new Vector3(9.5f, 0f, 0f), -90f);
                Place(fort, "Flag Scorpion", new Vector3(-7.2f, 2.4f, 10f), 0f, 0.8f);
                Place(fort, "Flag Scorpion", new Vector3(7.2f, 2.4f, 30f), 180f, 0.8f);
                Place(fort, "Bandit Camp", new Vector3(-13f, 0f, 34f), 90f);
                Place(fort, "Bandit Camp", new Vector3(13f, 0f, 14f), -90f);
                Place(fort, "Bandit Truck", new Vector3(13.5f, 0f, 30f), 180f);
            });
        }

        private static void RegisterEnvironments()
        {
            LevelsDatabase database = AssetDatabase.LoadAssetAtPath<LevelsDatabase>(DustwaterContentBuilder.LEVELS_DATABASE);
            SerializedObject so = new SerializedObject(database);
            SerializedProperty environments = so.FindProperty("environmentData");

            Register(environments, ENV_OUTSKIRTS, ENVIRONMENTS + "/Environment Outskirts.prefab");
            Register(environments, ENV_TOWN, ENVIRONMENTS + "/Environment Town.prefab");
            Register(environments, ENV_FORT, ENVIRONMENTS + "/Environment Fort.prefab");

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(database);
        }

        private static void Register(SerializedProperty list, string id, string prefabPath)
        {
            SerializedProperty element = null;
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).FindPropertyRelative("id").stringValue == id) element = list.GetArrayElementAtIndex(i);

            if (element == null)
            {
                list.InsertArrayElementAtIndex(list.arraySize);
                element = list.GetArrayElementAtIndex(list.arraySize - 1);
                element.FindPropertyRelative("id").stringValue = id;
            }

            element.FindPropertyRelative("prefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        #endregion

        #region Diorama

        public static readonly Vector3 DIORAMA_POSITION = new Vector3(-600f, 0f, 0f);

        private static void BuildDiorama()
        {
            var scene = EditorSceneManager.OpenScene(DustwaterContentBuilder.GAME_SCENE, OpenSceneMode.Single);

            GameObject old = GameObject.Find("Dustwater Town");
            if (old != null) Object.DestroyImmediate(old);

            GameObject root = new GameObject("Dustwater Town");
            root.transform.position = DIORAMA_POSITION;
            DustwaterTownView view = root.AddComponent<DustwaterTownView>();
            SerializedObject so = new SerializedObject(view);

            // ground, main road, backdrop
            Material material = DustwaterPalette.Material;
            AddMesh(root.transform, "Ground", new PaletteMesh().Box(new Vector3(90f, 0.4f, 90f), 0f, new Vector3(0f, -0.2f, 10f), Vector3.zero, Band.Sand, 0.3f, 0.55f).Build("Town_Ground"), material);
            AddMesh(root.transform, "Road", new PaletteMesh().Box(new Vector3(4.6f, 0.06f, 46f), 0f, new Vector3(0f, 0.02f, 6f), Vector3.zero, Band.Khaki, 0.4f, 0.7f).Build("Town_Road"), material);
            Place(root.transform, "Mesa A", new Vector3(-22f, 0f, 44f), 10f);
            Place(root.transform, "Mesa B", new Vector3(4f, 0f, 52f), -8f);
            Place(root.transform, "Mesa A", new Vector3(30f, 0f, 46f), 190f, 0.9f);

            // plots: camp / construction site / building (+ flag on upgraded buildings)
            var plots = new (BuildingType type, string piece, Vector3 position, float yaw, int chapter)[]
            {
                (BuildingType.CommandPost, "Command Post", new Vector3(-6.5f, 0f, -5f), 15f, 0),
                (BuildingType.Barracks, "Barracks", new Vector3(7f, 0f, -5f), -15f, 0),
                (BuildingType.Armory, "Armory", new Vector3(-7.5f, 0f, 5f), 20f, 0),
                (BuildingType.Workshop, "Workshop", new Vector3(7.5f, 0f, 5f), -20f, 0),
                (BuildingType.FieldHospital, "Field Hospital", new Vector3(-7.5f, 0f, 15f), 20f, 1),
                (BuildingType.Saloon, "Saloon", new Vector3(7.5f, 0f, 15.5f), -20f, 1),
                (BuildingType.Watchtower, "Watchtower", new Vector3(0f, 0f, 25f), 0f, 2),
            };

            SerializedProperty plotList = so.FindProperty("plots");
            plotList.arraySize = plots.Length;
            for (int i = 0; i < plots.Length; i++)
            {
                var p = plots[i];
                Transform plotRoot = new GameObject("Plot " + p.piece).transform;
                plotRoot.SetParent(root.transform, false);
                plotRoot.localPosition = p.position;
                plotRoot.localRotation = Quaternion.Euler(0f, p.yaw, 0f);

                GameObject camp = Place(plotRoot, "Bandit Camp", Vector3.zero);
                GameObject site = Place(plotRoot, "Construction Site", Vector3.zero);
                GameObject building = Place(plotRoot, p.piece, Vector3.zero);
                GameObject flag = Place(plotRoot, "Flag Ranger", new Vector3(2.4f, 0f, 1.4f), 0f, 0.8f);

                SerializedProperty element = plotList.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("type").intValue = (int)p.type;
                element.FindPropertyRelative("camp").objectReferenceValue = camp;
                element.FindPropertyRelative("site").objectReferenceValue = site;
                element.FindPropertyRelative("building").objectReferenceValue = building;
                element.FindPropertyRelative("flag").objectReferenceValue = flag;
            }

            // district flags: red while occupied, blue once the chapter is cleared
            var flagSpots = new (Vector3 position, int chapter)[]
            {
                (new Vector3(-3f, 0f, -9f), 0), (new Vector3(3.2f, 0f, -9f), 0), (new Vector3(-3.4f, 0f, 9f), 0),
                (new Vector3(3.4f, 0f, 11f), 1), (new Vector3(-3.4f, 0f, 19f), 1),
                (new Vector3(-3f, 0f, 27f), 2), (new Vector3(3f, 0f, 27f), 2),
            };

            SerializedProperty districts = so.FindProperty("districts");
            districts.arraySize = 3;
            for (int d = 0; d < 3; d++)
            {
                SerializedProperty district = districts.GetArrayElementAtIndex(d);
                district.FindPropertyRelative("chapter").intValue = d;
                SerializedProperty red = district.FindPropertyRelative("scorpionFlags");
                SerializedProperty blue = district.FindPropertyRelative("rangerFlags");
                red.arraySize = 0;
                blue.arraySize = 0;

                foreach (var spot in flagSpots)
                {
                    if (spot.chapter != d) continue;
                    GameObject scorpion = Place(root.transform, "Flag Scorpion", spot.position, 0f, 1f, "District " + d + " Flag Red");
                    GameObject ranger = Place(root.transform, "Flag Ranger", spot.position, 0f, 1f, "District " + d + " Flag Blue");
                    red.InsertArrayElementAtIndex(red.arraySize);
                    red.GetArrayElementAtIndex(red.arraySize - 1).objectReferenceValue = scorpion;
                    blue.InsertArrayElementAtIndex(blue.arraySize);
                    blue.GetArrayElementAtIndex(blue.arraySize - 1).objectReferenceValue = ranger;
                }
            }

            // landmarks and decoration
            Transform decor = new GameObject("Decor").transform;
            decor.SetParent(root.transform, false);
            Place(decor, "Water Tower", new Vector3(-15f, 0f, 24f), 10f);
            Place(decor, "Gas Station", new Vector3(16f, 0f, 25f), -25f);
            Place(decor, "Adobe A", new Vector3(-16f, 0f, 7f), 25f);
            Place(decor, "Adobe B", new Vector3(-15.5f, 0f, -6f), 30f);
            Place(decor, "Store Blue", new Vector3(16f, 0f, 9f), -25f);
            Place(decor, "Adobe A", new Vector3(16.5f, 0f, -5f), -30f);
            Place(decor, "Wagon", new Vector3(3.3f, 0f, 1.5f), 12f);
            for (int i = 0; i < 4; i++)
            {
                Place(decor, "Lamp", new Vector3(-2.9f, 0f, -6f + i * 9f), 180f, 0.9f);
                Place(decor, "Lamp", new Vector3(2.9f, 0f, -2f + i * 9f), 0f, 0.9f);
            }
            Place(decor, "Fence", new Vector3(-11.5f, 0f, -11f), 0f);
            Place(decor, "Fence", new Vector3(11.5f, 0f, -11f), 0f);
            Place(decor, "Cactus A", new Vector3(-20f, 0f, 14f));
            Place(decor, "Cactus B", new Vector3(21f, 0f, 3f), 40f);
            Place(decor, "Cactus A", new Vector3(12f, 0f, 35f), 10f);
            Place(decor, "Cactus B", new Vector3(-11f, 0f, 34f), -20f);
            Place(decor, "Rock A", new Vector3(-21f, 0f, -2f));
            Place(decor, "Rock B", new Vector3(22f, 0f, 16f));
            Place(decor, "Rock A", new Vector3(6f, 0f, 36f));

            // camera: pivot at the town centre, slow sway
            Transform pivot = new GameObject("Camera Pivot").transform;
            pivot.SetParent(root.transform, false);
            pivot.localPosition = new Vector3(0f, 0f, 9f);
            GameObject cameraObject = new GameObject("Town Camera");
            cameraObject.transform.SetParent(pivot, false);
            cameraObject.transform.localPosition = new Vector3(0f, 31f, -43f);
            cameraObject.transform.LookAt(pivot.position + new Vector3(0f, 0f, 3f));
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 38f;
            camera.nearClipPlane = 0.5f;
            camera.farClipPlane = 220f;
            camera.depth = 5f;
            camera.clearFlags = CameraClearFlags.Skybox;
            var additional = cameraObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            additional.renderPostProcessing = false;
            cameraObject.SetActive(false);

            so.FindProperty("townCamera").objectReferenceValue = camera;
            so.FindProperty("cameraPivot").objectReferenceValue = pivot;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static GameObject AddMesh(Transform parent, string name, Mesh mesh, Material material)
        {
            Mesh saved = DustwaterUnitsBuilder.SaveMesh(mesh, "Town/" + name);
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = saved;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            return go;
        }

        #endregion
    }
}
