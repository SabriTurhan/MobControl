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
    }
}
