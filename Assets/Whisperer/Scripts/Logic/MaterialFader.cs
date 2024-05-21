/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;

namespace Whisperer
{
    public class MaterialFader : MonoBehaviour
    {
        [SerializeField] private string _parameterName;
        [SerializeField] private Renderer _renderer;
        [SerializeField] private float _materialFadeTime = 2;
        private Material _material;

        private Progress _materialFader;

        private void Start()
        {
            _material = _renderer.material;
            _materialFader = new Progress(SetMaterial);
            _materialFader.Play(0);
        }

        private void SetMaterial(float progress)
        {
            _material.SetFloat(_parameterName, progress);
        }

        [ContextMenu("FadeDown")]
        public void FadeDown()
        {
            _materialFader.PlayFrom1(_materialFadeTime);
        }

        [ContextMenu("FadeUp")]
        public void FadeUp()
        {
            _materialFader.PlayFrom0(_materialFadeTime);
        }
    }
}
