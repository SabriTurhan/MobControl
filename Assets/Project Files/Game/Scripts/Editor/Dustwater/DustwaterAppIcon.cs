using System.IO;
using UnityEditor;
using UnityEngine;

namespace SquadCommand.Dustwater
{
    // App icon: the Ranger squad (intro actors, guns in hand) in front of the Dustwater saloon and watchtower,
    // low hero angle over a sky gradient. Overwrites Images/Icon/icon.png so the Player Settings keep their icon.
    public static class DustwaterAppIcon
    {
        public const string ICON_PATH = "Assets/Project Files/Game/Images/Icon/icon.png";
        private const int SIZE = 1024;
        private const string ACTORS = DustwaterUnitsBuilder.PREFABS + "/Actors/";
        private const string TOWN = DustwaterUnitsBuilder.PREFABS + "/Town/";

        private static readonly Color SKY_TOP = new Color32(64, 150, 255, 255);
        private static readonly Color SKY_BOTTOM = new Color32(190, 228, 255, 255);

        [MenuItem("Tools/Dustwater/Art/Render App Icon", priority = 125)]
        public static void Render()
        {
            Texture2D shot;
            using (ArtRenderer renderer = new ArtRenderer(new Color(0f, 0f, 0f, 0f), SIZE))
            {
                GameObject ground = new GameObject("Ground");
                ground.AddComponent<MeshFilter>().sharedMesh = new PaletteMesh().Box(new Vector3(40f, 0.2f, 40f), 0f, new Vector3(0f, -0.1f, 8f), Vector3.zero, Band.Sand, 0.3f, 0.55f).Build("Icon_Ground");
                ground.AddComponent<MeshRenderer>().sharedMaterial = DustwaterPalette.Material;
                renderer.Add(ground);

                Place(renderer, TOWN + "Saloon.prefab", new Vector3(-2.6f, 0f, 7.5f), 200f);
                Place(renderer, TOWN + "Watchtower.prefab", new Vector3(3.6f, 0f, 8.5f), 160f);
                Place(renderer, TOWN + "Flag Ranger.prefab", new Vector3(1.1f, 0f, 5.2f), 180f);
                Place(renderer, TOWN + "Cactus B.prefab", new Vector3(5.2f, 0f, 4.5f), 0f);

                // squad in a wedge facing the camera (-z), the Heavy in front
                Place(renderer, ACTORS + "Sharpshooter Actor.prefab", new Vector3(-1.0f, 0f, 0.9f), 200f, true);
                Place(renderer, ACTORS + "Pyro Actor.prefab", new Vector3(1.05f, 0f, 0.8f), 160f, true);
                Place(renderer, ACTORS + "Heavy Actor.prefab", new Vector3(0f, 0f, 0f), 172f, true);

                shot = renderer.RenderFrom(new Vector3(0.15f, 0.95f, -3.1f), new Vector3(0f, 1.3f, 1.2f), 44f);
            }

            // sky gradient behind the render
            Color[] pixels = shot.GetPixels();
            for (int y = 0; y < SIZE; y++)
            {
                Color sky = Color.Lerp(SKY_BOTTOM, SKY_TOP, y / (float)(SIZE - 1));
                for (int x = 0; x < SIZE; x++)
                {
                    int i = y * SIZE + x;
                    Color c = pixels[i];
                    pixels[i] = new Color(Mathf.Lerp(sky.r, c.r, c.a), Mathf.Lerp(sky.g, c.g, c.a), Mathf.Lerp(sky.b, c.b, c.a), 1f);
                }
            }
            shot.SetPixels(pixels);
            shot.Apply();

            File.WriteAllBytes(ICON_PATH, shot.EncodeToPNG());
            Object.DestroyImmediate(shot);
            AssetDatabase.ImportAsset(ICON_PATH, ImportAssetOptions.ForceUpdate);
            Debug.Log("[Dustwater] App icon rendered");
        }

        /// <summary>Store cover background (square render, the store art script crops it to 800x470 and adds the title).</summary>
        [MenuItem("Tools/Dustwater/Art/Render Store Cover Art", priority = 126)]
        public static void RenderCover()
        {
            Texture2D shot;
            using (ArtRenderer renderer = new ArtRenderer(new Color(0f, 0f, 0f, 0f), 1600))
            {
                GameObject ground = new GameObject("Ground");
                ground.AddComponent<MeshFilter>().sharedMesh = new PaletteMesh().Box(new Vector3(80f, 0.2f, 80f), 0f, new Vector3(0f, -0.1f, 15f), Vector3.zero, Band.Sand, 0.3f, 0.55f).Build("Cover_Ground");
                ground.AddComponent<MeshRenderer>().sharedMaterial = DustwaterPalette.Material;
                renderer.Add(ground);

                Place(renderer, TOWN + "Saloon.prefab", new Vector3(-5.2f, 0f, 9f), 205f);
                Place(renderer, TOWN + "Store Blue.prefab", new Vector3(-10.5f, 0f, 11f), 215f);
                Place(renderer, TOWN + "Watchtower.prefab", new Vector3(5.5f, 0f, 10f), 160f);
                Place(renderer, TOWN + "Water Tower.prefab", new Vector3(11f, 0f, 13f), 150f);
                Place(renderer, TOWN + "Flag Ranger.prefab", new Vector3(1.6f, 0f, 6.5f), 180f);
                Place(renderer, TOWN + "Cactus B.prefab", new Vector3(8.5f, 0f, 5f), 0f);
                Place(renderer, TOWN + "Mesa A.prefab", new Vector3(-16f, 0f, 30f), 10f);
                Place(renderer, TOWN + "Mesa B.prefab", new Vector3(14f, 0f, 34f), -15f);

                // the squad in a wide line, the Heavy in front
                Place(renderer, ACTORS + "Scout Actor.prefab", new Vector3(-2.9f, 0f, 1.6f), 205f, true);
                Place(renderer, ACTORS + "Sharpshooter Actor.prefab", new Vector3(-1.45f, 0f, 0.9f), 195f, true);
                Place(renderer, ACTORS + "Heavy Actor.prefab", new Vector3(0f, 0f, 0f), 175f, true);
                Place(renderer, ACTORS + "Pyro Actor.prefab", new Vector3(1.5f, 0f, 0.9f), 165f, true);
                Place(renderer, ACTORS + "Guardian Actor.prefab", new Vector3(3.0f, 0f, 1.7f), 155f, true);

                shot = renderer.RenderFrom(new Vector3(0f, 1.25f, -5.6f), new Vector3(0f, 1.7f, 2f), 52f);
            }

            File.WriteAllBytes(Path.Combine(Application.dataPath, "../Temp/cover_render.png"), shot.EncodeToPNG());
            Object.DestroyImmediate(shot);
            Debug.Log("[Dustwater] Cover art rendered to Temp/cover_render.png");
        }

        private static void Place(ArtRenderer renderer, string path, Vector3 position, float yaw, bool pose = false)
        {
            GameObject instance = renderer.Spawn(AssetDatabase.LoadAssetAtPath<GameObject>(path));
            instance.transform.position = position;
            instance.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            if (!pose) return;

            // idle body plus the weapon's arm pose (the game plays it on an upper-body layer)
            DustwaterIcons.Pose(instance);
            Animator animator = instance.GetComponentInChildren<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
                foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
                    if (clip.name.EndsWith("_pose")) { clip.SampleAnimation(animator.gameObject, 0f); break; }
            // self-shadowing on the skinned soldiers bands at this camera distance: they only cast
            foreach (Renderer part in instance.GetComponentsInChildren<Renderer>(true)) part.receiveShadows = false;
        }
    }
}
