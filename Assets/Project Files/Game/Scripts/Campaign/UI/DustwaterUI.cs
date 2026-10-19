using UnityEngine;
using UnityEngine.UI;

namespace SquadCommand
{
    /// <summary>
    /// Root of the Dustwater campaign screens: hub tabs (Town / Squad / Battle), operation briefing,
    /// rewards, story dialogs and the boss bar. Built at runtime from template sprites (see DWUI).
    /// </summary>
    public class DustwaterUI : MonoBehaviour
    {
        public static DustwaterUI Instance { get; private set; }

        /// <summary>Modal layer (overlay): briefing, rewards, story, intro, pickers, boss bar.</summary>
        public RectTransform Root { get; private set; }
        /// <summary>Hub layer (camera space, under the template menu so its popups stay on top).</summary>
        public RectTransform HudRoot { get; private set; }
        public RectTransform SafeArea { get; private set; }

        public HubView Hub { get; private set; }
        public CardsView Cards { get; private set; }
        public TownView Town { get; private set; }
        public BriefingView Briefing { get; private set; }
        public RewardView Reward { get; private set; }
        public StoryView Story { get; private set; }
        public BossBarView BossBar { get; private set; }

        private DustwaterUIStyle style;
        private GameObject hudObject;
        private RectTransform languagePicker;

        public static DustwaterUI Create(DustwaterUIStyle style)
        {
            DWUI.Style = style;

            GameObject go = NewCanvas("Dustwater UI");
            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            DustwaterUI ui = go.AddComponent<DustwaterUI>();
            ui.style = style;

            // the hub sits in the template menu's camera space with a lower order: the menu's popups
            // (no ads, settings) and side buttons draw above it and receive taps first
            ui.hudObject = NewCanvas("Dustwater HUD");
            Canvas hud = ui.hudObject.GetComponent<Canvas>();
            Canvas menuCanvas = FindMenuCanvas();
            if (menuCanvas != null && menuCanvas.worldCamera != null)
            {
                hud.renderMode = RenderMode.ScreenSpaceCamera;
                hud.worldCamera = menuCanvas.worldCamera;
                hud.planeDistance = menuCanvas.planeDistance + 0.5f;
                hud.sortingLayerID = menuCanvas.sortingLayerID;
                hud.sortingOrder = menuCanvas.sortingOrder - 5;
            }
            else
            {
                hud.renderMode = RenderMode.ScreenSpaceOverlay;
                hud.sortingOrder = 40;
            }

            ui.Build();
            return ui;
        }

        private static GameObject NewCanvas(string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            go.AddComponent<Canvas>();
            CanvasScaler scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0f;
            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        private static Canvas FindMenuCanvas()
        {
            UIMainMenu menu = FindFirstObjectByType<UIMainMenu>(FindObjectsInactive.Include);
            return menu != null ? menu.GetComponentInParent<Canvas>().rootCanvas : null;
        }

        private void Build()
        {
            Instance = this;
            Root = (RectTransform)transform;
            HudRoot = (RectTransform)hudObject.transform;

            SafeArea = DWUI.Rect(HudRoot, "Safe Area").Stretch();
            NotchSaveArea.RegisterRectTransform(SafeArea);
            RectTransform modalSafeArea = DWUI.Rect(Root, "Safe Area").Stretch();
            NotchSaveArea.RegisterRectTransform(modalSafeArea);

            Cards = new CardsView(SafeArea, Root);
            Town = new TownView(SafeArea);
            Hub = new HubView(SafeArea, this);
            BossBar = new BossBarView(modalSafeArea);
            Briefing = new BriefingView(Root);
            Reward = new RewardView(Root);
            Story = new StoryView(Root);

            UIMainMenu.Shown += OnMainMenuShown;
            UIMainMenu.Hidden += OnMainMenuHidden;
            Campaign.OnChanged += Refresh;
            Loc.OnLanguageChanged += OnLanguageChanged;
        }

        // the first level can finish loading after the hub is shown: refill the mission card once it is there
        private void Update()
        {
            if (Hub != null && Hub.IsVisible && LevelController.LevelData != null && Hub.ShownLevel != LevelController.LevelData)
                Hub.Refresh();
        }

        // every view builds its labels once: rebuild the whole UI in the new language
        private void OnLanguageChanged()
        {
            bool hubVisible = Hub.IsVisible;
            HubView.Tab tab = Hub.CurrentTab;
            DustwaterUIStyle uiStyle = style;

            Destroy(hudObject);
            Destroy(gameObject);

            DustwaterUI ui = Create(uiStyle);
            if (hubVisible)
            {
                ui.Hub.Show();
                ui.Hub.SelectTab(tab);
            }
        }

        public void OpenLanguagePicker()
        {
            if (languagePicker != null) Destroy(languagePicker.gameObject);

            languagePicker = DWUI.Rect(Root, "Language Picker").Stretch();
            Image dim = DWUI.Blocker(languagePicker, "Dim", DWUI.Style.dim);
            Button close = dim.gameObject.AddComponent<Button>();
            close.transition = Selectable.Transition.None;
            close.onClick.AddListener(() => Destroy(languagePicker.gameObject));

            Image panel = DWUI.Image(languagePicker, "Panel", DWUI.Style.panelBlue);
            panel.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820f, 1340f));
            panel.rectTransform.Fit(1340f, 820f);
            panel.raycastTarget = true;

