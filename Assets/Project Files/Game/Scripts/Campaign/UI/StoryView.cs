using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    // Short story dialog: speaker portrait, typed lines, tap to continue
    public class StoryView
    {
        private const float CHARS_PER_SECOND = 45f;

        private readonly RectTransform root;
        private readonly Image portrait;
        private readonly TextMeshProUGUI speaker;
        private readonly TextMeshProUGUI line;
        private readonly TextMeshProUGUI header;
        private readonly TextMeshProUGUI hint;
        private readonly RectTransform box;

        private List<string> lines;
        private int index;
        private System.Action onFinished;
        private TweenCase typing;

        public bool IsVisible => root.gameObject.activeSelf;

        public StoryView(RectTransform parent)
        {
            DustwaterUIStyle style = DWUI.Style;

            root = DWUI.Rect(parent, "Story").Stretch();
            Image dim = DWUI.Blocker(root, "Dim", new Color(0f, 0f, 0f, 0.55f));
            Button next = dim.gameObject.AddComponent<Button>();
            next.transition = Selectable.Transition.None;
            next.onClick.AddListener(Next);

            header = DWUI.Text(root, "Header", "", 80f, style.gold);
            header.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0f, 300f), new Vector2(1000f, 220f));

            Image boxImage = DWUI.Image(root, "Box", style.panelWhite);
            box = boxImage.rectTransform;
            box.anchorMin = new Vector2(0f, 0f);
            box.anchorMax = new Vector2(1f, 0f);
            box.pivot = new Vector2(0.5f, 0f);
            box.sizeDelta = new Vector2(-60f, 430f);
            box.anchoredPosition = new Vector2(0f, 160f);

            portrait = DWUI.Icon(box, "Portrait", null);
            portrait.rectTransform.Place(new Vector2(0f, 1f), new Vector2(170f, -10f), new Vector2(300f, 300f));

            Image nameBack = DWUI.Image(box, "Name Back", style.headerOrange);
            nameBack.rectTransform.Place(new Vector2(0f, 1f), new Vector2(560f, 0f), new Vector2(460f, 86f));
            speaker = DWUI.Text(nameBack.rectTransform, "Speaker", "", 44f);
            speaker.rectTransform.Stretch(10, 10, 0, 6);

            line = DWUI.Text(box, "Line", "", 48f, style.textDark, false, TextAlignmentOptions.TopLeft);
            line.rectTransform.Stretch(330, 40, 80, 70);

            hint = DWUI.Text(box, "Hint", Loc.Get("story.tap"), 30f, new Color32(150, 146, 170, 255), false, TextAlignmentOptions.Right);
            hint.rectTransform.Place(new Vector2(1f, 0f), new Vector2(-370f, 40f), new Vector2(680f, 40f));
            hint.textWrappingMode = TextWrappingModes.NoWrap;

            root.gameObject.SetActive(false);
        }

        public void Show(Sprite speakerPortrait, string speakerName, List<string> storyLines, System.Action finished, string headerText = "")
        {
            if (storyLines == null || storyLines.Count == 0)
            {
                finished?.Invoke();
                return;
            }

            root.gameObject.SetActive(true);
            portrait.sprite = speakerPortrait;
            portrait.gameObject.SetActive(speakerPortrait != null);
            speaker.text = speakerName;
            header.text = headerText;
            lines = storyLines;
            onFinished = finished;
            index = 0;

            box.anchoredPosition = new Vector2(0f, -500f);
            box.DOAnchoredPosition(new Vector2(0f, 160f), 0.3f, unscaledTime: true).SetEasing(Ease.Type.BackOut);
            if (!string.IsNullOrEmpty(headerText)) DWUI.Pop(header.transform);

            ShowLine();
        }

        private void ShowLine()
        {
            string text = lines[index];
            line.text = text;
            line.maxVisibleCharacters = 0;
            hint.gameObject.SetActive(false);

            typing?.KillActive();
            typing = Tween.DoFloat(0, text.Length, text.Length / CHARS_PER_SECOND, value => line.maxVisibleCharacters = (int)value, unscaledTime: true)
                .OnComplete(() =>
                {
                    line.maxVisibleCharacters = 99999;
                    hint.gameObject.SetActive(true);
                });
        }

        private void Next()
        {
            if (lines == null) return;

            // first tap completes the line
            if (line.maxVisibleCharacters < lines[index].Length)
            {
                typing?.KillActive();
                line.maxVisibleCharacters = 99999;
                hint.gameObject.SetActive(true);
                return;
            }

            index++;
            if (index < lines.Count)
            {
                ShowLine();
                return;
            }

            root.gameObject.SetActive(false);
            lines = null;
            System.Action callback = onFinished;
            onFinished = null;
            callback?.Invoke();
        }
    }
}
