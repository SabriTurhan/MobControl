#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// QA: walks through every screen and saves screenshots to Temp/QA/{tag}_{screen}.png.
    /// DustwaterUIShots.Run("1080x1920_tr", "tr") from play mode (Unity in front).
    /// </summary>
    public class DustwaterUIShots : MonoBehaviour
    {
        public static bool IsRunning { get; private set; }

        // sample content for the detail/story screens (act 2 QA: "guardian", 3)
        public static string DetailCard = "heavy";
        public static int StoryChapter = 1;
        public static int IntroAct = 0;

        private string shotTag;
        private string folder;

        public static void Run(string tag, int languageIndex, bool gameplay = true)
        {
            DustwaterUIShots shots = new GameObject("UI Shots").AddComponent<DustwaterUIShots>();
            shots.shotTag = tag;
            shots.folder = Path.Combine(Application.dataPath, "../Temp/QA");
            Directory.CreateDirectory(shots.folder);
            IsRunning = true;
            shots.StartCoroutine(shots.Sequence(languageIndex, gameplay));
        }

        private IEnumerator Shot(string screen, float wait = 0.6f)
        {
            yield return new WaitForSecondsRealtime(wait);
            yield return new WaitForEndOfFrame();
            Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(folder, shotTag + "_" + screen + ".png"), texture.EncodeToPNG());
            Destroy(texture);
        }

        private static object Call(object target, string method, params object[] args)
        {
            return target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Invoke(target, args);
        }

        private IEnumerator Sequence(int languageIndex, bool gameplay)
        {
            Loc.SetLanguage(languageIndex);
            yield return new WaitForSecondsRealtime(0.3f);
            DustwaterUI ui = DustwaterUI.Instance;

            ui.Hub.SelectTab(HubView.Tab.Battle);
            yield return Shot("01_hub");

            ui.Hub.SelectTab(HubView.Tab.Squad);
            yield return Shot("02_squad");
            Call(ui.Cards, "ShowDetail", CardWidget.Get(true, DetailCard));
            yield return Shot("03_card_detail");
            ui.Cards.Hide();

            ui.Hub.SelectTab(HubView.Tab.Town);
            yield return Shot("04_town", 1f);

            ui.Hub.SelectTab(HubView.Tab.Battle);
            ui.Briefing.Show();
            yield return Shot("05_briefing");
            Call(ui.Briefing, "OpenPicker", true, 0, LevelController.LevelData.Threats);
            yield return Shot("06_picker");
            ui.Briefing.Hide();

            Campaign.LevelResult result = new Campaign.LevelResult()
            {
                levelNumber = 12, supplies = 9,
                card = new CardReward() { isUnit = true, id = "grenadier", copies = 1, isNew = true, rarity = CardRarity.Epic },
            };
            ui.Reward.Show(result, null);
            yield return Shot("07_reward", 1.6f);
            Call(ui.Reward, "Close");

            ChapterData chapter = Campaign.Database.Chapters[StoryChapter];
            ui.Story.Show(DWUI.Style.commanderPortrait, Loc.Get("story.sergeant"), Loc.ChapterLines(StoryChapter, "intro", chapter.introLines), null, Loc.Format("hub.chapter", StoryChapter + 1, Loc.ChapterTitle(StoryChapter, chapter)));
            yield return Shot("08_story", 2.5f);
            for (int i = 0; i < 6 && ui.Story.IsVisible; i++) Call(ui.Story, "Next");

            DustwaterIntroPlayer intro = FindFirstObjectByType<DustwaterIntroPlayer>();
            if (intro != null)
            {
                if (IntroAct > 0) intro.PlayAct(IntroAct, null);
                else intro.Play(null);
                yield return Shot("08b_intro", 3.2f);

                // the title card at the end of the cinematic
                CanvasGroup title = null;
                for (float t = 0f; t < 60f; t += 0.1f)
                {
                    GameObject overlay = GameObject.Find("Intro");
                    Transform titleRoot = overlay != null ? overlay.transform.Find("Title") : null;
                    title = titleRoot != null ? titleRoot.GetComponent<CanvasGroup>() : null;
                    if (title == null || title.alpha > 0.98f) break;
                    yield return new WaitForSecondsRealtime(0.1f);
                }
                if (title != null) yield return Shot("08c_title", 0.1f);
                Call(intro, "Skip");
                yield return new WaitForSecondsRealtime(0.4f);
            }

            ui = DustwaterUI.Instance;
            ui.OpenLanguagePicker();
            yield return Shot("09_language");
            GameObject picker = GameObject.Find("Language Picker");
            if (picker != null) Destroy(picker);

            UIController.ShowPage<UISettings>();
            yield return Shot("10_settings", 0.9f);
            UIController.HidePage<UISettings>();
            yield return new WaitForSecondsRealtime(0.5f);

            UINoAdsPopUp noAds = FindFirstObjectByType<UINoAdsPopUp>(FindObjectsInactive.Include);
            if (noAds != null)
            {
                noAds.Show();
                yield return Shot("11_no_ads", 0.8f);
                noAds.gameObject.SetActive(false);
            }

            UIController.ShowPage<IAPStore.UIStore>();
            yield return Shot("12_store", 0.9f);
            UIController.HidePage<IAPStore.UIStore>();
            yield return new WaitForSecondsRealtime(0.5f);

            if (gameplay)
            {
                GameController.StartLevel();
                yield return Shot("13_game", 3.5f);
                GameController.OnGameFail();
                yield return Shot("14_game_over", 1.2f);
                UIController.HidePage<UIGameOver>();
                UIController.ShowPage<UIComplete>();
                yield return Shot("15_complete", 2.5f);
                UIController.HidePage<UIComplete>(GameController.ReturnToMainMenu);
                yield return new WaitForSecondsRealtime(1f);
                Campaign.PendingResult = null;
            }

            IsRunning = false;
            Destroy(gameObject);
        }
    }
}
#endif
