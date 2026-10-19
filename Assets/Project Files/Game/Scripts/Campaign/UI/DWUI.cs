using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SquadCommand
{
    // Small runtime UI kit for the Dustwater screens (template sprites + Fredoka fonts, 1080x1920 reference)
    public static class DWUI
    {
        public static DustwaterUIStyle Style { get; set; }

        public static RectTransform Rect(Transform parent, string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            RectTransform rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        public static RectTransform Place(this RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        public static RectTransform Stretch(this RectTransform rect, float left = 0, float right = 0, float top = 0, float bottom = 0)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
            return rect;
        }

        public static RectTransform Band(this RectTransform rect, float yMin, float yMax, float left = 0, float right = 0)
        {
            rect.anchorMin = new Vector2(0f, yMin);
            rect.anchorMax = new Vector2(1f, yMax);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, 0f);
            rect.offsetMax = new Vector2(-right, 0f);
            return rect;
        }

        public static Image Image(Transform parent, string name, Sprite sprite, Color? color = null, bool sliced = true)
        {
            RectTransform rect = Rect(parent, name);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color ?? Color.white;
            image.type = sliced && sprite != null && sprite.border != Vector4.zero ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
            image.preserveAspect = !sliced;
            image.raycastTarget = false;
            return image;
        }

        public static Image Icon(Transform parent, string name, Sprite sprite, Color? color = null)
        {
            Image image = Image(parent, name, sprite, color, false);
            image.preserveAspect = true;
            return image;
        }

        public static TextMeshProUGUI Text(Transform parent, string name, string text, float size, Color? color = null, bool outline = true, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            RectTransform rect = Rect(parent, name);
            TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.font = outline ? Style.fontOutline : Style.font;
            Material preset = outline ? Style.fontOutlineMaterial : Style.fontMaterial;
            if (preset != null) label.fontSharedMaterial = preset;
            label.text = text;
            label.fontSize = size;
            label.color = color ?? Color.white;
            label.alignment = alignment;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;
            // translations differ in length: shrink to fit instead of overflowing
            label.enableAutoSizing = true;
            label.fontSizeMax = size;
            label.fontSizeMin = size * 0.55f;
            return label;
        }

        public static Button Button(Transform parent, string name, Sprite sprite, string label, float fontSize, UnityAction onClick)
        {
            Image image = Image(parent, name, sprite);
            image.raycastTarget = true;
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.disabledColor = new Color(0.65f, 0.65f, 0.65f, 1f);
            button.colors = colors;

            if (onClick != null)
            {
                button.onClick.AddListener(() =>
                {
                    AudioController.PlaySound(AudioController.AudioClips.buttonSound);
                    onClick();
                });
            }

            if (!string.IsNullOrEmpty(label))
            {
                TextMeshProUGUI text = Text(button.transform, "Label", label, fontSize);
                text.rectTransform.Stretch(12, 12, 6, 14);
            }

            return button;
        }

        public static TextMeshProUGUI Label(this Button button) => button.GetComponentInChildren<TextMeshProUGUI>(true);

        public static CanvasGroup Group(GameObject gameObject)
        {
            CanvasGroup group = gameObject.GetComponent<CanvasGroup>();
            if (group == null) group = gameObject.AddComponent<CanvasGroup>();
            return group;
        }

        public static Image Blocker(Transform parent, string name, Color color)
        {
            Image image = Image(parent, name, Style.square, color);
            image.rectTransform.Stretch();
            image.raycastTarget = true;
            return image;
        }

        public static ScrollRect Scroll(Transform parent, string name, out RectTransform content)
        {
            RectTransform viewport = Rect(parent, name);
            Image mask = viewport.gameObject.AddComponent<Image>();
            mask.sprite = Style.square;
            mask.color = new Color(1f, 1f, 1f, 0.004f);
            viewport.gameObject.AddComponent<RectMask2D>();

            ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 30f;
            scroll.viewport = viewport;

            content = Rect(viewport, "Content");
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            scroll.content = content;

            VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            layout.spacing = 24f;
            layout.padding = new RectOffset(0, 0, 20, 40);

            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return scroll;
        }

        // fixed height for children of a scroll content
        public static T Height<T>(this T component, float height) where T : Component
        {
            LayoutElement element = component.GetComponent<LayoutElement>();
            if (element == null) element = component.gameObject.AddComponent<LayoutElement>();
            element.preferredHeight = height;
            element.minHeight = height;
            return component;
        }

        public static RectTransform Grid(Transform parent, string name, Vector2 cellSize, Vector2 spacing, int columns, bool fitHeight = true)
        {
            RectTransform rect = Rect(parent, name);
            GridLayoutGroup grid = rect.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = cellSize;
            grid.spacing = spacing;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
            grid.childAlignment = TextAnchor.UpperCenter;

            if (fitHeight)
            {
                ContentSizeFitter fitter = rect.gameObject.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            return rect;
        }

        public static RectTransform Row(Transform parent, string name, float spacing, TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            RectTransform rect = Rect(parent, name);
            HorizontalLayoutGroup row = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.spacing = spacing;
            row.childAlignment = alignment;
            row.childControlHeight = false;
            row.childControlWidth = false;
            row.childForceExpandHeight = false;
            row.childForceExpandWidth = false;
            return rect;
        }

        public static void Clear(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                Object.Destroy(parent.GetChild(i).gameObject);
        }

        public static void Pop(Transform transform, float delay = 0f)
        {
            // fitted panels pop into their fitted size
            ScaleToFit fit = transform.GetComponent<ScaleToFit>();
            if (fit != null)
            {
                fit.enabled = false;
                transform.localScale = Vector3.one * 0.6f;
                transform.DOScale(1f, 0.25f, delay, true).SetEasing(Ease.Type.BackOut).OnComplete(() => { if (fit != null) fit.enabled = true; });
                return;
            }

            transform.localScale = Vector3.one * 0.6f;
            transform.DOScale(1f, 0.25f, delay, true).SetEasing(Ease.Type.BackOut);
        }

        /// <summary>Panel keeps its design size on phones and shrinks uniformly on shorter screens.</summary>
        public static T Fit<T>(this T component, float height, float width = 1000f, float margin = 60f) where T : Component
        {
            ScaleToFit fit = component.GetComponent<ScaleToFit>();
            if (fit == null) fit = component.gameObject.AddComponent<ScaleToFit>();
            fit.Setup(height, width, margin);
            return component;
        }

        // "[money] 120  [supplies] 15" built from icons and labels; colours the numbers red when not affordable
        public static RectTransform CostRow(Transform parent, string name, int money, int supplies, float size, bool outline = true)
        {
            RectTransform row = Row(parent, name, size * 0.25f);
            if (money > 0) CostItem(row, Style.money, money, CurrencyController.HasAmount(CurrencyType.Money, money), size, outline);
            if (supplies > 0) CostItem(row, Style.supplies, supplies, CurrencyController.HasAmount(CurrencyType.Supplies, supplies), size, outline);
            if (money <= 0 && supplies <= 0)
            {
                TextMeshProUGUI free = Text(row, "Free", Loc.Get("ui.free"), size, Color.white, outline);
                free.rectTransform.sizeDelta = new Vector2(size * 3f, size * 1.2f);
            }

            return row;
        }

        private static void CostItem(Transform row, Sprite icon, int amount, bool affordable, float size, bool outline)
        {
            Image image = Icon(row, "Icon", icon);
            image.rectTransform.sizeDelta = new Vector2(size * 1.25f, size * 1.25f);

            string value = amount.ToString();
            TextMeshProUGUI label = Text(row, "Amount", value, size, affordable ? Color.white : new Color32(255, 120, 110, 255), outline, TextAlignmentOptions.Left);
            label.rectTransform.sizeDelta = new Vector2(size * 0.62f * value.Length + 8f, size * 1.2f);
        }
    }
}
