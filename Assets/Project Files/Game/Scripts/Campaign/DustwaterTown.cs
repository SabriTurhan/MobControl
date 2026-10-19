using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// The 3D town of Dustwater shown behind the Town tab.
    /// </summary>
    public class DustwaterTown : MonoBehaviour
    {
        [Tooltip("Story act whose town this is")]
        [SerializeField] protected int act;
        public int Act => act;

        private static readonly System.Collections.Generic.Dictionary<int, DustwaterTown> towns = new System.Collections.Generic.Dictionary<int, DustwaterTown>();

        /// <summary>The town of the current act.</summary>
        public static DustwaterTown Instance
        {
            get
            {
                int current = Campaign.IsInitialised ? Campaign.CurrentAct : 0;
                if (towns.TryGetValue(current, out DustwaterTown town) && town != null) return town;
                return towns.TryGetValue(0, out DustwaterTown first) ? first : null;
            }
        }

        public static DustwaterTown ForAct(int act) => towns.TryGetValue(act, out DustwaterTown town) ? town : null;

        protected virtual void Awake()
        {
            towns[act] = this;
        }

        /// <summary>What the Town tab displays (the town camera renders into it).</summary>
        public virtual Texture Texture => null;

        public virtual void Show() { }
        public virtual void Hide() { }
        public virtual void Refresh() { }
        public virtual void PlayBuilt(BuildingType type) { }
    }
}
