using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    /// <summary>
    /// Root of the Dustwater campaign screens: hub tabs (Town / Squad / Battle), operation briefing,
    /// rewards, story dialogs and the boss bar. Built at runtime from template sprites (see DWUI).
    /// </summary>
    public class DustwaterUI : MonoBehaviour
    {
        public static DustwaterUI Instance { get; private set; }

        public RectTransform Root { get; private set; }
        public RectTransform SafeArea { get; private set; }

        public HubView Hub { get; private set; }
        public CardsView Cards { get; private set; }
        public TownView Town { get; private set; }
        public BriefingView Briefing { get; private set; }
        public RewardView Reward { get; private set; }
        public StoryView Story { get; private set; }
        public BossBarView BossBar { get; private set; }

        public static DustwaterUI Create(DustwaterUIStyle style)
        {
            DWUI.Style = style;

            GameObject go = new GameObject("Dustwater UI", typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            CanvasScaler scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0f;
            go.AddComponent<GraphicRaycaster>();

            DustwaterUI ui = go.AddComponent<DustwaterUI>();
            ui.Build();
            return ui;
        }

        private void Build()
        {
            Instance = this;
            Root = (RectTransform)transform;

            SafeArea = DWUI.Rect(Root, "Safe Area").Stretch();
            NotchSaveArea.RegisterRectTransform(SafeArea);

            // order = draw order: views, then modals on top
            Cards = new CardsView(SafeArea);
            Town = new TownView(SafeArea);
            Hub = new HubView(SafeArea, this);
            BossBar = new BossBarView(SafeArea);
            Briefing = new BriefingView(Root);
            Reward = new RewardView(Root);
            Story = new StoryView(Root);

            UIMainMenu.Shown += OnMainMenuShown;
            UIMainMenu.Hidden += OnMainMenuHidden;
            Campaign.OnChanged += Refresh;
        }

        private float tickTimer;

        private void Update()
        {
            BossBar.Tick();

            tickTimer += Time.unscaledDeltaTime;
            if (tickTimer < 1f) return;
            tickTimer = 0f;

            Hub.Tick();
        }

        private void OnDestroy()
        {
            UIMainMenu.Shown -= OnMainMenuShown;
            UIMainMenu.Hidden -= OnMainMenuHidden;
            Campaign.OnChanged -= Refresh;
            BossBar?.Dispose();
        }

        private void OnMainMenuShown()
        {
            Hub.Show();

            if (Campaign.PendingResult != null)
            {
                Campaign.LevelResult result = Campaign.PendingResult;
                Campaign.PendingResult = null;
                Reward.Show(result, () => AfterReward(result));
            }
            else if (!Campaign.IsIntroSeen && DustwaterIntro.Instance != null)
            {
                DustwaterIntro.Instance.Play(() => Campaign.IsIntroSeen = true);
            }
        }

        private void AfterReward(Campaign.LevelResult result)
        {
            if (result.chapterCompleted >= 0)
            {
                ChapterData chapter = Campaign.Database.Chapters[result.chapterCompleted];
                int beat = result.chapterCompleted * 10 + 1;
                if (!Campaign.IsStoryBeatSeen(beat))
                {
                    Story.Show(DWUI.Style.commanderPortrait, DWUI.Style.commanderName, chapter.outroLines, () =>
                    {
                        Campaign.MarkStoryBeatSeen(beat);
                        if (DustwaterTown.Instance != null) Hub.SelectTab(HubView.Tab.Town);
                    });
                }
            }
        }

        private void OnMainMenuHidden()
        {
            Hub.Hide();
        }

        public void Refresh()
        {
            Hub.Refresh();
            if (Cards.IsVisible) Cards.Refresh();
            if (Town.IsVisible) Town.Refresh();
            if (Briefing.IsVisible) Briefing.Refresh();
        }

        /// <summary>Chapter intro before the first briefing of a chapter, then the briefing.</summary>
        public void OpenBriefing()
        {
            int chapterIndex = Campaign.CurrentChapterIndex;
            if (chapterIndex >= 0)
            {
                ChapterData chapter = Campaign.Database.Chapters[chapterIndex];
                int beat = chapterIndex * 10;
                if (!Campaign.IsStoryBeatSeen(beat))
                {
                    Story.Show(DWUI.Style.commanderPortrait, DWUI.Style.commanderName, chapter.introLines, () =>
                    {
                        Campaign.MarkStoryBeatSeen(beat);
                        Briefing.Show();
                    }, "CHAPTER " + (chapterIndex + 1) + ": " + chapter.title);
                    return;
                }
            }

            Briefing.Show();
        }

        /// <summary>Boss taunt before a boss mission, then the level starts.</summary>
        public void Deploy()
        {
            Briefing.Hide();

            LevelData level = LevelController.LevelData;
            ChapterData chapter = Campaign.CurrentChapter;
            if (level != null && level.IsBossLevel && chapter != null && !string.IsNullOrEmpty(chapter.bossTaunt))
            {
                Story.Show(chapter.bossPortrait, chapter.bossName.ToUpper(), new System.Collections.Generic.List<string>() { chapter.bossTaunt }, GameController.StartLevel);
                return;
            }

            GameController.StartLevel();
        }
    }
}
