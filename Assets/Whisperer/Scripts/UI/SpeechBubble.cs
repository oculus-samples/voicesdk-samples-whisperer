/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections;
using TMPro;
using UnityEngine;

namespace Whisperer
{
    public class SpeechBubble : FollowUI
    {
        [Header("References")] [SerializeField]
        protected CanvasGroup _canvasGroup;

        [SerializeField] protected TMP_Text textField;
        protected IEnumerator _fadeCoroutine;

        protected IEnumerator _fadeInWaitAndFadeOutCoroutine;
        private Progress _fadeProgress;

        protected virtual void Awake()
        {
            _canvasGroup.alpha = 0;
            textField.text = "";
            _fadeDuration = .2f;
        }

        protected override void Start()
        {
            base.Start();

            _fadeProgress = new Progress(SetOpacity);
            _fadeProgress.Reset();
        }

        protected override void SetOpacity(float opacity)
        {
            base.SetOpacity(opacity);

            _canvasGroup.alpha = opacity;
        }

        /// <summary>
        ///     Set speech text and show for duration before fading out
        /// </summary>
        /// <param name="speech"></param>
        /// <param name="showDuration"></param>
        /// <param name="audioClip"></param>
        /// <param name="volume"></param>
        /// <returns></returns>
        public virtual float SetSpeech(string speech, float showDuration, string audioClip = null, float volume = 1)
        {
            if (_fadeInWaitAndFadeOutCoroutine != null) StopCoroutine(_fadeInWaitAndFadeOutCoroutine);
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

            // Set the text
            textField.text = speech;

            // Start fade in, wait and fade out
            _fadeInWaitAndFadeOutCoroutine = FadeInWaitAndFadeOut(showDuration);
            StartCoroutine(_fadeInWaitAndFadeOutCoroutine);

            if (audioClip is not null)
                AudioManager.Instance.Play(audioClip, _followTransform, volume);

            return showDuration;
        }

        protected virtual IEnumerator FadeInWaitAndFadeOut(float wait)
        {
            _fadeProgress.Play(_fadeDuration, false);
            while (_opacity != 1) yield return null;

            yield return new WaitForSeconds(wait);
            _fadeProgress.PlayReverse(_fadeDuration, false);
        }


        /// <summary>
        ///     Set speech text and show.
        /// </summary>
        /// <param name="speech"></param>
        /// <param name="audioClip"></param>
        /// <param name="volume"></param>
        public virtual void SetSpeechStay(string speech, string audioClip = null, float volume = 1)
        {
            if (_fadeInWaitAndFadeOutCoroutine != null) StopCoroutine(_fadeInWaitAndFadeOutCoroutine);
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

            // Set the text
            textField.text = speech;

            // Start fade in
            _fadeProgress.Play(_fadeDuration, false);

            if (audioClip is not null)
                AudioManager.Instance.Play(audioClip, _followTransform, volume);
        }
    }
}
