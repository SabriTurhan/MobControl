using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    /// <summary>
    /// First-launch story intro, played in the town diorama:
    /// 1 the quiet town, 2 the Red Scorpion convoy rolls in, 3 the town falls (camps, red flags),
    /// 4 Boss Vex, 5 the Blue Rangers on the road outside town. Letterbox, captions, skip button.
    /// </summary>
    public class DustwaterIntroPlayer : DustwaterIntro
    {
        [SerializeField] DustwaterTownView town;

        [Header("Actors (visual-only prefabs)")]
        [SerializeField] GameObject banditActor;
        [SerializeField] GameObject vexActor;
        [SerializeField] GameObject truckPrefab;
        [SerializeField] List<GameObject> rangerActors = new List<GameObject>();

        [Header("Story")]
        [SerializeField] Sprite vexPortrait;
        [SerializeField] Sprite sergeantPortrait;

        private static readonly int MOVEMENT_SPEED = Animator.StringToHash("Movement Speed");

        private readonly List<GameObject> spawned = new List<GameObject>();
        private System.Action onFinished;
        private Coroutine routine;

        private RectTransform overlay;
        private RectTransform topBar;
        private RectTransform bottomBar;
        private TextMeshProUGUI caption;
        private TextMeshProUGUI speaker;
        private Image portrait;
        private CanvasGroup captionGroup;
        private CanvasGroup titleGroup;
        private Image fade;

        private Transform cameraTransform;
        private Vector3 cameraLocalPosition;
        private Quaternion cameraLocalRotation;

        public bool IsPlaying { get; private set; }

        public override void Play(System.Action onFinished)
        {
            if (town == null || DustwaterUI.Instance == null)
            {
                onFinished?.Invoke();
                return;
            }

            this.onFinished = onFinished;
            IsPlaying = true;
            BuildOverlay();
            routine = StartCoroutine(Run());
        }

        #region Shots

        private IEnumerator Run()
        {
            DustwaterUI.Instance.Hub.Hide();

            cameraTransform = town.TownCamera.transform;
            cameraLocalPosition = cameraTransform.localPosition;
            cameraLocalRotation = cameraTransform.localRotation;

            Transform root = town.transform;
            town.SetCinematicState(true);
            SetDistrictFlags(false, false);

            // 1. the quiet town
            yield return Fade(1f, 0f, 0.8f);
            StartCoroutine(MoveCamera(root, new Vector3(0f, 40f, -58f), new Vector3(0f, 0f, 12f), new Vector3(0f, 25f, -30f), new Vector3(0f, 0f, 14f), 5.5f));
            yield return Caption(null, null, Loc.Get("intro.1"), 4.6f);

            // 2. the convoy rolls in
            List<Transform> bandits = new List<Transform>();
            Transform truckA = Spawn(truckPrefab, root, new Vector3(-1.2f, 0f, 52f), 180f).transform;
            Transform truckB = Spawn(truckPrefab, root, new Vector3(1.4f, 0f, 62f), 180f).transform;
            for (int i = 0; i < 7; i++)
            {
                Transform bandit = Spawn(banditActor, root, new Vector3(-3f + i, 0f, 48f + (i % 3) * 3f), 180f).transform;
                SetRunning(bandit, true);
                bandits.Add(bandit);
            }

            StartCoroutine(MoveCamera(root, new Vector3(0f, 2.6f, -17f), new Vector3(0f, 1.6f, 22f), new Vector3(0f, 2.2f, -11f), new Vector3(0f, 1.4f, 18f), 6f));
            StartCoroutine(MoveObject(truckA, truckA.localPosition, new Vector3(-1.2f, 0f, 10f), 4.2f));
            StartCoroutine(MoveObject(truckB, truckB.localPosition, new Vector3(1.6f, 0f, 19f), 5f));
            for (int i = 0; i < bandits.Count; i++)
            {
                Vector3 target = new Vector3(-6f + i * 2f, 0f, 2f + (i % 3) * 4f);
                StartCoroutine(MoveObject(bandits[i], bandits[i].localPosition, target, 4.6f + i * 0.15f, () => { }));
            }
            yield return Caption(null, null, Loc.Get("intro.2"), 5.2f);
            foreach (Transform bandit in bandits) SetRunning(bandit, false);

            // 3. the town falls
            yield return Fade(0f, 1f, 0.35f);
            town.SetCinematicState(false);
            SetDistrictFlags(true, false);
            foreach (Transform bandit in bandits) bandit.localRotation = Quaternion.Euler(0f, 180f, 0f);
            StartCoroutine(MoveCamera(root, new Vector3(0f, 31f, -40f), new Vector3(0f, 0f, 10f), new Vector3(0f, 27f, -33f), new Vector3(0f, 0f, 10f), 4.5f));
            yield return Fade(1f, 0f, 0.35f);
            yield return Caption(null, null, Loc.Get("intro.3"), 3.8f);

            // 4. Boss Vex
            Transform vex = Spawn(vexActor, root, new Vector3(0f, 0f, -4f), 180f).transform;
            foreach (Transform bandit in bandits) bandit.gameObject.SetActive(false);
            Spawn(banditActor, root, new Vector3(-2.6f, 0f, -2.6f), 200f);
            Spawn(banditActor, root, new Vector3(2.6f, 0f, -2.6f), 160f);
            StartCoroutine(MoveCamera(root, new Vector3(0.8f, 4.4f, -15.5f), new Vector3(0f, 3.0f, -4f), new Vector3(0.4f, 4.0f, -13f), new Vector3(0f, 3.1f, -4f), 4.5f));
            StartCoroutine(Laugh(vex));
            yield return Caption(vexPortrait, Loc.Get("story.vex"), Loc.Get("intro.4"), 4.2f);

            // 5. the Blue Rangers
            yield return Fade(0f, 1f, 0.35f);
            for (int i = 0; i < rangerActors.Count; i++)
                Spawn(rangerActors[i], root, new Vector3(-2.25f + i * 1.5f, 0f, -26f - (i % 2) * 0.8f), 0f);
            StartCoroutine(MoveCamera(root, new Vector3(1.2f, 4.8f, -37f), new Vector3(0f, 1.6f, 4f), new Vector3(0f, 5.4f, -40f), new Vector3(0f, 1.8f, 6f), 6f));
            yield return Fade(1f, 0f, 0.35f);
            yield return Caption(sergeantPortrait, Loc.Get("story.sergeant"), Loc.Get("intro.5"), 4.8f);

            // title
            captionGroup.alpha = 0f;
            yield return FadeGroup(titleGroup, 0f, 1f, 0.5f);
            yield return WaitUnscaled(2.2f);
            yield return Fade(0f, 1f, 0.5f);

            Finish();
        }

        #endregion

        #region Helpers

        private GameObject Spawn(GameObject prefab, Transform parent, Vector3 localPosition, float yaw)
        {
            GameObject instance = Instantiate(prefab, parent);
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            spawned.Add(instance);
            return instance;
        }

        private static void SetRunning(Transform actor, bool running)
        {
            Animator animator = actor.GetComponentInChildren<Animator>();
            if (animator != null) animator.SetFloat(MOVEMENT_SPEED, running ? 1f : 0f);
        }

        private void SetDistrictFlags(bool red, bool blue)
        {
            foreach (Transform child in town.transform)
            {
                if (child.name.EndsWith("Flag Red")) child.gameObject.SetActive(red);
                else if (child.name.EndsWith("Flag Blue")) child.gameObject.SetActive(blue);
            }
        }

        private IEnumerator MoveCamera(Transform root, Vector3 fromPosition, Vector3 fromLook, Vector3 toPosition, Vector3 toLook, float duration)
        {
            int token = ++cameraToken;
            float t = 0f;
            while (t < 1f && token == cameraToken)
            {
                t += Time.unscaledDeltaTime / duration;
                float e = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
                cameraTransform.position = root.TransformPoint(Vector3.Lerp(fromPosition, toPosition, e));
                cameraTransform.LookAt(root.TransformPoint(Vector3.Lerp(fromLook, toLook, e)));
                yield return null;
            }
        }

        private int cameraToken;

        private IEnumerator MoveObject(Transform target, Vector3 from, Vector3 to, float duration, System.Action done = null)
        {
            float t = 0f;
            while (t < 1f && target != null)
            {
                t += Time.unscaledDeltaTime / duration;
                target.localPosition = Vector3.Lerp(from, to, Mathf.Clamp01(t));
                yield return null;
            }

            done?.Invoke();
        }

        private IEnumerator Laugh(Transform target)
        {
            Vector3 baseScale = target.localScale;
            float t = 0f;
            while (target != null && t < 4f)
            {
                t += Time.unscaledDeltaTime;
                float bounce = Mathf.Abs(Mathf.Sin(t * 9f)) * 0.04f * Mathf.Clamp01(t - 1.2f);
                target.localScale = new Vector3(baseScale.x * (1f + bounce), baseScale.y * (1f - bounce), baseScale.z * (1f + bounce));
                yield return null;
            }
        }

        private IEnumerator Caption(Sprite face, string name, string text, float duration)
        {
            portrait.sprite = face;
            portrait.gameObject.SetActive(face != null);
            speaker.gameObject.SetActive(!string.IsNullOrEmpty(name));
            speaker.text = name;
            caption.text = text;
            caption.maxVisibleCharacters = 0;

            yield return FadeGroup(captionGroup, 0f, 1f, 0.25f);

            float typed = 0f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                typed += Time.unscaledDeltaTime * 42f;
                caption.maxVisibleCharacters = (int)typed;
                yield return null;
            }

            caption.maxVisibleCharacters = 99999;
            yield return FadeGroup(captionGroup, 1f, 0f, 0.2f);
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / duration;
                fade.color = new Color(0f, 0f, 0f, Mathf.Lerp(from, to, Mathf.Clamp01(t)));
                yield return null;
            }
        }

        private static IEnumerator FadeGroup(CanvasGroup group, float from, float to, float duration)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / duration;
                group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t));
                yield return null;
            }
        }

        private static IEnumerator WaitUnscaled(float seconds)
        {
            float t = 0f;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        private void Skip()
        {
            if (!IsPlaying) return;

            if (routine != null) StopCoroutine(routine);
            StopAllCoroutines();
            Finish();
        }

        private void Finish()
        {
            IsPlaying = false;

            foreach (GameObject instance in spawned)
                if (instance != null) Destroy(instance);
            spawned.Clear();

            cameraTransform.localPosition = cameraLocalPosition;
            cameraTransform.localRotation = cameraLocalRotation;
            town.EndCinematic();

            if (overlay != null) Destroy(overlay.gameObject);

            DustwaterUI.Instance.Hub.Show();

            System.Action callback = onFinished;
            onFinished = null;
            callback?.Invoke();
        }

        #endregion

        #region Overlay

        private void BuildOverlay()
        {
            DustwaterUIStyle style = DWUI.Style;
            overlay = DWUI.Rect(DustwaterUI.Instance.Root, "Intro").Stretch();
            DWUI.Blocker(overlay, "Input Blocker", new Color(0f, 0f, 0f, 0f));

            topBar = DWUI.Image(overlay, "Top Bar", style.square, Color.black).rectTransform;
            topBar.anchorMin = new Vector2(0f, 0.88f);
            topBar.anchorMax = Vector2.one;
            topBar.offsetMin = topBar.offsetMax = Vector2.zero;
            bottomBar = DWUI.Image(overlay, "Bottom Bar", style.square, Color.black).rectTransform;
            bottomBar.anchorMin = Vector2.zero;
            bottomBar.anchorMax = new Vector2(1f, 0.16f);
            bottomBar.offsetMin = bottomBar.offsetMax = Vector2.zero;

            RectTransform captionRoot = DWUI.Rect(overlay, "Caption");
            captionRoot.anchorMin = new Vector2(0f, 0f);
            captionRoot.anchorMax = new Vector2(1f, 0.16f);
            captionRoot.offsetMin = new Vector2(40f, 20f);
            captionRoot.offsetMax = new Vector2(-40f, -10f);
            captionGroup = DWUI.Group(captionRoot.gameObject);
            captionGroup.alpha = 0f;

            portrait = DWUI.Icon(captionRoot, "Portrait", null);
            portrait.rectTransform.Place(new Vector2(0f, 0.5f), new Vector2(120f, 70f), new Vector2(260f, 260f));
            speaker = DWUI.Text(captionRoot, "Speaker", "", 44f, style.gold, true, TextAlignmentOptions.Left);
            speaker.rectTransform.Place(new Vector2(0f, 1f), new Vector2(520f, 10f), new Vector2(700f, 56f));
            caption = DWUI.Text(captionRoot, "Text", "", 50f, Color.white, true, TextAlignmentOptions.Left);
            caption.rectTransform.Stretch(270, 10, 40, 10);

            RectTransform title = DWUI.Rect(overlay, "Title").Stretch();
            titleGroup = DWUI.Group(title.gameObject);
            titleGroup.alpha = 0f;
            TextMeshProUGUI name = DWUI.Text(title, "Name", Loc.Get("intro.title"), 150f, style.gold);
            name.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(1060f, 200f));
            TextMeshProUGUI subtitle = DWUI.Text(title, "Subtitle", Loc.Get("intro.subtitle"), 74f, Color.white);
            subtitle.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(1000f, 100f));

            fade = DWUI.Image(overlay, "Fade", style.square, Color.black);
            fade.rectTransform.Stretch();
            fade.raycastTarget = false;

            Button skip = DWUI.Button(overlay, "Skip", style.buttonGray, Loc.Get("intro.skip"), 44f, Skip);
            skip.GetComponent<RectTransform>().Place(new Vector2(1f, 1f), new Vector2(-150f, -110f), new Vector2(220f, 110f));
        }

        #endregion
    }
}
