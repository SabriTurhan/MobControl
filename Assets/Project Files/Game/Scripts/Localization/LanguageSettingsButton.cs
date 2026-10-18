using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    /// <summary>
    /// Settings row: shows the current language and opens the language picker.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class LanguageSettingsButton : MonoBehaviour
    {
        [SerializeField] TMP_Text label;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() =>
            {
                AudioController.PlaySound(AudioController.AudioClips.buttonSound);
                if (DustwaterUI.Instance != null) DustwaterUI.Instance.OpenLanguagePicker();
            });
        }

        private void OnEnable()
        {
            Loc.OnLanguageChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            Loc.OnLanguageChanged -= Refresh;
        }

        private void Refresh()
        {
            if (label != null) label.text = Loc.Get("ui.language") + ": " + Loc.NATIVE_NAMES[Loc.LanguageIndex];
        }
    }
}
