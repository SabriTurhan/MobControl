using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Watermelon
{
    /// <summary>
    /// The 3D town of Dustwater behind the Town tab. Each plot shows a bandit camp while its district is
    /// occupied, a construction site once liberated, and the building after it is built. District flags turn
    /// from scorpion red to ranger blue when a chapter is cleared. Rendered by its own camera into the upper
    /// part of the screen (the building list covers the lower part).
    /// </summary>
    public class DustwaterTownView : DustwaterTown
    {
        [System.Serializable]
        public class Plot
        {
            public BuildingType type;
            public GameObject camp;
            public GameObject site;
            public GameObject building;
            public GameObject flag;
        }

        [System.Serializable]
        public class District
        {
            [Tooltip("Chapter index that liberates this district")]
            public int chapter;
            public List<GameObject> scorpionFlags = new List<GameObject>();
            public List<GameObject> rangerFlags = new List<GameObject>();
        }

        [SerializeField] Camera townCamera;
        [SerializeField] Transform cameraPivot;
        [SerializeField] List<Plot> plots = new List<Plot>();
        [SerializeField] List<District> districts = new List<District>();

        [Header("Camera")]
        [SerializeField] float swayAngle = 6f;
        [SerializeField] float swaySpeed = 0.18f;

        public Camera TownCamera => townCamera;
        public Transform CameraPivot => cameraPivot;
        public bool IsShown { get; private set; }

        private Quaternion pivotRotation;
        private bool cinematicOverride;
        private RenderTexture renderTexture;

        public const int TEXTURE_WIDTH = 1080;
        public const int TEXTURE_HEIGHT = 700;

        public override Texture Texture
        {
            get
            {
                if (renderTexture == null)
                {
                    renderTexture = new RenderTexture(TEXTURE_WIDTH, TEXTURE_HEIGHT, 24, RenderTextureFormat.ARGB32);
                    renderTexture.antiAliasing = 2;
                    renderTexture.name = "Dustwater Town";
                }

                return renderTexture;
            }
        }

        private void OnDestroy()
        {
            if (renderTexture != null) renderTexture.Release();
        }

        [Header("Fog while the town camera renders")]
        [SerializeField] float fogStart = 75f;
        [SerializeField] float fogEnd = 230f;

        private float savedFogStart;
        private float savedFogEnd;

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCamera;
            RenderPipelineManager.endCameraRendering += OnEndCamera;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCamera;
            RenderPipelineManager.endCameraRendering -= OnEndCamera;
        }

        // the game's fog (30-100m) is tuned for the road camera; the town camera sits further away
        private void OnBeginCamera(ScriptableRenderContext context, Camera camera)
        {
            if (camera != townCamera) return;

            savedFogStart = RenderSettings.fogStartDistance;
            savedFogEnd = RenderSettings.fogEndDistance;
            RenderSettings.fogStartDistance = fogStart;
            RenderSettings.fogEndDistance = fogEnd;
        }

        private void OnEndCamera(ScriptableRenderContext context, Camera camera)
        {
            if (camera != townCamera) return;

            RenderSettings.fogStartDistance = savedFogStart;
            RenderSettings.fogEndDistance = savedFogEnd;
        }

        protected override void Awake()
        {
            base.Awake();
            if (cameraPivot != null) pivotRotation = cameraPivot.localRotation;
            if (townCamera != null) townCamera.gameObject.SetActive(false);
        }

        public override void Show()
        {
            IsShown = true;
            Refresh();
            townCamera.targetTexture = (RenderTexture)Texture;
            townCamera.rect = new Rect(0f, 0f, 1f, 1f);
            townCamera.gameObject.SetActive(true);
        }

        public override void Hide()
        {
            IsShown = false;
            if (!cinematicOverride) townCamera.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!IsShown || cinematicOverride || cameraPivot == null) return;

            float angle = Mathf.Sin(Time.unscaledTime * swaySpeed * Mathf.PI * 2f) * swayAngle;
            cameraPivot.localRotation = pivotRotation * Quaternion.Euler(0f, angle, 0f);
        }

        public override void Refresh()
        {
            if (!Campaign.IsInitialised) return;

            foreach (Plot plot in plots)
            {
                BuildingData building = Campaign.Database.Buildings.Find(b => b.act == act && b.type == plot.type);
                int level = Campaign.GetBuildingLevel(building);
                bool unlocked = Campaign.IsBuildingUnlocked(building);
                SetPlot(plot, level > 0 ? 2 : unlocked ? 1 : 0, level >= 3);
            }

            foreach (District district in districts)
            {
                ChapterData chapter = district.chapter < Campaign.Database.Chapters.Count ? Campaign.Database.Chapters[district.chapter] : null;
                bool liberated = chapter != null && Campaign.LevelsCompleted >= chapter.lastLevel;
                SetFlags(district, liberated);
            }
        }

        private static void SetPlot(Plot plot, int state, bool flag)
        {
            if (plot.camp != null) plot.camp.SetActive(state == 0);
            if (plot.site != null) plot.site.SetActive(state == 1);
            if (plot.building != null) plot.building.SetActive(state == 2);
            if (plot.flag != null) plot.flag.SetActive(state == 2 && flag);
        }

        private static void SetFlags(District district, bool liberated)
        {
            foreach (GameObject flag in district.scorpionFlags) if (flag != null) flag.SetActive(!liberated);
            foreach (GameObject flag in district.rangerFlags) if (flag != null) flag.SetActive(liberated);
        }

        public override void PlayBuilt(BuildingType type)
        {
            Refresh();

            Plot plot = plots.Find(p => p.type == type);
            if (plot == null || plot.building == null) return;

            Transform building = plot.building.transform;
            building.localScale = new Vector3(1f, 0.2f, 1f);
            building.DOScale(Vector3.one, 0.45f, 0f, true).SetEasing(Ease.Type.BackOut);
        }

        public Transform GetPlot(BuildingType type)
        {
            Plot plot = plots.Find(p => p.type == type);
            return plot != null && plot.building != null ? plot.building.transform.parent : null;
        }

        #region Cinematic

        /// <summary>Intro: the town before (all built, blue) and after the bandits took over (camps, red).</summary>
        public void SetCinematicState(bool peaceful)
        {
            cinematicOverride = true;
            townCamera.targetTexture = null;
            townCamera.rect = new Rect(0f, 0f, 1f, 1f);
            townCamera.gameObject.SetActive(true);
            foreach (Plot plot in plots) SetPlot(plot, peaceful ? 2 : 0, false);
            foreach (District district in districts) SetFlags(district, peaceful);
        }

        public void EndCinematic()
        {
            cinematicOverride = false;
            townCamera.gameObject.SetActive(IsShown);
            if (cameraPivot != null) cameraPivot.localRotation = pivotRotation;
            Refresh();
        }

        #endregion
    }
}
