using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SquadCommand.Dustwater
{
    // Review helper: renders prefabs/objects into one contact sheet PNG (used while building art, not by the game)
    public static class DustwaterPreview
    {
        public static void Sheet(IList<GameObject> prefabs, string outPath, Vector3 direction, int columns = 4, Color? background = null)
        {
            List<Texture2D> tiles = new List<Texture2D>();
            using (ArtRenderer renderer = new ArtRenderer(background ?? new Color(0.55f, 0.72f, 0.9f, 1f)))
            {
                foreach (GameObject prefab in prefabs)
                {
                    if (prefab == null) continue;

                    GameObject instance = renderer.Spawn(prefab);
                    instance.transform.position = Vector3.zero;
                    tiles.Add(renderer.Render(instance, direction, 1.05f, false, 30f));
                    Object.DestroyImmediate(instance);
                }
            }

            SaveSheet(tiles, outPath, columns);
        }

        public static void SaveSheet(List<Texture2D> tiles, string outPath, int columns)
        {
            if (tiles.Count == 0) return;

            int size = tiles[0].width;
            int rows = Mathf.CeilToInt(tiles.Count / (float)columns);
            Texture2D sheet = new Texture2D(size * Mathf.Min(columns, tiles.Count), size * rows, TextureFormat.RGBA32, false);

            for (int i = 0; i < tiles.Count; i++)
            {
                int x = (i % columns) * size;
                int y = (rows - 1 - i / columns) * size;
                sheet.SetPixels(x, y, size, size, tiles[i].GetPixels());
                Object.DestroyImmediate(tiles[i]);
            }

            sheet.Apply();
            Directory.CreateDirectory(Path.GetDirectoryName(outPath));
            File.WriteAllBytes(outPath, sheet.EncodeToPNG());
            Object.DestroyImmediate(sheet);
        }
    }
}
