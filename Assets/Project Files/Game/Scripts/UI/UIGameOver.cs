using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class UIGameOver : UIPage
    {
        [Header("Settings")]
        [SerializeField] float noThanksDelay;

        [SerializeField] UIScaleAnimation levelFailed;

        [SerializeField] UIFadeAnimation backgroundFade;

        [SerializeField] UIScaleAnimation continueButtonScalable;
        [SerializeField] Button continueButton;

        [Header("No Thanks Label")]
        [SerializeField] Button noThanksButton;
        [SerializeField] TextMeshProUGUI noThanksText;

        private TweenCase continuePingPongCase;

        private TextMeshProUGUI continueLabel;
        private string continueDefaultText;
        private Vector2? continueLabelPosition;
        private Vector2? continueLabelPivot;

        public override void Init()
        {
            continueButton.onClick.AddListener(ContinueButton);
            noThanksButton.onClick.AddListener(NoThanksButton);
        }

        #region Show/Hide

        public override void PlayShowAnimation()
        {
            levelFailed.Hide(immediately: true);

            if (continueLabel == null) continueLabel = continueButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (continueLabel != null)
            {
                if (continueDefaultText == null) continueDefaultText = continueLabel.text;
                bool free = Campaign.IsInitialised && Campaign.FreeRevivesLeft > 0;
                continueLabel.text = free ? Loc.Get("ui.free_revive") : Loc.Get("ui.continue", continueDefaultText);

                // a free revive needs no video: hide the ad icon and centre the label
                Transform adIcon = continueButton.transform.Find("Ad Image");
                if (adIcon != null) adIcon.gameObject.SetActive(!free);
                RectTransform labelRect = continueLabel.rectTransform;
                if (continueLabelPosition == null) continueLabelPosition = labelRect.anchoredPosition;
                if (continueLabelPivot == null) continueLabelPivot = labelRect.pivot;
                labelRect.anchorMin = labelRect.anchorMax = free ? new Vector2(0.5f, 0.5f) : new Vector2(1f, 0.5f);
                labelRect.pivot = free ? new Vector2(0.5f, 0.5f) : continueLabelPivot.Value;
                labelRect.anchoredPosition = free ? Vector2.zero : continueLabelPosition.Value;
                continueLabel.alignment = free ? TextAlignmentOptions.Center : continueLabel.alignment;
            }
            continueButtonScalable.Hide(immediately: true);
            HideNoThanksButton();

            float fadeDuration = 0.3f;
            backgroundFade.Show(fadeDuration);

            Tween.DelayedCall(fadeDuration * 0.8f, delegate { 
            
                levelFailed.Show();
                
                ShowNoThanksButton(noThanksDelay);

                continueButtonScalable.Show(scaleMultiplier: 1.05f);

                continuePingPongCase = continueButtonScalable.Transform.DOPingPongScale(1.0f, 1.05f, 0.9f, Ease.Type.QuadIn, Ease.Type.QuadOut, unscaledTime: true);

                UIController.OnPageOpened(this);
            });

        }

        public override void PlayHideAnimation()
        {
            backgroundFade.Hide(0.3f);

            Tween.DelayedCall(0.3f, delegate {

                if (continuePingPongCase != null && continuePingPongCase.IsActive) continuePingPongCase.Kill();

                UIController.OnPageClosed(this);
            });
        }

        #endregion

        #region NoThanks Block

        public void ShowNoThanksButton(float delayToShow = 0.3f, bool immediately = false)
        {
            if (immediately)
            {
                noThanksButton.gameObject.SetActive(true);
                noThanksText.gameObject.SetActive(true);

                return;
            }

            Tween.DelayedCall(delayToShow, delegate { 

                noThanksButton.gameObject.SetActive(true);
                noThanksText.gameObject.SetActive(true);

            });
        }

        public void HideNoThanksButton()
        {
            noThanksButton.gameObject.SetActive(false);
            noThanksText.gameObject.SetActive(false);
        }

        #endregion

        #region Buttons 

        public void ContinueButton()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            // Field Hospital: free revives before the rewarded video
            if (Campaign.IsInitialised && Campaign.TryUseFreeRevive())
            {
                GameController.OnRevive();
                return;
            }

            AdsManager.ShowRewardBasedVideo((watched) =>
            {
                if (watched)
                {
                    GameController.OnRevive();
                }
            });
        }

        public void NoThanksButton()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            UIController.HidePage<UIGameOver>(GameController.ReturnToMainMenu);
        }

        #endregion
    }
}