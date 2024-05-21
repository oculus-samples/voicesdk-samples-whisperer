/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections.Generic;
using UnityEngine;

namespace Whisperer
{
    public class Chalkboard : MonoBehaviour
    {
        [SerializeField] private Renderer _rend;
        [SerializeField] private List<Texture> _textures;
        [SerializeField] private Transform _position;
        [SerializeField] private Material _mat;

        [SerializeField] private int _index,
            _direction;

        private Progress _fader;

        private void Start()
        {
            _mat = _rend.material;
            _fader = new Progress(SetShader);

            _mat.SetTexture("_Base_Texture", _textures[0]);
            _fader.Reset();
        }

        private void SetNextTex()
        {
            _index++;
            if (_index > _textures.Count - 1) return;

            var tex = _index % 2 == 0 ? "_Base_Texture" : "_Top_Texture";
            _mat.SetTexture(tex, _textures[_index]);

            _direction = _index % 2 == 0 ? -1 : 1;
        }

        public void FadeToNext(float duration)
        {
            if (_direction == 1) _fader.PlayFrom0(duration);
            else _fader.PlayFrom1(duration);

            AudioManager.Instance.Play("ChalkWrite", _position);
        }

        private void SetShader(float p)
        {
            _mat.SetFloat("_Lerp", p);

            if (p == 0 || p == 1) SetNextTex();
        }
    }
}
