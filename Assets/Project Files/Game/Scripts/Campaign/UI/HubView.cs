using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SquadCommand
{
    // Main menu hub: bottom tabs (Town / Squad / Battle), the next mission card, Supplies counter, Saloon income
    public class HubView
    {
        public enum Tab { Town, Squad, Battle }

        private readonly DustwaterUI ui;
        private readonly RectTransform root;
        private readonly RectTransform battlePanel;
        private readonly Button[] tabButtons = new Button[3];
        private readonly Image[] tabBacks = new Image[3];

        private readonly TextMeshProUGUI chapterText;
        private readonly TextMeshProUGUI missionText;
        private readonly TextMeshProUGUI operationText;
        private readonly RectTransform threatsRow;
        private readonly TextMeshProUGUI suppliesText;
        private readonly Button incomeButton;
        private readonly TextMeshProUGUI incomeText;
        private readonly Image squadBadge;
        private readonly Image townBadge;

        public Tab CurrentTab { get; private set; } = Tab.Battle;
        public bool IsVisible => root.gameObject.activeSelf;

        public HubView(RectTransform parent, DustwaterUI ui)
        {
            this.ui = ui;
            DustwaterUIStyle style = DWUI.Style;

            root = DWUI.Rect(parent, "Hub").Stretch();

            // Supplies counter under the template money panel
            Image pill = DWUI.Image(root, "Supplies Panel", style.roundedBack, new Color(0f, 0f, 0f, 0.35f));
            pill.rectTransform.anchorMin = pill.rectTransform.anchorMax = new Vector2(1f, 1f);
            pill.rectTransform.anchoredPosition = new Vector2(-190f, -205f);
            pill.rectTransform.sizeDelta = new Vector2(290f, 96f);
            Image suppliesIcon = DWUI.Icon(pill.rectTransform, "Icon", style.supplies);
            suppliesIcon.rectTransform.Place(new Vector2(0f, 0.5f), new Vector2(40f, 4f), new Vector2(118f, 118f));
            suppliesText = DWUI.Text(pill.rectTransform, "Amount", "0", 52f, Color.white, true, TextAlignmentOptions.Center);
            suppliesText.rectTransform.Stretch(90, 10, 0, 6);

            // Battle tab: next mission card
            battlePanel = DWUI.Rect(root, "Battle Panel");
            battlePanel.anchorMin = new Vector2(0f, 0f);
            battlePanel.anchorMax = new Vector2(1f, 0f);
            battlePanel.pivot = new Vector2(0.5f, 0f);
            battlePanel.anchoredPosition = new Vector2(0f, 250f);
            battlePanel.sizeDelta = new Vector2(-80f, 560f);

            Image card = DWUI.Image(battlePanel, "Mission Card", style.panelWhite, new Color(1f, 1f, 1f, 0.96f));
            card.rectTransform.Stretch();

            Image header = DWUI.Image(card.rectTransform, "Header", style.headerOrange);
            header.rectTransform.anchorMin = new Vector2(0f, 1f);
            header.rectTransform.anchorMax = new Vector2(1f, 1f);
            header.rectTransform.pivot = new Vector2(0.5f, 1f);
            header.rectTransform.sizeDelta = new Vector2(-30f, 96f);
            header.rectTransform.anchoredPosition = new Vector2(0f, -15f);
            chapterText = DWUI.Text(header.rectTransform, "Chapter", "", 44f);
            chapterText.rectTransform.Stretch(10, 10, 0, 6);

            missionText = DWUI.Text(card.rectTransform, "Mission", "", 70f, style.textDark, false);
            missionText.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(900f, 80f));
            operationText = DWUI.Text(card.rectTransform, "Operation", "", 40f, new Color32(120, 116, 140, 255), false);
            operationText.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0f, -222f), new Vector2(900f, 56f));

            threatsRow = DWUI.Row(card.rectTransform, "Threats", 18f);
            threatsRow.Place(new Vector2(0.5f, 1f), new Vector2(0f, -300f), new Vector2(900f, 96f));

            Button battle = DWUI.Button(card.rectTransform, "Battle Button", style.buttonGreen, Loc.Get("hub.battle"), 78f, () => ui.OpenBriefing());
            battle.GetComponent<RectTransform>().Place(new Vector2(0.5f, 0f), new Vector2(0f, 105f), new Vector2(560f, 170f));

            incomeButton = DWUI.Button(root, "Income Button", style.buttonOrange, null, 0f, CollectIncome);
            incomeButton.GetComponent<RectTransform>().Place(new Vector2(0f, 0f), new Vector2(170f, 900f), new Vector2(260f, 150f));
            Image incomeIcon = DWUI.Icon(incomeButton.transform, "Icon", style.money);
            incomeIcon.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(84f, 84f));
            incomeText = DWUI.Text(incomeButton.transform, "Amount", "+0", 40f);
            incomeText.rectTransform.Place(new Vector2(0.5f, 0f), new Vector2(0f, 42f), new Vector2(240f, 50f));

            // Tab bar
            Image bar = DWUI.Image(root, "Tab Bar", style.square, new Color32(40, 38, 58, 235));
            bar.rectTransform.anchorMin = new Vector2(0f, 0f);
            bar.rectTransform.anchorMax = new Vector2(1f, 0f);
            bar.rectTransform.pivot = new Vector2(0.5f, 0f);
            bar.rectTransform.sizeDelta = new Vector2(0f, 220f);
            bar.rectTransform.anchoredPosition = new Vector2(0f, -40f);
            bar.raycastTarget = true;

            string[] labels = { Loc.Get("hub.tab_town"), Loc.Get("hub.tab_squad"), Loc.Get("hub.tab_battle") };
            Sprite[] icons = { style.town, style.squad, style.battle };
            for (int i = 0; i < 3; i++)
            {
                Tab tab = (Tab)i;
                Button button = DWUI.Button(bar.rectTransform, labels[i] + " Tab", style.panelOutlinedDark, null, 0f, () => SelectTab(tab));
                RectTransform rect = button.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(i / 3f, 0f);
                rect.anchorMax = new Vector2((i + 1) / 3f, 1f);
                rect.offsetMin = new Vector2(10f, 50f);
                rect.offsetMax = new Vector2(-10f, -10f);
                tabBacks[i] = button.GetComponent<Image>();

                Image icon = DWUI.Icon(rect, "Icon", icons[i]);
                icon.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0f, -62f), new Vector2(110f, 110f));
                TextMeshProUGUI label = DWUI.Text(rect, "Label", labels[i], 40f);
                label.rectTransform.Place(new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(330f, 50f));
                tabButtons[i] = button;
            }

            townBadge = Badge(tabButtons[(int)Tab.Town].transform);
            squadBadge = Badge(tabButtons[(int)Tab.Squad].transform);

            root.gameObject.SetActive(false);
        }

        private Image Badge(Transform parent)
        {
            Image badge = DWUI.Image(parent, "Badge", DWUI.Style.circle, DWUI.Style.red);
            badge.rectTransform.Place(new Vector2(1f, 1f), new Vector2(-40f, -26f), new Vector2(54f, 54f));
            TextMeshProUGUI mark = DWUI.Text(badge.rectTransform, "Mark", "!", 40f);
            mark.rectTransform.Stretch(0, 0, 0, 4);
            badge.gameObject.SetActive(false);
            return badge;
        }

        public void Show()
        {
            root.gameObject.SetActive(true);
            SelectTab(Tab.Battle);
            DWUI.Pop(battlePanel);
        }

        public void Hide()
        {
            root.gameObject.SetActive(false);
            ui.Cards.Hide();
            ui.Town.Hide();
        }

        public void SelectTab(Tab tab)
        {
            CurrentTab = tab;
            for (int i = 0; i < 3; i++)
                tabBacks[i].sprite = i == (int)tab ? DWUI.Style.buttonOrange : DWUI.Style.panelOutlinedDark;

            battlePanel.gameObject.SetActive(tab == Tab.Battle);
            UIMainMenu.SetSideButtonsVisible(tab == Tab.Battle);
            if (tab == Tab.Squad) ui.Cards.Show(); else ui.Cards.Hide();
            if (tab == Tab.Town) ui.Town.Show(); else ui.Town.Hide();

            Refresh();
        }

        /// <summary>Level the mission card was filled from (the level can finish loading after the hub shows).</summary>
        public LevelData ShownLevel { get; private set; }

        public void Refresh()
        {
            if (!Campaign.IsInitialised) return;

            suppliesText.text = CurrencyController.Get(CurrencyType.Supplies).ToString();

            int chapterIndex = Campaign.CurrentChapterIndex;
            ChapterData chapter = Campaign.CurrentChapter;
            chapterText.text = chapter != null ? Loc.Format("hub.chapter", chapterIndex + 1, Loc.ChapterTitle(chapterIndex, chapter)) : Loc.Get("hub.patrol");
            missionText.text = Loc.Format("ui.mission_n", Campaign.CurrentLevelNumber);

            LevelData level = LevelController.LevelData;
            ShownLevel = level;
            operationText.text = level != null && !string.IsNullOrEmpty(level.OperationName) ? Loc.Format("hub.operation", Loc.OperationName(Campaign.CurrentLevelNumber, level.OperationName)) : "";

            DWUI.Clear(threatsRow);
            if (level != null)
            {
                if (level.IsBossLevel && chapter != null && chapter.bossPortrait != null)
                    ThreatChip(threatsRow, chapter.bossPortrait, Loc.Get("hub.boss"), DWUI.Style.red);

                foreach (ThreatType type in level.Threats)
                {
                    ThreatInfo info = Campaign.Database.GetThreat(type);
                    if (info != null) ThreatChip(threatsRow, info.IconFor(Campaign.CurrentAct), Loc.ThreatName(info), info.color);
                }
            }

            RefreshIncome();
            RefreshBadges();
        }

        private void ThreatChip(Transform parent, Sprite icon, string text, Color color)
        {
            Image chip = DWUI.Image(parent, "Chip", DWUI.Style.roundedBack, color);
            chip.rectTransform.sizeDelta = new Vector2(250f, 90f);
            Image image = DWUI.Icon(chip.rectTransform, "Icon", icon);
            image.rectTransform.Place(new Vector2(0f, 0.5f), new Vector2(46f, 4f), new Vector2(92f, 92f));
            TextMeshProUGUI label = DWUI.Text(chip.rectTransform, "Label", text, 34f);
            label.rectTransform.Stretch(90, 8, 0, 6);
            label.enableAutoSizing = true;
            label.fontSizeMin = 20f;
            label.fontSizeMax = 34f;
        }

        public void Tick()
        {
            if (IsVisible) RefreshIncome();
        }

        private void RefreshIncome()
        {
            int income = Campaign.PendingIncome;
            bool show = income > 0 && CurrentTab != Tab.Squad;
            incomeButton.gameObject.SetActive(show);
            if (show) incomeText.text = "+" + income;
        }

        private void RefreshBadges()
        {
            bool cardReady = false;
            foreach (UnitCardData unit in Campaign.Database.Units)
                cardReady |= Campaign.CanLevelUp(true, unit.id);
            foreach (WeaponCardData weapon in Campaign.Database.Weapons)
                cardReady |= Campaign.CanLevelUp(false, weapon.id);
            squadBadge.gameObject.SetActive(cardReady);

            bool canBuild = false;
            foreach (BuildingData building in Campaign.BuildingsOfAct(Campaign.CurrentAct))
                canBuild |= Campaign.CanBuild(building);
            townBadge.gameObject.SetActive(canBuild);
        }

        private void CollectIncome()
        {
            Campaign.CollectIncome();
        }
    }
}
