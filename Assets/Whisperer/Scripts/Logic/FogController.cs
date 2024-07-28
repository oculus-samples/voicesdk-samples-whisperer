/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;

namespace Whisperer
{
    public class FogController : MonoBehaviour
    {
        [SerializeField] private Vector2 _fogDensity;
        [SerializeField] private Color _color;
        private Progress _fader;

        private Color _orig;

        public void Reset()
        {
            _fader.ResetTo0();
        }

        private void Start()
        {
            _fader = new Progress(Fade);
            _orig = RenderSettings.fogColor;
        }

        public void FadeIn(float duration)
        {
            _fader.Play(duration);
        }

        private void Fade(float progress)
        {
            RenderSettings.fogDensity = Mathf.Lerp(_fogDensity.x, _fogDensity.y, progress);
            RenderSettings.fogColor = Color.Lerp(_orig, _color, progress);
        }
    }
}
