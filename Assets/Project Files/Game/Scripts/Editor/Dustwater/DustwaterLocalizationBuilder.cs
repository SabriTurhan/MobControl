using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace Watermelon.Dustwater
{
    // Localization setup:
    // 1. Fonts: Fredoka One's static atlases only hold ASCII. Two dynamic fallbacks are added to every Fredoka
    //    font asset: "Dustwater Fredoka" (Fredoka One + generated Turkish glyphs, covers all Latin languages) and
    //    "Nunito Black" (Cyrillic). Fallback materials copy the outline/shadow look of the primary material.
    //    Fallback atlases are prefilled with every character of the string table.
    // 2. Template labels get a LocalizedText component (matched by their English text / prefab).
    // 3. Settings gets a Language row.
    public static class DustwaterLocalizationBuilder
    {
        private const string FONTS = "Assets/Project Files/Game/Fonts";
        private const string DW_FONTS = FONTS + "/Dustwater";
        private const string TABLE = "Assets/Project Files/Game/Resources/Localization/strings.txt";
        private const string PREFABS = "Assets/Project Files/Game/Prefabs/";

        public const string SDF_FONT = DW_FONTS + "/Dustwater Rounded SDF.asset";
        public const string SDF_CYRILLIC = DW_FONTS + "/Dustwater Cyrillic SDF.asset";
        public const string MAT_PLAIN = DW_FONTS + "/Dustwater Rounded - Plain.mat";
        public const string MAT_OUTLINE = DW_FONTS + "/Dustwater Rounded - Outline.mat";
        public const string MAT_OUTLINE_LIGHT = DW_FONTS + "/Dustwater Rounded - Outline Light.mat";
        public const string MAT_SHADOW = DW_FONTS + "/Dustwater Rounded - Shadow.mat";
        public const string MAT_GLOW = DW_FONTS + "/Dustwater Rounded - Glow.mat";

        [MenuItem("Tools/Dustwater/Content/Localization (fonts, labels, settings)", priority = 230)]
        public static void BuildAll()
        {
            BuildFonts();
            BuildSdfFonts();
            SwapToSdf();
            LocalizeTemplateLabels();
            AddLanguageSettingsRow();
            AssetDatabase.SaveAssets();
            Debug.Log("[Dustwater] Localization set up");
        }

        #region Fonts

        private static void BuildFonts()
        {
            string characters = TableCharacters();

            TMP_FontAsset fredoka50 = DynamicFont(DW_FONTS + "/DustwaterFredoka.ttf", DW_FONTS + "/Dustwater Fredoka 50 SDF.asset", 50, 9, characters);
            TMP_FontAsset nunito50 = DynamicFont(DW_FONTS + "/NunitoBlack.ttf", DW_FONTS + "/Nunito Black 50 SDF.asset", 50, 9, characters);
            TMP_FontAsset fredoka120 = DynamicFont(DW_FONTS + "/DustwaterFredoka.ttf", DW_FONTS + "/Dustwater Fredoka 120 SDF.asset", 120, 14, characters);
            TMP_FontAsset nunito120 = DynamicFont(DW_FONTS + "/NunitoBlack.ttf", DW_FONTS + "/Nunito Black 120 SDF.asset", 120, 14, characters);

            foreach (string guid in AssetDatabase.FindAssets("t:TMP_FontAsset", new[] { FONTS + "/FredokaOne" }))
            {
                TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
                bool big = font.faceInfo.pointSize > 80;
                font.fallbackFontAssetTable = new List<TMP_FontAsset>() { big ? fredoka120 : fredoka50, big ? nunito120 : nunito50 };
                EditorUtility.SetDirty(font);
            }

            // anything else using TMP (LiberationSans defaults, core messages) falls back globally
            SerializedObject settings = new SerializedObject(TMP_Settings.instance);
            SerializedProperty global = settings.FindProperty("m_fallbackFontAssets");
            global.ClearArray();
            foreach (TMP_FontAsset fallback in new[] { fredoka50, nunito50 })
            {
                global.InsertArrayElementAtIndex(global.arraySize);
                global.GetArrayElementAtIndex(global.arraySize - 1).objectReferenceValue = fallback;
            }
            settings.ApplyModifiedPropertiesWithoutUndo();
        }

        private static string TableCharacters()
        {
            HashSet<char> set = new HashSet<char>();
            for (char c = (char)32; c < 127; c++) set.Add(c);
            foreach (char c in "«»¿¡–—…’‘“” ") set.Add(c);
            foreach (char c in File.ReadAllText(TABLE)) if (!char.IsControl(c)) set.Add(c);
            // upper/lower case of everything, culture independent and Turkish
            foreach (char c in set.ToList())
            {
                set.Add(char.ToUpperInvariant(c));
                set.Add(char.ToLowerInvariant(c));
            }
            set.Add('İ'); set.Add('ı');
            return new string(set.ToArray());
        }

        private static TMP_FontAsset DynamicFont(string fontPath, string assetPath, int pointSize, int padding, string characters)
        {
            AssetDatabase.ImportAsset(fontPath);
            Font source = AssetDatabase.LoadAssetAtPath<Font>(fontPath);

            TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (existing != null) AssetDatabase.DeleteAsset(assetPath);

            TMP_FontAsset font = TMP_FontAsset.CreateFontAsset(source, pointSize, padding, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
            font.name = Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.CreateAsset(font, assetPath);

            font.atlasTexture.name = font.name + " Atlas";
            font.material.name = font.name + " Material";
            AssetDatabase.AddObjectToAsset(font.atlasTexture, font);
            AssetDatabase.AddObjectToAsset(font.material, font);

            font.TryAddCharacters(characters, out string missing);
            // multi-atlas pages created while filling must be stored too
            foreach (Texture2D page in font.atlasTextures)
                if (page != null && !AssetDatabase.Contains(page)) { page.name = font.name + " Atlas " + font.atlasTextures.ToList().IndexOf(page); AssetDatabase.AddObjectToAsset(page, font); }

            if (!string.IsNullOrEmpty(missing))
            {
                string shown = new string(missing.Where(c => !char.IsWhiteSpace(c)).Distinct().Take(40).ToArray());
                if (shown.Length > 0) Debug.Log("[Dustwater] " + font.name + " lacks: " + shown + " (expected for the other script)");
            }

            EditorUtility.SetDirty(font);
            AssetDatabase.SaveAssets();
            return font;
        }

        #endregion

        #region SDF fonts

        // The template fonts are bitmap fonts with the outline/shadow painted into the atlas, which fallback glyphs
        // cannot copy. The game switches to an SDF version of the same typeface; the template styles become
        // material presets, and the Cyrillic fallback inherits whichever preset a text uses.
        private static void BuildSdfFonts()
        {
            string characters = TableCharacters();
            TMP_FontAsset rounded = SdfFont(DW_FONTS + "/DustwaterFredoka.ttf", SDF_FONT, characters);
            TMP_FontAsset cyrillic = SdfFont(DW_FONTS + "/NunitoBlack.ttf", SDF_CYRILLIC, characters);
            rounded.fallbackFontAssetTable = new List<TMP_FontAsset>() { cyrillic };
            EditorUtility.SetDirty(rounded);

            Color dark = new Color32(56, 54, 60, 255);
            Preset(rounded, MAT_PLAIN, m => { });
            Preset(rounded, MAT_OUTLINE, m =>
            {
                m.SetFloat("_OutlineWidth", 0.2f);
                m.SetColor("_OutlineColor", dark);
                Underlay(m, dark, 0f, -0.45f, 0.2f, 0f);
            });
            Preset(rounded, MAT_OUTLINE_LIGHT, m => Underlay(m, new Color(0f, 0f, 0f, 0.28f), 0f, -0.35f, 0.25f, 0.55f));
            Preset(rounded, MAT_SHADOW, m => Underlay(m, new Color32(30, 30, 46, 230), 0f, -0.75f, 0.12f, 0.05f));
            Preset(rounded, MAT_GLOW, m => Underlay(m, new Color(1f, 1f, 1f, 0.75f), 0f, 0f, 0.45f, 0.85f));
            AssetDatabase.SaveAssets();
        }

        private static TMP_FontAsset SdfFont(string fontPath, string assetPath, string characters)
        {
            AssetDatabase.ImportAsset(fontPath);
            Font source = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath) != null) AssetDatabase.DeleteAsset(assetPath);

            TMP_FontAsset font = TMP_FontAsset.CreateFontAsset(source, 72, 10, GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic, true);
            font.name = Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.CreateAsset(font, assetPath);
            font.atlasTexture.name = font.name + " Atlas";
            font.material.name = font.name + " Material";
            AssetDatabase.AddObjectToAsset(font.atlasTexture, font);
            AssetDatabase.AddObjectToAsset(font.material, font);

            font.TryAddCharacters(characters, out string _);
            for (int i = 0; i < font.atlasTextures.Length; i++)
            {
                Texture2D page = font.atlasTextures[i];
                if (page != null && !AssetDatabase.Contains(page)) { page.name = font.name + " Atlas " + i; AssetDatabase.AddObjectToAsset(page, font); }
            }

            EditorUtility.SetDirty(font);
            AssetDatabase.SaveAssets();
            return font;
        }

        private static void Preset(TMP_FontAsset font, string path, System.Action<Material> setup)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(font.material);
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = font.material.shader;
                material.CopyPropertiesFromMaterial(font.material);
            }

            material.SetTexture("_MainTex", font.atlasTexture);
            material.SetColor("_FaceColor", Color.white);
            material.SetFloat("_OutlineWidth", 0f);
            material.DisableKeyword("UNDERLAY_ON");
            setup(material);
            EditorUtility.SetDirty(material);
        }

        private static void Underlay(Material material, Color color, float offsetX, float offsetY, float dilate, float softness)
        {
            material.EnableKeyword("UNDERLAY_ON");
            material.SetColor("_UnderlayColor", color);
            material.SetFloat("_UnderlayOffsetX", offsetX);
            material.SetFloat("_UnderlayOffsetY", offsetY);
            material.SetFloat("_UnderlayDilate", dilate);
            material.SetFloat("_UnderlaySoftness", softness);
        }

        public static Material PresetFor(TMP_Text text)
        {
            string name = (text.fontSharedMaterial != null ? text.fontSharedMaterial.name : "") + " " + text.font.name;
            if (name.Contains("Outline Light")) return AssetDatabase.LoadAssetAtPath<Material>(MAT_OUTLINE_LIGHT);
            if (name.Contains("Outline")) return AssetDatabase.LoadAssetAtPath<Material>(MAT_OUTLINE);
            if (name.Contains("Shadow")) return AssetDatabase.LoadAssetAtPath<Material>(MAT_SHADOW);
            if (name.Contains("Glow")) return AssetDatabase.LoadAssetAtPath<Material>(MAT_GLOW);
            return AssetDatabase.LoadAssetAtPath<Material>(MAT_PLAIN);
        }

        private static bool IsFredoka(TMP_Text text) => text.font != null && (text.font.name.StartsWith("FredokaOne") || text.font.name.StartsWith("Fredoka One"));

        private static void SwapToSdf()
        {
            TMP_FontAsset sdf = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SDF_FONT);

            System.Action<GameObject> swap = root =>
            {
                foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    if (!IsFredoka(text)) continue;
                    Material preset = PresetFor(text);
                    text.font = sdf;
                    text.fontSharedMaterial = preset;
                    EditorUtility.SetDirty(text);
                }
            };

            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Project Files", "Assets/Watermelon Core" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                bool uses = false;
                foreach (TMP_Text text in asset.GetComponentsInChildren<TMP_Text>(true)) uses |= IsFredoka(text);
                if (uses) EditPrefab(path, swap);
            }

            foreach (string scenePath in new[] { "Assets/Project Files/Game/Scenes/Init.unity", DustwaterContentBuilder.GAME_SCENE })
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                foreach (GameObject root in scene.GetRootGameObjects()) swap(root);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            // the runtime UI style and TMP's default font
            DustwaterUIStyle style = AssetDatabase.LoadAssetAtPath<DustwaterUIStyle>(DustwaterContentBuilder.UI_STYLE);
            style.font = sdf;
            style.fontOutline = sdf;
            style.fontMaterial = AssetDatabase.LoadAssetAtPath<Material>(MAT_PLAIN);
            style.fontOutlineMaterial = AssetDatabase.LoadAssetAtPath<Material>(MAT_OUTLINE);
            EditorUtility.SetDirty(style);

            SerializedObject settings = new SerializedObject(TMP_Settings.instance);
            settings.FindProperty("m_defaultFontAsset").objectReferenceValue = sdf;
            settings.ApplyModifiedPropertiesWithoutUndo();
        }

        #endregion

        #region Template labels

        private static readonly Dictionary<string, string> TEXT_KEYS = new Dictionary<string, string>()
        {
            { "PLAY", "ui.play" }, { "NO MORE ADS!", "ui.no_more_ads" }, { "Not now", "ui.not_now" }, { "REMOVE ADS", "ui.remove_ads" },
            { "COMPLETED", "ui.complete_bottom" }, { "GET X3", "ui.get_x3" }, { "NO, THANKS", "ui.no_thanks" },
            { "LEVEL FAILED", "ui.level_failed" }, { "CONTINUE", "ui.continue" }, { "Settings", "ui.settings" }, { "Sound", "ui.sound" },
            { "Music", "ui.music" }, { "Vibration", "ui.vibration" }, { "Restore Purchases", "ui.restore" }, { "Privacy", "ui.privacy" },
            { "Terms Of Use", "ui.terms" }, { "STORE", "ui.store" }, { "STARTER PACK", "ui.starter_pack" }, { "NO ADS", "ui.no_ads" },
            { "Loading..", "ui.loading" },
        };

        // gate titles depend on the prefab, not the (copied) placeholder text
        private static readonly Dictionary<string, string> GATE_TITLES = new Dictionary<string, string>()
        {
            { "Gate Damage", "gate.damage" }, { "Gate Fire Rate", "gate.fire_rate" }, { "Gate Bullet Lifetime", "gate.range" },
            { "Gate Health", "gate.health" }, { "Gate Money", "gate.money" },
        };

        private static void LocalizeTemplateLabels()
        {
            List<string> prefabs = new List<string>()
            {
                PREFABS + "UI/Canvas/UI Main Menu.prefab", PREFABS + "UI/Canvas/UI Complete.prefab", PREFABS + "UI/Canvas/UI Game Over.prefab",
                PREFABS + "UI/Canvas/UI Settings.prefab", PREFABS + "UI Store/UI IAP Store.prefab",
            };
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { PREFABS + "UI Store/Offers" }))
                prefabs.Add(AssetDatabase.GUIDToAssetPath(guid));

            foreach (string path in prefabs)
                EditPrefab(path, root =>
                {
                    foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
                    {
                        string value = text.text.Trim();
                        string key = null;
                        if (TEXT_KEYS.TryGetValue(value, out string found)) key = found;
                        else if (value.StartsWith("Remove all pop-up ads")) key = "ui.no_ads_desc";
                        else if (value == "LEVEL" && text.name == "LevelText") key = "ui.complete_top";
                        if (key != null) SetKey(text, key);

                        // labels next to an icon inside a button (GET X3): start right after the icon
                        if (text.name == "GetText") FitBesideIcon(text);

                        // the store title box is narrower than its badge: give translations the badge width
                        if (text.name == "Store Header Text" && Mathf.Approximately(text.rectTransform.anchorMin.x, text.rectTransform.anchorMax.x) && text.rectTransform.sizeDelta.x < 400f)
                            text.rectTransform.sizeDelta = new Vector2(400f, text.rectTransform.sizeDelta.y);
                    }
                });

            foreach (var gate in GATE_TITLES)
                EditPrefab(PREFABS + "Gameplay/Gates/" + gate.Key + ".prefab", root =>
                {
                    foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
                        if (text.name == "Title Text") SetKey(text, gate.Value);
                });

            // names on gates/boosters come from code (unit and weapon names): let them shrink to fit
            foreach (string folder in new[] { PREFABS + "Gameplay/Gates", PREFABS + "Gameplay/Boosters" })
                foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { folder }))
                    EditPrefab(AssetDatabase.GUIDToAssetPath(guid), root =>
                    {
                        foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
                        {
                            float size = text.enableAutoSizing ? text.fontSizeMax : text.fontSize;
                            text.enableAutoSizing = true;
                            text.fontSizeMax = size;
                            text.fontSizeMin = Mathf.Min(text.fontSizeMin, size * 0.5f);
                            text.textWrappingMode = TextWrappingModes.NoWrap;
                        }
                    });

            // Init scene loading label
            var scene = EditorSceneManager.OpenScene("Assets/Project Files/Game/Scenes/Init.unity", OpenSceneMode.Single);
            foreach (TMP_Text text in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (text.text.Trim() == "Loading..") SetKey(text, "ui.loading");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void FitBesideIcon(TMP_Text text)
        {
            RectTransform parent = text.rectTransform.parent as RectTransform;
            float iconRight = float.MinValue;
            foreach (Transform child in parent)
            {
                if (child == text.transform || child.GetComponent<Image>() == null) continue;
                RectTransform icon = (RectTransform)child;
                Vector3[] corners = new Vector3[4];
                icon.GetLocalCorners(corners);
                float right = parent.InverseTransformPoint(icon.TransformPoint(corners[2])).x;
                iconRight = Mathf.Max(iconRight, right);
            }
            if (iconRight == float.MinValue) return;

            RectTransform rect = text.rectTransform;
            float left = iconRight - parent.rect.xMin + 8f;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, 0f);
            rect.offsetMax = new Vector2(-24f, 0f);
            text.alignment = TextAlignmentOptions.Center;
        }

        private static void SetKey(TMP_Text text, string key)
        {
            LocalizedText localized = text.GetComponent<LocalizedText>();
            if (localized == null) localized = text.gameObject.AddComponent<LocalizedText>();
            SerializedObject so = new SerializedObject(localized);
            so.FindProperty("key").stringValue = key;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EditPrefab(string path, System.Action<GameObject> edit)
        {
            if (!File.Exists(path)) return;
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            edit(root);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        #endregion

        #region Settings

        private static void AddLanguageSettingsRow()
        {
            EditPrefab(PREFABS + "UI/Canvas/UI Settings.prefab", root =>
            {
                Transform content = root.transform.Find("Panel Back/Content");
                Transform old = content.Find("Language Button");
                if (old != null) Object.DestroyImmediate(old.gameObject);

                Transform restore = content.Find("Restore Purchases Button");
                GameObject language = Object.Instantiate(restore.gameObject, content);
                language.name = "Language Button";
                language.transform.SetSiblingIndex(content.Find("Vibration Toggle").GetSiblingIndex() + 1);

                foreach (MonoBehaviour behaviour in language.GetComponents<MonoBehaviour>())
                    if (behaviour.GetType().Name == "SettingsRestoreButton") Object.DestroyImmediate(behaviour);

                Button button = language.GetComponent<Button>();
                SerializedObject buttonSo = new SerializedObject(button);
                buttonSo.FindProperty("m_OnClick.m_PersistentCalls.m_Calls").ClearArray();
                buttonSo.ApplyModifiedPropertiesWithoutUndo();

                TMP_Text label = language.GetComponentInChildren<TMP_Text>(true);
                LocalizedText localized = label.GetComponent<LocalizedText>();
                if (localized != null) Object.DestroyImmediate(localized);

                LanguageSettingsButton settingsButton = language.AddComponent<LanguageSettingsButton>();
                SerializedObject so = new SerializedObject(settingsButton);
                so.FindProperty("label").objectReferenceValue = label;
                so.ApplyModifiedPropertiesWithoutUndo();

                // room for the new row
                RectTransform panel = (RectTransform)root.transform.Find("Panel Back");
                LayoutElement element = language.GetComponent<LayoutElement>();
                float rowHeight = element != null ? element.preferredHeight : ((RectTransform)language.transform).sizeDelta.y;
                if (panel.sizeDelta.y < 845f + rowHeight) panel.sizeDelta = new Vector2(panel.sizeDelta.x, 845f + rowHeight + 20f);
            });
        }

        #endregion
    }
}
