/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;

namespace Whisperer
{
    public class AudioPlay : MonoBehaviour
    {
        [SerializeField] private Transform _positionTransform;
        [SerializeField] private string _audioClip;
        [SerializeField] private float _volume = 1;
        [SerializeField] private bool _playOnAwake;

        private void OnEnable()
        {
            if (_positionTransform == null) _positionTransform = transform;

            if (enabled && _playOnAwake) Play();
        }

        public void Play()
        {
            AudioManager.Instance.Play(_audioClip, _positionTransform, _volume);
        }

        public void Stop()
        {
            AudioManager.Instance.Stop(_audioClip);
        }
    }
}
