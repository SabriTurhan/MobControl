using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static Watermelon.Dustwater.DustwaterTownBuilder;

namespace Watermelon.Dustwater
{
    // Act 2 art: Frostpeak pieces, the snow road, seven winter road environments (one per chapter) on a
    // snow ground, and the Frostpeak diorama for the Town tab (DustwaterTownView with act = 1).
    public static class FrostTownBuilder
    {
        private const string ENVIRONMENTS = DustwaterUnitsBuilder.PREFABS + "/Environments";
        private const string ROADS = DustwaterUnitsBuilder.PREFABS + "/Roads";
        private const string DESERT_ENVIRONMENT = "Assets/Project Files/Game/Prefabs/Gameplay/Environment/Environment Desert.prefab";
        private const string DESERT_ROAD = "Assets/Project Files/Game/Prefabs/Gameplay/Roads/Road Desert.prefab";
        private const string GROUND_MATERIAL = "Assets/Project Files/Game/Materials/Final/Ground.mat";
        private const string GROUND_TEXTURE = "Assets/Project Files/Game/Textures/ground.png";
        private const string ENVIRONMENT_MATERIAL = "Assets/Project Files/Game/Materials/Final/Environment URP.mat";

        private const string SNOW_TEXTURE = DustwaterPalette.FOLDER + "/snow_ground.png";
        private const string SNOW_GROUND_MATERIAL = DustwaterPalette.MATERIAL_FOLDER + "/Snow Ground.mat";
        private const string SNOW_ROAD_MATERIAL = DustwaterPalette.MATERIAL_FOLDER + "/Snow Road URP.mat";

        public const string ROAD_SNOW = "road_snow";
        public const string SNOW_ROAD_PREFAB = ROADS + "/Road Snow.prefab";

        // chapter 4..10 environments (missions 31-100)
        public static readonly (string id, string name)[] ENVIRONMENT_IDS =
        {
            ("environment_pine_pass", "Pine Pass"),
            ("environment_frozen_lake", "Frozen Lake"),
            ("environment_lumber_camp", "Lumber Camp"),
            ("environment_frostpeak", "Frostpeak Town"),
            ("environment_crystal_mine", "Crystal Mine"),
            ("environment_wolf_fortress", "Wolf Fortress"),
            ("environment_summit", "Summit"),
        };

        public static string EnvironmentForMission(int mission) => ENVIRONMENT_IDS[Mathf.Clamp((mission - 31) / 10, 0, ENVIRONMENT_IDS.Length - 1)].id;

        public static readonly Vector3 DIORAMA_POSITION = new Vector3(-800f, 0f, 0f);

        [MenuItem("Tools/Dustwater/Art/Build Frostpeak (pieces, environments, diorama)", priority = 131)]
        public static void BuildAll()
        {
            Directory.CreateDirectory(ENVIRONMENTS);
            Directory.CreateDirectory(ROADS);

            BuildPieces();
            BuildMaterials();
            BuildRoad();
            BuildEnvironments();
            RegisterAll();
            BuildDiorama();

            AssetDatabase.SaveAssets();
            Debug.Log("[Dustwater] Frostpeak built");
        }

        #region Pieces

