#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SquadCommand
{
    /// <summary>
    /// Store media for Yandex Games: real gameplay screenshots per language (the play bot drives the squad)
    /// and frame sequences for the gameplay video. Play mode, Game view at 1080x1920, Unity in front.
    /// StoreMediaCapture.Shots("en", 0) -> Temp/Store/en/*.png
    /// StoreMediaCapture.Video(16, 14f, "desert") -> Temp/Store/video/desert/00000.png ... (30 fps)
    /// </summary>
    public class StoreMediaCapture : MonoBehaviour
    {
        public static bool IsRunning { get; private set; }

        private static string Root => Path.Combine(Application.dataPath, "../Temp/Store");

        public static void Shots(string tag, int languageIndex)
        {
            IsRunning = true;
            new GameObject("Store Media").AddComponent<StoreMediaCapture>().StartCoroutine(ShotsRoutine(tag, languageIndex));
        }

        public static void Video(int mission, float seconds, string name, float skipSeconds = 0f)
        {
            IsRunning = true;
            new GameObject("Store Media").AddComponent<StoreMediaCapture>().StartCoroutine(VideoRoutine(mission, seconds, name, skipSeconds));
        }

        #region Screenshots

        private static IEnumerator ShotsRoutine(string tag, int languageIndex)
        {
            string folder = Path.Combine(Root, tag);
            Directory.CreateDirectory(folder);

            Loc.SetLanguage(languageIndex);
            yield return new WaitForSecondsRealtime(0.5f);

            // gameplay: desert main street, a desert boss, Frostpeak, a Frostpeak boss
            yield return Gameplay(14, folder, "1_desert", 11f, false);
            yield return Gameplay(20, folder, "2_boss", 0f, true);
            yield return Gameplay(57, folder, "3_snow", 9f, false);
            yield return Gameplay(90, folder, "5_snow_boss", 0f, true);

            // briefing of a Frostpeak mission with the new cards in the squad
            SetProgress(44, act1Built: true, act2Level: 2);
            Campaign.SetSquad(new List<string>() { "guardian", "pyro", "scout", "heavy", "medic", "ranger" });
            Campaign.SetWeapon("burst_rifle");
            GameController.ReturnToMainMenu();
            yield return new WaitForSecondsRealtime(1.2f);
            DustwaterUI.Instance.Briefing.Show();
            yield return Capture(folder, "4_briefing", 0.8f);
            DustwaterUI.Instance.Briefing.Hide();

            // Dustwater rebuilt (town tab before the last chapter)
            SetProgress(29, act1Built: true, act2Level: 0);
            GameController.ReturnToMainMenu();
            yield return new WaitForSecondsRealtime(1.2f);
            DustwaterUI.Instance.Hub.SelectTab(HubView.Tab.Town);
            yield return Capture(folder, "6_town", 1.5f);
            DustwaterUI.Instance.Hub.SelectTab(HubView.Tab.Battle);

            IsRunning = false;
        }

        private static IEnumerator Gameplay(int mission, string folder, string name, float seconds, bool waitForBoss)
        {
            DustwaterPlayBot.Launch(mission, mission, 1f);
            yield return new WaitForSecondsRealtime(0.6f);
            RefreshMissionLabel();

            float t = 0.6f;
            while (t < 60f)
            {
                t += Time.unscaledDeltaTime;
                if (waitForBoss ? MovementManager.AdvanceMultiplier < 0.5f && t > 3f : t >= seconds) break;
                yield return null;
            }

            if (waitForBoss) yield return new WaitForSecondsRealtime(1.6f);
            yield return Capture(folder, name, 0f);

            StopBot();
            yield return new WaitForSecondsRealtime(0.8f);
        }

        private static void StopBot()
        {
            DustwaterPlayBot bot = FindFirstObjectByType<DustwaterPlayBot>();
            if (bot != null) Destroy(bot.gameObject);
            Time.timeScale = 1f;
            MovementManager.AdvanceMultiplier = 1f;
            UIController.HidePage<UIGame>();
            UIController.HidePage<UIGameOver>();
            UIController.HidePage<UIComplete>();
            GameController.ReturnToMainMenu();
        }

        // the bot starts levels directly, so the HUD label keeps the previous number unless refreshed
        private static void RefreshMissionLabel()
        {
            UILevelNumberText label = FindFirstObjectByType<UILevelNumberText>(FindObjectsInactive.Include);
            if (label != null)
                typeof(UILevelNumberText).GetMethod("UpdateLevelNumber", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.Invoke(label, null);
        }

        private static IEnumerator Capture(string folder, string name, float wait)
        {
            if (wait > 0f) yield return new WaitForSecondsRealtime(wait);
            yield return new WaitForEndOfFrame();
            Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(folder, name + ".png"), texture.EncodeToPNG());
            Destroy(texture);
        }

        private static void SetProgress(int completed, bool act1Built, int act2Level)
        {
            CampaignDatabase db = Campaign.Database;
            int[] buildings = new int[db.Buildings.Count];
            for (int i = 0; i < buildings.Length; i++)
            {
                BuildingData building = db.Buildings[i];
                if (building.act == 0 && act1Built) buildings[i] = building.levels.Count;
                else if (building.act == 1 && completed >= building.unlockAfterLevel) buildings[i] = Mathf.Min(act2Level, building.levels.Count);
            }

            Dictionary<string, int> units = new Dictionary<string, int>();
            foreach (UnitCardData unit in db.Units) if (unit.unlockAfterLevel <= completed) units[unit.id] = unit.isFiller ? 9 : 6;
            Dictionary<string, int> weapons = new Dictionary<string, int>();
            foreach (WeaponCardData weapon in db.Weapons) if (weapon.unlockAfterLevel <= completed) weapons[weapon.id] = 5;

            Campaign.DevSetProgress(completed, buildings, units, weapons);
            Campaign.IsIntroSeen = true;
            for (int c = 0; c < db.Chapters.Count; c++)
            {
                Campaign.MarkStoryBeatSeen(c * 10);
                Campaign.MarkStoryBeatSeen(c * 10 + 1);
            }
            Campaign.MarkStoryBeatSeen(1001);
        }

        #endregion

        #region Video

        private static IEnumerator VideoRoutine(int mission, float seconds, string name, float skipSeconds)
        {
            string folder = Path.Combine(Root, "video", name);
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
            Directory.CreateDirectory(folder);

            DustwaterPlayBot.Launch(mission, mission, 1f);
            yield return new WaitForSecondsRealtime(0.6f);
            RefreshMissionLabel();
            yield return new WaitForSecondsRealtime(skipSeconds);

            // fixed 30 fps game time: every rendered frame becomes one video frame
            Time.captureFramerate = 30;
            int frames = Mathf.RoundToInt(seconds * 30f);
            for (int i = 0; i < frames; i++)
            {
                yield return new WaitForEndOfFrame();
                Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();
                File.WriteAllBytes(Path.Combine(folder, i.ToString("00000") + ".png"), texture.EncodeToPNG());
                Destroy(texture);
            }
            Time.captureFramerate = 0;

            StopBot();
            IsRunning = false;
        }

        #endregion
    }
}
#endif
