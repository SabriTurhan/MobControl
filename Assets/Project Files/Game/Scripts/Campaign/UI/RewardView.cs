using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    // After a mission: the chest opens and reveals the commander card, plus the Supplies collected on the road
    public class RewardView
    {
        private readonly RectTransform root;
        private System.Action onClosed;

        public bool IsVisible => root.gameObject.activeSelf;

        public RewardView(RectTransform parent)
        {
            root = DWUI.Rect(parent, "Reward").Stretch();
            root.gameObject.SetActive(false);
        }

        public void Show(Campaign.LevelResult result, System.Action onClosed)
        {
            this.onClosed = onClosed;
            DustwaterUIStyle style = DWUI.Style;

            DWUI.Clear(root);
            root.gameObject.SetActive(true);
            DWUI.Blocker(root, "Dim", new Color(0.05f, 0.04f, 0.1f, 0.82f));

            Image shine = DWUI.Icon(root, "Shine", style.shine, new Color(1f, 0.9f, 0.5f, 0.55f));
            shine.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(1100f, 1100f));

            string titleText = result.isBoss ? "BOSS DEFEATED!" : "MISSION " + result.levelNumber + " CLEAR!";
            TextMeshProUGUI title = DWUI.Text(root, "Title", titleText, 92f, style.gold);
            title.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0f, 760f), new Vector2(1000f, 120f));
            DWUI.Pop(title.transform);

            Image chest = DWUI.Icon(root, "Chest", style.chest);
            chest.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(520f, 520f));
            DWUI.Pop(chest.transform, 0.1f);

            RectTransform rewards = DWUI.Rect(root, "Rewards");
            rewards.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000f, 1400f));
            rewards.gameObject.SetActive(false);

            if (result.card != null)
            {
                CardWidget.Info info = CardWidget.Get(result.card.isUnit, result.card.id);
                RectTransform card = CardWidget.Build(rewards, info, new Vector2(420f, 588f), true);
                card.Place(new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(420f, 588f));

                string tag = result.card.isNew ? "NEW!" : "+" + result.card.copies;
                Image badge = DWUI.Image(card, "Tag", style.roundedBack, result.card.isNew ? style.red : style.green);
                badge.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0f, 10f), new Vector2(230f, 84f));
                TextMeshProUGUI badgeText = DWUI.Text(badge.rectTransform, "Label", tag, 54f);
                badgeText.rectTransform.Stretch(0, 0, 0, 6);

                TextMeshProUGUI rarity = DWUI.Text(rewards, "Rarity", result.card.rarity.ToString().ToUpper() + (result.card.isUnit ? " UNIT" : " WEAPON"), 46f, style.RarityColor(result.card.rarity));
                rarity.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0f, -195f), new Vector2(900f, 70f));
            }

            if (result.supplies > 0)
            {
                RectTransform row = DWUI.Row(rewards, "Supplies", 16f);
                row.Place(new Vector2(0.5f, 0.5f), new Vector2(0f, -300f), new Vector2(900f, 110f));
                Image icon = DWUI.Icon(row, "Icon", style.supplies);
                icon.rectTransform.sizeDelta = new Vector2(120f, 120f);
                TextMeshProUGUI amount = DWUI.Text(row, "Amount", "+" + result.supplies + " SUPPLIES", 56f, Color.white, true, TextAlignmentOptions.Left);
                amount.rectTransform.sizeDelta = new Vector2(520f, 90f);
            }

            Button collect = DWUI.Button(rewards, "Collect", style.buttonGreen, "COLLECT", 76f, Close);
            collect.GetComponent<RectTransform>().Place(new Vector2(0.5f, 0.5f), new Vector2(0f, -500f), new Vector2(560f, 170f));

            // chest wobble, then it bursts into the card
            Tween.DelayedCall(0.45f, () =>
            {
                if (chest == null) return;
                chest.transform.DOPushScale(Vector3.one * 1.18f, Vector3.one * 0.2f, 0.35f, 0.18f, Ease.Type.SineOut, Ease.Type.BackIn, unscaledTime: true).OnComplete(() =>
                {
                    if (chest == null) return;
                    chest.gameObject.SetActive(false);
                    rewards.gameObject.SetActive(true);
                    DWUI.Pop(rewards);
                    AudioController.PlaySound(AudioController.AudioClips.win);
                });
            }, true);
        }

        private void Close()
        {
            root.gameObject.SetActive(false);
            System.Action callback = onClosed;
            onClosed = null;
            callback?.Invoke();
        }
    }
}
