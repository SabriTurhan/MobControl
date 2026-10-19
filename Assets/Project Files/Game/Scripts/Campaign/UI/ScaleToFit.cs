using UnityEngine;

namespace SquadCommand
{
    /// <summary>
    /// Shrinks a fixed-size panel uniformly when its parent is shorter (tablets, 3:4) than the panel needs.
    /// </summary>
    [ExecuteAlways]
    public class ScaleToFit : MonoBehaviour
    {
        [SerializeField] float designHeight = 1700f;
        [SerializeField] float designWidth = 1000f;
        [SerializeField] float margin = 60f;

        private RectTransform parent;

        public void Setup(float height, float width, float edge)
        {
            designHeight = height;
            designWidth = width;
            margin = edge;
            Apply();
        }

        private void OnEnable() => Apply();
        private void Update() => Apply();

        private void Apply()
        {
            if (parent == null) parent = transform.parent as RectTransform;
            if (parent == null) return;

            Rect area = parent.rect;
            float scale = Mathf.Min(1f, (area.height - margin) / designHeight, (area.width - margin * 0.5f) / designWidth);
            if (scale <= 0f) return;
            if (Mathf.Abs(transform.localScale.x - scale) > 0.001f) transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