        private static void BuildPieces()
        {
            // the Dustwater pieces are needed too (flags, trucks, construction site)
            DustwaterTownBuilder.BuildPiecesForReuse();

            Piece("Lodge", FrostBuildings.Lodge());
            Piece("Log Barracks", FrostBuildings.LogBarracks());
            Piece("Forge", FrostBuildings.Forge());
            Piece("Sawmill", FrostBuildings.Sawmill());
            Piece("Infirmary", FrostBuildings.Infirmary());
            Piece("Tavern", FrostBuildings.Tavern());
            Piece("Lookout", FrostBuildings.Lookout());
            Piece("Snow Site", FrostBuildings.SnowSite());
            Piece("Wolf Camp", FrostBuildings.WolfCamp());
            Piece("Flag Wolf", FrostBuildings.WolfFlag());
            Piece("Pine A", FrostBuildings.Pine(4.5f, 1));
            Piece("Pine B", FrostBuildings.Pine(6.5f, 2));
            Piece("Pine C", FrostBuildings.Pine(3.2f, 3));
            Piece("Ice Rock A", FrostBuildings.IceRock(1.3f, 4));
            Piece("Ice Rock B", FrostBuildings.IceRock(2.0f, 5));
            Piece("Mountain A", FrostBuildings.Mountain(18f, 16f, 6));
            Piece("Mountain B", FrostBuildings.Mountain(13f, 12f, 7));
            Piece("Frozen Lake", FrostBuildings.FrozenLake(5f));
            Piece("Snow Drift", FrostBuildings.SnowDrift(1.1f, 8));
            Piece("Crystals", FrostBuildings.Crystals(1.4f, 9));
            Piece("Lantern", FrostBuildings.LanternPost());
            Piece("Snowman", FrostBuildings.Snowman());
            Piece("Sled", FrostBuildings.Sled());
            Piece("Stone Wall", FrostBuildings.StoneWall(10f));
            Piece("Wolf Tower", FrostBuildings.WolfTower());
            Piece("Mine", FrostBuildings.MineEntrance());
            Piece("Cabin A", FrostBuildings.Cabin(4.2f, 3.6f, 2.4f, Band.Maroon, 11));
            Piece("Cabin B", FrostBuildings.Cabin(3.6f, 3.2f, 2.2f, Band.NightBlue, 12));
            Piece("Cabin C", FrostBuildings.Cabin(4.6f, 3.4f, 2.6f, Band.Moss, 13));
            Piece("Wolf Truck", DustwaterBuildings.Truck(Band.NightBlue));
        }

        #endregion

        #region Materials

        // snow version of the template ground: same noise, mapped to snow whites and ice-blue shadows
        private static void BuildMaterials()
        {
            Texture2D ground = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            ground.LoadImage(File.ReadAllBytes(GROUND_TEXTURE));
            Color[] pixels = ground.GetPixels();
            float min = 1f, max = 0f;
            foreach (Color c in pixels) { float l = c.grayscale; min = Mathf.Min(min, l); max = Mathf.Max(max, l); }
            Color low = DustwaterPalette.GetColor(Band.Frost), high = DustwaterPalette.GetColor(Band.Snow);
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.Lerp(low, high, Mathf.InverseLerp(min, max, pixels[i].grayscale) * 0.8f + 0.2f);
            ground.SetPixels(pixels);
            ground.Apply();
            File.WriteAllBytes(SNOW_TEXTURE, ground.EncodeToPNG());
            Object.DestroyImmediate(ground);
            AssetDatabase.ImportAsset(SNOW_TEXTURE, ImportAssetOptions.ForceUpdate);

            Material snow = CopyMaterial(GROUND_MATERIAL, SNOW_GROUND_MATERIAL);
            snow.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(SNOW_TEXTURE));
            snow.SetColor("_EmissionColor", new Color(0.1f, 0.13f, 0.2f, 1f));
            snow.SetColor("_SColor", new Color(0.7f, 0.82f, 1f, 1f));
            EditorUtility.SetDirty(snow);

            // icy tint on the template road atlas
            Material road = CopyMaterial(ENVIRONMENT_MATERIAL, SNOW_ROAD_MATERIAL);
            road.SetColor("_Color", new Color(0.8f, 0.88f, 1f, 1f));
            EditorUtility.SetDirty(road);
        }

