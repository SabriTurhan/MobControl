using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SquadCommand
{
    // Operation briefing: mission intel (threats, boss), squad slots and weapon card, then DEPLOY
    public class BriefingView
    {
        private readonly RectTransform root;
        private readonly RectTransform panel;
        private readonly RectTransform body;
        private readonly RectTransform picker;

        public bool IsVisible => root.gameObject.activeSelf;

        public BriefingView(RectTransform parent)
        {
            DustwaterUIStyle style = DWUI.Style;

            root = DWUI.Rect(parent, "Briefing").Stretch();
            DWUI.Blocker(root, "Dim", style.dim);

            Image panelImage = DWUI.Image(root, "Panel", style.panelWhite);
            panel = panelImage.rectTransform;
            panel.Place(new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(1000f, 1700f));
            panel.Fit(1700f, 1000f, 40f);
            panelImage.raycastTarget = true;

            body = DWUI.Rect(panel, "Body").Stretch();

            Button close = DWUI.Button(panel, "Close", style.buttonClose, null, 0f, Hide);
            close.GetComponent<RectTransform>().Place(new Vector2(1f, 1f), new Vector2(-20f, -20f), new Vector2(120f, 120f));

            Button deploy = DWUI.Button(panel, "Deploy", style.buttonGreen, Loc.Get("brief.deploy"), 84f, () => DustwaterUI.Instance.Deploy());
            deploy.GetComponent<RectTransform>().Place(new Vector2(0.5f, 0f), new Vector2(0f, 110f), new Vector2(620f, 180f));

            picker = DWUI.Rect(root, "Picker").Stretch();
            picker.gameObject.SetActive(false);

            root.gameObject.SetActive(false);
        }

        public void Show()
        {
            root.gameObject.SetActive(true);
            picker.gameObject.SetActive(false);
            Refresh();
            DWUI.Pop(panel);
        }

        public void Hide()
        {
            root.gameObject.SetActive(false);
        }

        public void Refresh()
        {
            DustwaterUIStyle style = DWUI.Style;
            DWUI.Clear(body);

            LevelData level = LevelController.LevelData;
            int chapterIndex = Campaign.CurrentChapterIndex;
            ChapterData chapter = Campaign.CurrentChapter;
            List<ThreatType> threats = level != null ? level.Threats : new List<ThreatType>();

            Image header = DWUI.Image(body, "Header", style.headerOrange);
            header.rectTransform.anchorMin = new Vector2(0f, 1f);
            header.rectTransform.anchorMax = new Vector2(1f, 1f);
            header.rectTransform.pivot = new Vector2(0.5f, 1f);
            header.rectTransform.sizeDelta = new Vector2(-180f, 100f);
            header.rectTransform.anchoredPosition = new Vector2(-40f, -26f);
            TextMeshProUGUI chapterText = DWUI.Text(header.rectTransform, "Chapter", chapter != null ? Loc.Format("hub.chapter", chapterIndex + 1, Loc.ChapterTitle(chapterIndex, chapter)) : Loc.Get("hub.patrol"), 44f);
            chapterText.rectTransform.Stretch(10, 10, 0, 6);

            TextMeshProUGUI mission = DWUI.Text(body, "Mission", Loc.Format("ui.mission_n", Campaign.CurrentLevelNumber), 76f, style.textDark, false);
            mission.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0f, -185f), new Vector2(900f, 90f));
            string operation = level != null && !string.IsNullOrEmpty(level.OperationName) ? Loc.Format("hub.operation", Loc.OperationName(Campaign.CurrentLevelNumber, level.OperationName)) : Loc.Get("brief.clear_road");
            TextMeshProUGUI operationText = DWUI.Text(body, "Operation", operation, 40f, new Color32(120, 116, 140, 255), false);
            operationText.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0f, -250f), new Vector2(900f, 56f));

            // INTEL
            float y = -320f;
            SectionLabel(Loc.Get("brief.intel"), y);
            y -= 70f;

            if (level != null && level.IsBossLevel && chapter != null)
            {
                IntelRow(chapter.bossPortrait, Loc.Format("brief.boss", Loc.Upper(Loc.ChapterBoss(chapterIndex, chapter))), Loc.Get("brief.boss_desc"), style.red, null, y);
                y -= 150f;
            }

            if (threats.Count == 0)
            {
                IntelRow(Campaign.Database.Units[0].portrait, Loc.Get("brief.light"), Loc.Get("brief.light_desc"), style.green, null, y);
                y -= 150f;
            }

            foreach (ThreatType type in threats)
            {
                ThreatInfo info = Campaign.Database.GetThreat(type);
                if (info == null) continue;
                IntelRow(info.IconFor(Campaign.CurrentAct), Loc.ThreatName(info), Loc.ThreatDescription(info), info.color, type, y);
                y -= 150f;
            }

            // SQUAD
            y -= 10f;
            IReadOnlyList<string> squad = Campaign.Squad;
            SectionLabel(Loc.Get("brief.squad") + "  <size=70%><color=#8A86A0>" + Loc.Format("brief.slots", squad.Count) + "</color></size>", y);
            y -= 60f;

            float slotWidth = Mathf.Min(176f, 880f / Mathf.Max(1, squad.Count) - 12f);
            Vector2 slotSize = new Vector2(slotWidth, slotWidth * 1.4f);
            RectTransform slots = DWUI.Row(body, "Slots", 12f);
            slots.Place(new Vector2(0.5f, 1f), new Vector2(0f, y - slotSize.y * 0.5f), new Vector2(900f, slotSize.y));
            for (int i = 0; i < squad.Count; i++)
            {
                int slot = i;
                CardWidget.Info info = CardWidget.Get(true, squad[i]);
                RectTransform card = CardWidget.Build(slots, info, slotSize, false);
                MakeTappable(card, () => OpenPicker(true, slot, threats));
                if (Counters(info, threats)) CounterRibbon(card);
            }
            y -= slotSize.y + 30f;

            // WEAPON
            SectionLabel(Loc.Get("brief.weapon"), y);
            y -= 60f;
            CardWidget.Info weapon = CardWidget.Get(false, Campaign.SelectedWeaponId);
            Vector2 weaponSize = new Vector2(150f, 210f);
            RectTransform weaponCard = CardWidget.Build(body, weapon, weaponSize, false);
            weaponCard.Place(new Vector2(0.5f, 1f), new Vector2(-300f, y - weaponSize.y * 0.5f), weaponSize);
            MakeTappable(weaponCard, () => OpenPicker(false, 0, threats));
            if (Counters(weapon, threats)) CounterRibbon(weaponCard);

            WeaponCardData weaponData = Campaign.Database.GetWeapon(Campaign.SelectedWeaponId);
            TextMeshProUGUI weaponText = DWUI.Text(body, "Weapon Info", weaponData != null ? "<b>" + Loc.Upper(Loc.WeaponName(weaponData)) + "</b>\n<size=80%>" + Loc.WeaponDescription(weaponData) + "</size>" : "", 40f, style.textDark, false, TextAlignmentOptions.Left);
            weaponText.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(120f, y - weaponSize.y * 0.5f), new Vector2(620f, weaponSize.y));

            TextMeshProUGUI hint = DWUI.Text(body, "Hint", Hint(threats), 34f, style.blue, false);
            hint.rectTransform.Place(new Vector2(0.5f, 0f), new Vector2(0f, 245f), new Vector2(880f, 70f));
        }

        private void SectionLabel(string text, float y)
        {
            TextMeshProUGUI label = DWUI.Text(body, "Section", text, 44f, DWUI.Style.textDark, false, TextAlignmentOptions.Left);
            label.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0f, y - 25f), new Vector2(880f, 56f));
        }

        private void IntelRow(Sprite icon, string title, string description, Color color, ThreatType? type, float y)
        {
            DustwaterUIStyle style = DWUI.Style;
            Image row = DWUI.Image(body, "Intel", style.roundedBack, new Color(color.r, color.g, color.b, 0.18f));
            row.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0f, y - 68f), new Vector2(900f, 136f));

            Image iconImage = DWUI.Icon(row.rectTransform, "Icon", icon);
            iconImage.rectTransform.Place(new Vector2(0f, 0.5f), new Vector2(76f, 4f), new Vector2(128f, 128f));

            TextMeshProUGUI titleText = DWUI.Text(row.rectTransform, "Title", title, 42f, color, false, TextAlignmentOptions.Left);
            titleText.rectTransform.Place(new Vector2(0f, 1f), new Vector2(360f, -36f), new Vector2(420f, 50f));
            TextMeshProUGUI descriptionText = DWUI.Text(row.rectTransform, "Description", description, 30f, style.textDark, false, TextAlignmentOptions.Left);
            descriptionText.rectTransform.Place(new Vector2(0f, 0f), new Vector2(360f, 38f), new Vector2(420f, 70f));

            if (type == null) return;

            // owned cards that counter this threat
            RectTransform counters = DWUI.Row(row.rectTransform, "Counters", 6f, TextAnchor.MiddleRight);
            counters.Place(new Vector2(1f, 0.5f), new Vector2(-130f, 0f), new Vector2(240f, 110f));
            int shown = 0;
            foreach (UnitCardData unit in Campaign.Database.Units)
            {
                if (shown >= 2 || !unit.counters.Contains(type.Value) || Campaign.GetUnitSave(unit.id) == null) continue;
                Image portrait = DWUI.Icon(counters, unit.id, unit.portrait);
                portrait.rectTransform.sizeDelta = new Vector2(100f, 100f);
                shown++;
            }
            foreach (WeaponCardData weapon in Campaign.Database.Weapons)
            {
                if (shown >= 2 || !weapon.counters.Contains(type.Value) || Campaign.GetWeaponSave(weapon.id) == null) continue;
                Image portrait = DWUI.Icon(counters, weapon.id, weapon.portrait);
                portrait.rectTransform.sizeDelta = new Vector2(100f, 100f);
                shown++;
            }
        }

        private static bool Counters(CardWidget.Info info, List<ThreatType> threats)
        {
            if (info == null || threats.Count == 0) return false;

            List<ThreatType> counters = info.isUnit ? Campaign.Database.GetUnit(info.id)?.counters : Campaign.Database.GetWeapon(info.id)?.counters;
            if (counters == null) return false;

            foreach (ThreatType threat in threats)
                if (counters.Contains(threat)) return true;

            return false;
        }

        private static string Hint(List<ThreatType> threats)
        {
            foreach (ThreatType threat in threats)
            {
                bool covered = false;
                string suggestion = null;

                foreach (string id in Campaign.Squad)
                {
                    UnitCardData unit = Campaign.Database.GetUnit(id);
                    if (unit != null && unit.counters.Contains(threat)) covered = true;
                }

                WeaponCardData weapon = Campaign.Database.GetWeapon(Campaign.SelectedWeaponId);
                if (weapon != null && weapon.counters.Contains(threat)) covered = true;
                if (covered) continue;

                foreach (UnitCardData unit in Campaign.Database.Units)
                    if (suggestion == null && unit.counters.Contains(threat) && Campaign.GetUnitSave(unit.id) != null) suggestion = Loc.UnitName(unit);
                foreach (WeaponCardData candidate in Campaign.Database.Weapons)
                    if (suggestion == null && candidate.counters.Contains(threat) && Campaign.GetWeaponSave(candidate.id) != null) suggestion = Loc.WeaponName(candidate);

                ThreatInfo info = Campaign.Database.GetThreat(threat);
                if (suggestion != null && info != null)
                    return Loc.Format("brief.tip", suggestion, Loc.ThreatName(info));
            }

            return threats.Count > 0 ? Loc.Get("brief.ready") : Loc.Get("brief.tap");
        }

        private static void MakeTappable(RectTransform card, UnityEngine.Events.UnityAction action)
        {
            Image image = card.GetComponent<Image>();
            image.raycastTarget = true;
            Button button = card.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() =>
            {
                AudioController.PlaySound(AudioController.AudioClips.buttonSound);
                action();
            });
        }

        private static void CounterRibbon(RectTransform card)
        {
            Image ribbon = DWUI.Image(card, "Counter", DWUI.Style.roundedBack, DWUI.Style.green);
            ribbon.rectTransform.anchorMin = ribbon.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            ribbon.rectTransform.sizeDelta = new Vector2(card.sizeDelta.x * 0.9f, 40f);
            ribbon.rectTransform.anchoredPosition = new Vector2(0f, 4f);
            TextMeshProUGUI label = DWUI.Text(ribbon.rectTransform, "Label", Loc.Get("brief.counter"), 26f);
            label.rectTransform.Stretch(0, 0, 0, 3);
        }

        #region Picker

        private void OpenPicker(bool units, int slot, List<ThreatType> threats)
        {
            DustwaterUIStyle style = DWUI.Style;
            DWUI.Clear(picker);
            picker.gameObject.SetActive(true);

            Image dim = DWUI.Blocker(picker, "Dim", style.dim);
            Button closeDim = dim.gameObject.AddComponent<Button>();
            closeDim.transition = Selectable.Transition.None;
            closeDim.onClick.AddListener(() => picker.gameObject.SetActive(false));

            Image box = DWUI.Image(picker, "Panel", style.panelBlue);
            box.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(960f, 1240f));
            box.rectTransform.Fit(1240f, 960f);
            box.raycastTarget = true;

            TextMeshProUGUI title = DWUI.Text(box.rectTransform, "Title", units ? Loc.Get("brief.choose_unit") : Loc.Get("brief.choose_weapon"), 60f);
            title.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(880f, 80f));

            RectTransform grid = DWUI.Grid(box.rectTransform, "Grid", new Vector2(270f, 378f), new Vector2(24f, 30f), 3);
            grid.anchorMin = grid.anchorMax = new Vector2(0.5f, 1f);
            grid.pivot = new Vector2(0.5f, 1f);
            grid.anchoredPosition = new Vector2(0f, -150f);
            grid.sizeDelta = new Vector2(880f, 0f);

            List<string> squad = new List<string>(Campaign.Squad);

            if (units)
            {
                foreach (UnitCardData unit in Campaign.Database.Units)
                {
                    CardWidget.Info info = CardWidget.Get(unit);
                    if (!info.Owned) continue;

                    RectTransform card = CardWidget.Build(grid, info, new Vector2(270f, 378f), false);
                    if (Counters(info, threats)) CounterRibbon(card);
                    if (!unit.isFiller && squad.Contains(unit.id) && squad[slot] != unit.id) Tag(card, Loc.Get("brief.in_squad"));

                    MakeTappable(card, () =>
                    {
                        int other = squad.IndexOf(unit.id);
                        if (!unit.isFiller && other >= 0 && other != slot)
                            squad[other] = Campaign.Database.StarterUnitId;
                        squad[slot] = unit.id;
                        Campaign.SetSquad(squad);
                        picker.gameObject.SetActive(false);
                    });
                }
            }
            else
            {
                foreach (WeaponCardData weapon in Campaign.Database.Weapons)
                {
                    CardWidget.Info info = CardWidget.Get(weapon);
                    if (!info.Owned) continue;

                    RectTransform card = CardWidget.Build(grid, info, new Vector2(270f, 378f), false);
                    if (Counters(info, threats)) CounterRibbon(card);
                    if (weapon.id == Campaign.SelectedWeaponId) Tag(card, Loc.Get("brief.equipped"));

                    MakeTappable(card, () =>
                    {
                        Campaign.SetWeapon(weapon.id);
                        picker.gameObject.SetActive(false);
                    });
                }
            }

            DWUI.Pop(box.transform);
        }

        private static void Tag(RectTransform card, string text)
        {
            Image tag = DWUI.Image(card, "Tag", DWUI.Style.roundedBack, new Color(0f, 0f, 0f, 0.65f));
            tag.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(card.sizeDelta.x * 0.85f, 60f));
            TextMeshProUGUI label = DWUI.Text(tag.rectTransform, "Label", text, 30f);
            label.rectTransform.Stretch(0, 0, 0, 4);
        }

        #endregion
    }
}
