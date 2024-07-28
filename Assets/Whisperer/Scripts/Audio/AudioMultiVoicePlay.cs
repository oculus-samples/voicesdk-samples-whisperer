/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections;
using UnityEngine;

namespace Whisperer
{
    public class AudioMultiVoicePlay : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip[] _clips;
        [SerializeField] private AutoPlay _autoPlay;
        [SerializeField] private Vector2 _autoMinMaxTime = new(5, 10);

        private int clipIndex = -1;
        private AudioClip nextClip;

        private void Awake()
        {
            if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            if (_autoPlay != AutoPlay.None)
                StartCoroutine(AutoPlayRoutine());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        public void PlayRandomClip()
        {
            if (_clips.Length == 0) return;

            clipIndex = Random.Range(0, _clips.Length);
            nextClip = _clips[clipIndex];

            Play();
        }

        public void PlayNextClip()
        {
            if (_clips.Length == 0) return;

            clipIndex = (clipIndex + 1) % _clips.Length;
            nextClip = _clips[clipIndex];

            Play();
        }

        private void Play()
        {
            _audioSource.PlayOneShot(nextClip);
        }

        private IEnumerator AutoPlayRoutine()
        {
            while (_autoPlay != AutoPlay.None)
            {
                yield return new WaitForSeconds(Random.Range(_autoMinMaxTime.x, _autoMinMaxTime.y));

                if (_autoPlay == AutoPlay.Random)
                    PlayRandomClip();
                else
                    PlayNextClip();
            }
        }

        private enum AutoPlay
        {
            None,
            Random,
            Sequential
        }
    }
}
