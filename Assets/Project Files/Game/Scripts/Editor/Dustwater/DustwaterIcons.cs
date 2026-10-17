using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Watermelon.Dustwater
{
    // Renders card portraits and UI icons in the style of the template's gate icons (Images/Base/Gates):
    // front-facing bust, blank face, soft toon light, thick dark outline, transparent background.
    public static class DustwaterIcons
    {
        public const string FOLDER = "Assets/Project Files/Game/Images/Dustwater";
        public const string PORTRAITS = FOLDER + "/Portraits";
        public const string ICONS = FOLDER + "/Icons";
        public const string SUPPLIES_ICON = ICONS + "/supplies.png";

        private const string IMPORT_TEMPLATE = "Assets/Project Files/Game/Images/Base/Gates/booster_soldier.png";
        private const string GATES = "Assets/Project Files/Game/Images/Base/Gates/";
        private const string PREFABS = "Assets/Project Files/Game/Prefabs/";

        private static readonly Color OUTLINE = new Color32(38, 36, 48, 255);

        public static readonly (string id, string prefab)[] PORTRAIT_SOURCES =
        {
            ("ranger", PREFABS + "Player/Humanoid Characters/Default Humanoid Character.prefab"),
            ("heavy", PREFABS + "Player/Humanoid Characters/Heavy Humanoid Character.prefab"),
            ("sharpshooter", DustwaterUnitsBuilder.SHARPSHOOTER),
            ("medic", DustwaterUnitsBuilder.MEDIC),
            ("grenadier", DustwaterUnitsBuilder.GRENADIER),
            ("bandit", PREFABS + "Enemy/Enemy Human Behavior.prefab"),
            ("bandit_heavy", PREFABS + "Enemy/Enemy Heavy Human Behavior.prefab"),
            ("bandit_sniper", DustwaterUnitsBuilder.BANDIT_SNIPER),
            ("boss_rattler", DustwaterUnitsBuilder.BOSS_RATTLER),
            ("boss_jaws", DustwaterUnitsBuilder.BOSS_JAWS),
            ("boss_vex", DustwaterUnitsBuilder.BOSS_VEX),
        };

        [MenuItem("Tools/Dustwater/Art/Render Portraits + Icons", priority = 120)]
        public static void RenderAll()
        {
            System.IO.Directory.CreateDirectory(PORTRAITS);
            System.IO.Directory.CreateDirectory(ICONS);

            using (ArtRenderer renderer = new ArtRenderer(new Color(0f, 0f, 0f, 0f)))
            {
                foreach (var source in PORTRAIT_SOURCES)
                {
                    GameObject instance = renderer.Spawn(AssetDatabase.LoadAssetAtPath<GameObject>(source.prefab));
                    Texture2D texture = RenderBust(renderer, instance);
                    Object.DestroyImmediate(instance);
                    Save(texture, PORTRAITS + "/" + source.id + ".png");
                }

                // Supplies: the pickup crate, 3/4 view
                GameObject supplies = renderer.Spawn(AssetDatabase.LoadAssetAtPath<GameObject>(DustwaterUnitsBuilder.SUPPLIES_DROP));
                Save(renderer.Render(supplies, new Vector3(-0.6f, -0.55f, -1f), 0.8f, true), SUPPLIES_ICON);
                Object.DestroyImmediate(supplies);

                GameObject sandbags = renderer.Spawn(AssetDatabase.LoadAssetAtPath<GameObject>(DustwaterUnitsBuilder.SANDBAGS));
                Save(renderer.Render(sandbags, new Vector3(-0.35f, -0.5f, -1f), 0.8f, true), ICONS + "/threat_barricades.png");
                Object.DestroyImmediate(sandbags);

                // Swarm: three bandits in a wedge
                GameObject group = new GameObject("Group");
                renderer.Add(group);
                Vector3[] offsets = { new Vector3(-0.55f, 0f, -0.3f), new Vector3(0.55f, 0f, -0.3f), new Vector3(0f, 0f, 0.2f) };
                foreach (Vector3 offset in offsets)
                {
                    GameObject bandit = renderer.Spawn(AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS + "Enemy/Enemy Human Behavior.prefab"));
                    Pose(bandit);
                    bandit.transform.SetParent(group.transform, false);
                    bandit.transform.localPosition = offset;
                    bandit.transform.localRotation = Quaternion.Euler(0f, 180f + offset.x * 20f, 0f);
                }
                Save(renderer.RenderFrom(new Vector3(0f, 1.9f, -3.6f), new Vector3(0f, 1.05f, 0f), 30f), ICONS + "/threat_swarm.png");
                Object.DestroyImmediate(group);
            }

            AssetDatabase.Refresh();
            Debug.Log("[Dustwater] Portraits and icons rendered");
        }

        // Head and shoulders, seen from the front (characters face +z)
        private static Texture2D RenderBust(ArtRenderer renderer, GameObject instance)
        {
            Pose(instance);

            SkinnedMeshRenderer body = instance.GetComponentInChildren<SkinnedMeshRenderer>();
            Transform head = System.Array.Find(body.bones, b => b.name == "Head");
            float scale = instance.transform.lossyScale.y;
            Vector3 focus = head.position + Vector3.up * 0.06f * scale;

            return renderer.RenderFrom(focus + new Vector3(0f, 0.2f, 2.15f) * scale, focus, 30f);
        }

        // Idle frame instead of the bind T-pose
        private static void Pose(GameObject instance)
        {
            Animator animator = instance.GetComponentInChildren<Animator>();
            if (animator == null || animator.runtimeAnimatorController == null) return;

            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == "Idle")
                {
                    clip.SampleAnimation(animator.gameObject, 0.3f);
                    return;
                }
            }
        }

        private static void Save(Texture2D texture, string path)
        {
            Texture2D outlined = ArtRenderer.Outline(texture, 12, 0, OUTLINE, OUTLINE);
            Object.DestroyImmediate(texture);
            ArtRenderer.SaveSprite(outlined, path, IMPORT_TEMPLATE);
            Object.DestroyImmediate(outlined);
        }

        public static Sprite Portrait(string id) => AssetDatabase.LoadAssetAtPath<Sprite>(PORTRAITS + "/" + id + ".png");
        public static Sprite Icon(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(ICONS + "/" + name + ".png");

        public static void AssignIcons(CampaignDatabase database)
        {
            foreach (UnitCardData unit in database.Units)
                unit.portrait = Portrait(unit.id);

            Dictionary<string, string> weaponArt = new Dictionary<string, string>()
            {
                { "pistol", "booster_pistol" }, { "shotgun", "booster_shotgun" }, { "rifle", "booster_rifle" }, { "minigun", "booster_minigun" },
            };
            foreach (WeaponCardData weapon in database.Weapons)
            {
                if (weaponArt.TryGetValue(weapon.id, out string file))
                    weapon.portrait = AssetDatabase.LoadAssetAtPath<Sprite>(GATES + file + ".png");
            }

            foreach (ThreatInfo threat in database.Threats)
            {
                switch (threat.type)
                {
                    case ThreatType.Armored: threat.icon = Portrait("bandit_heavy"); break;
                    case ThreatType.Swarm: threat.icon = Icon("threat_swarm"); break;
                    case ThreatType.Snipers: threat.icon = Portrait("bandit_sniper"); break;
                    case ThreatType.Barricades: threat.icon = Icon("threat_barricades"); break;
                    case ThreatType.Attrition: threat.icon = Portrait("bandit"); break;
                }
            }
        }
    }
}
