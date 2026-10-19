using System.IO;
using UnityEditor;
using UnityEngine;

namespace SquadCommand.Dustwater
{
    // New uniform colours for soldier_atlas.png. The template soldiers use 64px wide columns (0 = red team,
    // 1 = blue team) with a vertical gradient over the top 512px; the area x 384-767 is empty, so new team
    // columns are painted there with the same gradient profile as the blue column.
    public static class SoldierDyes
    {
        public const string ATLAS_PATH = "Assets/Project Files/Game/Textures/soldier_atlas.png";
        public const int COLUMN_WIDTH = 64;
        public const int ATLAS_SIZE = 1024;

        public const int RED = 0, BLUE = 1;
        public const int MEDIC_WHITE = 6, GRENADIER_OLIVE = 7, SHARPSHOOTER_TAN = 8, BOSS_BLACK = 9, BANDIT_PLUM = 10, BOSS_GOLD = 11;

        // act 2 dyes live in the empty bottom half of the atlas (columns 4-15): index = 100 + column
        public const int PYRO_CHARCOAL = 104, GUARDIAN_STEEL = 105, SCOUT_SNOW = 106, WOLF_ICE = 107, WOLF_NAVY = 108, WOLF_TEAL = 109,
            BOSS_CRIMSON = 110, BOSS_GLACIER = 111, BOSS_TIMBER = 112, BOSS_LAVENDER = 113, BOSS_CRYSTAL = 114, BOSS_NIGHT = 115;

        private static readonly (int column, Color32 color)[] DYES =
        {
            (MEDIC_WHITE, new Color32(236, 238, 242, 255)),
            (GRENADIER_OLIVE, new Color32(112, 140, 58, 255)),
            (SHARPSHOOTER_TAN, new Color32(190, 156, 96, 255)),
            (BOSS_BLACK, new Color32(58, 56, 66, 255)),
            (BANDIT_PLUM, new Color32(150, 58, 112, 255)),
            (BOSS_GOLD, new Color32(232, 168, 44, 255)),
            (PYRO_CHARCOAL, new Color32(72, 70, 80, 255)),
            (GUARDIAN_STEEL, new Color32(104, 124, 150, 255)),
            (SCOUT_SNOW, new Color32(196, 212, 228, 255)),
            (WOLF_ICE, new Color32(110, 168, 214, 255)),
            (WOLF_NAVY, new Color32(62, 82, 128, 255)),
            (WOLF_TEAL, new Color32(52, 138, 150, 255)),
            (BOSS_CRIMSON, new Color32(196, 52, 60, 255)),
            (BOSS_GLACIER, new Color32(84, 150, 214, 255)),
            (BOSS_TIMBER, new Color32(132, 90, 56, 255)),
            (BOSS_LAVENDER, new Color32(150, 116, 196, 255)),
            (BOSS_CRYSTAL, new Color32(56, 194, 222, 255)),
            (BOSS_NIGHT, new Color32(40, 44, 62, 255)),
        };

        [MenuItem("Tools/Dustwater/Art/Paint Soldier Dyes", priority = 101)]
        public static void Paint()
        {
            Texture2D atlas = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            atlas.LoadImage(File.ReadAllBytes(ATLAS_PATH));

            // gradient profile of the blue column: 0 at the top, 1 at the bottom of the 512px strip
            int blueX = BLUE * COLUMN_WIDTH + COLUMN_WIDTH / 2;
            Color top = atlas.GetPixel(blueX, ATLAS_SIZE - 1);
            Color bottom = atlas.GetPixel(blueX, ATLAS_SIZE - 512);

            foreach (var dye in DYES)
            {
                Color baseColor = dye.color;
                Color light = Color.Lerp(baseColor, Color.white, 0.45f);

                int column = dye.column % 100;
                int rowOffset = dye.column >= 100 ? 512 : 0;
                for (int row = 0; row < 512; row++)
                {
                    int y = ATLAS_SIZE - 1 - row;
                    int targetY = y - rowOffset;
                    Color blue = atlas.GetPixel(blueX, y);
                    float t = Mathf.InverseLerp(top.r, bottom.r, blue.r);
                    Color color = Color.Lerp(baseColor, light, t);

                    for (int x = 0; x < COLUMN_WIDTH; x++)
                        atlas.SetPixel(column * COLUMN_WIDTH + x, targetY, color);
                }
            }

            atlas.Apply();
            File.WriteAllBytes(ATLAS_PATH, atlas.EncodeToPNG());
            Object.DestroyImmediate(atlas);
            AssetDatabase.ImportAsset(ATLAS_PATH, ImportAssetOptions.ForceUpdate);
            Debug.Log("[Dustwater] Soldier dyes painted");
        }

        // Copy of a soldier mesh with the team column moved to another column
        public static Mesh Redye(Mesh source, int fromColumn, int toColumn, string name)
        {
            Mesh mesh = Object.Instantiate(source);
            mesh.name = name;

            Vector2[] uvs = mesh.uv;
            float offset = (toColumn % 100 - fromColumn) * COLUMN_WIDTH / (float)ATLAS_SIZE;
            float down = toColumn >= 100 ? 0.5f : 0f;
            for (int i = 0; i < uvs.Length; i++)
            {
                int column = Mathf.FloorToInt(uvs[i].x * ATLAS_SIZE / COLUMN_WIDTH);
                bool topHalf = uvs[i].y >= 0.5f;
                if (column == fromColumn && topHalf)
                {
                    uvs[i].x += offset;
                    uvs[i].y -= down;
                }
            }

            mesh.uv = uvs;
            return mesh;
        }
    }
}
