using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// First-launch intro cinematic.
    /// </summary>
    public class DustwaterIntro : MonoBehaviour
    {
        public static DustwaterIntro Instance { get; private set; }

        protected virtual void Awake()
        {
            Instance = this;
        }

        public virtual void Play(System.Action onFinished)
        {
            onFinished?.Invoke();
        }

        /// <summary>Story transition into a new act (1 = Frostpeak).</summary>
        public virtual void PlayAct(int act, System.Action onFinished)
        {
            onFinished?.Invoke();
        }
    }
}
