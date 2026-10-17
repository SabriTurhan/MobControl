using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// The 3D town of Dustwater shown behind the Town tab.
    /// </summary>
    public class DustwaterTown : MonoBehaviour
    {
        public static DustwaterTown Instance { get; private set; }

        protected virtual void Awake()
        {
            Instance = this;
        }

        public virtual void Show() { }
        public virtual void Hide() { }
        public virtual void Refresh() { }
        public virtual void PlayBuilt(BuildingType type) { }
    }
}
