/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Whisperer
{
    public class SpriteAnimator : MonoBehaviour
    {
        // Refs
        [Header("References")]
        [SerializeField] Image _sprite;
        [SerializeField] string _pathAndBaseSpriteName;
        [SerializeField] Vector2 _loopFrames;

        // Props
        [Header("Properties")]
        [SerializeField] float _fps = 24f;

        // Vars
        private float _frameDuration;
        private int _currentFrame;
        private bool _animate;
        private Progress _progress;
        private float _runTime;

        // Debug
        public bool _showLogs;

        private void Awake()
        {
            _frameDuration = 1 / _fps;
            _progress = new Progress(SetFrame);
            _progress.Loop = true;
            _runTime = _loopFrames.y / _fps;
        }

		private void OnEnable()
		{
            PlayAnimation();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        [ContextMenu("PlayAnimation")]
        private void PlayAnimation()
        {
            _progress.ResetTo0();
            _progress.Play(_runTime);

            //StopAllCoroutines();
            //StartCoroutine(Animate());
            //_animate = true;
        }

        [ContextMenu("StopAnimation")]
        private void StopAnimation()
        {
            _progress.Pause();

            //StopAllCoroutines();
            //_animate = false;
        }

        void SetFrame(float progress) {
            int currentFrame = Mathf.RoundToInt(progress * _loopFrames.y);
            _sprite.sprite = Resources.Load<Sprite>($"{_pathAndBaseSpriteName}{currentFrame}");
        }

		IEnumerator Animate()
        {
            _currentFrame = 0;

            while (true)
            {
                if (_currentFrame >= _loopFrames.y) // loopFrames.y <--- this is the last frame of each anim sequence
                {
                    _currentFrame = (int)_loopFrames.x; // <--- this is the start of the looped section
                }

                _sprite.sprite = Resources.Load<Sprite>($"{_pathAndBaseSpriteName}{_currentFrame}");
                if (_showLogs) Debug.Log($"{_pathAndBaseSpriteName}{_currentFrame}");

                _currentFrame++;
                yield return new WaitForSeconds(_frameDuration);
                yield return 0;
            }

        }
    }
}