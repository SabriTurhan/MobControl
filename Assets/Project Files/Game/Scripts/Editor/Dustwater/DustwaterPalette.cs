using System.IO;
using UnityEditor;
using UnityEngine;

namespace Watermelon.Dustwater
{
    // Generates the palette atlas used by every new Dustwater mesh and the two materials that read it
    // (copies of the template's Environment/Soldiers URP materials, so lighting, toon ramp and rim match).
    public static class DustwaterPalette
    {
        public const string FOLDER = "Assets/Project Files/Game/Textures/Dustwater";
        public const string TEXTURE_PATH = FOLDER + "/dustwater_palette.png";
        public const string MATERIAL_FOLDER = "Assets/Project Files/Game/Materials/Dustwater";
        public const string MATERIAL_PATH = MATERIAL_FOLDER + "/Dustwater URP.mat";
        public const string MATERIAL_GLOW_PATH = MATERIAL_FOLDER + "/Dustwater Units URP.mat";

        private static readonly Color32[] COLORS = new Color32[Band.COUNT]
        {
            new Color32(236, 199, 152, 255), // Adobe
            new Color32(204, 104, 62, 255),  // Terracotta
            new Color32(156, 100, 62, 255),  // Wood
            new Color32(100, 63, 42, 255),   // DarkWood
            new Color32(244, 208, 142, 255), // Sand
            new Color32(208, 124, 52, 255),  // Rust
            new Color32(228, 76, 74, 255),   // ScorpionRed
            new Color32(78, 132, 253, 255),  // RangerBlue
            new Color32(146, 151, 162, 255), // Steel
            new Color32(68, 72, 84, 255),    // Iron
            new Color32(226, 212, 178, 255), // Canvas
            new Color32(122, 142, 62, 255),  // Olive
            new Color32(80, 178, 84, 255),   // Cactus
            new Color32(84, 198, 246, 255),  // Water
            new Color32(255, 202, 64, 255),  // Gold
            new Color32(246, 246, 246, 255), // White
            new Color32(48, 48, 56, 255),    // Black
            new Color32(112, 128, 170, 255), // Slate
            new Color32(126, 101, 132, 255), // RoadPurple
            new Color32(160, 214, 255, 255), // Glass
            new Color32(255, 241, 204, 255), // Cream
            new Color32(172, 70, 50, 255),   // Brick
            new Color32(204, 188, 142, 255), // Khaki
            new Color32(242, 182, 140, 255), // Skin
            new Color32(168, 218, 78, 255),  // Lime
            new Color32(126, 40, 58, 255),   // Maroon
            new Color32(255, 152, 52, 255),  // Orange
            new Color32(44, 152, 142, 255),  // Teal
            new Color32(242, 124, 154, 255), // Pink
            new Color32(44, 64, 124, 255),   // Navy
            new Color32(238, 228, 204, 255), // Bone
            new Color32(255, 94, 34, 255),   // Fire
            new Color32(244, 248, 252, 255), // Snow
            new Color32(150, 212, 242, 255), // IceBlue
            new Color32(86, 146, 200, 255),  // DeepIce
            new Color32(46, 118, 86, 255),   // Pine
            new Color32(30, 80, 64, 255),    // PineDark
            new Color32(214, 200, 176, 255), // Fur
            new Color32(126, 102, 82, 255),  // FurDark
            new Color32(132, 138, 156, 255), // WolfGrey
            new Color32(120, 226, 255, 255), // Crystal
            new Color32(214, 232, 246, 255), // Frost
            new Color32(148, 96, 58, 255),   // Log
            new Color32(255, 196, 96, 255),  // Lantern
            new Color32(44, 56, 96, 255),    // NightBlue
            new Color32(62, 62, 68, 255),    // Charcoal
            new Color32(255, 124, 44, 255),  // Ember
            new Color32(112, 132, 74, 255),  // Moss
        };

        public static Color GetColor(int band) => COLORS[band];

        [MenuItem("Tools/Dustwater/Art/Generate Palette + Materials", priority = 100)]
        public static void Generate()
        {
            Directory.CreateDirectory(FOLDER);
            Directory.CreateDirectory(MATERIAL_FOLDER);

            int size = Band.ATLAS_SIZE;
            int height = Band.ATLAS_HEIGHT;
            Texture2D texture = new Texture2D(size, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * height];

            for (int band = 0; band < Band.COUNT; band++)
            {
                Color baseColor = COLORS[band];
                Color light = Color.Lerp(baseColor, Color.white, 0.28f);
                Color dark = baseColor * 0.62f;
                dark.a = 1f;

                for (int row = 0; row < Band.BAND_HEIGHT; row++)
                {
                    int y = height - 1 - (band * Band.BAND_HEIGHT + row);
                    for (int x = 0; x < size; x++)
                    {
                        float t = x / (float)(size - 1);
                        Color color = t < 0.5f ? Color.Lerp(light, baseColor, t * 2f) : Color.Lerp(baseColor, dark, (t - 0.5f) * 2f);
                        pixels[y * size + x] = color;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(TEXTURE_PATH, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(TEXTURE_PATH, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(TEXTURE_PATH);
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();

            Texture2D palette = AssetDatabase.LoadAssetAtPath<Texture2D>(TEXTURE_PATH);
            CreateMaterial(MATERIAL_PATH, "Assets/Project Files/Game/Materials/Final/Environment URP.mat", palette, false);
            CreateMaterial(MATERIAL_GLOW_PATH, "Assets/Project Files/Game/Materials/Final/Soldiers URP.mat", palette, true);

            AssetDatabase.SaveAssets();
            Debug.Log("[Dustwater] Palette + materials generated");
        }

        private static void CreateMaterial(string path, string templatePath, Texture2D palette, bool hitFlash)
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

            material.SetTexture("_MainTex", palette);
            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", palette);
            EditorUtility.SetDirty(material);
        }

        public static Material Material => AssetDatabase.LoadAssetAtPath<Material>(MATERIAL_PATH);
        public static Material GlowMaterial => AssetDatabase.LoadAssetAtPath<Material>(MATERIAL_GLOW_PATH);
    }
}