            TMPro.TextMeshProUGUI title = DWUI.Text(panel.rectTransform, "Title", Loc.Get("ui.choose_language"), 60f);
            title.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(760f, 90f));

            for (int i = 0; i < Loc.LANGUAGES.Length; i++)
            {
                int index = i;
                bool current = i == Loc.LanguageIndex;
                Button button = DWUI.Button(panel.rectTransform, Loc.LANGUAGES[i], current ? DWUI.Style.buttonGreen : DWUI.Style.buttonGray, Loc.NATIVE_NAMES[i], 54f, () =>
                {
                    Destroy(languagePicker.gameObject);
                    Loc.SetLanguage(index);
                });
                button.GetComponent<RectTransform>().Place(new Vector2(0.5f, 1f), new Vector2(0f, -210f - i * 136f), new Vector2(620f, 120f));
            }

            DWUI.Pop(panel.transform);
        }

        private void OnDestroy()
        {
            UIMainMenu.Shown -= OnMainMenuShown;
            UIMainMenu.Hidden -= OnMainMenuHidden;
            Campaign.OnChanged -= Refresh;
            Loc.OnLanguageChanged -= OnLanguageChanged;
            if (hudObject != null) Destroy(hudObject);
            BossBar?.Dispose();
        }

        private bool pendingPurchasesChecked;

        private void OnMainMenuShown()
        {
            Hub.Show();

            // Yandex Games: the player can interact from here on
            YandexGames.GameReady();

            if (Campaign.PendingResult != null)
            {
                Campaign.LevelResult result = Campaign.PendingResult;
                Campaign.PendingResult = null;

                // fullscreen ad at the logical pause after a mission (the platform limits the frequency),
                // never in the first missions
                if (result.levelNumber >= 3)
                    AdsManager.ShowInterstitial(_ => Reward.Show(result, () => AfterReward(result)));
                else
                    Reward.Show(result, () => AfterReward(result));
            }
            else if (!pendingPurchasesChecked && YandexIAPWrapper.HasPending && UIMainMenu.PurchasesOffered)
            {
                // purchases paid in an earlier session but not granted: the store offers grant them
                pendingPurchasesChecked = true;
                UIController.ShowPage<IAPStore.UIStore>();
            }
            else if (!Campaign.IsIntroSeen && DustwaterIntro.Instance != null)
            {
                DustwaterIntro.Instance.Play(() => Campaign.IsIntroSeen = true);
            }
            else
            {
                TryPlayActIntro(null);
            }
        }

        // first visit of a new act: its cinematic plays once (story beat 1000 + act)
        private bool TryPlayActIntro(System.Action onFinished)
        {
            int act = Campaign.CurrentAct;
            int beat = 1000 + act;
            if (act <= 0 || Campaign.IsStoryBeatSeen(beat) || DustwaterIntro.Instance == null) return false;

            DustwaterIntro.Instance.PlayAct(act, () =>
            {
                Campaign.MarkStoryBeatSeen(beat);
                onFinished?.Invoke();
            });
            return true;
        }

        private void AfterReward(Campaign.LevelResult result)
        {
            if (result.chapterCompleted >= 0)
            {
                ChapterData chapter = Campaign.Database.Chapters[result.chapterCompleted];
                int beat = result.chapterCompleted * 10 + 1;
                if (!Campaign.IsStoryBeatSeen(beat))
                {
                    Story.Show(DWUI.Style.commanderPortrait, Loc.Get("story.sergeant"), Loc.ChapterLines(result.chapterCompleted, "outro", chapter.outroLines), () =>
                    {
                        Campaign.MarkStoryBeatSeen(beat);
                        System.Action showTown = () => { if (DustwaterTown.Instance != null) Hub.SelectTab(HubView.Tab.Town); };
                        if (!TryPlayActIntro(showTown)) showTown();
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
                    Story.Show(DWUI.Style.commanderPortrait, Loc.Get("story.sergeant"), Loc.ChapterLines(chapterIndex, "intro", chapter.introLines), () =>
                    {
                        Campaign.MarkStoryBeatSeen(beat);
                        Briefing.Show();
                    }, Loc.Format("hub.chapter", chapterIndex + 1, Loc.ChapterTitle(chapterIndex, chapter)));
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
            int chapterIndex = Campaign.CurrentChapterIndex;
            if (level != null && level.IsBossLevel && chapter != null && !string.IsNullOrEmpty(chapter.bossTaunt))
            {
                Story.Show(chapter.bossPortrait, Loc.Upper(Loc.ChapterBoss(chapterIndex, chapter)), new System.Collections.Generic.List<string>() { Loc.ChapterTaunt(chapterIndex, chapter) }, GameController.StartLevel);
                return;
            }

            GameController.StartLevel();
        }
    }
}
