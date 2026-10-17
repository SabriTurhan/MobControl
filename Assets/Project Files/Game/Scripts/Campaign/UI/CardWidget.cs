using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    // Commander card: rarity-coloured frame, portrait, level badge, copies bar and name strip
    public static class CardWidget
    {
        public class Info
        {
            public bool isUnit;
            public string id;
            public string displayName;
            public CardRarity rarity;
            public Sprite portrait;
            public int unlockAfterLevel;
            public CardSave save;

            public bool Owned => save != null;
        }

        public static Info Get(UnitCardData data) => new Info()
        {
            isUnit = true, id = data.id, displayName = data.displayName, rarity = data.rarity, portrait = data.portrait,
            unlockAfterLevel = data.unlockAfterLevel, save = Campaign.GetUnitSave(data.id),
        };

        public static Info Get(WeaponCardData data) => new Info()
        {
            isUnit = false, id = data.id, displayName = data.displayName, rarity = data.rarity, portrait = data.portrait,
            unlockAfterLevel = data.unlockAfterLevel, save = Campaign.GetWeaponSave(data.id),
        };

        public static Info Get(bool isUnit, string id)
        {
            if (isUnit)
            {
                UnitCardData unit = Campaign.Database.GetUnit(id);
                return unit != null ? Get(unit) : null;
            }

            WeaponCardData weapon = Campaign.Database.GetWeapon(id);
            return weapon != null ? Get(weapon) : null;
        }

        public static RectTransform Build(Transform parent, Info info, Vector2 size, bool showProgress = true)
        {
            DustwaterUIStyle style = DWUI.Style;
            float s = size.x / 300f;

            Image frame = DWUI.Image(parent, "Card " + info.id, style.CardSprite(info.rarity));
            RectTransform rect = frame.rectTransform;
            rect.sizeDelta = size;

            // keep the rounded corners small on small cards
            frame.pixelsPerUnitMultiplier = Mathf.Max(1f, 2.4f / s);

            bool owned = info.Owned;
            if (!owned) frame.color = new Color(0.45f, 0.45f, 0.5f, 1f);

            // portrait window
            Image window = DWUI.Image(rect, "Window", style.itemBackground, owned ? new Color(1f, 1f, 1f, 0.35f) : new Color(0f, 0f, 0f, 0.25f));
            window.rectTransform.Stretch(14 * s, 14 * s, 14 * s, size.y * 0.3f);
            window.pixelsPerUnitMultiplier = Mathf.Max(1f, 2.4f / s);

            Image portrait = DWUI.Icon(window.rectTransform, "Portrait", owned || info.portrait != null ? info.portrait : style.questionMark);
            portrait.rectTransform.Stretch(6 * s, 6 * s, 6 * s, 0);
            if (!owned)
            {
                portrait.color = new Color(0.08f, 0.08f, 0.12f, 1f);
                Image question = DWUI.Icon(window.rectTransform, "Locked", style.questionMark, new Color(1f, 1f, 1f, 0.9f));
                question.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(110, 110) * s);
            }

            // name strip
            Image strip = DWUI.Image(rect, "Name Strip", style.roundedBack, new Color(0f, 0f, 0f, 0.35f));
            strip.rectTransform.anchorMin = new Vector2(0f, 0f);
            strip.rectTransform.anchorMax = new Vector2(1f, 0f);
            strip.rectTransform.pivot = new Vector2(0.5f, 0f);
            strip.rectTransform.sizeDelta = new Vector2(-28 * s, size.y * 0.15f);
            strip.rectTransform.anchoredPosition = new Vector2(0f, 14 * s);
            TextMeshProUGUI name = DWUI.Text(strip.rectTransform, "Name", info.displayName.ToUpper(), 34 * s);
            name.rectTransform.Stretch(4, 4, 0, 4);
            name.enableAutoSizing = true;
            name.fontSizeMin = 18 * s;
            name.fontSizeMax = 34 * s;

            if (owned)
            {
                Image badge = DWUI.Image(rect, "Level Badge", style.circle, style.dark);
                badge.rectTransform.anchorMin = badge.rectTransform.anchorMax = new Vector2(0f, 1f);
                badge.rectTransform.anchoredPosition = new Vector2(46 * s, -46 * s);
                badge.rectTransform.sizeDelta = new Vector2(84, 84) * s;
                TextMeshProUGUI level = DWUI.Text(badge.rectTransform, "Level", info.save.level.ToString(), 44 * s);
                level.rectTransform.Stretch(0, 0, 0, 6 * s);

                if (showProgress)
                {
                    int needed = Campaign.GetCopiesNeeded(info.save.level);
                    bool max = needed < 0;
                    bool ready = !max && info.save.copies >= needed;

                    Image bar = DWUI.Image(rect, "Copies Bar", style.roundedBack, new Color(0f, 0f, 0f, 0.45f));
                    bar.rectTransform.anchorMin = new Vector2(0f, 0f);
                    bar.rectTransform.anchorMax = new Vector2(1f, 0f);
                    bar.rectTransform.pivot = new Vector2(0.5f, 0f);
                    bar.rectTransform.sizeDelta = new Vector2(-40 * s, 44 * s);
                    bar.rectTransform.anchoredPosition = new Vector2(0f, size.y * 0.15f + 24 * s);

                    Image fill = DWUI.Image(bar.rectTransform, "Fill", style.roundedBack, ready ? style.green : style.blue);
                    float fraction = max ? 1f : Mathf.Clamp01(info.save.copies / (float)needed);
                    fill.rectTransform.anchorMin = Vector2.zero;
                    fill.rectTransform.anchorMax = new Vector2(Mathf.Max(0.08f, fraction), 1f);
                    fill.rectTransform.offsetMin = new Vector2(4, 4) * s;
                    fill.rectTransform.offsetMax = new Vector2(-4, -4) * s;

                    TextMeshProUGUI copies = DWUI.Text(bar.rectTransform, "Copies", max ? "MAX" : info.save.copies + "/" + needed, 28 * s);
                    copies.rectTransform.Stretch(0, 0, 0, 2 * s);

                    if (ready)
                    {
                        Image arrow = DWUI.Icon(rect, "Ready", style.plus, style.green);
                        arrow.rectTransform.anchorMin = arrow.rectTransform.anchorMax = new Vector2(1f, 1f);
                        arrow.rectTransform.anchoredPosition = new Vector2(-40 * s, -40 * s);
                        arrow.rectTransform.sizeDelta = new Vector2(64, 64) * s;
                    }
                }
            }
            else
            {
                TextMeshProUGUI locked = DWUI.Text(rect, "Unlock", "MISSION " + info.unlockAfterLevel, 30 * s, new Color(1f, 1f, 1f, 0.9f));
                locked.rectTransform.anchorMin = new Vector2(0f, 0f);
                locked.rectTransform.anchorMax = new Vector2(1f, 0f);
                locked.rectTransform.sizeDelta = new Vector2(0f, 50 * s);
                locked.rectTransform.anchoredPosition = new Vector2(0f, size.y * 0.15f + 40 * s);
            }

            return rect;
        }
    }
}
