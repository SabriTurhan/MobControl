using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    // Squad tab: every unit and weapon card, copies progress, level-up, and a detail popup
    public class CardsView
    {
        private static readonly Vector2 CARD_SIZE = new Vector2(300f, 420f);

        private readonly RectTransform root;
        private readonly RectTransform content;
        private readonly RectTransform detail;

        public bool IsVisible => root.gameObject.activeSelf;

        public CardsView(RectTransform parent, RectTransform modalParent)
        {
            DustwaterUIStyle style = DWUI.Style;

            root = DWUI.Rect(parent, "Cards View").Stretch(0, 0, 270, 180);

            Image panel = DWUI.Image(root, "Panel", style.panelBlue, new Color(1f, 1f, 1f, 0.97f));
            panel.rectTransform.Stretch(30, 30, 0, 0);
            panel.raycastTarget = true;

            TextMeshProUGUI title = DWUI.Text(panel.rectTransform, "Title", Loc.Get("cards.title"), 64f);
            title.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(900f, 80f));

            DWUI.Scroll(panel.rectTransform, "Scroll", out content).GetComponent<RectTransform>().Stretch(20, 20, 130, 20);

            // the detail popup covers the whole screen, top bar included
            detail = DWUI.Rect(modalParent, "Card Detail").Stretch();
            detail.gameObject.SetActive(false);

            root.gameObject.SetActive(false);
        }

        public void Show()
        {
            root.gameObject.SetActive(true);
            Refresh();
            DWUI.Pop(root);
        }

        public void Hide()
        {
            root.gameObject.SetActive(false);
            detail.gameObject.SetActive(false);
        }

        public void Refresh()
        {
            DWUI.Clear(content);

            Section(Loc.Get("cards.units"), Loc.Get("cards.units_sub"));
            RectTransform units = DWUI.Grid(content, "Units", new Vector2(CARD_SIZE.x, CARD_SIZE.y + 110f), new Vector2(24f, 24f), 3, false);
            foreach (UnitCardData unit in Campaign.Database.Units)
                Cell(units, CardWidget.Get(unit));

            Section(Loc.Get("cards.weapons"), Loc.Get("cards.weapons_sub"));
            RectTransform weapons = DWUI.Grid(content, "Weapons", new Vector2(CARD_SIZE.x, CARD_SIZE.y + 110f), new Vector2(24f, 24f), 3, false);
            foreach (WeaponCardData weapon in Campaign.Database.Weapons)
                Cell(weapons, CardWidget.Get(weapon));
        }

        private void Section(string title, string subtitle)
        {
            RectTransform section = DWUI.Rect(content, title).Height(120f);
            TextMeshProUGUI label = DWUI.Text(section, "Title", title, 54f, Color.white, true, TextAlignmentOptions.Left);
            label.rectTransform.Stretch(40, 20, 0, 50);
            TextMeshProUGUI sub = DWUI.Text(section, "Subtitle", subtitle, 32f, new Color(1f, 1f, 1f, 0.8f), false, TextAlignmentOptions.Left);
            sub.rectTransform.Stretch(40, 20, 66, 0);
        }

        private void Cell(Transform grid, CardWidget.Info info)
        {
            RectTransform cell = DWUI.Rect(grid, "Cell " + info.id);

            RectTransform card = CardWidget.Build(cell, info, CARD_SIZE);
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 1f);
            card.pivot = new Vector2(0.5f, 1f);
            card.anchoredPosition = Vector2.zero;

            Button tap = card.gameObject.AddComponent<Button>();
            tap.transition = Selectable.Transition.None;
            card.GetComponent<Image>().raycastTarget = true;
            tap.onClick.AddListener(() => ShowDetail(info));

            if (!info.Owned) return;

            int needed = Campaign.GetCopiesNeeded(info.save.level);
            if (needed < 0) return;

            bool ready = info.save.copies >= needed;
            int cost = Campaign.GetLevelUpMoney(info.save.level);
            Button upgrade = DWUI.Button(cell, "Upgrade", ready ? DWUI.Style.buttonGreen : DWUI.Style.buttonGray, null, 0f, () =>
            {
                if (Campaign.LevelUp(info.isUnit, info.id))
                {
                    AudioController.PlaySound(AudioController.AudioClips.buttonSound);
                    Refresh();
                }
            });
            RectTransform rect = upgrade.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(280f, 96f);
            upgrade.interactable = Campaign.CanLevelUp(info.isUnit, info.id);

            if (ready)
            {
                RectTransform costRow = DWUI.CostRow(rect, "Cost", cost, 0, 40f);
                costRow.Stretch(10, 10, 6, 16);
            }
            else
            {
                TextMeshProUGUI label = DWUI.Text(rect, "Label", Loc.Format("cards.level", info.save.level), 36f);
                label.rectTransform.Stretch(10, 10, 6, 16);
            }
        }

        private void ShowDetail(CardWidget.Info info)
        {
            DustwaterUIStyle style = DWUI.Style;
            DWUI.Clear(detail);
            detail.gameObject.SetActive(true);

            Image dim = DWUI.Blocker(detail, "Dim", style.dim);
            Button close = dim.gameObject.AddComponent<Button>();
            close.transition = Selectable.Transition.None;
            close.onClick.AddListener(() => detail.gameObject.SetActive(false));

            Image panel = DWUI.Image(detail, "Panel", style.panelWhite);
            panel.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(900f, 1180f));
            panel.rectTransform.Fit(1300f, 900f);
            panel.raycastTarget = true;

            RectTransform card = CardWidget.Build(panel.rectTransform, info, new Vector2(420f, 588f));
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 1f);
            card.pivot = new Vector2(0.5f, 1f);
            card.anchoredPosition = new Vector2(0f, -50f);

            string description;
            List<ThreatType> counters;
            string stats = "";
            if (info.isUnit)
            {
                UnitCardData unit = Campaign.Database.GetUnit(info.id);
                description = Loc.UnitDescription(unit);
                counters = unit.counters;
                UnitStats now = Campaign.GetUnitStats(unit);
                stats = Loc.Format("cards.stats_unit", now.healthMult.ToString("0.##", Loc.Culture), now.damageMult.ToString("0.##", Loc.Culture));
                if (now.healPerSecond > 0) stats += "\n" + Loc.Format("cards.stats_heal", now.healPerSecond.ToString("0.#", Loc.Culture));
            }
            else
            {
                WeaponCardData weapon = Campaign.Database.GetWeapon(info.id);
                description = Loc.WeaponDescription(weapon);
                counters = weapon.counters;
                stats = info.Owned ? Loc.Format("cards.stats_weapon", Campaign.GetLevelFactor(info.save.level).ToString("0.##", Loc.Culture)) : "";
            }

            TextMeshProUGUI descriptionText = DWUI.Text(panel.rectTransform, "Description", description, 44f, style.textDark, false);
            descriptionText.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0f, -720f), new Vector2(800f, 130f));

            TextMeshProUGUI statsText = DWUI.Text(panel.rectTransform, "Stats", info.Owned ? stats + "\n<size=80%>" + Loc.Format("cards.per_level", (Campaign.Database.StatPerLevel * 100).ToString("0", Loc.Culture)) + "</size>" : Loc.Format("cards.unlocks", info.unlockAfterLevel), 40f, style.blue, false);
            statsText.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0f, -860f), new Vector2(800f, 140f));

            if (counters != null && counters.Count > 0)
            {
                RectTransform row = DWUI.Row(panel.rectTransform, "Counters", 16f);
                row.Place(new Vector2(0.5f, 0f), new Vector2(0f, 110f), new Vector2(820f, 100f));
                TextMeshProUGUI label = DWUI.Text(row, "Label", Loc.Get("cards.strong_vs"), 36f, style.textDark, false);
                label.rectTransform.sizeDelta = new Vector2(300f, 80f);
                foreach (ThreatType type in counters)
                {
                    ThreatInfo threat = Campaign.Database.GetThreat(type);
                    if (threat == null) continue;
                    Image chip = DWUI.Image(row, "Chip", style.roundedBack, threat.color);
                    chip.rectTransform.sizeDelta = new Vector2(250f, 84f);
                    TextMeshProUGUI chipLabel = DWUI.Text(chip.rectTransform, "Label", Loc.ThreatName(threat), 34f);
                    chipLabel.rectTransform.Stretch(6, 6, 0, 6);
                }
            }

            DWUI.Pop(panel.transform);
        }
    }
}
