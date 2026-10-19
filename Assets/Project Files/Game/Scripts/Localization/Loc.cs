using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace SquadCommand
{
    /// <summary>
    /// String tables for every text in the game (Resources/Localization/strings.txt, tab separated:
    /// key, en, tr, de, fr, es, pt, ru, it). The first launch picks the device language, the choice is saved.
    /// </summary>
    public static class Loc
    {
        public static readonly string[] LANGUAGES = { "en", "tr", "de", "fr", "es", "pt", "ru", "it" };
        public static readonly string[] NATIVE_NAMES = { "English", "Türkçe", "Deutsch", "Français", "Español", "Português", "Русский", "Italiano" };
        private static readonly string[] CULTURES = { "en-US", "tr-TR", "de-DE", "fr-FR", "es-ES", "pt-BR", "ru-RU", "it-IT" };

        private const string PREFS_KEY = "dustwater_language";
        private const string TABLE_PATH = "Localization/strings";

        private static Dictionary<string, string[]> table;
        private static int languageIndex = -1;

        public static event SimpleCallback OnLanguageChanged;

        public static string Language
        {
            get
            {
                EnsureLoaded();
                return LANGUAGES[languageIndex];
            }
        }

        public static int LanguageIndex
        {
            get
            {
                EnsureLoaded();
                return languageIndex;
            }
        }

        public static CultureInfo Culture => new CultureInfo(CULTURES[LanguageIndex]);

        private static void EnsureLoaded()
        {
            if (table != null) return;

            table = new Dictionary<string, string[]>();
            TextAsset asset = Resources.Load<TextAsset>(TABLE_PATH);
            if (asset != null)
            {
                string[] lines = asset.text.Split('\n');
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i].TrimEnd('\r');
                    if (line.Length == 0) continue;

                    string[] cells = line.Split('\t');
                    string[] values = new string[LANGUAGES.Length];
                    for (int l = 0; l < values.Length; l++)
                        values[l] = l + 1 < cells.Length ? cells[l + 1].Replace("\\n", "\n") : "";
                    table[cells[0]] = values;
                }
            }
            else
            {
                Debug.LogError("[Loc] Missing Resources/" + TABLE_PATH);
            }

            string saved = PlayerPrefs.GetString(PREFS_KEY, "");
            languageIndex = System.Array.IndexOf(LANGUAGES, saved);
            if (languageIndex < 0) languageIndex = DetectDeviceLanguage();
        }

        public static int DetectDeviceLanguage()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Turkish: return 1;
                case SystemLanguage.German: return 2;
                case SystemLanguage.French: return 3;
                case SystemLanguage.Spanish: return 4;
                case SystemLanguage.Portuguese: return 5;
                case SystemLanguage.Russian:
                case SystemLanguage.Ukrainian:
                case SystemLanguage.Belarusian: return 6;
                case SystemLanguage.Italian: return 7;
                default: return 0;
            }
        }

        public static void SetLanguage(int index)
        {
            EnsureLoaded();
            if (index < 0 || index >= LANGUAGES.Length || index == languageIndex) return;

            languageIndex = index;
            PlayerPrefs.SetString(PREFS_KEY, LANGUAGES[index]);
            PlayerPrefs.Save();
            OnLanguageChanged?.Invoke();
        }

        public static bool Has(string key)
        {
            EnsureLoaded();
            return table.ContainsKey(key);
        }

        /// <summary>Translation for the current language, English if empty, the fallback (or the key) if missing.</summary>
        public static string Get(string key, string fallback = null)
        {
            EnsureLoaded();
            if (key != null && table.TryGetValue(key, out string[] values))
            {
                string value = values[languageIndex];
                if (!string.IsNullOrEmpty(value)) return value;
                if (!string.IsNullOrEmpty(values[0])) return values[0];
            }

            return fallback ?? key;
        }

        public static string Format(string key, params object[] args)
        {
            return string.Format(Culture, Get(key), args);
        }

        /// <summary>Upper case with the rules of the current language (Turkish i -> İ).</summary>
        public static string Upper(string text) => text == null ? null : text.ToUpper(Culture);

        #region Game data

        public static string UnitName(UnitCardData unit) => Get("unit." + unit.id + ".name", unit.displayName);
        public static string UnitDescription(UnitCardData unit) => Get("unit." + unit.id + ".desc", unit.description);
        public static string WeaponName(WeaponCardData weapon) => Get("weapon." + weapon.id + ".name", weapon.displayName);
        public static string WeaponDescription(WeaponCardData weapon) => Get("weapon." + weapon.id + ".desc", weapon.description);
        private static string BuildingKey(BuildingData building) => "building." + (string.IsNullOrEmpty(building.id) ? building.type.ToString() : building.id);
        public static string BuildingName(BuildingData building) => Get(BuildingKey(building) + ".name", building.displayName);
        public static string BuildingDescription(BuildingData building) => Get(BuildingKey(building) + ".desc", building.description);
        // the effect text is shared by every building of the same type
        public static string BuildingValue(BuildingData building, float value) => string.Format(Culture, Get("building." + building.type + ".format", building.valueFormat), value);
        public static string TownName(int act) => Get("town.title." + act, act == 0 ? "DUSTWATER" : "FROSTPEAK");
        public static string ThreatName(ThreatInfo threat) => Get("threat." + threat.type + ".name", threat.displayName);
        public static string ThreatDescription(ThreatInfo threat)
        {
            // Frostpeak: same threats, White Wolves wording
            string key = "threat." + threat.type + ".desc";
            if (Campaign.IsInitialised && Campaign.CurrentAct > 0 && Has(key + "." + Campaign.CurrentAct)) key += "." + Campaign.CurrentAct;
            return Get(key, threat.description);
        }
        public static string CharacterName(string characterId, string fallback) => Get("char." + characterId, fallback);
        public static string GunName(string gunId, string fallback) => Get("gun." + gunId, fallback);
        public static string OperationName(int missionNumber, string fallback) => Get("op." + missionNumber, fallback);

        public static string ChapterTitle(int chapter, ChapterData data) => Get("chapter." + chapter + ".title", data.title);
        public static string ChapterBoss(int chapter, ChapterData data) => Get("chapter." + chapter + ".boss", data.bossName);
        public static string ChapterTaunt(int chapter, ChapterData data) => Get("chapter." + chapter + ".taunt", data.bossTaunt);

        public static List<string> ChapterLines(int chapter, string part, List<string> fallback)
        {
            List<string> lines = new List<string>();
            for (int i = 0; i < fallback.Count; i++)
                lines.Add(Get("chapter." + chapter + "." + part + "." + i, fallback[i]));
            return lines;
        }

        #endregion
    }
}
