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

            // 2. the convoy rolls in through the town entrance (behind the camera), stops in front of the watchtower
            List<Transform> bandits = new List<Transform>();
            Transform truckA = Spawn(truckPrefab, root, new Vector3(-1.2f, 0f, -34f), 0f).transform;
            Transform truckB = Spawn(truckPrefab, root, new Vector3(1.15f, 0f, -44f), 0f).transform;
            StartCoroutine(Drive(truckA, new Vector3(-1.2f, 0f, 8f), 10f));
            StartCoroutine(Drive(truckB, new Vector3(1.15f, 0f, 16f), 10f));

            // bandits run up the road behind the trucks, then spread out between them
            Vector3[] targets =
            {
                new Vector3(-2.0f, 0f, 0.2f), new Vector3(0f, 0f, 2.0f), new Vector3(2.0f, 0f, 0.5f), new Vector3(1.6f, 0f, 9.0f),
                new Vector3(-1.9f, 0f, 12.5f), new Vector3(0.4f, 0f, 11.8f), new Vector3(-0.9f, 0f, 3.8f),
            };
            for (int i = 0; i < targets.Length; i++)
            {
                Vector3 start = new Vector3(-1.5f + (i % 3) * 1.5f, 0f, -14f - i * 1.0f);
                Transform bandit = Spawn(banditActor, root, start, 0f).transform;
                bandits.Add(bandit);
                StartCoroutine(Walk(bandit, new List<Vector3>() { new Vector3(start.x, 0f, -1.5f), targets[i] }, 7.5f, true, 0.3f + i * 0.1f));
            }

            // high over the road: trucks and bandits pass under the camera into town, no buildings in the way
            StartCoroutine(MoveCamera(root, new Vector3(0f, 4.8f, -17f), new Vector3(0f, 1.0f, 14f), new Vector3(0f, 4.2f, -12.5f), new Vector3(0f, 1.0f, 12f), 6f));
            yield return Caption(null, null, Loc.Get("intro.2"), 5.2f);

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
            // the Rangers march towards the town while Vex waits at the far end of the street
            for (int i = 0; i < rangerActors.Count; i++)
            {
                Vector3 start = new Vector3(-2.25f + i * 1.5f, 0f, -29f - (i % 2) * 0.8f);
                Transform ranger = Spawn(rangerActors[i], root, start, 0f).transform;
                StartCoroutine(Walk(ranger, new List<Vector3>() { start + new Vector3(0f, 0f, 7.5f) }, 1.45f, false, 0.2f));
            }
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
            // the soldier animator starts in the run blend: actors stand still until they move
            SetSpeed(instance.transform, 0f);
            return instance;
        }

        // blend tree: 0 idle, 0.5 walk, 1 run
        private static void SetSpeed(Transform actor, float blend)
        {
            Animator animator = actor.GetComponentInChildren<Animator>();
            if (animator != null) animator.SetFloat(MOVEMENT_SPEED, blend);
        }

        /// <summary>Walks/runs along waypoints facing the way it goes, then stands idle.</summary>
        private IEnumerator Walk(Transform actor, List<Vector3> points, float speed, bool run, float delay)
        {
            yield return WaitUnscaled(delay);

            foreach (Vector3 point in points)
            {
                Vector3 direction = point - actor.localPosition;
                direction.y = 0f;
                if (direction.sqrMagnitude < 0.0001f) continue;
                actor.localRotation = Quaternion.LookRotation(direction);
                SetSpeed(actor, run ? 1f : 0.5f);

                while (actor != null && (point - actor.localPosition).sqrMagnitude > 0.0004f)
                {
                    actor.localPosition = Vector3.MoveTowards(actor.localPosition, point, speed * Time.unscaledDeltaTime);
                    yield return null;
                }
            }

            if (actor != null) SetSpeed(actor, 0f);
        }

        /// <summary>Vehicle drive with a short brake at the end.</summary>
        private IEnumerator Drive(Transform vehicle, Vector3 target, float speed)
        {
            Vector3 start = vehicle.localPosition;
            float duration = Vector3.Distance(start, target) / speed;
            float t = 0f;
            while (vehicle != null && t < 1f)
            {
                t += Time.unscaledDeltaTime / duration;
                float eased = 1f - (1f - Mathf.Clamp01(t)) * (1f - Mathf.Clamp01(t));
                vehicle.localPosition = Vector3.Lerp(start, target, Mathf.Lerp(Mathf.Clamp01(t), eased, Mathf.Clamp01(t)));
                yield return null;
            }
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
