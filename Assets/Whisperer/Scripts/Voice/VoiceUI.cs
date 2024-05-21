/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Whisperer
{
        /// <summary>
        ///     Handles the canvas UI for each listenable object, displaying the mic input level, transcription, and response
        ///     status
        /// </summary>
        public class VoiceUI : ListenableUI
    {
        [Header("Wit Status and Transcription UI References")] [SerializeField]
        protected CanvasGroup _canvasGroup;

        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private TMP_Text _transcriptionText;
        [SerializeField] private Image _iconUIImage;
        [SerializeField] private RectTransform _voiceVolumeIndicator;

        [Header("Tooltip UI References")] [SerializeField]
        private TMP_Text _tooltipText;

        [SerializeField] private CanvasGroup _tooltipCanvasGroup;
        [SerializeField] private InputActionReference _tooltipInputAction;

        [Header("Asset References")] [SerializeField]
        private Sprite _listeningIcon;

        [SerializeField] private Sprite _thinkingIcon;
        [SerializeField] private Sprite _sucessIcon;
        [SerializeField] private Sprite _failIcon;
        [SerializeField] private Sprite _errorIcon;
        [SerializeField] private Sprite _nonActionableIcon;
        private Progress _fadeProgress;
        private IEnumerator _fadeRoutine;
        private Vector3 _initialVoiceStatusScale;
        private Progress _tooltipFadeProgress;

        private bool _triggerPressed;

        public string LastTranscriptionCache { get; private set; }

        protected override void Start()
        {
            base.Start();

            _initialVoiceStatusScale = _voiceVolumeIndicator.transform.localScale;

            _appVoiceExperience.VoiceEvents.OnPartialTranscription.AddListener(HandlePartialTranscription);

            Reset();

            /// Destroy UI if a Listenable is destroyed
            _listenable.OnDestroyed.AddListener(DestroySelf);

            /// Set tooltip text
            if (_listenable.TooltipDefinition != null)
                SetTooltipText(_listenable.TooltipDefinition);

            /// Setup activation fade coroutine
            _fadeRoutine = WaitAndFadeOut();
            _fadeProgress = new Progress(FadeOutAndReset);

            /// Setup tooltip fade coroutine
            _tooltipFadeProgress = new Progress(FadeTooltip);
        }

        protected override void Update()
        {
            base.Update();

            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
            RespondToVoiceVolume();
            HandleTooltip();
        }

        #region Wit Status and Transcription Visibility

        protected override void Reset()
        {
            base.Reset();

            if (_rectTransform == null) return;

            /// Start collapsed and hidden
            SetTranscriptionText("");
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 256);
            _iconUIImage.sprite = _listeningIcon;

            /// Hide tooltips
            _tooltipCanvasGroup.alpha = 0;
        }

        /// <summary>
        ///     Sets UI visibility
        /// </summary>
        /// <param name="visible"></param>
        public void SetVisible(bool visible)
        {
            if (visible)
            {
                StopCoroutine(_fadeRoutine);
                _fadeProgress.Play(0);
            }
            else
            {
                StartHideTimer();
            }
        }

        public void FadeOut()
        {
            if (_fadeRoutine != null)
                StopCoroutine(_fadeRoutine);

            _fadeProgress.PlayReverse(0);
        }

        private void StartHideTimer()
        {
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);

            _fadeRoutine = WaitAndFadeOut();
            StartCoroutine(_fadeRoutine);
        }

        private IEnumerator WaitAndFadeOut()
        {
            yield return new WaitForSeconds(1f);

            if (_opacity != 0) _fadeProgress.PlayReverse(_fadeDuration);
        }

        private void FadeOutAndReset(float opacity)
        {
            SetOpacity(opacity);

            if (opacity == 0) Reset();
        }

        protected override void SetOpacity(float opacity)
        {
            base.SetOpacity(opacity);
            if (_canvasGroup != null) _canvasGroup.alpha = opacity;
        }

        private void DestroySelf(Listenable l)
        {
            _appVoiceExperience.VoiceEvents.OnPartialTranscription.RemoveListener(HandlePartialTranscription);
            //Destroy(gameObject);
        }

        #endregion

        #region Icons and Transcrition

        /// <summary>
        ///     Scales audio input level sprite
        /// </summary>
        private void RespondToVoiceVolume()
        {
            if (!_voiceVolumeIndicator.gameObject.activeInHierarchy)
                return;

            var normalizedVolume = Mathf.Clamp(_micInputValue.AudioLevel * 30, 0f, 1f);
            var volumeScale = new Vector3(_initialVoiceStatusScale.x * normalizedVolume,
                _initialVoiceStatusScale.y * normalizedVolume, _initialVoiceStatusScale.z * normalizedVolume);

            _voiceVolumeIndicator.transform.localScale = volumeScale;
        }

        public void SetUIState(string action, bool success, bool error)
        {
            /// Set icon and play audio
            _iconUIImage.sprite = error ? _errorIcon : success ? _sucessIcon : _failIcon;

            /// If intent is non-actionable (opening an open drawer)
            if (action == "" && success)
                _iconUIImage.sprite = _nonActionableIcon;
            else
                AudioManager.Instance.Play(success ? "WitSuccess" : "WitFail", transform, 0.3f);
        }

        private void SetTranscriptionText(string text)
        {
            LastTranscriptionCache = text;
            _transcriptionText.text = text;
        }

        private void HandlePartialTranscription(string value)
        {
            if (_listenable.IsSelected)
            {
                _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1000);
                _iconUIImage.sprite = _thinkingIcon;
                SetTranscriptionText(value);
            }
        }

        #endregion

        #region Utterance Tooltip Display

        /// <summary>
        ///     Show/Hide tooltips
        /// </summary>
        private void HandleTooltip()
        {
            if (_listenable.TooltipDefinition == null)
                return;

            /// Hide if we've deselected, or tooltip has been disabled
            if (!_listenable.IsSelected || !_listenable.TooltipEnabled)
            {
                HideTooltip();
                return;
            }

            if (_listenable.IsSelected && _tooltipInputAction.action.ReadValue<float>() == 1) ShowTooltip();

            if (_listenable.IsSelected && _tooltipInputAction.action.ReadValue<float>() == 0) HideTooltip();
        }

        private void SetTooltipText(UtteranceTooltipDefinition utteranceTooltips)
        {
            if (utteranceTooltips.exampleUtterances.Count == 0)
                return;

            var tooltips = "";

            /// Randomize the utterances
            var randomUtterances = Utilities.RandomizeList(utteranceTooltips.exampleUtterances);

            /// Grab the first two
            tooltips += randomUtterances[0] + '\n';
            tooltips += randomUtterances[1] + '\n';

            /// Set it
            _tooltipText.text = tooltips;
        }

        private void FadeTooltip(float opacity)
        {
            _tooltipCanvasGroup.alpha = opacity;

            /// When hidden, load next round of random utterances
            if (opacity == 0)
                if (_listenable.TooltipDefinition != null)
                    SetTooltipText(_listenable.TooltipDefinition);
        }

        private void ShowTooltip()
        {
            if (_triggerPressed)
                return;

            _triggerPressed = true;
            _tooltipFadeProgress.Play(_fadeDuration);
        }

        private void HideTooltip()
        {
            if (!_triggerPressed)
                return;

            _triggerPressed = false;
            _tooltipFadeProgress.PlayReverse(_fadeDuration);
        }

        #endregion
    }
}
