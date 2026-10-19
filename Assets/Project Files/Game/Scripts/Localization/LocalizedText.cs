using TMPro;
using UnityEngine;

namespace SquadCommand
{
    /// <summary>
    /// Shows the translation of <see cref="key"/> on the TMP text of this object and follows language changes.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] string key;
        public string Key => key;

        private TMP_Text label;
        private float originalSize = -1f;

        public void SetKey(string key)
        {
            this.key = key;
            Apply();
        }

        private void OnEnable()
        {
            Loc.OnLanguageChanged += Apply;
            Apply();
        }

        private void OnDisable()
        {
            Loc.OnLanguageChanged -= Apply;
        }

        private void Apply()
        {
            if (string.IsNullOrEmpty(key)) return;
            if (label == null) label = GetComponent<TMP_Text>();
            Fit();
            label.text = Loc.Get(key, label.text);
        }

        // translations can be much longer than the English label: shrink to fit, keep one-line labels on one line
        private void Fit()
        {
            if (originalSize > 0f) return;

            originalSize = label.enableAutoSizing ? label.fontSizeMax : label.fontSize;

            // some template boxes are smaller than their own English text: grow them first so auto-fit
            // only shrinks translations that are really longer
            RectTransform rect = label.rectTransform;
            Vector2 preferred = label.GetPreferredValues(label.text, rect.rect.width, 0f);
            if (preferred.y > rect.rect.height + 1f)
                rect.sizeDelta += new Vector2(0f, preferred.y - rect.rect.height);
            float minimum = label.enableAutoSizing ? Mathf.Min(label.fontSizeMin, originalSize * 0.55f) : originalSize * 0.55f;
            label.enableAutoSizing = true;
            label.fontSizeMax = originalSize;
            label.fontSizeMin = minimum;

            // short single-line labels (buttons, headers) shrink instead of wrapping
            bool singleLineBox = label.rectTransform.rect.height < originalSize * 1.9f;
            bool shortLabel = label.text.Length <= 16;
            if ((singleLineBox || shortLabel) && !label.text.Contains("\n")) label.textWrappingMode = TextWrappingModes.NoWrap;
        }
    }
}