        private static Material CopyMaterial(string templatePath, string path)
        {
            Material template = AssetDatabase.LoadAssetAtPath<Material>(templatePath);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(template);
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.CopyPropertiesFromMaterial(template);
            }
            return material;
        }

        #endregion

        #region Road and environments

        private static void BuildRoad()
        {
            PaletteMesh m = new PaletteMesh();
            for (int side = -1; side <= 1; side += 2)
            {
                m.Box(new Vector3(0.7f, 0.14f, 10.02f), 0.05f, new Vector3(side * 3.55f, 0.02f, 5f), Vector3.zero, Band.Snow, 0f, 0.3f);
                for (int i = 0; i < 3; i++)
                    m.Sphere(0.45f, 3, 8, new Vector3(side * 3.75f, 0.02f, 1.6f + i * 3.4f + side * 0.6f), new Vector3(1.3f, 0.45f, 2.2f), Band.Snow, 0f, 0.3f);
            }
            Mesh snowBanks = DustwaterUnitsBuilder.SaveMesh(m.Build("Road_SnowBanks"), "Town/Road_SnowBanks");

            DustwaterUnitsBuilder.Variant(DESERT_ROAD, SNOW_ROAD_PREFAB, root =>
            {
                root.name = "Road Snow";
                Material material = AssetDatabase.LoadAssetAtPath<Material>(SNOW_ROAD_MATERIAL);
                foreach (MeshRenderer renderer in root.GetComponentsInChildren<MeshRenderer>(true)) renderer.sharedMaterial = material;
                DustwaterUnitsBuilder.Attach(root.transform, "Snow Banks", snowBanks, DustwaterPalette.Material).transform.localPosition = new Vector3(0f, 0f, -5f);
            });
        }

        private static GameObject Environment(string name, System.Action<Transform> decorate)
        {
            return DustwaterUnitsBuilder.Variant(DESERT_ENVIRONMENT, ENVIRONMENTS + "/Environment " + name + ".prefab", root =>
            {
                root.name = "Environment " + name;
                Material snow = AssetDatabase.LoadAssetAtPath<Material>(SNOW_GROUND_MATERIAL);
                foreach (Transform child in root.transform)
                    if (child.name.StartsWith("Ground")) child.GetComponent<MeshRenderer>().sharedMaterial = snow;

                Transform props = root.transform.Find("Prop");
                if (props != null) props.gameObject.SetActive(false);

                Transform extra = new GameObject("Frostpeak").transform;
                extra.SetParent(root.transform, false);
                decorate(extra);
            });
        }

        // the road is 6.6m wide: decoration starts at |x| 5
        private static void PineRows(Transform t, float near, int seed)
        {
            System.Random r = new System.Random(seed);
            for (int i = 0; i < 8; i++)
            {
                float z = i * 5f + (float)r.NextDouble() * 2f;
                string piece = r.Next(3) == 0 ? "Pine B" : r.Next(2) == 0 ? "Pine A" : "Pine C";
                Place(t, piece, new Vector3(-(near + (float)r.NextDouble() * 4f), 0f, z), (float)r.NextDouble() * 90f);
                Place(t, piece == "Pine B" ? "Pine A" : "Pine B", new Vector3(near + (float)r.NextDouble() * 4f, 0f, z + 2.5f), (float)r.NextDouble() * 90f);
            }
        }

        private static void BuildEnvironments()
        {
            Environment("Pine Pass", t =>
            {
                PineRows(t, 6f, 1);
                Place(t, "Flag Wolf", new Vector3(-5.6f, 0f, 9f), 20f);
                Place(t, "Flag Wolf", new Vector3(5.8f, 0f, 30f), 160f);
                Place(t, "Ice Rock B", new Vector3(-6f, 0f, 21f));
                Place(t, "Snow Drift", new Vector3(5.6f, 0f, 15f));
                Place(t, "Mountain A", new Vector3(-24f, 0f, 30f), 20f);
                Place(t, "Mountain B", new Vector3(24f, 0f, 12f), -10f);
            });

            Environment("Frozen Lake", t =>
            {
                Place(t, "Frozen Lake", new Vector3(-12f, 0f, 14f), 0f, 1.4f);
                Place(t, "Frozen Lake", new Vector3(13f, 0f, 30f), 0f, 1.2f);
                Place(t, "Ice Rock A", new Vector3(-5.6f, 0f, 4f));
                Place(t, "Ice Rock B", new Vector3(6.4f, 0f, 12f));
                Place(t, "Snowman", new Vector3(5.8f, 0f, 22f), 200f);
                Place(t, "Wolf Camp", new Vector3(-9f, 0f, 32f), 70f);
                Place(t, "Pine A", new Vector3(-7f, 0f, 26f));
                Place(t, "Pine B", new Vector3(8f, 0f, 2f));
                Place(t, "Snow Drift", new Vector3(-5.5f, 0f, 36f));
                Place(t, "Mountain B", new Vector3(-26f, 0f, 36f));
            });

            Environment("Lumber Camp", t =>
            {
                PineRows(t, 9f, 3);
                Place(t, "Sawmill", new Vector3(-8f, 0f, 8f), 90f);
                Place(t, "Sled", new Vector3(5.6f, 0f, 16f), 15f);
                Place(t, "Cabin C", new Vector3(8.5f, 0f, 28f), -90f);
                Place(t, "Flag Wolf", new Vector3(-5.6f, 0f, 24f), 0f);
                Place(t, "Wolf Truck", new Vector3(-7.5f, 0f, 33f), 170f);
            });

            Environment("Frostpeak Town", t =>
            {
                string[] left = { "Cabin A", "Tavern", "Cabin B", "Cabin C" };
                string[] right = { "Cabin B", "Cabin C", "Forge", "Cabin A" };
                for (int i = 0; i < 4; i++)
                {
                    Place(t, left[i], new Vector3(-9f, 0f, 5f + i * 10f), -90f);
                    Place(t, right[i], new Vector3(9f, 0f, i * 10f), 90f);
                    Place(t, "Lantern", new Vector3(-5.3f, 0f, i * 10f), 180f);
                    Place(t, "Lantern", new Vector3(5.3f, 0f, 5f + i * 10f), 0f);
                }
                Place(t, "Sled", new Vector3(5.8f, 0f, 24f), 8f);
                Place(t, "Flag Wolf", new Vector3(-5.8f, 0f, 18f), 0f);
                Place(t, "Snowman", new Vector3(5.8f, 0f, 37f), 200f);
            });

            Environment("Crystal Mine", t =>
            {
                Place(t, "Mine", new Vector3(-10f, 0f, 12f), 90f);
                Place(t, "Mine", new Vector3(10.5f, 0f, 32f), -90f);
                Place(t, "Crystals", new Vector3(-5.8f, 0f, 3f), 20f);
                Place(t, "Crystals", new Vector3(6f, 0f, 9f), -40f);
                Place(t, "Crystals", new Vector3(-6.2f, 0f, 27f), 70f);
                Place(t, "Ice Rock B", new Vector3(6.5f, 0f, 20f));
                Place(t, "Ice Rock A", new Vector3(-5.8f, 0f, 36f));
                Place(t, "Mountain A", new Vector3(-22f, 0f, 22f));
                Place(t, "Mountain A", new Vector3(23f, 0f, 6f), 40f);
            });

            Environment("Wolf Fortress", t =>
            {
                for (int i = 0; i < 4; i++)
                {
                    Place(t, "Stone Wall", new Vector3(-6.8f, 0f, 5f + i * 10f), 90f);
                    Place(t, "Stone Wall", new Vector3(6.8f, 0f, 5f + i * 10f), -90f);
                }
                Place(t, "Wolf Tower", new Vector3(-9.5f, 0f, 20f), 90f);
                Place(t, "Wolf Tower", new Vector3(9.5f, 0f, 0f), -90f);
                Place(t, "Flag Wolf", new Vector3(-7f, 3.4f, 10f), 0f, 0.8f);
                Place(t, "Flag Wolf", new Vector3(7f, 3.4f, 30f), 180f, 0.8f);
                Place(t, "Wolf Camp", new Vector3(-13f, 0f, 34f), 90f);
                Place(t, "Wolf Truck", new Vector3(13.5f, 0f, 30f), 180f);
            });

            Environment("Summit", t =>
            {
                Place(t, "Mountain A", new Vector3(-16f, 0f, 14f), 10f);
                Place(t, "Mountain B", new Vector3(15f, 0f, 30f), -20f);
                Place(t, "Mountain B", new Vector3(17f, 0f, 2f), 50f, 0.8f);
                Place(t, "Crystals", new Vector3(-5.8f, 0f, 30f), 10f);
                Place(t, "Ice Rock B", new Vector3(5.8f, 0f, 14f));
                Place(t, "Ice Rock A", new Vector3(-5.6f, 0f, 2f));
                Place(t, "Snow Drift", new Vector3(5.5f, 0f, 24f));
                Place(t, "Snow Drift", new Vector3(-5.6f, 0f, 20f));
                Place(t, "Flag Wolf", new Vector3(5.8f, 0f, 36f), 160f);
            });
        }

        private static void RegisterAll()
        {
            LevelsDatabase database = AssetDatabase.LoadAssetAtPath<LevelsDatabase>(DustwaterContentBuilder.LEVELS_DATABASE);
            SerializedObject so = new SerializedObject(database);

            SerializedProperty environments = so.FindProperty("environmentData");
            foreach (var environment in ENVIRONMENT_IDS)
                Register(environments, environment.id, ENVIRONMENTS + "/Environment " + environment.name + ".prefab");

            Register(so.FindProperty("roadsData"), ROAD_SNOW, SNOW_ROAD_PREFAB);

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(database);
        }

        #endregion

        #region Diorama

        private static void BuildDiorama()
        {
            var scene = EditorSceneManager.OpenScene(DustwaterContentBuilder.GAME_SCENE, OpenSceneMode.Single);

            GameObject old = GameObject.Find("Frostpeak Town");
            if (old != null) Object.DestroyImmediate(old);

            GameObject root = new GameObject("Frostpeak Town");
            root.transform.position = DIORAMA_POSITION;
            DustwaterTownView view = root.AddComponent<DustwaterTownView>();
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("act").intValue = 1;

            Material material = DustwaterPalette.Material;
            AddMesh(root.transform, "Frost Ground", new PaletteMesh().Box(new Vector3(90f, 0.4f, 90f), 0f, new Vector3(0f, -0.2f, 10f), Vector3.zero, Band.Snow, 0.3f, 0.55f).Build("Frost_Town_Ground"), material);
            AddMesh(root.transform, "Frost Road", new PaletteMesh().Box(new Vector3(4.6f, 0.06f, 46f), 0f, new Vector3(0f, 0.02f, 6f), Vector3.zero, Band.Frost, 0.4f, 0.7f).Build("Frost_Town_Road"), material);
            Place(root.transform, "Mountain A", new Vector3(-24f, 0f, 46f), 10f, 1.3f);
            Place(root.transform, "Mountain B", new Vector3(3f, 0f, 54f), -8f, 1.6f);
            Place(root.transform, "Mountain A", new Vector3(30f, 0f, 48f), 190f, 1.2f);
            Place(root.transform, "Frozen Lake", new Vector3(17f, 0f, 30f), 0f, 1.1f);

            var plots = new (BuildingType type, string piece, Vector3 position, float yaw)[]
            {
                (BuildingType.CommandPost, "Lodge", new Vector3(-6.5f, 0f, -5f), 15f),
                (BuildingType.Barracks, "Log Barracks", new Vector3(7f, 0f, -5f), -15f),
                (BuildingType.Armory, "Forge", new Vector3(-7.5f, 0f, 5f), 20f),
                (BuildingType.Workshop, "Sawmill", new Vector3(7.5f, 0f, 5f), -20f),
                (BuildingType.FieldHospital, "Infirmary", new Vector3(-7.5f, 0f, 15f), 20f),
                (BuildingType.Saloon, "Tavern", new Vector3(7.5f, 0f, 15.5f), -20f),
                (BuildingType.Watchtower, "Lookout", new Vector3(0f, 0f, 25f), 0f),
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

                SerializedProperty element = plotList.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("type").intValue = (int)p.type;
                element.FindPropertyRelative("camp").objectReferenceValue = Place(plotRoot, "Wolf Camp", Vector3.zero);
                element.FindPropertyRelative("site").objectReferenceValue = Place(plotRoot, "Snow Site", Vector3.zero);
                element.FindPropertyRelative("building").objectReferenceValue = Place(plotRoot, p.piece, Vector3.zero);
                element.FindPropertyRelative("flag").objectReferenceValue = Place(plotRoot, "Flag Ranger", new Vector3(2.4f, 0f, 1.4f), 0f, 0.8f);
            }

            // one district per Frostpeak chapter (chapters 4-10 = indices 3-9): wolf flags turn blue
            var flagSpots = new (Vector3 position, int chapter)[]
            {
                (new Vector3(-3f, 0f, -9f), 3), (new Vector3(3.2f, 0f, -9f), 4), (new Vector3(-3.4f, 0f, 9f), 5),
                (new Vector3(3.4f, 0f, 11f), 6), (new Vector3(-3.4f, 0f, 19f), 7),
                (new Vector3(-3f, 0f, 27f), 8), (new Vector3(3f, 0f, 27f), 9),
            };

            SerializedProperty districts = so.FindProperty("districts");
            districts.arraySize = flagSpots.Length;
            for (int d = 0; d < flagSpots.Length; d++)
            {
                SerializedProperty district = districts.GetArrayElementAtIndex(d);
                district.FindPropertyRelative("chapter").intValue = flagSpots[d].chapter;
                SerializedProperty red = district.FindPropertyRelative("scorpionFlags");
                SerializedProperty blue = district.FindPropertyRelative("rangerFlags");
                red.arraySize = 1;
                blue.arraySize = 1;
                red.GetArrayElementAtIndex(0).objectReferenceValue = Place(root.transform, "Flag Wolf", flagSpots[d].position, 0f, 1f, "District " + d + " Flag Red");
                blue.GetArrayElementAtIndex(0).objectReferenceValue = Place(root.transform, "Flag Ranger", flagSpots[d].position, 0f, 1f, "District " + d + " Flag Blue");
            }

            Transform decor = new GameObject("Decor").transform;
            decor.SetParent(root.transform, false);
            Place(decor, "Cabin A", new Vector3(-16f, 0f, 7f), 25f);
            Place(decor, "Cabin B", new Vector3(-15.5f, 0f, -6f), 30f);
            Place(decor, "Cabin C", new Vector3(16f, 0f, 9f), -25f);
            Place(decor, "Cabin A", new Vector3(16.5f, 0f, -5f), -30f);
            Place(decor, "Sled", new Vector3(3.3f, 0f, 1.5f), 12f);
            Place(decor, "Snowman", new Vector3(-3.4f, 0f, 2f), 160f, 0.8f);
            for (int i = 0; i < 4; i++)
            {
                Place(decor, "Lantern", new Vector3(-2.9f, 0f, -6f + i * 9f), 180f, 0.9f);
                Place(decor, "Lantern", new Vector3(2.9f, 0f, -2f + i * 9f), 0f, 0.9f);
            }
            var pines = new[] { new Vector3(-20f, 0f, 14f), new Vector3(21f, 0f, 3f), new Vector3(12f, 0f, 36f), new Vector3(-11f, 0f, 34f), new Vector3(-21f, 0f, -2f), new Vector3(-19f, 0f, 26f), new Vector3(22f, 0f, 18f), new Vector3(-13f, 0f, -14f), new Vector3(14f, 0f, -14f) };
            for (int i = 0; i < pines.Length; i++) Place(decor, i % 3 == 0 ? "Pine B" : i % 3 == 1 ? "Pine A" : "Pine C", pines[i], i * 37f);
            Place(decor, "Ice Rock B", new Vector3(22f, 0f, 10f));
            Place(decor, "Ice Rock A", new Vector3(6f, 0f, 36f));
            Place(decor, "Snow Drift", new Vector3(-10f, 0f, 22f));

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
            cameraObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>().renderPostProcessing = false;
            cameraObject.SetActive(false);

            so.FindProperty("townCamera").objectReferenceValue = camera;
            so.FindProperty("cameraPivot").objectReferenceValue = pivot;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        #endregion
    }
}
