/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Whisperer
{
    public class SupersController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _textField;
        [SerializeField] private List<string> _supers;
        [SerializeField] private float _fadeTime;
        [SerializeField] private float _preDelay;

        private Progress _fader;

        private void Start()
        {
            LevelLoader.Instance.OnLevelWillBeginUnload.AddListener(Begin);
            LevelLoader.Instance.OnLevelLoadComplete.AddListener(End);

            _fader = new Progress(Fade);
            _canvasGroup.alpha = 0;
        }

        private void Begin(float delay)
        {
            _textField.text = _supers[LevelLoader.Instance.NextIndex];
            StartCoroutine(ShowSuper());
        }

        private IEnumerator ShowSuper()
        {
            yield return new WaitForSeconds(_preDelay);
            UXManager.Instance.OpenDisplay("Supers");
            _fader.Play(_fadeTime);
        }

        private void End()
        {
            _fader.PlayReverse(_fadeTime);
        }

        private void Fade(float progress)
        {
            _canvasGroup.alpha = progress;
            if (progress == 0) UXManager.Instance.CloseDisplay("Supers");
        }
    }
}
