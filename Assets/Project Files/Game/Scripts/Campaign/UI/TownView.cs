using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    // Town tab: the 3D town of Dustwater on top, the building list below
    public class TownView
    {
        private readonly RectTransform root;
        private readonly RectTransform content;
        private readonly TextMeshProUGUI subtitle;

        public bool IsVisible => root.gameObject.activeSelf;

        public TownView(RectTransform parent)
        {
            DustwaterUIStyle style = DWUI.Style;

            root = DWUI.Rect(parent, "Town View").Stretch(0, 0, 0, 180);

            Image header = DWUI.Image(root, "Header", style.roundedBack, new Color(0f, 0f, 0f, 0.4f));
            header.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0f, -330f), new Vector2(760f, 150f));
            TextMeshProUGUI title = DWUI.Text(header.rectTransform, "Title", "DUSTWATER", 72f);
            title.rectTransform.Stretch(10, 10, 6, 60);
            subtitle = DWUI.Text(header.rectTransform, "Subtitle", "", 36f, new Color(1f, 0.92f, 0.7f, 1f));
            subtitle.rectTransform.Stretch(10, 10, 86, 6);

            Image panel = DWUI.Image(root, "Panel", style.panelBlue, new Color(1f, 1f, 1f, 0.97f));
            panel.rectTransform.anchorMin = new Vector2(0f, 0f);
            panel.rectTransform.anchorMax = new Vector2(1f, 0.52f);
            panel.rectTransform.offsetMin = new Vector2(30f, 0f);
            panel.rectTransform.offsetMax = new Vector2(-30f, 0f);
            panel.raycastTarget = true;

            DWUI.Scroll(panel.rectTransform, "Scroll", out content).GetComponent<RectTransform>().Stretch(20, 20, 20, 20);

            root.gameObject.SetActive(false);
        }

        public void Show()
        {
            root.gameObject.SetActive(true);
            if (DustwaterTown.Instance != null) DustwaterTown.Instance.Show();
            Refresh();
        }

        public void Hide()
        {
            if (!root.gameObject.activeSelf) return;

            root.gameObject.SetActive(false);
            if (DustwaterTown.Instance != null) DustwaterTown.Instance.Hide();
        }

        public void Refresh()
        {
            int liberated = 0;
            foreach (ChapterData chapter in Campaign.Database.Chapters)
                if (Campaign.LevelsCompleted >= chapter.lastLevel) liberated++;
            subtitle.text = liberated == 0 ? "Occupied by the Red Scorpions" : liberated >= Campaign.Database.Chapters.Count ? "Free at last!" : liberated + " of " + Campaign.Database.Chapters.Count + " districts liberated";

            DWUI.Clear(content);
            foreach (BuildingData building in Campaign.Database.Buildings)
                Row(building);

            if (DustwaterTown.Instance != null) DustwaterTown.Instance.Refresh();
        }

        private void Row(BuildingData building)
        {
            DustwaterUIStyle style = DWUI.Style;
            int level = Campaign.GetBuildingLevel(building.type);
            int max = building.levels.Count;
            bool unlocked = Campaign.IsBuildingUnlocked(building.type);
            BuildingLevelData next = Campaign.GetNextBuildingLevel(building.type);

            Image row = DWUI.Image(content, building.displayName, style.panelWhite, unlocked ? Color.white : new Color(0.75f, 0.75f, 0.8f, 1f));
            row.Height(250f);

            Image iconBack = DWUI.Image(row.rectTransform, "Icon Back", style.itemBackground, new Color32(255, 236, 200, 255));
            iconBack.rectTransform.Place(new Vector2(0f, 0.5f), new Vector2(125f, 0f), new Vector2(200f, 200f));
            Image icon = DWUI.Icon(iconBack.rectTransform, "Icon", building.icon != null ? building.icon : style.town);
            icon.rectTransform.Stretch(10, 10, 10, 10);
            if (!unlocked) icon.color = new Color(0.1f, 0.1f, 0.15f, 0.85f);

            TextMeshProUGUI name = DWUI.Text(row.rectTransform, "Name", building.displayName.ToUpper(), 46f, style.textDark, false, TextAlignmentOptions.Left);
            name.rectTransform.Place(new Vector2(0f, 1f), new Vector2(470f, -50f), new Vector2(440f, 60f));

            // level pips
            RectTransform pips = DWUI.Row(row.rectTransform, "Pips", 8f, TextAnchor.MiddleLeft);
            pips.Place(new Vector2(0f, 1f), new Vector2(470f, -100f), new Vector2(440f, 34f));
            for (int i = 0; i < max; i++)
            {
                Image pip = DWUI.Image(pips, "Pip", style.circle, i < level ? style.gold : new Color(0f, 0f, 0f, 0.18f));
                pip.rectTransform.sizeDelta = new Vector2(30f, 30f);
            }

            string effect;
            if (!unlocked) effect = "Unlocks after mission " + building.unlockAfterLevel;
            else if (level == 0) effect = building.description;
            else
            {
                string now = string.Format(building.valueFormat, building.levels[level - 1].value);
                effect = next != null ? now + "  <color=#3EC85A>> " + string.Format(building.valueFormat, next.value) + "</color>" : now;
            }

            TextMeshProUGUI effectText = DWUI.Text(row.rectTransform, "Effect", effect, 32f, new Color32(110, 106, 130, 255), false, TextAlignmentOptions.TopLeft);
            effectText.rectTransform.Place(new Vector2(0f, 1f), new Vector2(470f, -170f), new Vector2(440f, 100f));

            if (!unlocked) return;

            if (next == null)
            {
                TextMeshProUGUI maxed = DWUI.Text(row.rectTransform, "Max", "MAX", 52f, style.gold);
                maxed.rectTransform.Place(new Vector2(1f, 0.5f), new Vector2(-130f, 0f), new Vector2(220f, 100f));
                return;
            }

            bool canBuild = Campaign.CanBuild(building.type);
            Button button = DWUI.Button(row.rectTransform, "Build", canBuild ? style.buttonGreen : style.buttonGray, null, 0f, () =>
            {
                if (Campaign.Build(building.type))
                {
                    if (DustwaterTown.Instance != null) DustwaterTown.Instance.PlayBuilt(building.type);
                    Refresh();
                }
            });
            button.interactable = canBuild;
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.Place(new Vector2(1f, 0.5f), new Vector2(-140f, 0f), new Vector2(240f, 190f));

            TextMeshProUGUI label = DWUI.Text(rect, "Label", level == 0 ? "BUILD" : "UPGRADE", 40f);
            label.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0f, -46f), new Vector2(230f, 56f));
            RectTransform cost = DWUI.CostRow(rect, "Cost", next.money, next.supplies, 30f);
            cost.Place(new Vector2(0.5f, 0f), new Vector2(0f, 62f), new Vector2(230f, 60f));
        }
    }
}
