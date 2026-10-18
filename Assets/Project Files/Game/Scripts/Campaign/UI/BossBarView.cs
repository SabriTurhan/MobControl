using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    // Big boss health bar at the top of the screen while a boss is alive in the level
    public class BossBarView
    {
        private readonly RectTransform root;
        private readonly Image fill;
        private readonly Image portrait;
        private readonly TextMeshProUGUI nameText;

        public BossBarView(RectTransform parent)
        {
            DustwaterUIStyle style = DWUI.Style;

            Image back = DWUI.Image(parent, "Boss Bar", style.roundedBack, new Color(0.1f, 0.08f, 0.14f, 0.85f));
            root = back.rectTransform;
            root.Place(new Vector2(0.5f, 1f), new Vector2(40f, -300f), new Vector2(760f, 120f));

            Image barBack = DWUI.Image(root, "Bar Back", style.roundedBack, new Color(0f, 0f, 0f, 0.6f));
            barBack.rectTransform.Stretch(130, 20, 62, 18);
            fill = DWUI.Image(barBack.rectTransform, "Fill", style.roundedBack, style.red);
            fill.rectTransform.Stretch(4, 4, 4, 4);

            nameText = DWUI.Text(root, "Name", "BOSS", 40f, Color.white, true, TextAlignmentOptions.Left);
            nameText.rectTransform.Stretch(140, 20, 6, 60);

            Image ring = DWUI.Image(root, "Portrait Back", style.circle, style.red);
            ring.rectTransform.Place(new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(170f, 170f));
            portrait = DWUI.Icon(ring.rectTransform, "Portrait", null);
            portrait.rectTransform.Stretch(8, 8, 8, 8);

            root.gameObject.SetActive(false);
            BossBehavior.OnBossChanged += OnBossChanged;
        }

        public void Dispose()
        {
            BossBehavior.OnBossChanged -= OnBossChanged;
        }

        private void OnBossChanged()
        {
            BossBehavior boss = BossBehavior.Active;
            if (boss == null)
            {
                root.gameObject.SetActive(false);
                return;
            }

            ChapterData chapter = Campaign.IsInitialised ? Campaign.CurrentChapter : null;
            nameText.text = chapter != null ? Loc.Upper(Loc.ChapterBoss(Campaign.CurrentChapterIndex, chapter)) : boss.BossName;
            portrait.sprite = chapter != null ? chapter.bossPortrait : null;
            portrait.gameObject.SetActive(portrait.sprite != null);
        }

        public void Tick()
        {
            BossBehavior boss = BossBehavior.Active;
            bool show = boss != null && GameController.IsGameplayActive && boss.HealthFraction > 0f && boss.Enemy != null && boss.Enemy.Health != null;
            if (root.gameObject.activeSelf != show)
            {
                root.gameObject.SetActive(show);
                if (show) DWUI.Pop(root);
            }

            if (show)
                fill.rectTransform.anchorMax = new Vector2(boss.HealthFraction, 1f);
        }
    }
}
